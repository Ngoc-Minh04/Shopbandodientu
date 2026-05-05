namespace Shopbandodientu.Models.DTOs;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public UserInfo User { get; set; } = null!;
}

public class UserInfo
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string? Hoten { get; set; }
    public string Role { get; set; } = null!;
}
