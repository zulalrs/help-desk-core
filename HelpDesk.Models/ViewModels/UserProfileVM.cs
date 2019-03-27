using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HelpDesk.Models.ViewModels
{
    public class UserProfileVM
    {
        public string Id { get; set; }

        [StringLength(50)]
        [Required]
        [DisplayName("Adı")]
        public string Name { get; set; }

        [StringLength(60)]
        [Required]
        [DisplayName("Soyadı")]
        public string Surname { get; set; }

        [StringLength(50)]
        [Required]
        [DisplayName("Kullanıcı Adı")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; }

        [DisplayName("Kayıt Tarihi")]
        public DateTime RegisterDate { get; set; } = DateTime.Now;

        [DisplayName("Aktivasyon Kodu")]
        public string ActivationCode { get; set; }

        public string AvatarPath { get; set; }

        //[DisplayName("Konum")]
        //public Locations Location { get; set; }

        //[DisplayName("Fotoğraf")]
        //public HttpPostedFileBase PostedFile { get; set; }
    }
}
