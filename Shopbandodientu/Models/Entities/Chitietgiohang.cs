using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Chitietgiohang
{
    public int Id { get; set; }

    public int? Giohangid { get; set; }

    public int? Sanphamid { get; set; }

    public int Soluong { get; set; }

    public virtual Giohang? Giohang { get; set; }

    public virtual Sanpham? Sanpham { get; set; }
}
