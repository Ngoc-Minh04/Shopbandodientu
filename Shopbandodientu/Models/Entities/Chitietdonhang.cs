using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Chitietdonhang
{
    public int Id { get; set; }

    public int? Donhangid { get; set; }

    public int? Sanphamid { get; set; }

    public int Soluong { get; set; }

    public decimal? Dongia { get; set; }

    public virtual Donhang? Donhang { get; set; }

    public virtual Sanpham? Sanpham { get; set; }
}
