namespace BookNest.Data.DTO
{
    public class ProcessPaymentRequest
    {
        public int OrderId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public string VoucherCode { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
