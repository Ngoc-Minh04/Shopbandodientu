using Microsoft.EntityFrameworkCore;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Models.Entities;
using System.Data;

namespace Shopbandodientu.Services;

// Interface và class nằm cùng 1 file
public interface IOrderServices
{
    Task<dynamic> CreateOrderAsync(int userId, CreateOrderRequest request);
    Task<dynamic> GetOrderHistoryAsync(int userId);
    Task<dynamic> GetOrderDetailAsync(int userId, int orderId);
    Task<dynamic> CancelOrderAsync(int userId, int orderId);
}

public class OrderServices : IOrderServices
{
    private readonly MinhContext _context;
    private readonly IDiscountServices _discountServices;

    public OrderServices(MinhContext context, IDiscountServices discountServices)
    {
        _context = context;
        _discountServices = discountServices;
    }

    /// <summary>
    /// Đặt hàng từ giỏ hàng (sử dụng Transaction)
    /// </summary>
    public async Task<dynamic> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // 1. Lấy giỏ hàng kèm chi tiết sản phẩm
            var giohang = await _context.Giohangs
                .Include(gh => gh.Chitietgiohangs)
                    .ThenInclude(ct => ct.Sanpham)
                .FirstOrDefaultAsync(gh => gh.Taikhoanid == userId);

            // 2. Validate giỏ hàng không rỗng
            if (giohang == null || !giohang.Chitietgiohangs.Any())
            {
                return new { code = 400, message = "Giỏ hàng trống, không thể đặt hàng" };
            }

            // 3. Kiểm tra tất cả sản phẩm còn đủ số lượng tồn kho
            foreach (var chitiet in giohang.Chitietgiohangs)
            {
                if (chitiet.Sanpham == null)
                {
                    await transaction.RollbackAsync();
                    return new { code = 400, message = "Có sản phẩm không tồn tại trong giỏ hàng" };
                }

                if (chitiet.Sanpham.Trangthai == false)
                {
                    await transaction.RollbackAsync();
                    return new { code = 400, message = $"Sản phẩm '{chitiet.Sanpham.Tensanpham}' hiện không còn bán" };
                }

                if (chitiet.Soluong > chitiet.Sanpham.Soluong)
                {
                    await transaction.RollbackAsync();
                    return new { code = 400, message = $"Sản phẩm '{chitiet.Sanpham.Tensanpham}' không đủ số lượng. Chỉ còn {chitiet.Sanpham.Soluong} sản phẩm" };
                }
            }

            // 4. Tính tổng tiền ban đầu
            decimal tongTien = giohang.Chitietgiohangs.Sum(ct => ct.Sanpham!.Gia * ct.Soluong);

            // 5. Áp dụng mã giảm giá (nếu có)
            decimal discountAmount = 0;
            int? discountId = null;
            if (!string.IsNullOrEmpty(request.DiscountCode))
            {
                var discountResult = await _discountServices.ValidateAndApplyAsync(
                    request.DiscountCode,
                    tongTien,
                    userId
                );

                if (!discountResult.IsValid)
                {
                    await transaction.RollbackAsync();
                    return new { code = 400, message = discountResult.Message };
                }

                discountAmount = discountResult.DiscountAmount;
                discountId = discountResult.DiscountId;
            }

            // 6. Tính tổng tiền sau giảm
            decimal finalAmount = Math.Max(0, tongTien - discountAmount);

            // 7. Tạo đơn hàng mới
            var donhang = new Donhang
            {
                Taikhoanid = userId,
                Ngaydat = DateTime.Now,
                Tongtien = finalAmount, // Sử dụng giá sau giảm
                Trangthai = "Chờ xử lý",
                Tennguoinhan = request.Tennguoinhan,
                Diachigiaohang = request.Diachigiaohang,
                Sdtnguoinhan = request.Sdtnguoinhan
            };

            _context.Donhangs.Add(donhang);
            await _context.SaveChangesAsync();

            // 8. Update orderId vào usage history (nếu có discount)
            if (discountId.HasValue)
            {
                var usageRecord = await _context.Lichsusudungmagiamgia
                    .FirstOrDefaultAsync(l => l.Magiamgiaid == discountId
                                           && l.Taikhoanid == userId
                                           && l.Donhangid == null);
                if (usageRecord != null)
                {
                    usageRecord.Donhangid = donhang.Id;
                }
            }

            // 9. Tạo chi tiết đơn hàng và trừ tồn kho
            foreach (var chitiet in giohang.Chitietgiohangs)
            {
                // Tạo chi tiết đơn hàng (lưu đơn giá tại thời điểm đặt)
                var chitietDonhang = new Chitietdonhang
                {
                    Donhangid = donhang.Id,
                    Sanphamid = chitiet.Sanphamid,
                    Soluong = chitiet.Soluong,
                    Dongia = chitiet.Sanpham!.Gia // Lưu giá tại thời điểm đặt hàng
                };
                _context.Chitietdonhangs.Add(chitietDonhang);

                // Trừ số lượng tồn kho
                chitiet.Sanpham.Soluong -= chitiet.Soluong;
                
                // Tự động set trangthai = false nếu hết hàng
                if (chitiet.Sanpham.Soluong == 0)
                {
                    chitiet.Sanpham.Trangthai = false;
                }
            }

            // 7. Xóa giỏ hàng (xóa chi tiết giỏ hàng)
            _context.Chitietgiohangs.RemoveRange(giohang.Chitietgiohangs);

            // 8. Lưu tất cả thay đổi
            await _context.SaveChangesAsync();

