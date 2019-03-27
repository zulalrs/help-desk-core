using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HelpDesk.Models.ViewModels
{
    public class LoginVM
    {
        [Required]
        [MinLength(1, ErrorMessage = "Kullanıcı Adı alanı gereklidir!")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }

        [StringLength(10, MinimumLength = 5, ErrorMessage = "Şifreniz 5-10 karakter arası olmalıdır!")]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}
