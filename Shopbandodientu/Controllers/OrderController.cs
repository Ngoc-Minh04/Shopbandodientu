using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Services;
using System.Security.Claims;

namespace Shopbandodientu.Controllers;

[ApiController]
[Route("api/donhang")]
[Authorize(Roles = "Customer")]
public class OrderController : ControllerBase
{
    private readonly IOrderServices _orderServices;

    public OrderController(IOrderServices orderServices)
    {
        _orderServices = orderServices;
    }

    /// <summary>
    /// Lấy userId từ JWT token
    /// </summary>
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("sub")?.Value;
        return int.Parse(userIdClaim!);
    }

    /// <summary>
    /// Đặt hàng từ giỏ hàng
    /// </summary>
    /// <param name="request">Thông tin giao hàng</param>
    /// <returns>Thông tin đơn hàng đã tạo</returns>
    [HttpPost("taodathang")]
    public async Task<dynamic> CreateOrder([FromBody] CreateOrderRequest request)
    {
        int userId = GetUserId();
        return await _orderServices.CreateOrderAsync(userId, request);
    }

    /// <summary>
    /// Xem lịch sử đơn hàng
    /// </summary>
    /// <returns>Danh sách đơn hàng</returns>
    [HttpGet("lichsu")]
    public async Task<dynamic> GetOrderHistory()
    {
        int userId = GetUserId();
        return await _orderServices.GetOrderHistoryAsync(userId);
    }

    /// <summary>
    /// Xem chi tiết đơn hàng
    /// </summary>
    /// <param name="id">ID đơn hàng</param>
    /// <returns>Chi tiết đơn hàng</returns>
    [HttpGet("chitiet/{id}")]
    public async Task<dynamic> GetOrderDetail(int id)
    {
        int userId = GetUserId();
        return await _orderServices.GetOrderDetailAsync(userId, id);
    }

    /// <summary>
    /// Hủy đơn hàng (trong 24h)
    /// </summary>
    /// <param name="orderId">ID đơn hàng cần hủy</param>
    /// <returns>Kết quả hủy đơn</returns>
    [HttpPost("huy")]
    public async Task<dynamic> CancelOrder([FromBody] int orderId)
    {
        int userId = GetUserId();
        return await _orderServices.CancelOrderAsync(userId, orderId);
    }
}
