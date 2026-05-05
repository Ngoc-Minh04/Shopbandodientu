namespace Shopbandodientu.Models.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Matkhau { get; set; } = null!;

    /// <summary>
    /// Validate dữ liệu đầu vào
    /// </summary>
    /// <returns>Null nếu hợp lệ, string lỗi nếu không hợp lệ</returns>
    public string? Validate()
    {
        // Kiểm tra Email
        if (string.IsNullOrWhiteSpace(Email))
            return "Email không được để trống";

        // Kiểm tra Mật khẩu
        if (string.IsNullOrWhiteSpace(Matkhau))
            return "Mật khẩu không được để trống";

        return null; // Hợp lệ
    }
}
