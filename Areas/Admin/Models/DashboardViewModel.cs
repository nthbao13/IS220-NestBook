namespace BookNest.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int NewCustomers { get; set; }
        public List<RecentOrderViewModel> RecentOrders { get; set; }
        public List<TopProductViewModel> TopProducts { get; set; }
    }
}
