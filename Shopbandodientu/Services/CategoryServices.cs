using Microsoft.EntityFrameworkCore;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Models.Entities;

namespace Shopbandodientu.Services;

// Interface và class nằm cùng 1 file
public interface ICategoryServices
{
    Task<dynamic> GetListAsync();
    Task<dynamic> GetDetailAsync(int id);
    Task<dynamic> CreateAsync(CreateCategoryRequest request);
    Task<dynamic> UpdateAsync(UpdateCategoryRequest request);
    Task<dynamic> DeleteAsync(int id);
}

public class CategoryServices : ICategoryServices
{
    private readonly MinhContext _context;

    public CategoryServices(MinhContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách tất cả danh mục (kèm số lượng sản phẩm)
    /// </summary>
    public async Task<dynamic> GetListAsync()
    {
        try
        {
            var categories = await _context.Danhmucs
                .AsNoTracking()
                .Select(dm => new
                {
                    dm.Id,
                    dm.Tendanhmuc,
                    // Đếm số lượng sản phẩm còn hoạt động trong danh mục
                    SoLuongSanpham = dm.Sanphams.Count(sp => sp.Trangthai == true)
                })
                .OrderBy(dm => dm.Id)
                .ToListAsync();

            return new
            {
                code = 200,
                message = "Lấy danh sách danh mục thành công",
                data = categories
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Lấy chi tiết danh mục (kèm danh sách sản phẩm)
    /// </summary>
    public async Task<dynamic> GetDetailAsync(int id)
    {
        try
        {
            var danhmuc = await _context.Danhmucs
                .AsNoTracking()
                .Include(dm => dm.Sanphams.Where(sp => sp.Trangthai == true))
                .FirstOrDefaultAsync(dm => dm.Id == id);

            if (danhmuc == null)
            {
                return new { code = 404, message = "Không tìm thấy danh mục" };
            }

            var response = new
            {
                id = danhmuc.Id,
                tendanhmuc = danhmuc.Tendanhmuc,
                soLuongSanpham = danhmuc.Sanphams.Count,
                sanphams = danhmuc.Sanphams.Select(sp => new
                {
                    sp.Id,
                    sp.Tensanpham,
                    sp.Gia,
                    sp.Soluong,
                    sp.Thuonghieu,
                    sp.Khuyenmai,
                    sp.Trangthai
                }).ToList()
            };

            return new
            {
                code = 200,
                message = "Lấy chi tiết danh mục thành công",
                data = response
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Tạo mới danh mục (Admin only)
    /// </summary>
    public async Task<dynamic> CreateAsync(CreateCategoryRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Kiểm tra tên danh mục đã tồn tại chưa
            bool nameExists = await _context.Danhmucs
                .AsNoTracking()
                .AnyAsync(dm => dm.Tendanhmuc.ToLower() == request.Tendanhmuc.ToLower());

            if (nameExists)
            {
                return new { code = 400, message = "Tên danh mục đã tồn tại" };
            }

            // Tạo entity Danhmuc
            var danhmuc = new Danhmuc
            {
                Tendanhmuc = request.Tendanhmuc
            };

            // Lưu vào DB
            _context.Danhmucs.Add(danhmuc);
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Tạo danh mục thành công",
                data = new
                {
                    id = danhmuc.Id,
                    tendanhmuc = danhmuc.Tendanhmuc
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Cập nhật danh mục (Admin only)
    /// </summary>
    public async Task<dynamic> UpdateAsync(UpdateCategoryRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Kiểm tra danh mục tồn tại
            var danhmuc = await _context.Danhmucs
                .FirstOrDefaultAsync(dm => dm.Id == request.Id);

            if (danhmuc == null)
            {
                return new { code = 404, message = "Không tìm thấy danh mục" };
            }

            // Kiểm tra tên danh mục mới đã tồn tại chưa (trừ chính nó)
            bool nameExists = await _context.Danhmucs
                .AsNoTracking()
                .AnyAsync(dm => dm.Tendanhmuc.ToLower() == request.Tendanhmuc.ToLower() 
                             && dm.Id != request.Id);

            if (nameExists)
            {
                return new { code = 400, message = "Tên danh mục đã tồn tại" };
            }

            // Cập nhật thông tin
            danhmuc.Tendanhmuc = request.Tendanhmuc;
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Cập nhật danh mục thành công",
                data = new
                {
                    id = danhmuc.Id,
                    tendanhmuc = danhmuc.Tendanhmuc
                }
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Xóa danh mục (Admin only)
    /// </summary>
    public async Task<dynamic> DeleteAsync(int id)
    {
        try
        {
            // Kiểm tra danh mục tồn tại
            var danhmuc = await _context.Danhmucs
                .Include(dm => dm.Sanphams)
                .FirstOrDefaultAsync(dm => dm.Id == id);

            if (danhmuc == null)
            {
                return new { code = 404, message = "Không tìm thấy danh mục" };
            }

            // Kiểm tra còn sản phẩm trong danh mục không
            if (danhmuc.Sanphams.Any())
            {
                return new { code = 400, message = "Không thể xóa danh mục vì còn sản phẩm thuộc danh mục này" };
            }

            // Xóa danh mục
            _context.Danhmucs.Remove(danhmuc);
            await _context.SaveChangesAsync();

            return new
            {
                code = 200,
                message = "Xóa danh mục thành công"
            };
        }
        catch (Exception ex)
        {
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }
}
