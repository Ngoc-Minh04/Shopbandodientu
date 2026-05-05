namespace Shopbandodientu.Models.DTOs;

public class CreateCategoryRequest
{
    public string Tendanhmuc { get; set; } = null!;

    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Tendanhmuc))
            return "Tên danh mục không được để trống";
        
        if (Tendanhmuc.Length > 100)
            return "Tên danh mục không được vượt quá 100 ký tự";
        
        return null; // Hợp lệ
    }
}
