using BookNest.Models.ViewModel.HomeHandleViewModel;

namespace BookNest.Models.ViewModel.BookHandleViewModel
{
    public class BookIndexViewModel
    {
        public List<ParentCategoryViewModel2> ParentCategories { get; set; }
        public List<PublisherViewModel> PublisherCategories { get; set; }
        public List<BookViewModel> Books { get; set; }

        public int TotalPages {  get; set; }
        public int Page { get; set; }

        public ParentCategoryViewModel? ParentCategoryId { get; set; }
        public CategoryViewModel? CategoryId { get; set; }

        public List<int>? PublisherId { get; set; }
        public List<int>? RangePrice { get; set; }
        public string? keyword { get; set; }
    }
}
