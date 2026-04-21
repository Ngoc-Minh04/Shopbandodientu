using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Donhang
{
    public int Id { get; set; }

    public int? Taikhoanid { get; set; }

    public DateTime? Ngaydat { get; set; }

    public decimal? Tongtien { get; set; }

    public string? Trangthai { get; set; }

    public string? Tennguoinhan { get; set; }

    public string? Diachigiaohang { get; set; }

    public string? Sdtnguoinhan { get; set; }

    public virtual ICollection<Chitietdonhang> Chitietdonhangs { get; set; } = new List<Chitietdonhang>();

    public virtual ICollection<Lichsusudungmagiamgium> Lichsusudungmagiamgia { get; set; } = new List<Lichsusudungmagiamgium>();

    public virtual Taikhoan? Taikhoan { get; set; }

    public virtual ICollection<Thanhtoan> Thanhtoans { get; set; } = new List<Thanhtoan>();
}
