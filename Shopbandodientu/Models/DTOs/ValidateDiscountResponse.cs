namespace Shopbandodientu.Models.DTOs;

public class ValidateDiscountResponse
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public int? DiscountId { get; set; }
    public string? Macode { get; set; }
    public string? Tenchuongtrinh { get; set; }
}
