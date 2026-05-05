namespace Shopbandodientu.Models.DTOs;

public class DiscountListRequest
{
    public bool? Trangthai { get; set; } // null = all, true = active, false = deleted
    public string? TimeFilter { get; set; } // "active", "expired", "upcoming", null = all
    public string? Keyword { get; set; } // Tìm kiếm theo mã code hoặc tên chương trình
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Validate request data
    /// </summary>
    public string? Validate()
    {
        if (PageIndex <= 0)
            return "PageIndex phải lớn hơn 0";

        if (PageSize <= 0 || PageSize > 100)
            return "PageSize phải từ 1 đến 100";

        if (!string.IsNullOrEmpty(TimeFilter))
        {
            var validFilters = new[] { "active", "expired", "upcoming" };
            if (!validFilters.Contains(TimeFilter.ToLower()))
                return "TimeFilter chỉ nhận giá trị: active, expired, upcoming";
        }

        return null; // Hợp lệ
    }
}
