using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Web.Models;

namespace HelpDesk.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult Error400()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Error500()
        {
            return View();
        }
    }
}
