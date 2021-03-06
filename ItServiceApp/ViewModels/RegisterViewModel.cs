using System.ComponentModel.DataAnnotations;

namespace ItServiceApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı alanı gereklidir")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Ad alanı gereklidir")]
        [Display(Name = "Ad")]
        [StringLength(50)]
        public string Name { get; set; }


        [Required(ErrorMessage = "Soyad alanı gereklidir")]
        [Display(Name = "Soyad")]
        [StringLength(50)]
        public string Surname { get; set; }


        [Required(ErrorMessage = "Eposta alanı gereklidir")]
        [EmailAddress]
        public string Email { get; set; }


        [Required(ErrorMessage = "Şifre alanı gereklidir")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifreniz minimum altı karakterli olmalıdır.")]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        [Required(ErrorMessage = "Şifre tekrar alanı gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare(nameof(Password), ErrorMessage = "Şifreler uyuşmuyor")]
        public string ConfirmPassword { get; set; }
    }
}
