namespace BookNest.Areas.Admin.Models
{
    public class RecentOrderViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }
}
