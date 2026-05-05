using System;

namespace Shopbandodientu.Models.DTOs;

public class CreateDiscountRequest
{
    public string Macode { get; set; } = null!;
    public string Tenchuongtrinh { get; set; } = null!;
    public string? Mota { get; set; }
    public string Loaigiamgia { get; set; } = null!; // "percent" hoặc "fixed"
    public decimal Giatrigiam { get; set; }
    public decimal? Giamtoida { get; set; } // Bắt buộc nếu loại percent
    public decimal Giatridonhangtoithieu { get; set; }
    public int Soluong { get; set; }
    public DateTime Ngaybatdau { get; set; }
    public DateTime Ngayketthuc { get; set; }

    /// <summary>
    /// Validate request data
    /// </summary>
    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Macode))
            return "Mã code không được để trống";

        if (Macode.Length > 50)
            return "Mã code không được vượt quá 50 ký tự";

        if (string.IsNullOrWhiteSpace(Tenchuongtrinh))
            return "Tên chương trình không được để trống";

        if (Tenchuongtrinh.Length > 255)
            return "Tên chương trình không được vượt quá 255 ký tự";

        if (string.IsNullOrWhiteSpace(Loaigiamgia))
            return "Loại giảm giá không được để trống";

        if (Loaigiamgia != "percent" && Loaigiamgia != "fixed")
            return "Loại giảm giá chỉ nhận giá trị 'percent' hoặc 'fixed'";

        if (Loaigiamgia == "percent")
        {
            if (Giatrigiam < 0 || Giatrigiam > 100)
                return "Giá trị giảm theo % phải từ 0 đến 100";

            if (!Giamtoida.HasValue || Giamtoida.Value <= 0)
                return "Giảm tối đa phải lớn hơn 0 khi loại giảm giá là percent";
        }

        if (Loaigiamgia == "fixed")
        {
            if (Giatrigiam <= 0)
                return "Giá trị giảm cố định phải lớn hơn 0";
        }

        if (Soluong <= 0)
            return "Số lượng mã phải lớn hơn 0";

        if (Giatridonhangtoithieu < 0)
            return "Giá trị đơn hàng tối thiểu phải lớn hơn hoặc bằng 0";

        if (Ngayketthuc <= Ngaybatdau)
            return "Ngày kết thúc phải lớn hơn ngày bắt đầu";

        return null; // Hợp lệ
    }
}
