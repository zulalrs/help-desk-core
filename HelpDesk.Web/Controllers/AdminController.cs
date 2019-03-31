using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.BLL.Account;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly MembershipTools _membershipTools;
        public AdminController(MembershipTools membershipTools)
        {
            _membershipTools = membershipTools;
        }
        public IActionResult Index()
        {
            return View(_membershipTools.UserManager.Users.ToList());
        }
    }
}