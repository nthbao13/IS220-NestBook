using System.ComponentModel.DataAnnotations;

namespace BookNest.Models.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập thông tin")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập thông tin")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập thông tin")]
        [MaxLength(10)]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
