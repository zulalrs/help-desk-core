using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace HelpDesk.Models.ViewModels
{
    public class UpdateUserRoleVM
    {
        public string Id { get; set; }

        [DisplayName("Roller")]
        public List<string> Roles { get; set; }
    }
}
