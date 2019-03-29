using HelpDesk.BLL.Account;
using HelpDesk.BLL.Helpers;
using HelpDesk.BLL.Services.Senders;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HelpDesk.DAL;
using EmailService = HelpDesk.BLL.Services.Senders.EmailService;

namespace HelpDesk.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly MembershipTools _membershipTools;
        private readonly MyContext _dbContext;

        public AccountController(MembershipTools membershipTools, IHostingEnvironment hostingEnvironment, MyContext dbContext)
        {
            _membershipTools = membershipTools;
            _hostingEnvironment = hostingEnvironment;
            _dbContext = dbContext;

            var roleNames = Enum.GetNames(typeof(IdentityRoles));
            foreach (var roleName in roleNames)
            {
                if (!_membershipTools.RoleManager.RoleExistsAsync(roleName).Result)
                {
                    var role = new ApplicationRole()
                    {
                        Name = roleName
                    };

                    var task = _membershipTools.RoleManager.CreateAsync(role).Result;
                    Task.Run(() => task);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _membershipTools.UserManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return RedirectToAction("Index", "Home");
            var data = AutoMapper.Mapper.Map<ApplicationUser, UserProfileVM>(user);
            return View(data);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }
            try
            {

                var user = await _membershipTools.UserManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    ModelState.AddModelError("UserName", "Bu kullanıcı adı daha önceden alınmıştır");
                    return View("Register", model);
                }

                var newUser = new ApplicationUser()
                {
                    AvatarPath = "/assets/images/icon-noprofile.png",
                    EmailConfirmed = false,
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    UserName = model.UserName,
                    //Location = model.Location == Models.Enums.Locations.KonumYok ? Models.Enums.Locations.Beşiktaş : model.Location,
                };
                newUser.ActivationCode = StringHelpers.GetCode();

                var result = await _membershipTools.UserManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    switch (_membershipTools.UserManager.Users.Count())
                    {
                        case 1:
                            await _membershipTools.UserManager.AddToRoleAsync(newUser, "Admin");
                            break;
                        case 2:
                            await _membershipTools.UserManager.AddToRoleAsync(newUser, "Operator");
                            break;
                        case 3:
                            await _membershipTools.UserManager.AddToRoleAsync(newUser, "Technician");
                            break;
                        default:
                            await _membershipTools.UserManager.AddToRoleAsync(newUser, "Customer");
                            break;
                    }

                    //string SiteUrl = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host +
                    //                 (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);

                    //var emailService = new EmailService();
                    //var body = $"Merhaba <b>{newUser.Name} {newUser.Surname}</b><br>Hesabınızı aktif etmek için aşağıdaki linke tıklayınız<br> <a href='{SiteUrl}/account/activation?code={newUser.ActivationCode}' >Aktivasyon Linki </a> ";
                    //await emailService.SendAsync(new MailModel() { Body = body, Subject = "Sitemize Hoşgeldiniz" }, newUser.Email);
                }
                else
                {
                    var err = "";
                    foreach (var resultError in result.Errors)
                    {
                        err += resultError.Description;
                    }
                    ModelState.AddModelError(String.Empty, err);
                    return View("Register", model);
                }

                TempData["Message1"] = "Kaydınız alınmıştır. Lütfen giriş yapınız";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Message"] = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Register",
                    ControllerName = "Account",
                    ErrorCode = 500
                };
                return RedirectToAction("Error500", "Home");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                //var user = await _userManager.GetUserAsync(HttpContext.User);
                // user.Id;
                return RedirectToAction("Index", "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {

                    return View("Login", model);
                }

                var result = await _membershipTools.SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);

                if (result.Succeeded)
                    return RedirectToAction("Index", "Account");

                ModelState.AddModelError(String.Empty, "Kullanıcı adı veya şifre hatalı");
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Message"] = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "Index",
                    ControllerName = "Account",
                    ErrorCode = 500
                };
                return RedirectToAction("Error500", "Home");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Logout()
        {
            await _membershipTools.SignInManager.SignOutAsync();
            //return RedirectToAction("Index", "Home");
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<ActionResult> UserProfile()
        {
            try
            {
                var user = await _membershipTools.UserManager.GetUserAsync(HttpContext.User);
                var data = new UserProfileVM()
                {
                    Email = user.Email,
                    Id = user.Id,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Surname = user.Surname,
                    UserName = user.UserName,
                    AvatarPath = string.IsNullOrEmpty(user.AvatarPath) ? "/assets/images/icon-noprofile.png" : user.AvatarPath,
                    //Location = user.Location
                };

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Message"] = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "UserProfile",
                    ControllerName = "Account",
                    ErrorCode = 500
                };
                return RedirectToAction("Error500", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserProfile(UserProfileVM model)
        {
            var user = await _membershipTools.UserManager.FindByIdAsync(model.Id);

            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }


            //if (model.PostedFile != null &&
            //       model.PostedFile.Length > 0)
            //{
            //    var file = model.PostedFile;
            //    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            //    string extName = Path.GetExtension(file.FileName);
            //    fileName = StringHelpers.UrlFormatConverter(fileName);
            //    fileName += StringHelpers.GetCode();
            //    var directorypath = Server.MapPath("~/Upload/");
            //    var filepath = Server.MapPath("~/Upload/") + fileName + extName;

            //    if (!Directory.Exists(directorypath))
            //    {
            //        Directory.CreateDirectory(directorypath);
            //    }

            //    file.SaveAs(filepath);

            //    WebImage img = new WebImage(filepath);
            //    img.Resize(250, 250, false);
            //    img.AddTextWatermark("TeknikServis");
            //    img.Save(filepath);
            //    var oldPath = user.AvatarPath;
            //    if (oldPath != "/assets/images/icon-noprofile.png")
            //    {
            //        System.IO.File.Delete(Server.MapPath(oldPath));
            //    }
            //    user.AvatarPath = "/Upload/" + fileName + extName;
            //}

            try
            {
                user.Name = model.Name;
                user.Surname = model.Surname;
                user.PhoneNumber = model.PhoneNumber;
                //user.Location = model.Location;
                if (user.Email != model.Email)
                {
                    //todo tekrar aktivasyon maili gönderilmeli. rolü de aktif olmamış role çevrilmeli.
                }
                user.Email = model.Email;

                await _membershipTools.UserManager.UpdateAsync(user);
                TempData["Message"] = "Güncelleme işlemi başarılı.";
                return RedirectToAction("UserProfile");
            }
            catch (Exception ex)
            {
                TempData["Message"] = new ErrorVM()
                {
                    Text = $"Bir hata oluştu: {ex.Message}",
                    ActionName = "UserProfile",
                    ControllerName = "Account",
                    ErrorCode = 500
                };
                return RedirectToAction("Error500", "Home");
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ChangePasswordVM model)
        {
            try
            {
                var user = await _membershipTools.UserManager.GetUserAsync(HttpContext.User);

                //var id = _membershipTools.IHttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                //var user = await _membershipTools.UserManager.FindByIdAsync(id);

                var data = new ChangePasswordVM()
                {
                    OldPassword = model.OldPassword,
                    NewPassword = model.NewPassword,
                    ConfirmNewPassword = model.ConfirmNewPassword
                };

                model = data;
                if (!ModelState.IsValid)
                {
                    return RedirectToAction("Index", "Home");
                }

                var result = await _membershipTools.UserManager.ChangePasswordAsync(await _membershipTools.UserManager.GetUserAsync(HttpContext.User),
                    model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    var emailService = new EmailService();
                    var body = $"Merhaba <b>{user.Name} {user.Surname}</b><br>Hesabınızın şifresi değiştirilmiştir. <br> Bilginiz dahilinde olmayan değişiklikler için hesabınızı güvence altına almanızı öneririz.</p>";
                    emailService.Send(new EmailModel() { Body = body, Subject = "Şifre Değiştirme hk." }, user.Email);

                    return RedirectToAction("Logout", "Account");
                }
                else
                {
                    var err = "";
                    foreach (var resultError in result.Errors)
                    {
                        err += resultError + " ";
                    }
                    ModelState.AddModelError("", err);
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = new ErrorVM()
                {
                    Text = $"Bir hata oluştu {ex.Message}",
                    ActionName = "ChangePassword",
                    ControllerName = "Account",
                    ErrorCode = 500
                };
                return RedirectToAction("Error500", "Home");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> RecoverPassword(RecoverPasswordVM model)
        {
            //try
            //{
            //    var user =await _membershipTools.UserManager.FindByEmailAsync(model.Email);
            //    var userManager = NewUserManager();

            //    if (user == null)
            //    {
            //        ModelState.AddModelError(string.Empty, $"{model.Email} mail adresine kayıtlı bir üyeliğe erişilemedi");
            //        return View(model);
            //    }

            //    var newPassword = StringHelpers.GetCode().Substring(0, 6);
            //    await _membershipTools.UserManager.ResetPasswordAsync(user,_membershipTools.UserManager.GeneratePasswordResetTokenAsync(user),
            //        _membershipTools.UserManager.PasswordHasher.HashPassword(user, newPassword));

            //    await _membershipTools.UserManager.RemovePasswordAsync(user);
            //    await _membershipTools.UserManager.AddPasswordAsync(
            //        _membershipTools.UserManager.PasswordHasher.HashPassword(newPassword));

            //    //await userStore.SetPasswordHashAsync(user, userManager.PasswordHasher.HashPassword(newPassword));
            //    var result = _membershipTools.DbContext.SaveChanges();
            //    if (result == 0)
            //    {
            //        TempData["Message"] = new ErrorVM()
            //        {
            //            Text = $"Bir hata oluştu",
            //            ActionName = "RecoverPassword",
            //            ControllerName = "Account",
            //            ErrorCode = 500
            //        };
            //        return RedirectToAction("Error500", "Home");
            //    }

            //    var emailService = new EmailService();
            //    var body = $"Merhaba <b>{user.Name} {user.Surname}</b><br>Hesabınızın parolası sıfırlanmıştır<br> Yeni parolanız: <b>{newPassword}</b> <p>Yukarıdaki parolayı kullanarak sitemize giriş yapabilirsiniz.</p>";
            //    emailService.Send(new EmailModel() { Body = body, Subject = $"{user.UserName} Şifre Kurtarma" }, user.Email);
            //}
            //catch (Exception ex)
            //{
            //    TempData["Message"] = new ErrorVM()
            //    {
            //        Text = $"Bir hata oluştu {ex.Message}",
            //        ActionName = "RecoverPassword",
            //        ControllerName = "Account",
            //        ErrorCode = 500
            //    };
            //    return RedirectToAction("Error500", "Home");
            //}
            //TempData["Message"] = $"{model.Email} mail adresine yeni şifre gönderildi.";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Activation(string code)
        {
            try
            {
                var user = _membershipTools.UserManager.Users.FirstOrDefault(x => x.ActivationCode == code);

                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        ViewBag.Message = $"<span class='alert alert-success'>Bu hesap daha önce aktive edilmiştir.</span>";
                    }
                    else
                    {
                        user.EmailConfirmed = true;
                        _dbContext.SaveChanges();
                        ViewBag.Message = $"<span class='alert alert-success'>Aktivasyon işleminiz başarılı</span>";
                    }
                }
                else
                {
                    ViewBag.Message = $"<span class='alert alert-danger'>Aktivasyon başarısız</span>";
                }
            }
            catch (Exception)
            {
                ViewBag.Message = "<span class='alert alert-danger'>Aktivasyon işleminde bir hata oluştu</span>";
            }

            return View();
        }
    }
}