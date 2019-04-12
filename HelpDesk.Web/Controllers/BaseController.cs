using HelpDesk.BLL.Account;
using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Web.Controllers
{
    public class BaseController : Controller
    {
        private readonly MembershipTools _membershipTools;
        private readonly IRepository<Issue, string> _issueRepo;

        public BaseController(MembershipTools membershipTools)
        {
            _membershipTools = membershipTools;
        }

        public BaseController(MembershipTools membershipTools, IRepository<Issue, string> issueRepo)
        {
            _membershipTools = membershipTools;
            _issueRepo = issueRepo;
        }

        protected List<SelectListItem> GetRoleList()
        {
            var data = new List<SelectListItem>();
            _membershipTools.NewRoleStore().Roles
                .ToList()
                .ForEach(x =>
                {
                    data.Add(new SelectListItem()
                    {
                        Text = $"{x.Name}",
                        Value = x.Id
                    });
                });
            return data;
        }

        protected async Task<List<SelectListItem>> GetTechnicianList()
        {
            var data = new List<SelectListItem>();
            var userManager = _membershipTools.UserManager;

            var users = userManager.Users.ToList();

            var techIds = _issueRepo.GetAll(x => x.IssueState == IssueStates.İşlemde || x.IssueState == IssueStates.Atandı).Select(x => x.TechnicianId).ToList();

            foreach (var user in users)
            {
                if (await userManager.IsInRoleAsync(user, IdentityRoles.Technician.ToString()))
                {
                    if (!techIds.Contains(user.Id))
                    {
                        data.Add(new SelectListItem()
                        {
                            Text = $"{user.Name} {user.Surname} ({await _membershipTools.GetTechPoint(user.Id)})",
                            Value = user.Id
                        });
                    }
                }
            }
            return data;
        }
    }
}