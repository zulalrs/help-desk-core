using HelpDesk.BLL.Repository.Abstracts;
using HelpDesk.DAL;
using HelpDesk.Models.Entities;

namespace HelpDesk.BLL.Repository
{
    public class IssueLogRepo : RepositoryBase<IssueLog, string>
    {
        private readonly MyContext _dbContext;
        public IssueLogRepo(MyContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
