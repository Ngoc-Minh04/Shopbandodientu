using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Giohang
{
    public int Id { get; set; }

    public int? Taikhoanid { get; set; }

    public DateTime? Ngaycapnhat { get; set; }

    public virtual ICollection<Chitietgiohang> Chitietgiohangs { get; set; } = new List<Chitietgiohang>();

    public virtual Taikhoan? Taikhoan { get; set; }
}
