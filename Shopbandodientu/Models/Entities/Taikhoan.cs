using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Taikhoan
{
    public int Id { get; set; }

    public string? Hoten { get; set; }

    public string Email { get; set; } = null!;

    public string Matkhau { get; set; } = null!;

    public string? Sodienthoai { get; set; }

    public string? Diachi { get; set; }

    public string? Anhdaidien { get; set; }

    public DateTime? Ngaytao { get; set; }

    public bool? Trangthai { get; set; }

    public int? Loaitaikhoanid { get; set; }

    public int? Iddanhmuc { get; set; }

    public string? Code { get; set; }

    public virtual ICollection<Danhgium> Danhgia { get; set; } = new List<Danhgium>();

    public virtual ICollection<Donhang> Donhangs { get; set; } = new List<Donhang>();

    public virtual ICollection<Giohang> Giohangs { get; set; } = new List<Giohang>();

    public virtual Danhmuc? IddanhmucNavigation { get; set; }

    public virtual ICollection<Lichsusudungmagiamgium> Lichsusudungmagiamgia { get; set; } = new List<Lichsusudungmagiamgium>();

    public virtual Loaitaikhoan? Loaitaikhoan { get; set; }

    public virtual ICollection<Tinnhan> TinnhanNguoiguis { get; set; } = new List<Tinnhan>();

    public virtual ICollection<Tinnhan> TinnhanNguoinhans { get; set; } = new List<Tinnhan>();
}
