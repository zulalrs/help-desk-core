using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;

namespace HelpDesk.BLL.Repository
{
    public class IssueRepo : RepositoryBase<Issue, string>
    {
        private readonly MyContext _dbContext;
        public IssueRepo(MyContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
