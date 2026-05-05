using System;

namespace Shopbandodientu.Models.DTOs;

public class UpdateDiscountRequest
{
    public int Id { get; set; }
    public string Tenchuongtrinh { get; set; } = null!;
    public string? Mota { get; set; }
    public decimal Giatrigiam { get; set; }
    public decimal? Giamtoida { get; set; }
    public decimal Giatridonhangtoithieu { get; set; }
    public int Soluong { get; set; }
    public DateTime Ngaybatdau { get; set; }
    public DateTime Ngayketthuc { get; set; }

    /// <summary>
    /// Validate request data
    /// </summary>
    public string? Validate()
    {
        if (Id <= 0)
            return "ID không hợp lệ";

        if (string.IsNullOrWhiteSpace(Tenchuongtrinh))
            return "Tên chương trình không được để trống";

        if (Tenchuongtrinh.Length > 255)
            return "Tên chương trình không được vượt quá 255 ký tự";

        if (Giatrigiam <= 0)
            return "Giá trị giảm phải lớn hơn 0";

        if (Soluong <= 0)
            return "Số lượng mã phải lớn hơn 0";

        if (Giatridonhangtoithieu < 0)
            return "Giá trị đơn hàng tối thiểu phải lớn hơn hoặc bằng 0";

        if (Ngayketthuc <= Ngaybatdau)
            return "Ngày kết thúc phải lớn hơn ngày bắt đầu";

        return null; // Hợp lệ
    }
}
