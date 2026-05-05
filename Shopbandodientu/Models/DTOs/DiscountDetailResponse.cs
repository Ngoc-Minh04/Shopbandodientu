using System;

namespace Shopbandodientu.Models.DTOs;

public class DiscountDetailResponse
{
    public int Id { get; set; }
    public string Macode { get; set; } = null!;
    public string Tenchuongtrinh { get; set; } = null!;
    public string? Mota { get; set; }
    public string Loaigiamgia { get; set; } = null!;
    public decimal Giatrigiam { get; set; }
    public decimal? Giamtoida { get; set; }
    public decimal Giatridonhangtoithieu { get; set; }
    public int Soluong { get; set; }
    public int Soluongdasudung { get; set; }
    public int Soluongconlai { get; set; }
    public DateTime Ngaybatdau { get; set; }
    public DateTime Ngayketthuc { get; set; }
    public bool Trangthai { get; set; }
    public DateTime Ngaytao { get; set; }
    public decimal TongGiatriDagiam { get; set; }
    public int TongLuotSudung { get; set; }
}
