using System.ComponentModel.DataAnnotations;

namespace ItServiceApp.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı alanı gereklidir.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Şifre alanı gereklidir.")]
        //[StringLength(100),MinLength(6,ErrorMessage ="Şifreniz minimum 6 karekterli olmalıdır.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifreniz minimum 6 karekterli olmalıdır.")]

        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}