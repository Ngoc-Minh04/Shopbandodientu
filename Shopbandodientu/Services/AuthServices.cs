using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shopbandodientu.Models.DTOs;
using Shopbandodientu.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Shopbandodientu.Services;

// Interface và class nằm cùng 1 file
public interface IAuthServices
{
    Task<dynamic> RegisterAsync(RegisterRequest request);
    Task<dynamic> LoginAsync(LoginRequest request);
}

public class AuthServices : IAuthServices
{
    private readonly MinhContext _context;
    private readonly IConfiguration _configuration;

    public AuthServices(MinhContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    public async Task<dynamic> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Kiểm tra email đã tồn tại
            bool emailExists = await _context.Taikhoans
                .AsNoTracking()
                .AnyAsync(t => t.Email == request.Email);

            if (emailExists)
            {
                return new { code = 400, message = "Email đã tồn tại trong hệ thống" };
            }

            // Hash mật khẩu
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Matkhau);

            // Tạo entity Taikhoan
            var taikhoan = new Taikhoan
            {
                Email = request.Email,
                Matkhau = hashedPassword,
                Hoten = request.Hoten,
                Sodienthoai = request.Sodienthoai,
                Diachi = request.Diachi,
                Loaitaikhoanid = 1, // Customer
                Trangthai = true,
                Ngaytao = DateTime.Now
            };

            // Lưu vào DB
            _context.Taikhoans.Add(taikhoan);
            await _context.SaveChangesAsync();

            // Trả về thông tin tài khoản (không trả mật khẩu)
            return new
            {
                code = 200,
                message = "Đăng ký thành công",
                data = new
                {
                    id = taikhoan.Id,
                    email = taikhoan.Email,
                    hoten = taikhoan.Hoten,
                    sodienthoai = taikhoan.Sodienthoai,
                    diachi = taikhoan.Diachi,
                    ngaytao = taikhoan.Ngaytao
                }
            };
        }
        catch (Exception ex)
        {
            // Log exception nếu cần
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    public async Task<dynamic> LoginAsync(LoginRequest request)
    {
        try
        {
            // Validate request
            var validationError = request.Validate();
            if (validationError != null)
            {
                return new { code = 400, message = validationError };
            }

            // Kiểm tra email tồn tại và lấy thông tin tài khoản
            var taikhoan = await _context.Taikhoans
                .AsNoTracking()
                .Include(t => t.Loaitaikhoan)
                .FirstOrDefaultAsync(t => t.Email == request.Email);

            if (taikhoan == null)
            {
                return new { code = 401, message = "Email hoặc mật khẩu không đúng" };
            }

            // Kiểm tra tài khoản có bị khóa không
            if (taikhoan.Trangthai == false)
            {
                return new { code = 403, message = "Tài khoản đã bị khóa" };
            }

            // Verify mật khẩu
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Matkhau, taikhoan.Matkhau);
            if (!isPasswordValid)
            {
                return new { code = 401, message = "Email hoặc mật khẩu không đúng" };
            }

            // Lấy role name
            string roleName = taikhoan.Loaitaikhoan?.Tenloai ?? "Customer";

            // Tạo JWT token
            var token = GenerateJwtToken(taikhoan.Id, taikhoan.Email, roleName);
            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440");

            // Trả về thông tin đăng nhập
            return new
            {
                code = 200,
                message = "Đăng nhập thành công",
                data = new LoginResponse
                {
                    AccessToken = token,
                    ExpiresIn = expireMinutes * 60, // Convert to seconds
                    User = new UserInfo
                    {
                        Id = taikhoan.Id,
                        Email = taikhoan.Email,
                        Hoten = taikhoan.Hoten,
                        Role = roleName
                    }
                }
            };
        }
        catch (Exception ex)
        {
            // Log exception nếu cần
            return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
        }
    }

    /// <summary>
    /// Tạo JWT token
    /// </summary>
    private string GenerateJwtToken(int userId, string email, string role)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
