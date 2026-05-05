using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Services;

namespace Shopbandodientu.Controllers;

[ApiController]
[Route("api/sanpham")]
public class ProductController : ControllerBase
{
    private readonly IProductServices _productServices;

    public ProductController(IProductServices productServices)
    {
        _productServices = productServices;
    }

    /// <summary>
    /// Lấy danh sách sản phẩm (Public)
    /// </summary>
    /// <param name="request">Tham số tìm kiếm, lọc, phân trang</param>
    /// <returns>Danh sách sản phẩm</returns>
    [HttpPost("danhsach")]
    public async Task<dynamic> GetList([FromBody] ProductListRequest request)
    {
        return await _productServices.GetListAsync(request);
    }

    /// <summary>
    /// Lấy chi tiết sản phẩm (Public)
    /// </summary>
    /// <param name="id">ID sản phẩm</param>
    /// <returns>Chi tiết sản phẩm</returns>
    [HttpGet("chitiet/{id}")]
    public async Task<dynamic> GetDetail(int id)
    {
        return await _productServices.GetDetailAsync(id);
    }

    /// <summary>
    /// Tạo mới sản phẩm (Admin only)
    /// </summary>
    /// <param name="request">Thông tin sản phẩm</param>
    /// <returns>Thông tin sản phẩm đã tạo</returns>
    [HttpPost("tao")]
    [Authorize(Roles = "Admin")]
    public async Task<dynamic> Create([FromBody] CreateProductRequest request)
    {
        return await _productServices.CreateAsync(request);
    }

    /// <summary>
    /// Cập nhật sản phẩm (Admin only)
    /// </summary>
    /// <param name="request">Thông tin sản phẩm cần cập nhật</param>
    /// <returns>Thông tin sản phẩm đã cập nhật</returns>
    [HttpPost("capnhat")]
    [Authorize(Roles = "Admin")]
    public async Task<dynamic> Update([FromBody] UpdateProductRequest request)
    {
        return await _productServices.UpdateAsync(request);
    }

    /// <summary>
    /// Xóa sản phẩm - Soft delete (Admin only)
    /// </summary>
    /// <param name="id">ID sản phẩm</param>
    /// <returns>Kết quả xóa</returns>
    [HttpPost("xoa")]
    [Authorize(Roles = "Admin")]
    public async Task<dynamic> Delete([FromBody] int id)
    {
        return await _productServices.DeleteAsync(id);
    }
}
