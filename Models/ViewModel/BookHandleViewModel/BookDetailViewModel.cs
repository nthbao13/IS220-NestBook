using BookNest.Controllers;
using BookNest.Models.MappingDBModel;
using System.Net;

namespace BookNest.Models.ViewModel.BookHandleViewModel
{
    public class BookDetailViewModel
    {
        public Book Book { get; set; }
        public List<BookViewModel> ListBook { get; set; }
    }
}
