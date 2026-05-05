using System;

namespace Shopbandodientu.Models.DTOs;

public class UsageHistoryRequest
{
    public int DiscountId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? TaikhoanId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Validate request data
    /// </summary>
    public string? Validate()
    {
        if (DiscountId <= 0)
            return "DiscountId không hợp lệ";

        if (PageIndex <= 0)
            return "PageIndex phải lớn hơn 0";

        if (PageSize <= 0 || PageSize > 100)
            return "PageSize phải từ 1 đến 100";

        if (FromDate.HasValue && ToDate.HasValue && ToDate.Value < FromDate.Value)
            return "ToDate phải lớn hơn hoặc bằng FromDate";

        return null; // Hợp lệ
    }
}
