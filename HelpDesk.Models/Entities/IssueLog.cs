using HelpDesk.Models.Abstracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelpDesk.Models.Entities
{
    public class IssueLog : BaseEntity<string>
    {
        public IssueLog()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Required]
        public string IssueId { get; set; }

        [StringLength(200)]
        [DisplayName("Açıklama")]
        public string Description { get; set; }

        [DisplayName("Tarafından")]
        public string FromWhom { get; set; }

        [ForeignKey("IssueId")]
        public virtual Issue Issue { get; set; }
    }
}
