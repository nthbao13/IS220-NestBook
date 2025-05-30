using System;
using System.Collections.Generic;

namespace BookNest.Models.MappingDBModel;

public partial class Order
{
    public int Id { get; set; }

    public int? UserId { get; set; }
    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Status { get; set; }
    
    public DateTime? CreateAt { get; set; }

    public int? From { get; set; } // buy_now = 0; cart = 1

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
}
