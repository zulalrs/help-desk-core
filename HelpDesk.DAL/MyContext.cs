using HelpDesk.Models.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelpDesk.DAL
{
    public class MyContext:IdentityDbContext<ApplicationUser,ApplicationRole,string>
    {
        public MyContext(DbContextOptions<MyContext> options)
            :base(options)
        {

        }
    }
}
