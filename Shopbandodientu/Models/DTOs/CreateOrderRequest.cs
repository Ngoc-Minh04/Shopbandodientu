using System.Text.RegularExpressions;

namespace Shopbandodientu.Models.DTOs;

public class CreateOrderRequest
{
    public string Tennguoinhan { get; set; } = null!;
    public string Diachigiaohang { get; set; } = null!;
    public string Sdtnguoinhan { get; set; } = null!;
    public string? DiscountCode { get; set; } // Mã giảm giá (optional)

    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Tennguoinhan))
            return "Tên người nhận không được để trống";
        
        if (Tennguoinhan.Length > 100)
            return "Tên người nhận không được vượt quá 100 ký tự";
        
        if (string.IsNullOrWhiteSpace(Diachigiaohang))
            return "Địa chỉ giao hàng không được để trống";
        
        if (Diachigiaohang.Length > 255)
            return "Địa chỉ giao hàng không được vượt quá 255 ký tự";
        
        if (string.IsNullOrWhiteSpace(Sdtnguoinhan))
            return "Số điện thoại người nhận không được để trống";
        
        // Validate số điện thoại (10-11 số)
        if (!Regex.IsMatch(Sdtnguoinhan, @"^\d{10,11}$"))
            return "Số điện thoại phải có 10-11 chữ số";
        
        return null; // Hợp lệ
    }
}
