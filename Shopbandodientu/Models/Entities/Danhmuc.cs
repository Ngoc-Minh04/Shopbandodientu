using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Danhmuc
{
    public int Id { get; set; }

    public string Tendanhmuc { get; set; } = null!;

    public virtual ICollection<Sanpham> Sanphams { get; set; } = new List<Sanpham>();

    public virtual ICollection<Taikhoan> Taikhoans { get; set; } = new List<Taikhoan>();
}
