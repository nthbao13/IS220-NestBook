using System;
using System.Collections.Generic;

namespace BookNest.Models.MappingDBModel;

public partial class Voucher
{
    public int Id { get; set; }

    public string? VoucherCode { get; set; }

    public bool? Type { get; set; }

    public decimal? Value { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public int? UsageLimit { get; set; }

    public int? UsedCount { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
