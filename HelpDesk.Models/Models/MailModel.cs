using System;
using System.Collections.Generic;
using System.Text;

namespace HelpDesk.Models.ViewModels
{
    public class MailModel
    {
        public string Body { get; set; }
        public string Subject { get; set; }
        public string Destination { get; set; }
    }
}
