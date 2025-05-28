namespace BookNest.Models.ViewModel
{
    public class BookViewModel
    {
        public int Id { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string ImageUrl { get; set; }
        public decimal FirstPrice { get; set; }
        public decimal SecondPrice { get; set; }
    }
}
