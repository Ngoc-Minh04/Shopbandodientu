using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Loaitaikhoan
{
    public int Id { get; set; }

    public string Tenloai { get; set; } = null!;

    public virtual ICollection<Taikhoan> Taikhoans { get; set; } = new List<Taikhoan>();
}
