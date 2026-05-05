using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Services;
using System.Security.Claims;

namespace Shopbandodientu.Controllers;

[ApiController]
[Route("api/giohang")]
[Authorize(Roles = "Customer")]
public class CartController : ControllerBase
{
    private readonly ICartServices _cartServices;

    public CartController(ICartServices cartServices)
    {
        _cartServices = cartServices;
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
    /// Thêm sản phẩm vào giỏ hàng
    /// </summary>
    /// <param name="request">Thông tin sản phẩm cần thêm</param>
    /// <returns>Kết quả thêm sản phẩm</returns>
    [HttpPost("them")]
    public async Task<dynamic> AddToCart([FromBody] AddToCartRequest request)
    {
        int userId = GetUserId();
        return await _cartServices.AddToCartAsync(userId, request);
    }

    /// <summary>
    /// Xem giỏ hàng
    /// </summary>
    /// <returns>Danh sách sản phẩm trong giỏ hàng</returns>
    [HttpGet("xem")]
    public async Task<dynamic> GetCart()
    {
        int userId = GetUserId();
        return await _cartServices.GetCartAsync(userId);
    }

    /// <summary>
    /// Cập nhật số lượng sản phẩm trong giỏ
    /// </summary>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPost("capnhat")]
    public async Task<dynamic> UpdateCartItem([FromBody] UpdateCartItemRequest request)
    {
        int userId = GetUserId();
        return await _cartServices.UpdateCartItemAsync(userId, request);
    }

    /// <summary>
    /// Xóa sản phẩm khỏi giỏ hàng
    /// </summary>
    /// <param name="sanphamId">ID sản phẩm cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    [HttpPost("xoa")]
    public async Task<dynamic> RemoveFromCart([FromBody] int sanphamId)
    {
        int userId = GetUserId();
        return await _cartServices.RemoveFromCartAsync(userId, sanphamId);
    }

    /// <summary>
    /// Xóa toàn bộ giỏ hàng
    /// </summary>
    /// <returns>Kết quả xóa</returns>
    [HttpPost("xoatatca")]
    public async Task<dynamic> ClearCart()
    {
        int userId = GetUserId();
        return await _cartServices.ClearCartAsync(userId);
    }
}
