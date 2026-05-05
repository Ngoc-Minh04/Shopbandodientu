using Microsoft.AspNetCore.Mvc;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Services;

namespace Shopbandodientu.Controllers;

[ApiController]
[Route("api/xacthuc")]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authServices;

    public AuthController(IAuthServices authServices)
    {
        _authServices = authServices;
    }

    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    /// <param name="request">Thông tin đăng ký</param>
    /// <returns>Thông tin tài khoản đã tạo</returns>
    [HttpPost("dangky")]
    public async Task<dynamic> Register([FromBody] RegisterRequest request)
    {
        return await _authServices.RegisterAsync(request);
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    /// <param name="request">Thông tin đăng nhập</param>
    /// <returns>JWT token và thông tin user</returns>
    [HttpPost("dangnhap")]
    public async Task<dynamic> Login([FromBody] LoginRequest request)
    {
        return await _authServices.LoginAsync(request);
    }
}
