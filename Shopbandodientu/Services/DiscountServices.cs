using Microsoft.EntityFrameworkCore;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Models.Entities;
using System.Data;

namespace Shopbandodientu.Services;

// Interface và class nằm cùng 1 file
public interface IDiscountServices
{
    Task<dynamic> CreateAsync(CreateDiscountRequest request);
    Task<dynamic> UpdateAsync(UpdateDiscountRequest request);
    Task<dynamic> DeleteAsync(int id);
    Task<dynamic> GetListAsync(DiscountListRequest request);
    Task<dynamic> GetDetailAsync(int id);
    Task<dynamic> ValidateDiscountAsync(ValidateDiscountRequest request);
    Task<ValidateDiscountResponse> ValidateAndApplyAsync(string macode, decimal orderAmount, int taikhoanId);
    Task<dynamic> RollbackDiscountAsync(int donhangId);
    Task<dynamic> ToggleStatusAsync(ToggleDiscountStatusRequest request);
    Task<dynamic> GetUsageHistoryAsync(UsageHistoryRequest request);
}

public class DiscountServices : IDiscountServices
{
    private readonly MinhContext _context;

    public DiscountServices(MinhContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tạo mã giảm giá mới (Admin only)
    /// </summary>
    public async Task<dynamic> CreateAsync(CreateDiscountRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Kiểm tra mã code unique
            bool macodeExists = await _context.Magiamgia
                .AnyAsync(m => m.Macode == request.Macode);
            
            if (macodeExists)
            {
                return new { code = 400, message = "Mã code đã tồn tại trong hệ thống" };
            }

            // Tạo entity Magiamgium
            var magiamgia = new Magiamgium
            {
                Macode = request.Macode,
                Tenchuongtrinh = request.Tenchuongtrinh,
                Mota = request.Mota,
                Loaigiamgia = request.Loaigiamgia,
                Giatrigiam = request.Giatrigiam,
                Giamtoida = request.Giamtoida,
                Giatridonhangtoithieu = request.Giatridonhangtoithieu,
                Soluong = request.Soluong,
                Soluongdasudung = 0, // Default
                Ngaybatdau = request.Ngaybatdau,
                Ngayketthuc = request.Ngayketthuc,
                Trangthai = true, // Default
                Ngaytao = DateTime.Now
            };

            // Lưu vào DB
            _context.Magiamgia.Add(magiamgia);
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Tạo mã giảm giá thành công",
                data = new
                {
                    id = magiamgia.Id,
                    macode = magiamgia.Macode,
                    tenchuongtrinh = magiamgia.Tenchuongtrinh,
                    loaigiamgia = magiamgia.Loaigiamgia,
                    giatrigiam = magiamgia.Giatrigiam,
                    soluong = magiamgia.Soluong,
                    trangthai = magiamgia.Trangthai
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Cập nhật mã giảm giá (Admin only)
    /// </summary>
    public async Task<dynamic> UpdateAsync(UpdateDiscountRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Kiểm tra mã giảm giá tồn tại
            var magiamgia = await _context.Magiamgia
                .FirstOrDefaultAsync(m => m.Id == request.Id);

            if (magiamgia == null)
            {
                return new { code = 404, message = "Không tìm thấy mã giảm giá" };
            }

            // Validate không cho phép giảm số lượng xuống dưới số lượng đã sử dụng
            if (request.Soluong < magiamgia.Soluongdasudung)
            {
                return new { code = 400, message = $"Số lượng mã không thể nhỏ hơn số lượng đã sử dụng ({magiamgia.Soluongdasudung})" };
            }

            // Validate giá trị giảm theo loại
            if (magiamgia.Loaigiamgia == "percent")
            {
                if (request.Giatrigiam < 0 || request.Giatrigiam > 100)
                {
                    return new { code = 400, message = "Giá trị giảm theo % phải từ 0 đến 100" };
                }
                if (!request.Giamtoida.HasValue || request.Giamtoida.Value <= 0)
                {
                    return new { code = 400, message = "Giảm tối đa phải lớn hơn 0 khi loại giảm giá là percent" };
                }
            }

            // Cập nhật các fields được phép
            // KHÔNG cho phép thay đổi Macode và Loaigiamgia
            magiamgia.Tenchuongtrinh = request.Tenchuongtrinh;
            magiamgia.Mota = request.Mota;
            magiamgia.Giatrigiam = request.Giatrigiam;
            magiamgia.Giamtoida = request.Giamtoida;
            magiamgia.Giatridonhangtoithieu = request.Giatridonhangtoithieu;
            magiamgia.Soluong = request.Soluong;
            magiamgia.Ngaybatdau = request.Ngaybatdau;
            magiamgia.Ngayketthuc = request.Ngayketthuc;

            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Cập nhật mã giảm giá thành công",
                data = new
                {
                    id = magiamgia.Id,
                    macode = magiamgia.Macode,
                    tenchuongtrinh = magiamgia.Tenchuongtrinh,
                    loaigiamgia = magiamgia.Loaigiamgia,
                    giatrigiam = magiamgia.Giatrigiam,
                    soluong = magiamgia.Soluong,
                    soluongdasudung = magiamgia.Soluongdasudung
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Xóa mã giảm giá - Soft delete (Admin only)
    /// </summary>
    public async Task<dynamic> DeleteAsync(int id)
    {
        try
        {
            // Kiểm tra mã giảm giá tồn tại
            var magiamgia = await _context.Magiamgia
                .FirstOrDefaultAsync(m => m.Id == id);

            if (magiamgia == null)
            {
                return new { code = 404, message = "Không tìm thấy mã giảm giá" };
            }

            // Soft delete: Set trangthai = false
            magiamgia.Trangthai = false;
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Xóa mã giảm giá thành công"
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Lấy danh sách mã giảm giá với filter và phân trang (Admin only)
    /// </summary>
    public async Task<dynamic> GetListAsync(DiscountListRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Query cơ bản
            var query = _context.Magiamgia.AsNoTracking();

            // Filter theo trạng thái
            if (request.Trangthai.HasValue)
            {
                query = query.Where(m => m.Trangthai == request.Trangthai.Value);
            }

            // Filter theo thời gian hiệu lực
            if (!string.IsNullOrEmpty(request.TimeFilter))
            {
                var now = DateTime.Now;
                switch (request.TimeFilter.ToLower())
                {
                    case "active": // Đang hiệu lực
                        query = query.Where(m => m.Ngaybatdau <= now && m.Ngayketthuc >= now);
                        break;
                    case "expired": // Hết hạn
                        query = query.Where(m => m.Ngayketthuc < now);
                        break;
                    case "upcoming": // Chưa bắt đầu
                        query = query.Where(m => m.Ngaybatdau > now);
                        break;
                }
            }

            // Filter theo keyword (tìm kiếm mã code hoặc tên chương trình)
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.ToLower();
                query = query.Where(m => m.Macode.ToLower().Contains(keyword) 
                                      || m.Tenchuongtrinh.ToLower().Contains(keyword));
            }

            // Sắp xếp theo ngày tạo giảm dần
            query = query.OrderByDescending(m => m.Ngaytao);

            // Đếm tổng số
            var totalItems = await query.CountAsync();

            // Phân trang
            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(m => new
                {
                    m.Id,
                    m.Macode,
                    m.Tenchuongtrinh,
                    m.Mota,
                    m.Loaigiamgia,
                    m.Giatrigiam,
                    m.Giamtoida,
                    m.Giatridonhangtoithieu,
                    m.Soluong,
                    m.Soluongdasudung,
                    Soluongconlai = m.Soluong - (m.Soluongdasudung ?? 0),
                    m.Ngaybatdau,
                    m.Ngayketthuc,
                    m.Trangthai,
                    m.Ngaytao
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);

            return new
            {
                code = 200,
                message = "Lấy danh sách mã giảm giá thành công",
                data = new
                {
                    items,
                    pagination = new
                    {
                        pageIndex = request.PageIndex,
                        pageSize = request.PageSize,
                        totalItems,
                        totalPages
                    }
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Lấy chi tiết mã giảm giá (Admin only)
    /// </summary>
    public async Task<dynamic> GetDetailAsync(int id)
    {
        try
        {
            var magiamgia = await _context.Magiamgia
                .AsNoTracking()
                .Include(m => m.Lichsusudungmagiamgia)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (magiamgia == null)
            {
                return new { code = 404, message = "Không tìm thấy mã giảm giá" };
            }

            // Tính số lượng còn lại
            int soluongconlai = magiamgia.Soluong - (magiamgia.Soluongdasudung ?? 0);

            // Tính tổng giá trị đã giảm và tổng lượt sử dụng từ usage history
            decimal tongGiatriDagiam = magiamgia.Lichsusudungmagiamgia.Sum(ls => ls.Giatrigiamthucte);
            int tongLuotSudung = magiamgia.Lichsusudungmagiamgia.Count;

            var response = new DiscountDetailResponse
            {
                Id = magiamgia.Id,
                Macode = magiamgia.Macode,
                Tenchuongtrinh = magiamgia.Tenchuongtrinh,
                Mota = magiamgia.Mota,
                Loaigiamgia = magiamgia.Loaigiamgia,
                Giatrigiam = magiamgia.Giatrigiam,
                Giamtoida = magiamgia.Giamtoida,
                Giatridonhangtoithieu = magiamgia.Giatridonhangtoithieu ?? 0,
                Soluong = magiamgia.Soluong,
                Soluongdasudung = magiamgia.Soluongdasudung ?? 0,
                Soluongconlai = soluongconlai,
                Ngaybatdau = magiamgia.Ngaybatdau,
                Ngayketthuc = magiamgia.Ngayketthuc,
                Trangthai = magiamgia.Trangthai ?? true,
                Ngaytao = magiamgia.Ngaytao ?? DateTime.Now,
                TongGiatriDagiam = tongGiatriDagiam,
                TongLuotSudung = tongLuotSudung
            };

            return new
            {
                code = 200,
                message = "Lấy chi tiết mã giảm giá thành công",
                data = response
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của mã giảm giá (Customer)
    /// </summary>
    public async Task<dynamic> ValidateDiscountAsync(ValidateDiscountRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Tìm mã giảm giá
            var magiamgia = await _context.Magiamgia
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Macode == request.Macode);

            if (magiamgia == null)
            {
                return new
                {
                    code = 200,
                    message = "Mã giảm giá không tồn tại",
                    data = new ValidateDiscountResponse
                    {
                        IsValid = false,
                        Message = "Mã giảm giá không tồn tại",
                        DiscountAmount = 0,
                        FinalAmount = request.OrderAmount
                    }
                };
            }

            // Kiểm tra trạng thái
            if (magiamgia.Trangthai == false)
            {
                return new
                {
                    code = 200,
                    message = "Mã giảm giá đã bị vô hiệu hóa",
                    data = new ValidateDiscountResponse
                    {
                        IsValid = false,
                        Message = "Mã giảm giá đã bị vô hiệu hóa",
                        DiscountAmount = 0,
                        FinalAmount = request.OrderAmount
                    }
                };
            }

            // Kiểm tra thời gian hiệu lực
            var now = DateTime.Now;
            if (now < magiamgia.Ngaybatdau)
            {
                return new
                {
                    code = 200,
                    message = "Mã giảm giá chưa bắt đầu",
                    data = new ValidateDiscountResponse
                    {
                        IsValid = false,
                        Message = $"Mã giảm giá có hiệu lực từ {magiamgia.Ngaybatdau:dd/MM/yyyy HH:mm}",
                        DiscountAmount = 0,
                        FinalAmount = request.OrderAmount
                    }
                };
            }

            if (now > magiamgia.Ngayketthuc)
            {
                return new
                {
                    code = 200,
                    message = "Mã giảm giá đã hết hạn",
                    data = new ValidateDiscountResponse
                    {
                        IsValid = false,
                        Message = $"Mã giảm giá đã hết hạn vào {magiamgia.Ngayketthuc:dd/MM/yyyy HH:mm}",
                        DiscountAmount = 0,
                        FinalAmount = request.OrderAmount
                    }
                };
            }

            // Kiểm tra số lượng còn lại
            int soluongconlai = magiamgia.Soluong - (magiamgia.Soluongdasudung ?? 0);
            if (soluongconlai <= 0)
            {
                return new
                {
                    code = 200,
                    message = "Mã giảm giá đã hết lượt sử dụng",
                    data = new ValidateDiscountResponse
                    {
                        IsValid = false,
                        Message = "Mã giảm giá đã hết lượt sử dụng",
                        DiscountAmount = 0,
                        FinalAmount = request.OrderAmount
                    }
                };
            }

            // Kiểm tra giá trị đơn hàng tối thiểu
            if (request.OrderAmount < (magiamgia.Giatridonhangtoithieu ?? 0))
            {
                return new
                {
                    code = 200,
                    message = "Giá trị đơn hàng chưa đủ điều kiện",
                    data = new ValidateDiscountResponse
                    {
                        IsValid = false,
                        Message = $"Đơn hàng tối thiểu {magiamgia.Giatridonhangtoithieu:N0}đ để sử dụng mã này",
                        DiscountAmount = 0,
                        FinalAmount = request.OrderAmount
                    }
                };
            }

            // Tính giá trị giảm
            decimal discountAmount = 0;
            if (magiamgia.Loaigiamgia == "percent")
            {
                discountAmount = request.OrderAmount * magiamgia.Giatrigiam / 100;
                // Áp dụng giảm tối đa
                if (magiamgia.Giamtoida.HasValue && discountAmount > magiamgia.Giamtoida.Value)
                {
                    discountAmount = magiamgia.Giamtoida.Value;
                }
            }
            else if (magiamgia.Loaigiamgia == "fixed")
            {
                discountAmount = magiamgia.Giatrigiam;
            }

            decimal finalAmount = Math.Max(0, request.OrderAmount - discountAmount);

            return new
            {
                code = 200,
                message = "Mã giảm giá hợp lệ",
                data = new ValidateDiscountResponse
                {
                    IsValid = true,
                    Message = "Mã giảm giá hợp lệ",
                    DiscountAmount = discountAmount,
                    FinalAmount = finalAmount,
                    DiscountId = magiamgia.Id,
                    Macode = magiamgia.Macode,
                    Tenchuongtrinh = magiamgia.Tenchuongtrinh
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Validate và áp dụng mã giảm giá (gọi từ OrderServices)
    /// Sử dụng Serializable transaction để xử lý race condition
    /// </summary>
    public async Task<ValidateDiscountResponse> ValidateAndApplyAsync(string macode, decimal orderAmount, int taikhoanId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            // Tìm và lock mã giảm giá
            var magiamgia = await _context.Magiamgia
                .FirstOrDefaultAsync(m => m.Macode == macode);

            if (magiamgia == null)
            {
                await transaction.RollbackAsync();
                return new ValidateDiscountResponse
                {
                    IsValid = false,
                    Message = "Mã giảm giá không tồn tại",
                    DiscountAmount = 0,
                    FinalAmount = orderAmount
                };
            }

            // Kiểm tra trạng thái
            if (magiamgia.Trangthai == false)
            {
                await transaction.RollbackAsync();
                return new ValidateDiscountResponse
                {
                    IsValid = false,
                    Message = "Mã giảm giá đã bị vô hiệu hóa",
                    DiscountAmount = 0,
                    FinalAmount = orderAmount
                };
            }

            // Kiểm tra thời gian hiệu lực
            var now = DateTime.Now;
            if (now < magiamgia.Ngaybatdau || now > magiamgia.Ngayketthuc)
            {
                await transaction.RollbackAsync();
                return new ValidateDiscountResponse
                {
                    IsValid = false,
                    Message = "Mã giảm giá không còn hiệu lực",
                    DiscountAmount = 0,
                    FinalAmount = orderAmount
                };
            }

            // Kiểm tra số lượng còn lại (sau khi lock)
            int soluongconlai = magiamgia.Soluong - (magiamgia.Soluongdasudung ?? 0);
            if (soluongconlai <= 0)
            {
                await transaction.RollbackAsync();
                return new ValidateDiscountResponse
                {
                    IsValid = false,
                    Message = "Mã giảm giá đã hết lượt sử dụng",
                    DiscountAmount = 0,
                    FinalAmount = orderAmount
                };
            }

            // Kiểm tra giá trị đơn hàng tối thiểu
            if (orderAmount < (magiamgia.Giatridonhangtoithieu ?? 0))
            {
                await transaction.RollbackAsync();
                return new ValidateDiscountResponse
                {
                    IsValid = false,
                    Message = $"Đơn hàng tối thiểu {magiamgia.Giatridonhangtoithieu:N0}đ để sử dụng mã này",
                    DiscountAmount = 0,
                    FinalAmount = orderAmount
                };
            }

            // Tính giá trị giảm
            decimal discountAmount = 0;
            if (magiamgia.Loaigiamgia == "percent")
            {
                discountAmount = orderAmount * magiamgia.Giatrigiam / 100;
                if (magiamgia.Giamtoida.HasValue && discountAmount > magiamgia.Giamtoida.Value)
                {
                    discountAmount = magiamgia.Giamtoida.Value;
                }
            }
            else if (magiamgia.Loaigiamgia == "fixed")
            {
                discountAmount = magiamgia.Giatrigiam;
            }

            decimal finalAmount = Math.Max(0, orderAmount - discountAmount);

            // Increment số lượng đã sử dụng
            magiamgia.Soluongdasudung = (magiamgia.Soluongdasudung ?? 0) + 1;

            // Tạo usage history record (Donhangid sẽ được update sau)
            var usageHistory = new Lichsusudungmagiamgium
            {
                Magiamgiaid = magiamgia.Id,
                Taikhoanid = taikhoanId,
                Donhangid = null, // Sẽ được update sau khi tạo đơn hàng
                Giatrigiamthucte = discountAmount,
                Ngaysudung = DateTime.Now
            };
            _context.Lichsusudungmagiamgia.Add(usageHistory);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ValidateDiscountResponse
            {
                IsValid = true,
                Message = "Áp dụng mã giảm giá thành công",
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                DiscountId = magiamgia.Id,
                Macode = magiamgia.Macode,
                Tenchuongtrinh = magiamgia.Tenchuongtrinh
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ValidateDiscountResponse
            {
                IsValid = false,
                Message = "Đã xảy ra lỗi khi áp dụng mã giảm giá: " + ex.Message,
                DiscountAmount = 0,
                FinalAmount = orderAmount
            };
        }
    }

    /// <summary>
    /// Hoàn lại mã giảm giá khi hủy đơn hàng
    /// </summary>
    public async Task<dynamic> RollbackDiscountAsync(int donhangId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Tìm usage history record theo donhangId
            var usageHistory = await _context.Lichsusudungmagiamgia
                .Include(ls => ls.Magiamgia)
                .FirstOrDefaultAsync(ls => ls.Donhangid == donhangId);

            if (usageHistory == null)
            {
                // Không có mã giảm giá được sử dụng cho đơn hàng này
                await transaction.CommitAsync();
                return new { code = 200, message = "Không có mã giảm giá cần hoàn lại" };
            }

            // Decrement số lượng đã sử dụng (minimum 0)
            if (usageHistory.Magiamgia != null)
            {
                usageHistory.Magiamgia.Soluongdasudung = Math.Max(0, (usageHistory.Magiamgia.Soluongdasudung ?? 0) - 1);
            }

            // Xóa usage history record
            _context.Lichsusudungmagiamgia.Remove(usageHistory);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new { code = 200, message = "Hoàn lại mã giảm giá thành công" };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new { code = 500, message = "Đã xảy ra lỗi khi hoàn lại mã giảm giá: " + ex.Message };
        }
    }

    /// <summary>
    /// Kích hoạt/Vô hiệu hóa mã giảm giá (Admin only)
    /// </summary>
    public async Task<dynamic> ToggleStatusAsync(ToggleDiscountStatusRequest request)
    {
        try
        {
            // Validate discount tồn tại
            var magiamgia = await _context.Magiamgia
                .FirstOrDefaultAsync(m => m.Id == request.Id);

            if (magiamgia == null)
            {
                return new { code = 404, message = "Không tìm thấy mã giảm giá" };
            }

            // Update trạng thái
            magiamgia.Trangthai = request.Trangthai;
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = request.Trangthai 
                    ? "Kích hoạt mã giảm giá thành công" 
                    : "Vô hiệu hóa mã giảm giá thành công",
                data = new
                {
                    id = magiamgia.Id,
                    macode = magiamgia.Macode,
                    trangthai = magiamgia.Trangthai
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Xem lịch sử sử dụng mã giảm giá (Admin only)
    /// </summary>
    public async Task<dynamic> GetUsageHistoryAsync(UsageHistoryRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Kiểm tra mã giảm giá tồn tại
            var discountExists = await _context.Magiamgia
                .AnyAsync(m => m.Id == request.DiscountId);

            if (!discountExists)
            {
                return new { code = 404, message = "Không tìm thấy mã giảm giá" };
            }

            // Query usage history
            IQueryable<Lichsusudungmagiamgium> query = _context.Lichsusudungmagiamgia
                .AsNoTracking()
                .Where(ls => ls.Magiamgiaid == request.DiscountId)
                .Include(ls => ls.Taikhoan)
                .Include(ls => ls.Donhang);

            // Filter theo khoảng thời gian
            if (request.FromDate.HasValue)
            {
                query = query.Where(ls => ls.Ngaysudung >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(ls => ls.Ngaysudung <= request.ToDate.Value);
            }

            // Filter theo tài khoản
            if (request.TaikhoanId.HasValue)
            {
                query = query.Where(ls => ls.Taikhoanid == request.TaikhoanId.Value);
            }

            // Sắp xếp theo ngày sử dụng giảm dần
            query = query.OrderByDescending(ls => ls.Ngaysudung);

            // Tính tổng giá trị đã giảm và tổng số lượt sử dụng (trước khi phân trang)
            var tongGiatriDagiam = await query.SumAsync(ls => ls.Giatrigiamthucte);
            var tongLuotSudung = await query.CountAsync();

            // Phân trang
            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ls => new
                {
                    ls.Id,
                    ls.Magiamgiaid,
                    ls.Taikhoanid,
                    TenTaikhoan = ls.Taikhoan != null ? ls.Taikhoan.Hoten : "N/A",
                    ls.Donhangid,
                    TrangthaiDonhang = ls.Donhang != null ? ls.Donhang.Trangthai : "N/A",
                    ls.Giatrigiamthucte,
                    ls.Ngaysudung
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)tongLuotSudung / request.PageSize);

            return new
            {
                code = 200,
                message = "Lấy lịch sử sử dụng mã giảm giá thành công",
                data = new
                {
                    items,
                    pagination = new
                    {
                        pageIndex = request.PageIndex,
                        pageSize = request.PageSize,
                        totalItems = tongLuotSudung,
                        totalPages
                    },
                    summary = new
                    {
                        tongGiatriDagiam,
                        tongLuotSudung
                    }
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }
}
