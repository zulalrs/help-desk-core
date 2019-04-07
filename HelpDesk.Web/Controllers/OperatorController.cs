using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.Models.Entities;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Web.Controllers
{
    public class OperatorController : BaseController
    {
        private readonly MembershipTools _membershipTools;
        private readonly IRepository<Issue, string> _issueRepo;
        public OperatorController(MembershipTools membershipTools, IRepository<Issue, string> issueRepo) : base(membershipTools,issueRepo)
        {
            _membershipTools = membershipTools;
            _issueRepo = issueRepo;
        }
        public IActionResult Index()
        {
            try
            {
                var data = _issueRepo.GetAll(x => x.OperatorId == null)
                    .Select(x => Mapper.Map<IssueVM>(x)).ToList();
                if (data != null)
                    return View(data);
            }
            catch (Exception ex)
            {
                TempData["Message"] = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Operator",
                    ErrorCode = 500
                };
                return RedirectToAction("Error500", "Home");
            }
            return View();
        }
    }
}
