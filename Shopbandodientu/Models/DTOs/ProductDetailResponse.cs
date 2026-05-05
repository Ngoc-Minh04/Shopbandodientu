namespace Shopbandodientu.Models.DTOs;

public class ProductDetailResponse
{
    public int Id { get; set; }
    public string Tensanpham { get; set; } = null!;
    public string? Mota { get; set; }
    public decimal Gia { get; set; }
    public int Soluong { get; set; }
    public string? Thuonghieu { get; set; }
    public int? Khuyenmai { get; set; }
    public bool? Trangthai { get; set; }
    public DateTime? Ngaythem { get; set; }
    
    // Danh mục
    public int? DanhmucId { get; set; }
    public string? TenDanhmuc { get; set; }
    
    // Hình ảnh
    public List<string> HinhAnhs { get; set; } = new List<string>();
    
    // Thông số kỹ thuật
    public string? Cpu { get; set; }
    public string? Vga { get; set; }
    public string? Ram { get; set; }
    public string? Rom { get; set; }
    
    // Đánh giá
    public double? AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
