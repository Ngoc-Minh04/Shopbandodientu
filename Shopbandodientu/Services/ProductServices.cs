using Microsoft.EntityFrameworkCore;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Models.Entities;

namespace Shopbandodientu.Services;

// Interface và class nằm cùng 1 file
public interface IProductServices
{
    Task<dynamic> GetListAsync(ProductListRequest request);
    Task<dynamic> GetDetailAsync(int id);
    Task<dynamic> CreateAsync(CreateProductRequest request);
    Task<dynamic> UpdateAsync(UpdateProductRequest request);
    Task<dynamic> DeleteAsync(int id);
}

public class ProductServices : IProductServices
{
    private readonly MinhContext _context;

    public ProductServices(MinhContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách sản phẩm với tìm kiếm, lọc, phân trang
    /// </summary>
    public async Task<dynamic> GetListAsync(ProductListRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Query cơ bản - chỉ lấy sản phẩm còn hoạt động
            var query = _context.Sanphams
                .AsNoTracking()
                .Where(sp => sp.Trangthai == true);

            // Filter theo keyword
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.ToLower();
                query = query.Where(sp => sp.Tensanpham.ToLower().Contains(keyword) 
                                       || (sp.Thuonghieu != null && sp.Thuonghieu.ToLower().Contains(keyword)));
            }

            // Filter theo danh mục
            if (request.DanhmucId.HasValue)
            {
                query = query.Where(sp => sp.Danhmucid == request.DanhmucId.Value);
            }

            // Filter theo khoảng giá
            if (request.MinPrice.HasValue)
            {
                query = query.Where(sp => sp.Gia >= request.MinPrice.Value);
            }
            if (request.MaxPrice.HasValue)
            {
                query = query.Where(sp => sp.Gia <= request.MaxPrice.Value);
            }

            // Sắp xếp
            query = request.SortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(sp => sp.Gia),
                "price_desc" => query.OrderByDescending(sp => sp.Gia),
                "newest" => query.OrderByDescending(sp => sp.Ngaythem),
                _ => query.OrderByDescending(sp => sp.Id) // Mặc định
            };

            // Đếm tổng số
            var totalItems = await query.CountAsync();

