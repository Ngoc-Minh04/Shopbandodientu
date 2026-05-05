namespace Shopbandodientu.Models.DTOs;

public class AddToCartRequest
{
    public int Sanphamid { get; set; }
    public int Soluong { get; set; }

    public string? Validate()
    {
        if (Sanphamid <= 0)
            return "ID sản phẩm không hợp lệ";
        
        if (Soluong <= 0)
            return "Số lượng phải lớn hơn 0";
        
        return null; // Hợp lệ
    }
}
