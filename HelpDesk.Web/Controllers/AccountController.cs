using HelpDesk.BLL.Helpers;
using HelpDesk.BLL.Services.Senders;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HelpDesk.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signinManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signinManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signinManager = signinManager;
            _httpContextAccessor = httpContextAccessor;

            var roleNames = Enum.GetNames(typeof(IdentityRoles));
            foreach (var roleName in roleNames)
            {
                if (!_roleManager.RoleExistsAsync(roleName).Result)
                {
                    var role = new ApplicationRole()
                    {
                        Name = roleName
                    };

                    var task = _roleManager.CreateAsync(role).Result;
                    Task.Run(() => task);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index(LoginVM model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                RedirectToAction("Index", "Home");
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

                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    ModelState.AddModelError("UserName", "Bu kullanıcı adı daha önceden alınmıştır");
                    return View("Register", model);
                }

                var newUser = new ApplicationUser()
                {
                    //AvatarPath = "/assets/images/icon-noprofile.png",
                    EmailConfirmed = false,
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    UserName = model.UserName,
                    //Location = model.Location == Models.Enums.Locations.KonumYok ? Models.Enums.Locations.Beşiktaş : model.Location,
                };
                newUser.ActivationCode = StringHelpers.GetCode();

                var result = await _userManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    switch (_userManager.Users.Count())
                    {
                        case 1:
                            await _userManager.AddToRoleAsync(newUser, "Admin");
                            break;
                        case 2:
                            await _userManager.AddToRoleAsync(newUser, "Operator");
                            break;
                        case 3:
                            await _userManager.AddToRoleAsync(newUser, "Technician");
                            break;
                        default:
                            await _userManager.AddToRoleAsync(newUser, "Customer");
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

                var result = await _signinManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);

                if (result.Succeeded)
                    return RedirectToAction("Index", "Account", model);

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
                var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
                var user = await _userManager.FindByIdAsync(id);

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

                var result = await _userManager.ChangePasswordAsync(
                    await _userManager.FindByIdAsync(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value),
                    model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    var emailService = new EmailService();
                    var body = $"Merhaba <b>{user.Name} {user.Surname}</b><br>Hesabınızın şifresi değiştirilmiştir. <br> Bilginiz dahilinde olmayan değişiklikler için hesabınızı güvence altına almanızı öneririz.</p>";
                    emailService.Send(new MailModel() { Body = body, Subject = "Şifre Değiştirme hk." }, user.Email);

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
    }
}