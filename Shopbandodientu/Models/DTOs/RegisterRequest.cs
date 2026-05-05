using System.Text.RegularExpressions;

namespace Shopbandodientu.Models.DTOs;

public class RegisterRequest
{
    public string Email { get; set; } = null!;
    public string Matkhau { get; set; } = null!;
    public string? Hoten { get; set; }
    public string? Sodienthoai { get; set; }
    public string? Diachi { get; set; }

    /// <summary>
    /// Validate dữ liệu đầu vào
    /// </summary>
    /// <returns>Null nếu hợp lệ, string lỗi nếu không hợp lệ</returns>
    public string? Validate()
    {
        // Kiểm tra Email
        if (string.IsNullOrWhiteSpace(Email))
            return "Email không được để trống";

        // Kiểm tra format email
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(Email))
            return "Email không đúng định dạng";

        // Kiểm tra Mật khẩu
        if (string.IsNullOrWhiteSpace(Matkhau))
            return "Mật khẩu không được để trống";

        if (Matkhau.Length < 8)
            return "Mật khẩu phải có ít nhất 8 ký tự";

        // Kiểm tra Số điện thoại (nếu có)
        if (!string.IsNullOrWhiteSpace(Sodienthoai))
        {
            var phoneRegex = new Regex(@"^\d{10,11}$");
            if (!phoneRegex.IsMatch(Sodienthoai))
                return "Số điện thoại phải có 10-11 chữ số";
        }

        return null; // Hợp lệ
    }
}
