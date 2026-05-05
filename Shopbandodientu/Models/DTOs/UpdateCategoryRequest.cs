namespace Shopbandodientu.Models.DTOs;

public class UpdateCategoryRequest
{
    public int Id { get; set; }
    public string Tendanhmuc { get; set; } = null!;

    public string? Validate()
    {
        if (Id <= 0)
            return "Id không hợp lệ";
        
        if (string.IsNullOrWhiteSpace(Tendanhmuc))
            return "Tên danh mục không được để trống";
        
        if (Tendanhmuc.Length > 100)
            return "Tên danh mục không được vượt quá 100 ký tự";
        
        return null; // Hợp lệ
    }
}
