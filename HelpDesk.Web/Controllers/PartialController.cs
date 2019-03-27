using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers
{
    public class PartialController : Controller
    {
        public PartialViewResult DrawerPartial()
        {
            var data = new List<string>();
            return PartialView("Partial/_DrawerPartial", data);
        }
        public PartialViewResult HeaderPartial()
        {
            return PartialView("Partial/_HeaderPartial");
        }
        public PartialViewResult ModalPartial()
        {
            return PartialView("Partial/_ModalPartial");
        }
        public PartialViewResult FooterPartial()
        {
            return PartialView("Partial/_FooterPartial");
        }
    }
}