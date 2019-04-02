using HelpDesk.BLL.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.Web.Controllers
{
    public class BaseController : Controller
    {
        private readonly MembershipTools _membershipTools;

        public BaseController(MembershipTools membershipTools)
        {

            _membershipTools = membershipTools;
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
    }
}