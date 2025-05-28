using System;
using System.Collections.Generic;

namespace BookNest.Models.MappingDBModel;

public partial class PaymentType
{
    public int Id { get; set; }

    public string? Type { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
