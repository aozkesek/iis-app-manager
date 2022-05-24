using System.ComponentModel.DataAnnotations;

namespace Org.Apps.SystemManager.Presentation.Models
{
    public class UserPasswordChange
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre -tekrar-")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifreler aynı değil.")]
        public string ConfirmPassword { get; set; }
    }
}