namespace Shopbandodientu.Models.DTOs;

public class CreateProductRequest
{
    public string Tensanpham { get; set; } = null!;
    public string? Mota { get; set; }
    public decimal Gia { get; set; }
    public int Soluong { get; set; }
    public int? Danhmucid { get; set; }
    public string? Thuonghieu { get; set; }
    public int? Khuyenmai { get; set; }

    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Tensanpham))
            return "Tên sản phẩm không được để trống";
        
        if (Tensanpham.Length > 255)
            return "Tên sản phẩm không được vượt quá 255 ký tự";
        
        if (Gia <= 0)
            return "Giá phải lớn hơn 0";
        
        if (Soluong < 0)
            return "Số lượng phải >= 0";
        
        if (Khuyenmai.HasValue && (Khuyenmai.Value < 0 || Khuyenmai.Value > 100))
            return "Khuyến mãi phải từ 0-100%";
        
        if (Thuonghieu != null && Thuonghieu.Length > 30)
            return "Thương hiệu không được vượt quá 30 ký tự";
        
        return null; // Hợp lệ
    }
}
