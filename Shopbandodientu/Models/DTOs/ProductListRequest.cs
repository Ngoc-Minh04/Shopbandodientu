namespace Shopbandodientu.Models.DTOs;

public class ProductListRequest
{
    public string? Keyword { get; set; }
    public int? DanhmucId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } // price_asc, price_desc, newest

    public string? Validate()
    {
        if (PageIndex < 1)
            return "PageIndex phải >= 1";
        
        if (PageSize < 1 || PageSize > 100)
            return "PageSize phải từ 1-100";
        
        if (MinPrice.HasValue && MinPrice.Value < 0)
            return "MinPrice phải >= 0";
        
        if (MaxPrice.HasValue && MaxPrice.Value < 0)
            return "MaxPrice phải >= 0";
        
        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice.Value > MaxPrice.Value)
            return "MinPrice không được lớn hơn MaxPrice";
        
        return null; // Hợp lệ
    }
}
