using Microsoft.EntityFrameworkCore;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Models.Entities;

namespace Shopbandodientu.Services;

// Interface và class nằm cùng 1 file
public interface ICartServices
{
    Task<dynamic> AddToCartAsync(int userId, AddToCartRequest request);
    Task<dynamic> GetCartAsync(int userId);
    Task<dynamic> UpdateCartItemAsync(int userId, UpdateCartItemRequest request);
    Task<dynamic> RemoveFromCartAsync(int userId, int sanphamId);
    Task<dynamic> ClearCartAsync(int userId);
}

public class CartServices : ICartServices
{
    private readonly MinhContext _context;

    public CartServices(MinhContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Thêm sản phẩm vào giỏ hàng
    /// </summary>
    public async Task<dynamic> AddToCartAsync(int userId, AddToCartRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Kiểm tra sản phẩm tồn tại và còn hàng
            var sanpham = await _context.Sanphams
                .AsNoTracking()
                .FirstOrDefaultAsync(sp => sp.Id == request.Sanphamid);

            if (sanpham == null)
            {
                return new { code = 404, message = "Không tìm thấy sản phẩm" };
            }

            if (sanpham.Trangthai == false)
            {
                return new { code = 400, message = "Sản phẩm hiện không còn bán" };
            }

            // Kiểm tra số lượng tồn kho
            if (request.Soluong > sanpham.Soluong)
            {
                return new { code = 400, message = $"Số lượng tồn kho không đủ. Chỉ còn {sanpham.Soluong} sản phẩm" };
            }

            // Tìm hoặc tạo giỏ hàng
            var giohang = await _context.Giohangs
                .Include(gh => gh.Chitietgiohangs)
                .FirstOrDefaultAsync(gh => gh.Taikhoanid == userId);

            if (giohang == null)
            {
                // Tạo giỏ hàng mới
                giohang = new Giohang
                {
                    Taikhoanid = userId,
                    Ngaycapnhat = DateTime.Now
                };
                _context.Giohangs.Add(giohang);
                await _context.SaveChangesAsync();
            }

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var chitiet = await _context.Chitietgiohangs
                .FirstOrDefaultAsync(ct => ct.Giohangid == giohang.Id && ct.Sanphamid == request.Sanphamid);

            if (chitiet != null)
            {
                // Sản phẩm đã có trong giỏ → Cộng dồn số lượng
                int newQuantity = chitiet.Soluong + request.Soluong;
                
                // Kiểm tra lại tồn kho sau khi cộng dồn
                if (newQuantity > sanpham.Soluong)
                {
                    return new { code = 400, message = $"Số lượng tồn kho không đủ. Bạn đã có {chitiet.Soluong} sản phẩm trong giỏ, chỉ còn {sanpham.Soluong} sản phẩm" };
                }

                chitiet.Soluong = newQuantity;
            }
            else
            {
                // Thêm mới sản phẩm vào giỏ
                chitiet = new Chitietgiohang
                {
                    Giohangid = giohang.Id,
                    Sanphamid = request.Sanphamid,
                    Soluong = request.Soluong
                };
                _context.Chitietgiohangs.Add(chitiet);
            }

            // Cập nhật ngày cập nhật giỏ hàng
            giohang.Ngaycapnhat = DateTime.Now;
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Thêm sản phẩm vào giỏ hàng thành công",
                data = new
                {
                    sanphamId = request.Sanphamid,
                    soluong = chitiet.Soluong
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Xem giỏ hàng
    /// </summary>
    public async Task<dynamic> GetCartAsync(int userId)
    {
        try
        {
            // Tìm giỏ hàng
            var giohang = await _context.Giohangs
                .AsNoTracking()
                .Include(gh => gh.Chitietgiohangs)
                    .ThenInclude(ct => ct.Sanpham)
                .FirstOrDefaultAsync(gh => gh.Taikhoanid == userId);

            if (giohang == null || !giohang.Chitietgiohangs.Any())
            {
                return new
                {
                    code = 200,
                    message = "Giỏ hàng trống",
                    data = new
                    {
                        items = new List<object>(),
                        tongTien = 0
                    }
                };
            }

            // Tính toán thông tin giỏ hàng
            var items = giohang.Chitietgiohangs
                .Where(ct => ct.Sanpham != null)
                .Select(ct => new
                {
                    sanphamId = ct.Sanphamid,
                    tensanpham = ct.Sanpham!.Tensanpham,
                    gia = ct.Sanpham.Gia,
                    soluong = ct.Soluong,
                    thuonghieu = ct.Sanpham.Thuonghieu,
                    khuyenmai = ct.Sanpham.Khuyenmai,
                    trangthai = ct.Sanpham.Trangthai,
                    soluongTonkho = ct.Sanpham.Soluong,
                    // Tính thành tiền cho từng sản phẩm
                    thanhTien = ct.Sanpham.Gia * ct.Soluong,
                    hinhAnhDauTien = ct.Sanpham.Sanphamhinhanhs.FirstOrDefault() != null 
                        ? ct.Sanpham.Sanphamhinhanhs.FirstOrDefault()!.Duongdan 
                        : null
                })
                .ToList();

            // Tính tổng tiền toàn bộ giỏ hàng
            decimal tongTien = items.Sum(item => item.thanhTien);

            return new
            {
                code = 200,
                message = "Lấy giỏ hàng thành công",
                data = new
                {
                    items,
                    tongTien,
                    soLuongSanpham = items.Count,
                    ngayCapnhat = giohang.Ngaycapnhat
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Cập nhật số lượng sản phẩm trong giỏ
    /// </summary>
    public async Task<dynamic> UpdateCartItemAsync(int userId, UpdateCartItemRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Tìm giỏ hàng
            var giohang = await _context.Giohangs
                .FirstOrDefaultAsync(gh => gh.Taikhoanid == userId);

            if (giohang == null)
            {
                return new { code = 404, message = "Không tìm thấy giỏ hàng" };
            }

            // Tìm chi tiết giỏ hàng
            var chitiet = await _context.Chitietgiohangs
                .Include(ct => ct.Sanpham)
                .FirstOrDefaultAsync(ct => ct.Giohangid == giohang.Id && ct.Sanphamid == request.Sanphamid);

            if (chitiet == null)
            {
                return new { code = 404, message = "Sản phẩm không có trong giỏ hàng" };
            }

            // Nếu số lượng = 0 → Xóa sản phẩm khỏi giỏ
            if (request.Soluong == 0)
            {
                _context.Chitietgiohangs.Remove(chitiet);
                giohang.Ngaycapnhat = DateTime.Now;
                await _context.SaveChangesAsync();

                return new
                {
                    code = 200,
                    message = "Đã xóa sản phẩm khỏi giỏ hàng"
                };
            }

            // Kiểm tra số lượng tồn kho
            if (chitiet.Sanpham != null && request.Soluong > chitiet.Sanpham.Soluong)
            {
                return new { code = 400, message = $"Số lượng tồn kho không đủ. Chỉ còn {chitiet.Sanpham.Soluong} sản phẩm" };
            }

            // Cập nhật số lượng
            chitiet.Soluong = request.Soluong;
            giohang.Ngaycapnhat = DateTime.Now;
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Cập nhật số lượng thành công",
                data = new
                {
                    sanphamId = request.Sanphamid,
                    soluong = chitiet.Soluong
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Xóa sản phẩm khỏi giỏ hàng
    /// </summary>
    public async Task<dynamic> RemoveFromCartAsync(int userId, int sanphamId)
    {
        try
        {
            // Tìm giỏ hàng
            var giohang = await _context.Giohangs
                .FirstOrDefaultAsync(gh => gh.Taikhoanid == userId);

            if (giohang == null)
            {
                return new { code = 404, message = "Không tìm thấy giỏ hàng" };
            }

            // Tìm chi tiết giỏ hàng
            var chitiet = await _context.Chitietgiohangs
                .FirstOrDefaultAsync(ct => ct.Giohangid == giohang.Id && ct.Sanphamid == sanphamId);

            if (chitiet == null)
            {
                return new { code = 404, message = "Sản phẩm không có trong giỏ hàng" };
            }

            // Xóa sản phẩm
            _context.Chitietgiohangs.Remove(chitiet);
            giohang.Ngaycapnhat = DateTime.Now;
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Xóa sản phẩm khỏi giỏ hàng thành công"
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Xóa toàn bộ giỏ hàng
    /// </summary>
    public async Task<dynamic> ClearCartAsync(int userId)
    {
        try
        {
            // Tìm giỏ hàng
            var giohang = await _context.Giohangs
                .Include(gh => gh.Chitietgiohangs)
                .FirstOrDefaultAsync(gh => gh.Taikhoanid == userId);

            if (giohang == null)
            {
                return new { code = 404, message = "Không tìm thấy giỏ hàng" };
            }

            // Xóa tất cả chi tiết giỏ hàng
            _context.Chitietgiohangs.RemoveRange(giohang.Chitietgiohangs);
            giohang.Ngaycapnhat = DateTime.Now;
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Đã xóa toàn bộ giỏ hàng"
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }
}
