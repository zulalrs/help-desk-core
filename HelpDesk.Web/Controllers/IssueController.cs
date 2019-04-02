using AutoMapper;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Repository;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Web.Controllers
{
    public class IssueController : BaseController
    {
        private readonly MembershipTools _membershipTools;
        public IssueController(MembershipTools membershipTools):base(membershipTools)
        {
            _membershipTools = membershipTools;
        }
        [HttpGet]
        //[Route("arizakayit_anasayfa")]
        public async Task<ActionResult> Index()
        {
            try
            {
                var user = await _membershipTools.UserManager.GetUserAsync(HttpContext.User);
                var id = user.Id;
                var data = new IssueRepo().GetAll(x => x.CustomerId == id).Select(x => Mapper.Map<IssueVM>(x)).ToList();
                if (data != null)
                {
                    return View(data);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Issue",
                    ErrorCode = 500
                };
                return RedirectToAction("Error500", "Home");
            }
            return View();
        }
    }
}