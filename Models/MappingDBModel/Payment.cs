using System;
using System.Collections.Generic;

namespace BookNest.Models.MappingDBModel;

public partial class Payment
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? PaymentTypeId { get; set; }

    public int? TotalPrice { get; set; }

    public string? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public string? TransactionRef { get; set; } // Store our custom transaction reference
    public string? VnpTransactionNo { get; set; } // Store VNPay's transaction number
    public string? VnpResponseCode { get; set; } // Store VNPay's response code

    public virtual Order? Order { get; set; }

    public virtual PaymentType? PaymentType { get; set; }
}
