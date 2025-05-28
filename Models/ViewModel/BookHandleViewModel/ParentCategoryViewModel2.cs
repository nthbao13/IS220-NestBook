using System.Security.Policy;

namespace BookNest.Models.ViewModel.BookHandleViewModel
{
    public class ParentCategoryViewModel2
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public List<CategoryViewModel> CategoryViewModels { get; set; }
    }
}
