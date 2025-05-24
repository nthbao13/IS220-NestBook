using System.ComponentModel.DataAnnotations;

namespace BookNest.Models.ViewModel
{
    public class EnterEmailViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
    }
}
