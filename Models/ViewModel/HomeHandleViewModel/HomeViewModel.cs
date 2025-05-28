namespace BookNest.Models.ViewModel.HomeHandleViewModel
{
    public class HomeViewModel
    {
        public List<ParentCategoryViewModel> ParentCategories { get; set; }
        public List<BookViewModel> HotBooks { get; set; }
        public List<BookViewModel> LiteratureBooks { get; set; }
        public List<BookViewModel> EconomyBooks { get; set; }
        public List<BookViewModel> PsychoBooks { get; set; }
        public List<BookViewModel> ChildrenBooks { get; set; }
        public List<BookViewModel> ScienceBooks { get; set; }
        public List<BookViewModel> TechnologyBooks { get; set; }
    }
}
