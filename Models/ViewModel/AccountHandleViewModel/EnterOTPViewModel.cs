using System.ComponentModel.DataAnnotations;

namespace BookNest.Models.ViewModel.AccountHandleViewModel
{
    public class EnterOTPViewModel
    {
        [Required(ErrorMessage = "OTP không được để trống")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP phải gồm 6 chữ số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP phải là số gồm 6 chữ số")]
        public string Otp { get; set; }
    }

}
