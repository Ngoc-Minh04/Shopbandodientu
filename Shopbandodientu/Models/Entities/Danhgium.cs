using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Danhgium
{
    public int Id { get; set; }

    public int? Sanphamid { get; set; }

    public int? Taikhoanid { get; set; }

    public string? Noidung { get; set; }

    public int? Diem { get; set; }

    public DateTime? Ngaydanhgia { get; set; }

    public virtual Sanpham? Sanpham { get; set; }

    public virtual Taikhoan? Taikhoan { get; set; }
}
