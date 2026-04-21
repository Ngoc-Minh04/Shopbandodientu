using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Thanhtoan
{
    public int Id { get; set; }

    public int? Donhangid { get; set; }

    public string? Phuongthuc { get; set; }

    public decimal? Sotien { get; set; }

    public string? Trangthai { get; set; }

    public DateTime? Ngaythanhtoan { get; set; }

    public string? Magiaodich { get; set; }

    public string? Congthanhtoan { get; set; }

    public virtual Donhang? Donhang { get; set; }
}
