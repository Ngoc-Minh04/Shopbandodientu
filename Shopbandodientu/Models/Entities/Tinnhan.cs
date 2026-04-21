using System;
using System.Collections.Generic;

namespace Shopbandodientu.Models.Entities;

public partial class Tinnhan
{
    public int Id { get; set; }

    public int? Nguoiguiid { get; set; }

    public int? Nguoinhanid { get; set; }

    public string? Noidung { get; set; }

    public DateTime? Thoigian { get; set; }

    public virtual Taikhoan? Nguoigui { get; set; }

    public virtual Taikhoan? Nguoinhan { get; set; }
}