            // 9. Commit transaction
            await transaction.CommitAsync();

            return new
            {
                code = 200,
                message = "Đặt hàng thành công",
                data = new
                {
                    orderId = donhang.Id,
                    tongTien = tongTien,
                    discountAmount = discountAmount,
                    finalAmount = finalAmount,
                    trangthai = donhang.Trangthai,
                    ngayDat = donhang.Ngaydat
                }
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new { code = 500, message = "Đã xảy ra lỗi khi đặt hàng: " + ex.Message };
        }
    }

    /// <summary>
    /// Xem lịch sử đơn hàng
    /// </summary>
    public async Task<dynamic> GetOrderHistoryAsync(int userId)
    {
        try
        {
            var orders = await _context.Donhangs
                .AsNoTracking()
                .Where(dh => dh.Taikhoanid == userId)
                .OrderByDescending(dh => dh.Ngaydat)
                .Select(dh => new
                {
                    orderId = dh.Id,
                    ngayDat = dh.Ngaydat,
                    tongTien = dh.Tongtien,
                    trangthai = dh.Trangthai,
                    tennguoinhan = dh.Tennguoinhan,
                    diachigiaohang = dh.Diachigiaohang,
                    sdtnguoinhan = dh.Sdtnguoinhan,
                    soLuongSanpham = dh.Chitietdonhangs.Count
                })
                .ToListAsync();

            return new
            {
                code = 200,
                message = "Lấy lịch sử đơn hàng thành công",
                data = orders
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Xem chi tiết đơn hàng
    /// </summary>
    public async Task<dynamic> GetOrderDetailAsync(int userId, int orderId)
    {
        try
        {
            var donhang = await _context.Donhangs
                .AsNoTracking()
                .Include(dh => dh.Chitietdonhangs)
                    .ThenInclude(ct => ct.Sanpham)
                .FirstOrDefaultAsync(dh => dh.Id == orderId);

            if (donhang == null)
            {
                return new { code = 404, message = "Không tìm thấy đơn hàng" };
            }

            // Kiểm tra đơn hàng có thuộc về user không
            if (donhang.Taikhoanid != userId)
            {
                return new { code = 403, message = "Bạn không có quyền xem đơn hàng này" };
            }

            var orderDetail = new
            {
                orderId = donhang.Id,
                ngayDat = donhang.Ngaydat,
                tongTien = donhang.Tongtien,
                trangthai = donhang.Trangthai,
                tennguoinhan = donhang.Tennguoinhan,
                diachigiaohang = donhang.Diachigiaohang,
                sdtnguoinhan = donhang.Sdtnguoinhan,
                items = donhang.Chitietdonhangs.Select(ct => new
                {
                    sanphamId = ct.Sanphamid,
                    tensanpham = ct.Sanpham?.Tensanpham,
                    dongia = ct.Dongia,
                    soluong = ct.Soluong,
                    thanhTien = ct.Dongia * ct.Soluong,
                    thuonghieu = ct.Sanpham?.Thuonghieu
                }).ToList()
            };

            return new
            {
                code = 200,
                message = "Lấy chi tiết đơn hàng thành công",
                data = orderDetail
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Hủy đơn hàng (trong 24h, hoàn lại tồn kho)
    /// </summary>
    public async Task<dynamic> CancelOrderAsync(int userId, int orderId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Tìm đơn hàng
            var donhang = await _context.Donhangs
                .Include(dh => dh.Chitietdonhangs)
                    .ThenInclude(ct => ct.Sanpham)
                .FirstOrDefaultAsync(dh => dh.Id == orderId);

            if (donhang == null)
            {
                return new { code = 404, message = "Không tìm thấy đơn hàng" };
            }

            // Kiểm tra đơn hàng có thuộc về user không
            if (donhang.Taikhoanid != userId)
            {
                return new { code = 403, message = "Bạn không có quyền hủy đơn hàng này" };
            }

            // Kiểm tra trạng thái đơn hàng
            if (donhang.Trangthai != "Chờ xử lý")
            {
                return new { code = 400, message = $"Không thể hủy đơn hàng có trạng thái '{donhang.Trangthai}'" };
            }

            // Kiểm tra thời gian (chỉ hủy được trong 24h)
            if (donhang.Ngaydat.HasValue)
            {
                var hoursSinceOrder = (DateTime.Now - donhang.Ngaydat.Value).TotalHours;
                if (hoursSinceOrder > 24)
                {
                    return new { code = 400, message = "Chỉ có thể hủy đơn hàng trong vòng 24 giờ sau khi đặt" };
                }
            }

            // Hoàn lại số lượng tồn kho
            foreach (var chitiet in donhang.Chitietdonhangs)
            {
                if (chitiet.Sanpham != null)
                {
                    chitiet.Sanpham.Soluong += chitiet.Soluong;
                    
                    // Kích hoạt lại sản phẩm nếu có hàng
                    if (chitiet.Sanpham.Soluong > 0)
                    {
                        chitiet.Sanpham.Trangthai = true;
                    }
                }
            }

            // Hoàn lại mã giảm giá (nếu có)
            await _discountServices.RollbackDiscountAsync(orderId);

            // Cập nhật trạng thái đơn hàng
            donhang.Trangthai = "Đã hủy";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new
            {
                code = 200,
                message = "Hủy đơn hàng thành công"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new { code = 500, message = "Đã xảy ra lỗi khi hủy đơn hàng: " + ex.Message };
        }
    }
}
