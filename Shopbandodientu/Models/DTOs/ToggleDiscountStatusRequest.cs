namespace Shopbandodientu.Models.DTOs;

public class ToggleDiscountStatusRequest
{
    public int Id { get; set; }
    public bool Trangthai { get; set; }

    /// <summary>
    /// Validate request data
    /// </summary>
    public string? Validate()
    {
        if (Id <= 0)
            return "ID không hợp lệ";

        return null; // Hợp lệ
    }
}
