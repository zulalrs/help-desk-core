using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models.IdentityEntities
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        [Required]
        [DisplayName("Adı")]
        public string Name { get; set; }

        [StringLength(60)]
        [Required]
        [DisplayName("Soyadı")]
        public string Surname { get; set; }

        [DisplayName("Kayıt Tarihi")]
        public DateTime RegisterDate { get; set; } = DateTime.Now;

        [DisplayName("Aktivasyon Kodu")]
        public string ActivationCode { get; set; }

        public string AvatarPath { get; set; }

        [Display(Name = "Enlem")]
        public double? Latitude { get; set; }

        [Display(Name = "Boylam")]
        public double? Longitude { get; set; }
    }
}
