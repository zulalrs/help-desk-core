using AutoMapper;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.BLL.Services.Senders;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Models.Models;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Web.Controllers
{
    public class TechnicianController : BaseController
    {
        private readonly MembershipTools _membershipTools;
        private readonly IRepository<Issue, string> _issueRepo;
        private readonly IRepository<IssueLog, string> _issueLogRepo;
        private readonly IRepository<Survey, string> _surveyRepo;
        public TechnicianController(MembershipTools membershipTools, IRepository<Issue, string> issueRepo, IRepository<IssueLog, string> issueLogRepo, IRepository<Survey, string> surveyRepo) : base(membershipTools, issueRepo)
        {
            _membershipTools = membershipTools;
            _issueRepo = issueRepo;
            _issueLogRepo = issueLogRepo;
            _surveyRepo = surveyRepo;
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Technician")]
        public IActionResult Index()
        {
            try
            {
                var id = _membershipTools.UserManager.GetUserId(HttpContext.User);
                var data = _issueRepo.GetAll(x => x.TechnicianId == id && x.IssueState != IssueStates.Tamamlandı).Select(x => Mapper.Map<IssueVM>(x)).ToList();
                if (data != null)
                {
                    return View(data);
                }
            }
            catch (Exception ex)
            {
                var errorVM = new ErrorVM()
                {
                    Text = $"Bir hata oluştu. {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Technician",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(errorVM);
                return RedirectToAction("Error500", "Home");
            }
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Technician")]
        public ActionResult Details(string id)
        {
            var issue = _issueRepo.GetById(id);
            var data = Mapper.Map<Issue, IssueVM>(issue);
            return View(data);
        }

        [HttpPost]
        public JsonResult GetJob(string id)
        {
            try
            {
                var issue = _issueRepo.GetById(id);
                if (issue == null)
                {
                    return Json(new ResponseData()
                    {
                        message = "Bulunamadi.",
                        success = false
                    });
                }
                issue.IssueState = IssueStates.İşlemde;
                _issueRepo.Update(issue);

                var issueLog = new IssueLog()
                {
                    IssueId = issue.Id,
                    Description = "Teknisyen işi aldı.",
                    FromWhom = "Teknisyen"
                };
                _issueLogRepo.Insert(issueLog);

                return Json(new ResponseData()
                {
                    message = "İş onayı başarılı",
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Json(new ResponseData()
                {
                    message = $"Bir hata oluştu: {ex.Message}",
                    success = false
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Technician")]
        public ActionResult UpdateJob(IssueVM model)
        {
            try
            {
                var repo = _issueRepo;
                var issue = repo.GetById(model.IssueId);
                if (issue == null)
                {
                    TempData["Message2"] = "Arıza kaydı bulunamadi.";
                    return RedirectToAction("Index", "Technician");
                }

                issue.TechReport = model.TechReport;
                issue.ServiceCharge += model.ServiceCharge;
                issue.UpdatedDate = DateTime.Now;
                repo.Update(issue);

                var issueLog = new IssueLog()
                {
                    IssueId = issue.Id,
                    Description = $"Güncelleme: {issue.TechReport}",
                    FromWhom = "Teknisyen"
                };
                _issueLogRepo.Insert(issueLog);

                return RedirectToAction("Index", "Technician");
            }
            catch (Exception ex)
            {
                var errorVM = new ErrorVM()
                {
                    Text = $"Bir hata oluştu. {ex.Message}",
                    ActionName = "UpdateJob",
                    ControllerName = "Technician",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(errorVM);
                return RedirectToAction("Error500", "Home");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Technician")]
        public async Task<ActionResult> FinishJob(IssueVM model)
        {
            try
            {
                var issueRepo = _issueRepo;
                var issue = issueRepo.GetById(model.IssueId);
                if (issue == null)
                {
                    TempData["Message2"] = "Arıza kaydı bulunamadi.";
                    return RedirectToAction("Index", "Technician");
                }

                issue.IssueState = IssueStates.Tamamlandı;
                issue.ClosedDate = DateTime.Now;
                issueRepo.Update(issue);
                TempData["Message"] = $"{issue.Description} adlı iş tamamlandı.";

                var survey = new Survey();
                var surveyRepo = _surveyRepo;
                surveyRepo.Insert(survey);
                issue.SurveyId = survey.Id;
                issueRepo.Update(issue);

                var user = await _membershipTools.NewUserStore().FindByIdAsync(issue.CustomerId);
                var usernamesurname = await _membershipTools.GetNameSurname(issue.CustomerId);

                var uri = new UriBuilder()
                {
                    Scheme = Uri.UriSchemeHttps
                };
                var hostComponents = Request.Host.ToUriComponent();
                string SiteUrl = uri.Scheme + System.Uri.SchemeDelimiter + hostComponents;

                var emailService = new EmailService();
                var body = $"Merhaba <b>{usernamesurname}</b><br>{issue.Description} adlı arıza kaydınız kapanmıştır.<br>Değerlendirmeniz için aşağıda linki bulunan anketi doldurmanızı rica ederiz.<br> <a href='{SiteUrl}/issue/survey?code={issue.SurveyId}' >Anket Linki </a> ";
                await emailService.SendAsync(new EmailModel() { Body = body, Subject = "Değerlendirme Anketi" }, user.Email);

                var issueLog = new IssueLog()
                {
                    IssueId = issue.Id,
                    Description = "İş tamamlandı.",
                    FromWhom = "Teknisyen"
                };
                _issueLogRepo.Insert(issueLog);

                return RedirectToAction("Index", "Technician");
            }
            catch (Exception ex)
            {
                var errorVM = new ErrorVM()
                {
                    Text = $"Bir hata oluştu. {ex.Message}",
                    ActionName = "FinishJob",
                    ControllerName = "Technician",
                    ErrorCode = "500"
                };
                TempData["ErrorMessage"] = JsonConvert.SerializeObject(errorVM);
                return RedirectToAction("Error500", "Home");
            }
        }

    }
}