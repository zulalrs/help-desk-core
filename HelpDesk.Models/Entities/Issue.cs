using HelpDesk.Models.Abstracts;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models.Entities
{

    public class Issue : BaseEntity<string>
    {
        public Issue()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Required]
        public string CustomerId { get; set; }
        public string OperatorId { get; set; }
        public string TechnicianId { get; set; }
        public string SurveyId { get; set; }

        [StringLength(250)]
        [DisplayName("Açıklama")]
        public string Description { get; set; }

        [DisplayName("Ürün")]
        public ProductTypes ProductType { get; set; }

        [DisplayName("Fotoğraf")]
        [NotMapped]
        public List<string> PhotoPath { get; set; }

        [DisplayName("Güncel Durum")]
        public IssueStates IssueState { get; set; } = IssueStates.Beklemede;

        //[DisplayName("Konum")]
        //[Required]
        //public Locations Location { get; set; }

        [Required]
        [DisplayName("Satın Alma Tarihi")]
        public DateTime PurchasedDate { get; set; }

        [DisplayName("Garanti Durumu")]
        public bool WarrantyState { get; set; } = false;

        [DisplayName("Servis Bedeli")]
        public decimal ServiceCharge { get; set; } = 100;

        [StringLength(250)]
        [DisplayName("Operatör Notu")]
        public string OptReport { get; set; }

        [StringLength(250)]
        [DisplayName("Teknisyen Raporu")]
        public string TechReport { get; set; }

        [DisplayName("Arıza Kapanma Tarihi")]
        public DateTime? ClosedDate { get; set; }

        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        [ForeignKey("OperatorId")]
        public virtual ApplicationUser Operator { get; set; }

        [ForeignKey("TechnicianId")]
        public virtual ApplicationUser Technician { get; set; }

        [ForeignKey("SurveyId")]
        public virtual Survey Survey { get; set; }

        public virtual ICollection<Photograph> Photographs { get; set; } = new List<Photograph>();
        public virtual ICollection<IssueLog> IssueLogs { get; set; } = new List<IssueLog>();
    }
}
