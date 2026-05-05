using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Services;

namespace Shopbandodientu.Controllers;

[ApiController]
[Route("api/danhmuc")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryServices _categoryServices;

    public CategoryController(ICategoryServices categoryServices)
    {
        _categoryServices = categoryServices;
    }

    /// <summary>
    /// Lấy danh sách tất cả danh mục (Public)
    /// </summary>
    /// <returns>Danh sách danh mục kèm số lượng sản phẩm</returns>
    [HttpGet("danhsach")]
    public async Task<dynamic> GetList()
    {
        return await _categoryServices.GetListAsync();
    }

    /// <summary>
    /// Lấy chi tiết danh mục (Public)
    /// </summary>
    /// <param name="id">ID danh mục</param>
    /// <returns>Chi tiết danh mục kèm danh sách sản phẩm</returns>
    [HttpGet("chitiet/{id}")]
    public async Task<dynamic> GetDetail(int id)
    {
        return await _categoryServices.GetDetailAsync(id);
    }

    /// <summary>
    /// Tạo mới danh mục (Admin only)
    /// </summary>
    /// <param name="request">Thông tin danh mục</param>
    /// <returns>Thông tin danh mục đã tạo</returns>
    [HttpPost("tao")]
    [Authorize(Roles = "Admin")]
    public async Task<dynamic> Create([FromBody] CreateCategoryRequest request)
    {
        return await _categoryServices.CreateAsync(request);
    }

    /// <summary>
    /// Cập nhật danh mục (Admin only)
    /// </summary>
    /// <param name="request">Thông tin danh mục cần cập nhật</param>
    /// <returns>Thông tin danh mục đã cập nhật</returns>
    [HttpPost("capnhat")]
    [Authorize(Roles = "Admin")]
    public async Task<dynamic> Update([FromBody] UpdateCategoryRequest request)
    {
        return await _categoryServices.UpdateAsync(request);
    }

    /// <summary>
    /// Xóa danh mục (Admin only)
    /// </summary>
    /// <param name="id">ID danh mục</param>
    /// <returns>Kết quả xóa</returns>
    [HttpPost("xoa")]
    [Authorize(Roles = "Admin")]
    public async Task<dynamic> Delete([FromBody] int id)
    {
        return await _categoryServices.DeleteAsync(id);
    }
}
