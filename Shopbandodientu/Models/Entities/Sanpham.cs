using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Sanpham
{
    public int Id { get; set; }

    public string Tensanpham { get; set; } = null!;

    public string? Mota { get; set; }

    public decimal Gia { get; set; }

    public int Soluong { get; set; }

    public int? Danhmucid { get; set; }

    public DateTime? Ngaythem { get; set; }

    public bool? Trangthai { get; set; }

    public string? Thuonghieu { get; set; }

    public int? Khuyenmai { get; set; }

    public virtual ICollection<Chitietdonhang> Chitietdonhangs { get; set; } = new List<Chitietdonhang>();

    public virtual ICollection<Chitietgiohang> Chitietgiohangs { get; set; } = new List<Chitietgiohang>();

    public virtual ICollection<Danhgium> Danhgia { get; set; } = new List<Danhgium>();

    public virtual Danhmuc? Danhmuc { get; set; }

    public virtual ICollection<Sanphamhinhanh> Sanphamhinhanhs { get; set; } = new List<Sanphamhinhanh>();

    public virtual ICollection<Thongso> Thongsos { get; set; } = new List<Thongso>();
}
