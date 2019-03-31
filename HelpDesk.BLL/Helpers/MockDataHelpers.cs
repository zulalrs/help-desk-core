using HelpDesk.BLL.Account;
using HelpDesk.Models.IdentityEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HelpDesk.BLL.Helpers
{
    public static class MockDataHelpers
    {
        private static readonly MembershipTools _membershipTools;

        public static async void AddMockUsersAsync()
        {
            List<ApplicationUser> Users = new List<ApplicationUser>();

            for (int i = 0; i < 3; i++)
            {
                var adm = new ApplicationUser()
                {
                    Email = $"admin{i + 1}@gmail.com",
                    UserName = $"admin{i + 1}",
                    Name = $"Admin{i + 1}",
                    Surname = $"Surname"
                    //Location = Models.Enums.Locations.Beşiktaş
                };
                Users.Add(adm);
                var opr = new ApplicationUser()
                {
                    Email = $"operator{i + 1}@gmail.com",
                    UserName = $"operator{i + 1}",
                    Name = $"Operator{i + 1}",
                    Surname = $"Surname"
                    //Location = Models.Enums.Locations.Esenler
                };
                Users.Add(opr);
                var tech = new ApplicationUser()
                {
                    Email = $"technician{i + 1}@gmail.com",
                    UserName = $"technician{i + 1}",
                    Name = $"Technician{i + 1}",
                    Surname = $"Surname"
                    //Location = Models.Enums.Locations.Kağıthane
                };
                Users.Add(tech);
                var cust = new ApplicationUser()
                {
                    Email = $"customer{i + 1}@gmail.com",
                    UserName = $"customer{i + 1}",
                    Name = $"Customer{i + 1}",
                    Surname = $"Surname"
                    //Location = Models.Enums.Locations.Kadıköy
                };
                Users.Add(cust);
            }

            foreach (var user in Users)
            {
                var newPassword = "Aa123456*";
                var result = await _membershipTools.UserManager.CreateAsync(user, newPassword);
                user.AvatarPath = "/assets/images/icon-noprofile.png";
                user.EmailConfirmed = true;
                user.RegisterDate = DateTime.Now;
                user.PhoneNumber = "123456789";
                user.PhoneNumberConfirmed = true;

                if (result.Succeeded)
                {
                    switch (_membershipTools.UserManager.Users.Count())
                    {
                        case 1:
                        case 5:
                        case 9:
                            await _membershipTools.UserManager.AddToRoleAsync(user, "Admin");
                            break;
                        case 2:
                        case 6:
                        case 10:
                            await _membershipTools.UserManager.AddToRoleAsync(user, "Operator");
                            break;
                        case 3:
                        case 7:
                        case 11:
                            await _membershipTools.UserManager.AddToRoleAsync(user, "Technician");
                            break;
                        default:
                            await _membershipTools.UserManager.AddToRoleAsync(user, "Customer");
                            break;
                    }
                }
            }
        }
    }

}
