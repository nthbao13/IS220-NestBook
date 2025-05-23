using System.ComponentModel.DataAnnotations;

namespace BookNest.Models.ViewModel
{
    public class ResetPasswordViewModel
    {
        public string Phone { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string confirmPassword { get; set; }
    }
}