            // Phân trang
            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(sp => new
                {
                    sp.Id,
                    sp.Tensanpham,
                    sp.Mota,
                    sp.Gia,
                    sp.Soluong,
                    sp.Thuonghieu,
                    sp.Khuyenmai,
                    sp.Trangthai,
                    sp.Ngaythem,
                    DanhmucId = sp.Danhmucid,
                    TenDanhmuc = sp.Danhmuc != null ? sp.Danhmuc.Tendanhmuc : null,
                    // Lấy hình ảnh đầu tiên
                    HinhAnhDauTien = sp.Sanphamhinhanhs.FirstOrDefault() != null 
                        ? sp.Sanphamhinhanhs.FirstOrDefault()!.Duongdan 
                        : null
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);

            return new
            {
                code = 200,
                message = "Lấy danh sách sản phẩm thành công",
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
    /// Lấy chi tiết sản phẩm
    /// </summary>
    public async Task<dynamic> GetDetailAsync(int id)
    {
        try
        {
            var sanpham = await _context.Sanphams
                .AsNoTracking()
                .Include(sp => sp.Danhmuc)
                .Include(sp => sp.Sanphamhinhanhs)
                .Include(sp => sp.Thongsos)
                .Include(sp => sp.Danhgia)
                .FirstOrDefaultAsync(sp => sp.Id == id);

            if (sanpham == null)
            {
                return new { code = 404, message = "Không tìm thấy sản phẩm" };
            }

            // Tính điểm đánh giá trung bình
            double? averageRating = null;
            int totalReviews = sanpham.Danhgia.Count;
            if (totalReviews > 0)
            {
                averageRating = sanpham.Danhgia.Average(dg => dg.Diem ?? 0);
            }

            // Lấy thông số kỹ thuật đầu tiên (nếu có)
            var thongso = sanpham.Thongsos.FirstOrDefault();

            var response = new ProductDetailResponse
            {
                Id = sanpham.Id,
                Tensanpham = sanpham.Tensanpham,
                Mota = sanpham.Mota,
                Gia = sanpham.Gia,
                Soluong = sanpham.Soluong,
                Thuonghieu = sanpham.Thuonghieu,
                Khuyenmai = sanpham.Khuyenmai,
                Trangthai = sanpham.Trangthai,
                Ngaythem = sanpham.Ngaythem,
                DanhmucId = sanpham.Danhmucid,
                TenDanhmuc = sanpham.Danhmuc?.Tendanhmuc,
                HinhAnhs = sanpham.Sanphamhinhanhs
                    .Where(ha => !string.IsNullOrEmpty(ha.Duongdan))
                    .Select(ha => ha.Duongdan!)
                    .ToList(),
                Cpu = thongso?.Cpu,
                Vga = thongso?.Vga,
                Ram = thongso?.Ram,
                Rom = thongso?.Rom,
                AverageRating = averageRating,
                TotalReviews = totalReviews
            };

            return new
            {
                code = 200,
                message = "Lấy chi tiết sản phẩm thành công",
                data = response
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Tạo mới sản phẩm (Admin only)
    /// </summary>
    public async Task<dynamic> CreateAsync(CreateProductRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Kiểm tra danh mục tồn tại (nếu có)
            if (request.Danhmucid.HasValue)
            {
                bool danhmucExists = await _context.Danhmucs
                    .AnyAsync(dm => dm.Id == request.Danhmucid.Value);
                
                if (!danhmucExists)
                {
                    return new { code = 400, message = "Danh mục không tồn tại" };
                }
            }

            // Tạo entity Sanpham
            var sanpham = new Sanpham
            {
                Tensanpham = request.Tensanpham,
                Mota = request.Mota,
                Gia = request.Gia,
                Soluong = request.Soluong,
                Danhmucid = request.Danhmucid,
                Thuonghieu = request.Thuonghieu,
                Khuyenmai = request.Khuyenmai,
                Ngaythem = DateTime.Now,
                // Tự động set trangthai = false nếu số lượng = 0
                Trangthai = request.Soluong > 0
            };

            // Lưu vào DB
            _context.Sanphams.Add(sanpham);
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Tạo sản phẩm thành công",
                data = new
                {
                    id = sanpham.Id,
                    tensanpham = sanpham.Tensanpham,
                    gia = sanpham.Gia,
                    soluong = sanpham.Soluong,
                    trangthai = sanpham.Trangthai
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Cập nhật sản phẩm (Admin only)
    /// </summary>
    public async Task<dynamic> UpdateAsync(UpdateProductRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Kiểm tra sản phẩm tồn tại
            var sanpham = await _context.Sanphams
                .FirstOrDefaultAsync(sp => sp.Id == request.Id);

            if (sanpham == null)
            {
                return new { code = 404, message = "Không tìm thấy sản phẩm" };
            }

            // Kiểm tra danh mục tồn tại (nếu có)
            if (request.Danhmucid.HasValue)
            {
                bool danhmucExists = await _context.Danhmucs
                    .AnyAsync(dm => dm.Id == request.Danhmucid.Value);
                
                if (!danhmucExists)
                {
                    return new { code = 400, message = "Danh mục không tồn tại" };
                }
            }

            // Cập nhật thông tin
            sanpham.Tensanpham = request.Tensanpham;
            sanpham.Mota = request.Mota;
            sanpham.Gia = request.Gia;
            sanpham.Soluong = request.Soluong;
            sanpham.Danhmucid = request.Danhmucid;
            sanpham.Thuonghieu = request.Thuonghieu;
            sanpham.Khuyenmai = request.Khuyenmai;
            // Tự động set trangthai = false nếu số lượng = 0
            sanpham.Trangthai = request.Soluong > 0;

            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Cập nhật sản phẩm thành công",
                data = new
                {
                    id = sanpham.Id,
                    tensanpham = sanpham.Tensanpham,
                    gia = sanpham.Gia,
                    soluong = sanpham.Soluong,
                    trangthai = sanpham.Trangthai
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Xóa sản phẩm - Soft delete (Admin only)
    /// </summary>
    public async Task<dynamic> DeleteAsync(int id)
    {
        try
        {
            // Kiểm tra sản phẩm tồn tại
            var sanpham = await _context.Sanphams
                .FirstOrDefaultAsync(sp => sp.Id == id);

            if (sanpham == null)
            {
                return new { code = 404, message = "Không tìm thấy sản phẩm" };
            }

            // Soft delete: Set trangthai = false
            sanpham.Trangthai = false;
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Xóa sản phẩm thành công"
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }
}
