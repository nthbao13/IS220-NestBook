namespace BookNest.Data.DTO
{
    public class CreateOrderRequest
    {
        public int From { get; set; }
        public List<Product> Products { get; set; }
    }
}
