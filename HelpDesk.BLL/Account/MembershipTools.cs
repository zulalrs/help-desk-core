using HelpDesk.BLL.Repository;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.Models.IdentityEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HelpDesk.DAL;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace HelpDesk.BLL.Account
{
    public class MembershipTools
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly MyContext _db;

        public MembershipTools(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor, MyContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }
        
        public UserManager<ApplicationUser> UserManager
        {
            get { return _userManager; }
        }
        public RoleManager<ApplicationRole> RoleManager
        {
            get { return _roleManager; }
        }
        public SignInManager<ApplicationUser> SignInManager
        {
            get { return _signInManager; }
        }
        public IHttpContextAccessor IHttpContextAccessor
        {
            get { return _httpContextAccessor; }
        }
        public UserStore<ApplicationUser> NewUserStore() => new UserStore<ApplicationUser>(_db ?? new MyContext());

        public async Task<string> GetRole(string userId)
        {
            ApplicationUser user;
            string role = "";
            if (string.IsNullOrEmpty(userId))
            {
                var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(id))
                    return "";

                user = await UserManager.FindByIdAsync(id);

            }
            else
            {
                user = UserManager.FindByIdAsync(userId).Result;
                var roles = UserManager.GetRolesAsync(user).Result;
                ApplicationRole roleuser;
                foreach (var item in roles)
                {
                    roleuser = RoleManager.FindByNameAsync(item).Result;
                    role = roleuser.ToString();
                }
            }

            return $"{role}";
        }
        public async Task<string> GetNameSurname(string userId)
        {
            ApplicationUser user;
            if (string.IsNullOrEmpty(userId))
            {
                var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(id))
                {
                    return "";
                }

                user = await UserManager.FindByIdAsync(id);
            }
            else
            {
                user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }
            }

            return $"{user.Name} {user.Surname}";
        }

        public async Task<string> GetNameSurnameCurrent()
        {
            ApplicationUser user;
            var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            user = await _userManager.FindByIdAsync(id);
            return $"{user.Name} {user.Surname}";
        }

        public async Task<string> GetEmailCurrent(string userId)
        {
            ApplicationUser user;
            if (string.IsNullOrEmpty(userId))
            {
                var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(id))
                {
                    return "";
                }

                user = await UserManager.FindByIdAsync(id);
            }
            else
            {
                user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }
            }
            return $"{user.Email}";
        }

        public int GetIssueCount()
        {
            var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return new IssueRepo().GetAll(x => x.CustomerId == id).Count;
        }

        public async Task<string> GetAvatarPath(string userId)
        {
            ApplicationUser user;
            if (string.IsNullOrEmpty(userId))
            {
                var id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(id))
                {
                    return "~/assets/images/icon-noprofile.png";
                }

                user = await _userManager.FindByIdAsync(id);
            }
            else
            {
                user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return "~/assets/images/icon-noprofile.png";
                }
            }

            return $"{user.AvatarPath}";
        }

        //public static string GetTechPoint(string techId)
        //{
        //    var tech = NewUserManager().FindById(techId);
        //    if (tech == null)
        //        return "0";
        //    var issues = new IssueRepo().GetAll(x => x.TechnicianId == techId);
        //    if (issues == null)
        //        return "0";
        //    var isDoneIssues = new List<Issue>();
        //    foreach (var issue in issues)
        //    {
        //        var survey = new SurveyRepo().GetById(issue.SurveyId);
        //        if (survey.IsDone)
        //            isDoneIssues.Add(issue);
        //    }

        //    var count = 0.0;
        //    foreach (var item in isDoneIssues)
        //    {
        //        var survey = new SurveyRepo().GetById(item.SurveyId);
        //        count += survey.TechPoint;
        //    }

        //    return isDoneIssues.Count != 0 ? $"{count / isDoneIssues.Count}" : "0";
        //}
    }
}