using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelpDesk.Models.Abstracts
{
    public abstract class BaseEntity<T>
    {
        [Key]
        [Column(Order = 1)]
        public T Id { get; set; }

        [DisplayName("Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [DisplayName("Güncelleme Tarihi")]
        public DateTime? UpdatedDate { get; set; }
    }

    //public abstract class AuditEntity
    //{
    //    public DateTime CreatedDate { get; set; } = DateTime.Now;
    //    [StringLength(450)]
    //    public string CreatedUserId { get; set; }

    //    public DateTime? UpdatedDate { get; set; }
    //    [StringLength(450)]
    //    public string UpdatedUserId { get; set; }
    //}
}