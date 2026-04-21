using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Sanphamhinhanh
{
    public int Id { get; set; }

    public int? Sanphamid { get; set; }

    public string? Duongdan { get; set; }

    public virtual Sanpham? Sanpham { get; set; }
}
