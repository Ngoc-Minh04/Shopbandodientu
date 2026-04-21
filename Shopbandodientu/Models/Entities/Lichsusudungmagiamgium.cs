using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Lichsusudungmagiamgium
{
    public int Id { get; set; }

    public int? Magiamgiaid { get; set; }

    public int? Taikhoanid { get; set; }

    public int? Donhangid { get; set; }

    public decimal Giatrigiamthucte { get; set; }

    public DateTime? Ngaysudung { get; set; }

    public virtual Donhang? Donhang { get; set; }

    public virtual Magiamgium? Magiamgia { get; set; }

    public virtual Taikhoan? Taikhoan { get; set; }
}
