using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.BLL.Account;
using HelpDesk.Models.Enums;
using HelpDesk.Models.IdentityEntities;
using HelpDesk.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static HelpDesk.BLL.Account.MembershipTools;

namespace HelpDesk.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signinManager;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signinManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signinManager = signinManager;

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

        public async Task<IActionResult> Index()
        {
            //var userManager = NewUserManager();
            //var userId = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
            //var user = userManager.FindById(userId);
            //if (user == null)
            //    RedirectToAction("Error500", "Home");
            //var data = Mapper.Map<User, UserProfileVM>(user);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var data = AutoMapper.Mapper.Map<ApplicationUser, UserProfileVM>(user);
            return View(data);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                //var user= await _userManager.GetUserAsync(HttpContext.User);
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

    }
}