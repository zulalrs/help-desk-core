using AutoMapper;
using HelpDesk.BLL.Account;
using HelpDesk.BLL.Helpers;
using HelpDesk.BLL.Services.Senders;
using HelpDesk.Models.IdentityEntities;
using HelpDesk.Models.Models;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Web.Controllers
{
    public class AdminController : BaseController
    {
        private readonly MembershipTools _membershipTools;
        private readonly IHostingEnvironment _hostingEnvironment;

        public AdminController(MembershipTools membershipTools, IHostingEnvironment hostingEnvironment) :base(membershipTools)
        {
            _membershipTools = membershipTools;
            _hostingEnvironment = hostingEnvironment;
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

                var roles = userManager.GetRolesAsync(user).Result;
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
                await userManager.RemoveFromRoleAsync(user, roleManager.FindByNameAsync(identityUserRole).ToString());
            }

            for (int i = 0; i < seciliRoller.Length; i++)
            {
                await userManager.AddToRoleAsync(user, seciliRoller[i]);
            }

            return RedirectToAction("EditUser", new { id = userId });
        }
    }
}