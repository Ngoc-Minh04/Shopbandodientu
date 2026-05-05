namespace Shopbandodientu.Models.DTOs;

public class ValidateDiscountRequest
{
    public string Macode { get; set; } = null!;
    public decimal OrderAmount { get; set; }

    /// <summary>
    /// Validate request data
    /// </summary>
    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Macode))
            return "Mã code không được để trống";

        if (OrderAmount <= 0)
            return "Giá trị đơn hàng phải lớn hơn 0";

        return null; // Hợp lệ
    }
}
