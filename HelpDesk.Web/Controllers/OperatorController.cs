using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Repository;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.BLL.Services.Senders;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace HelpDesk.Web.Controllers
{
    public class OperatorController : BaseController
    {
        private readonly MembershipTools _membershipTools;
        private readonly IRepository<Issue, string> _issueRepo;
        private readonly IRepository<IssueLog, string> _issueLogRepo;
        private readonly IRepository<Photograph, string> _photoRepo;

        List<SelectListItem> Technicians = new List<SelectListItem>();

        public OperatorController(MembershipTools membershipTools, IRepository<Issue, string> issueRepo, IRepository<IssueLog, string> issueLogRepo, IRepository<Photograph, string> photoRepo) : base(membershipTools, issueRepo)
        {
            _membershipTools = membershipTools;
            _issueRepo = issueRepo;
            _issueLogRepo = issueLogRepo;
            _photoRepo = photoRepo;
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
                var errorVM = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Operator",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(errorVM);
                return RedirectToAction("Error500", "Home");
            }
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Operator")]
        public async Task<ActionResult> Details(string id)
        {
            //ViewBag.TechnicianList = await GetTechnicianList();

            var issue = _issueRepo.GetById(id);
            var photoPath = _photoRepo.GetAll(x => x.IssueId == id).Select(y=>y.Path).ToList();
            issue.PhotoPath = photoPath;

            if (issue == null)
            {
                TempData["Message2"] = "Arıza kaydı bulunamadi.";
                return RedirectToAction("Index", "Operator");
            }
            var user = await _membershipTools.UserManager.GetUserAsync(HttpContext.User);
            var userid = user.Id;
            if (userid == null)
            {
                return RedirectToAction("Index", "Issue");
            }

            var data = Mapper.Map<Issue, IssueVM>(issue);

            var techIds = _issueRepo.GetAll(x => x.IssueState == IssueStates.İşlemde || x.IssueState == IssueStates.Atandı).Select(x => x.TechnicianId).ToList();
            var technicians = _membershipTools.UserManager.GetUsersInRoleAsync("Technician").Result;

            for (int i = 0; i < technicians.Count; i++)
            {
                var distance = 0.0;
                string distanceString = "";
                var technician = technicians[i];
                if (!techIds.Contains(technician.Id))
                {
                    if (technician.Latitude.HasValue && technician.Longitude.HasValue && data.Latitude.HasValue && data.Longitude.HasValue)
                    {
                        var issueCoordinate = new GeoCoordinate(data.Latitude.Value, data.Longitude.Value);
                        var technicianCoordinate = new GeoCoordinate(technician.Latitude.Value, technician.Longitude.Value);

                        distance = issueCoordinate.GetDistanceTo(technicianCoordinate) / 1000;
                        distanceString = $"(~{Convert.ToInt32(distance)} km)";
                    }
                    
                    Technicians.Add(new SelectListItem()
                    {
                        Text = technician.Name + " " + technician.Surname + " (" + await _membershipTools.GetTechPoint(user.Id) + ")" + distanceString,
                        Value = technician.Id
                    });
                }
                else
                    continue;
            }

            ViewBag.TechnicianList = Technicians;

            if (issue.OperatorId == null)
            {
                issue.OperatorId = userid;
                if (_issueRepo.Update(issue) > 0)
                {
                    issue.IssueState = IssueStates.KabulEdildi;
                    data.IssueState = issue.IssueState;
                    _issueRepo.Update(issue);

                    var issueLog = new IssueLog()
                    {
                        IssueId = issue.Id,
                        Description = "Operatör tarafından kabul edildi.",
                        FromWhom = "Operatör"
                    };
                    _issueLogRepo.Insert(issueLog);

                    return View(data);
                }
            }

            return View(data);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Operator")]
        public ActionResult AllIssues()
        {

            try
            {
                var data = _issueRepo.GetAll().Select(x => Mapper.Map<IssueVM>(x)).ToList();
                if (data != null)
                {
                    return View(data);
                }
            }
            catch (Exception ex)
            {
                var errorVM = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "AllIssues",
                    ControllerName = "Operator",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(errorVM);
                return RedirectToAction("Error500", "Home");
            }
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Operator")]
        public async Task<ActionResult> AssignedIssues()
        {
            try
            {
                var user = await _membershipTools.UserManager.GetUserAsync(HttpContext.User);
                var id = user.Id;
                var data = _issueRepo.GetAll(x => x.OperatorId == id && x.TechnicianId == null).Select(x => Mapper.Map<IssueVM>(x)).ToList();
                if (data != null)
                {
                    return View(data);
                }
            }
            catch (Exception ex)
            {
                var errorVM = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "AssignedIssues",
                    ControllerName = "Operator",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(errorVM);
                return RedirectToAction("Error500", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Operator")]
        public async Task<ActionResult> AssignTech(IssueVM model)
        {
            try
            {
                var issue = _issueRepo.GetById(model.IssueId);
                issue.TechnicianId = model.TechnicianId;
                issue.IssueState = IssueStates.Atandı;
                issue.OptReport = model.OptReport;
                _issueRepo.Update(issue);
                var technician = await _membershipTools.NewUserStore().FindByIdAsync(issue.TechnicianId);
                TempData["Message"] =
                    $"{issue.Description} adlı arızaya {technician.Name}  {technician.Surname} teknisyeni atandı.";

                var customer = await _membershipTools.UserManager.FindByIdAsync(issue.CustomerId);
                var emailService = new EmailService();
                var body = $"Merhaba <b>{await _membershipTools.GetNameSurname(issue.CustomerId)}</b><br>{issue.Description} adlı arızanız onaylanmıştır ve görevli teknisyen en kısa sürede yola çıkacaktır.";
                await emailService.SendAsync(new EmailModel()
                {
                    Body = body,
                    Subject = $"{issue.Description} adlı arıza hk."
                }, customer.Email);

                var issueLog = new IssueLog()
                {
                    IssueId = issue.Id,
                    Description = "Teknisyene atandı.",
                    FromWhom = "Operatör"
                };
                _issueLogRepo.Insert(issueLog);

                return RedirectToAction("AllIssues", "Operator");
            }
            catch (Exception ex)
            {
                var errorVM = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "AssignTechAsync",
                    ControllerName = "Operator",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(errorVM);
                return RedirectToAction("Error500", "Home");
            }
        }
    }
}
