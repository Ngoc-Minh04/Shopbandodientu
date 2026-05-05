using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Services;

namespace Shopbandodientu.Controllers;

[ApiController]
[Route("api/magiamgia")]
[Authorize(Roles = "Admin")]
public class DiscountController : ControllerBase
{
    private readonly IDiscountServices _discountServices;

    public DiscountController(IDiscountServices discountServices)
    {
        _discountServices = discountServices;
    }

    /// <summary>
    /// Tạo mã giảm giá mới (Admin only)
    /// </summary>
    [HttpPost("tao")]
    public async Task<dynamic> Create([FromBody] CreateDiscountRequest request)
    {
        return await _discountServices.CreateAsync(request);
    }

    /// <summary>
    /// Cập nhật mã giảm giá (Admin only)
    /// </summary>
    [HttpPost("capnhat")]
    public async Task<dynamic> Update([FromBody] UpdateDiscountRequest request)
    {
        return await _discountServices.UpdateAsync(request);
    }

    /// <summary>
    /// Xóa mã giảm giá - Soft delete (Admin only)
    /// </summary>
    [HttpPost("xoa")]
    public async Task<dynamic> Delete([FromBody] int id)
    {
        return await _discountServices.DeleteAsync(id);
    }

    /// <summary>
    /// Lấy danh sách mã giảm giá với filter và phân trang (Admin only)
    /// </summary>
    [HttpPost("danhsach")]
    public async Task<dynamic> GetList([FromBody] DiscountListRequest request)
    {
        return await _discountServices.GetListAsync(request);
    }

    /// <summary>
    /// Lấy chi tiết mã giảm giá (Admin only)
    /// </summary>
    [HttpGet("chitiet/{id}")]
    public async Task<dynamic> GetDetail(int id)
    {
        return await _discountServices.GetDetailAsync(id);
    }

    /// <summary>
    /// Kích hoạt/Vô hiệu hóa mã giảm giá (Admin only)
    /// </summary>
    [HttpPost("doitrangthai")]
    public async Task<dynamic> ToggleStatus([FromBody] ToggleDiscountStatusRequest request)
    {
        return await _discountServices.ToggleStatusAsync(request);
    }

    /// <summary>
    /// Xem lịch sử sử dụng mã giảm giá (Admin only)
    /// </summary>
    [HttpGet("lichsusudung/{id}")]
    public async Task<dynamic> GetUsageHistory(int id, [FromQuery] UsageHistoryRequest request)
    {
        request.DiscountId = id;
        return await _discountServices.GetUsageHistoryAsync(request);
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của mã giảm giá (Customer)
    /// </summary>
    [HttpPost("kiemtra")]
    [Authorize(Roles = "Customer")]
    public async Task<dynamic> ValidateDiscount([FromBody] ValidateDiscountRequest request)
    {
        return await _discountServices.ValidateDiscountAsync(request);
    }
}
