using AutoMapper;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Helpers;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.BLL.Services.Senders;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using HelpDesk.Models.Models;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly MembershipTools _membershipTools;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRepository<Issue, string> _issueRepo;
        private readonly IRepository<Survey, string> _surveyRepo;

        public AdminController(MembershipTools membershipTools, IHostingEnvironment hostingEnvironment, IRepository<Issue, string> issueRepo, IRepository<Survey, string> surveyRepo) :base(membershipTools)
        {
            _membershipTools = membershipTools;
            _hostingEnvironment = hostingEnvironment;
            _issueRepo = issueRepo;
            _surveyRepo = surveyRepo;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_membershipTools.UserManager.Users.ToList());
        }

        [HttpPost]
        public async Task<JsonResult> SendCode(string id)
        {
            try
            {
                var userStore = _membershipTools.NewUserStore();
                var user = await userStore.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new ResponseData()
                    {
                        message = "Kullanıcı bulunamadı.",
                        success = false
                    });
                }

                if (user.EmailConfirmed)
                {
                    return Json(new ResponseData()
                    {
                        message = "Kullanıcı zaten e-postasını onaylamış.",
                        success = false
                    });
                }

                user.ActivationCode = StringHelpers.GetCode();
                await userStore.UpdateAsync(user);
                userStore.Context.SaveChanges();
                var uri = new UriBuilder()
                {
                    Scheme = Uri.UriSchemeHttps
                };
                var hostComponents = Request.Host.ToUriComponent();
                string SiteUrl = uri.Scheme + System.Uri.SchemeDelimiter + hostComponents;

                var emailService = new EmailService();
                var body =
                    $"Merhaba <b>{user.Name} {user.Surname}</b><br>Hesabınızı aktif etmek için aşağıdaki linke tıklayınız.<br> <a href='{SiteUrl}/account/activation?code={user.ActivationCode}' >Aktivasyon Linki </a> ";
                await emailService.SendAsync(new EmailModel()
                {
                    Body = body,
                    Subject = "Sitemize Hoşgeldiniz"
                }, user.Email);
                return Json(new ResponseData()
                {
                    message = "Kullanıcıya yeni aktivasyon maili gönderildi.",
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
        public async Task<JsonResult> SendPassword(string id)
        {
            try
            {
                var userStore =_membershipTools.NewUserStore();
                var user = await userStore.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new ResponseData()
                    {
                        message = "Kullanıcı bulunamadı",
                        success = false
                    });
                }

                var newPassword = StringHelpers.GetCode().Substring(0, 6)+"A0*";
                await userStore.SetPasswordHashAsync(user, _membershipTools.UserManager.PasswordHasher.HashPassword(user, newPassword));
                await userStore.UpdateAsync(user);
                userStore.Context.SaveChanges();
                var uri = new UriBuilder()
                {
                    Scheme = Uri.UriSchemeHttps
                };
                var hostComponents = Request.Host.ToUriComponent();
                string SiteUrl = uri.Scheme + System.Uri.SchemeDelimiter + hostComponents;
                var emailService = new EmailService();
                var body =
                    $"Merhaba <b>{user.Name} {user.Surname}</b><br>Hesabınızın parolası sıfırlanmıştır<br> Yeni parolanız: <b>{newPassword}</b> <p>Yukarıdaki parolayı kullanarak sistemize giriş yapabilirsiniz.</p>";
                emailService.Send(new EmailModel() { Body = body, Subject = $"{user.UserName} Şifre Kurtarma" },
                    user.Email);

                return Json(new ResponseData()
                {
                    message = "Şifre sıfırlama maili gönderilmiştir.",
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

        [HttpGet]
        public async Task<ActionResult> EditUser(string id)
        {
            try
            {
                var userManager = _membershipTools.UserManager;
                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return RedirectToAction("Index");
                }

                var roles =await userManager.GetRolesAsync(user);
                var roller = GetRoleList();
                foreach (var role in roles)
                {
                    foreach (var selectListItem in roller)
                    {
                        if (selectListItem.Value == role)
                        {
                            selectListItem.Selected = true;
                        }
                    }
                }

                ViewBag.RoleList = roller;
                var model = Mapper.Map<ApplicationUser, UserProfileVM>(user);
                //var model = new UserProfileVM()
                //{
                //    AvatarPath = user.AvatarPath,
                //    Name = user.Name,
                //    Email = user.Email,
                //    Surname = user.Surname,
                //    Id = user.Id,
                //    PhoneNumber = user.PhoneNumber,
                //    UserName = user.UserName
                //};
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Message"] = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "EditUser",
                    ControllerName = "Admin",
                    ErrorCode = 500
                };
                return RedirectToAction("Error500", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser(UserProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userManager = _membershipTools.UserManager;
                var user = userManager.FindByIdAsync(model.Id).Result;
                Mapper.Map<UserProfileVM,ApplicationUser>(model,user);
                //user.Name = model.Name;
                //user.Surname = model.Surname;
                //user.PhoneNumber = model.PhoneNumber;
                //user.Email = model.Email;

                if (model.PostedFile != null &&
                    model.PostedFile.Length > 0)
                {
                    var file = model.PostedFile;
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string extName = Path.GetExtension(file.FileName);
                    fileName = StringHelpers.UrlFormatConverter(fileName);
                    fileName += StringHelpers.GetCode();

                    var webpath = _hostingEnvironment.WebRootPath;
                    var directorypath = Path.Combine(webpath, "Uploads");
                    var filePath = Path.Combine(directorypath, fileName + extName);

                    if (!Directory.Exists(directorypath))
                    {
                        Directory.CreateDirectory(directorypath);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    user.AvatarPath = "/Uploads/" + fileName + extName;
                }

                await userManager.UpdateAsync(user);
                TempData["Message"] = "Güncelleme işlemi başarılı";
                return RedirectToAction("EditUser", new { id = user.Id });
            }
            catch (Exception ex)
            {
                TempData["Message"] = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Admin",
                    ErrorCode = 500
                };
                return RedirectToAction("Error500", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUserRoles(UpdateUserRoleVM model)
        {
            var userId = model.Id;
            var rolIdler = model.Roles;
            var roleManager = _membershipTools.RoleManager;
            var seciliRoller = new string[rolIdler.Count];
            for (var i = 0; i < rolIdler.Count; i++)
            {
                var rid = rolIdler[i];
                seciliRoller[i] =roleManager.FindByIdAsync(rid).Result.ToString();
            }

            var userManager = _membershipTools.UserManager;
            var user =await userManager.FindByIdAsync(userId);
            var roles = _membershipTools.UserManager.GetRolesAsync(user).Result;

            foreach (var identityUserRole in roles)
            {
                await userManager.RemoveFromRoleAsync(user, identityUserRole);
            }

            for (int i = 0; i < seciliRoller.Length; i++)
            {
                await userManager.AddToRoleAsync(user, seciliRoller[i]);
            }

            return RedirectToAction("EditUser", new { id = userId });
        }

        [HttpGet]
        public ActionResult Reports()
        {
            try
            {
                var issueRepo =_issueRepo;
                var surveyRepo = _surveyRepo;
                var issueList = issueRepo.GetAll(x => x.SurveyId != null).ToList();

                var surveyList = surveyRepo.GetAll().Where(x => x.IsDone).ToList();
                var totalSpeed = 0.0;
                var totalTech = 0.0;
                var totalPrice = 0.0;
                var totalSatisfaction = 0.0;
                var totalSolving = 0.0;
                var count = issueList.Count;

                if (count == 0)
                {
                    TempData["Message2"] = "Rapor oluşturmak için yeterli kayıt bulunamadı.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var survey in surveyList)
                {
                    totalSpeed += survey.Speed;
                    totalTech += survey.TechPoint;
                    totalPrice += survey.Pricing;
                    totalSatisfaction += survey.Satisfaction;
                    totalSolving += survey.Solving;
                }

                var totalDays = 0;
                foreach (var issue in issueList)
                {
                    totalDays += issue.ClosedDate.Value.DayOfYear - issue.CreatedDate.DayOfYear;
                }

                ViewBag.AvgSpeed = totalSpeed / count;
                ViewBag.AvgTech = totalTech / count;
                ViewBag.AvgPrice = totalPrice / count;
                ViewBag.AvgSatisfaction = totalSatisfaction / count;
                ViewBag.AvgSolving = totalSolving / count;
                ViewBag.AvgTime = totalDays / issueList.Count;

                return View(surveyList);
            }
            catch (Exception ex)
            {
                TempData["Message"] = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Reports",
                    ControllerName = "Admin",
                    ErrorCode = 500
                };
                return RedirectToAction("Error500", "Home");
            }
        }
        [HttpGet]
        public JsonResult GetDailyReport()
        {
            try
            {
                var dailyIssues = _issueRepo.GetAll(x => x.CreatedDate.DayOfYear == DateTime.Now.DayOfYear).Count;

                return Json(new ResponseData()
                {
                    data = dailyIssues,
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Json(new ResponseData()
                {
                    data = 0,
                    message = ex.Message,
                    success = false
                });
            }
        }
        [HttpGet]
        public JsonResult GetWeeklyReport()
        {
            try
            {
                List<WeeklyReport> weeklies = new List<WeeklyReport>();

                for (int i = 6; i >= 0; i--)
                {
                    var data = _issueRepo.GetAll(x => x.CreatedDate.DayOfYear == DateTime.Now.AddDays(-i).DayOfYear).Count();
                    weeklies.Add(new WeeklyReport()
                    {
                        date = DateTime.Now.AddDays(-i).ToShortDateString(),
                        count = data
                    });
                }

                return Json(new ResponseData()
                {
                    data = weeklies,
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Json(new ResponseData()
                {
                    message = ex.Message,
                    success = false
                });
            }
        }

        [HttpGet]
        public JsonResult GetDailyProfit()
        {
            try
            {
                var dailyIssues = _issueRepo.GetAll(x => x.CreatedDate.DayOfYear == DateTime.Now.DayOfYear && x.ClosedDate != null);

                decimal data = 0;
                foreach (var item in dailyIssues)
                {
                    data += item.ServiceCharge;
                }
                return Json(new ResponseData()
                {
                    data = data,
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Json(new ResponseData()
                {
                    message = ex.Message,
                    success = false
                });
            }
        }

        [HttpGet]
        public JsonResult GetSurveyReport()
        {
            try
            {
                var surveys = _surveyRepo;
                var count = surveys.GetAll().Count;
                var quest1 = surveys.GetAll().Select(x => x.Satisfaction).Sum() / count;
                var quest2 = surveys.GetAll().Select(x => x.TechPoint).Sum() / count;
                var quest3 = surveys.GetAll().Select(x => x.Speed).Sum() / count;
                var quest4 = surveys.GetAll().Select(x => x.Pricing).Sum() / count;
                var quest5 = surveys.GetAll().Select(x => x.Solving).Sum() / count;

                var data = new List<SurveyReport>();
                data.Add(new SurveyReport()
                {
                    question = "Genel Memnuniyet",
                    point = quest1
                });
                data.Add(new SurveyReport()
                {
                    question = "Teknisyen",
                    point = quest2
                });
                data.Add(new SurveyReport()
                {
                    question = "Hız",
                    point = quest3
                });
                data.Add(new SurveyReport()
                {
                    question = "Fiyat",
                    point = quest4
                });
                data.Add(new SurveyReport()
                {
                    question = "Çözüm Odaklılık",
                    point = quest5
                });

                return Json(new ResponseData()
                {
                    message = $"{data.Count} adet kayıt bulundu",
                    success = true,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return Json(new ResponseData()
                {
                    message = "Kayıt bulunamadı " + ex.Message,
                    success = false
                });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetTechReport()
        {
            try
            {
                var userManager = _membershipTools.UserManager;
                var users = userManager.Users.ToList();
                var data = new List<TechReport>();
                foreach (var user in users)
                {
                    if (await userManager.IsInRoleAsync(user, IdentityRoles.Technician.ToString()))
                    {
                        var techIssues = _issueRepo.GetAll(x => x.TechnicianId == user.Id);
                        foreach (var issue in techIssues)
                        {
                            if (issue.ClosedDate != null)
                            {
                                data.Add(new TechReport()
                                {
                                    nameSurname = await _membershipTools.GetNameSurname(user.Id),
                                    point = double.Parse(await _membershipTools.GetTechPoint(user.Id))
                                });
                            }
                        }
                    }
                }

                return Json(new ResponseData()
                {
                    success = true,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return Json(new ResponseData()
                {
                    message = $"{ex.Message}",
                    success = false
                });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetBestTech()
        {
            try
            {
                var userManager = _membershipTools.UserManager;
                var users = userManager.Users.ToList();
                var unclosed = _issueRepo.GetAll(x => x.ClosedDate != null);
                var minutes = unclosed.Min(x => x.ClosedDate?.Minute - x.CreatedDate.Minute);
                var data = new Issue();
                foreach (var user in users)
                {
                    if (await userManager.IsInRoleAsync(user, IdentityRoles.Technician.ToString()))
                    {
                        data = _issueRepo.GetAll(x => x.TechnicianId == user.Id && x.ClosedDate?.Minute - x.CreatedDate.Minute == minutes).FirstOrDefault();
                        if (data != null)
                            break;
                    }
                }

                return Json(new ResponseData()
                {
                    data = $"{await _membershipTools.GetNameSurname(data.TechnicianId)} ({minutes} dk)",
                    success = true,
                });
            }
            catch (Exception ex)
            {
                return Json(new ResponseData()
                {
                    message = $"{ex.Message}",
                    success = false
                });
            }
        }

    }
}