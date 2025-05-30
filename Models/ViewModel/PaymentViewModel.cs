using BookNest.Models.MappingDBModel;

namespace BookNest.Models.ViewModel
{
    public class PaymentViewModel
    {
        public int OrderId { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
