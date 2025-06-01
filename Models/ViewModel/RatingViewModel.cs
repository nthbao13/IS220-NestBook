using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace BookNest.Models.ViewModel
{
    public class RatingViewModel
    {
        public int userId { get; set; }
        public string username { get; set; }
        public string imageurl { get; set; }
        public int rating { get; set; }
        public string content { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
