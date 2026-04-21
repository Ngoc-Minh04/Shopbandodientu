using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Thongso
{
    public int Id { get; set; }

    public int? Idsanpham { get; set; }

    public string? Cpu { get; set; }

    public string? Vga { get; set; }

    public string? Ram { get; set; }

    public string? Rom { get; set; }

    public virtual Sanpham? IdsanphamNavigation { get; set; }
}
