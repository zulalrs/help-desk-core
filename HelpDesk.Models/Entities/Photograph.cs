using HelpDesk.Models.Abstracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HelpDesk.Models.Entities
{
    public class Photograph : BaseEntity<string>
    {
        public Photograph()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        [Required]
        [DisplayName("Yol")]
        public string Path { get; set; }

        public string IssueId { get; set; }

        [ForeignKey("IssueId")]
        public virtual Issue Issue { get; set; }
    }
}
