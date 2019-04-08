using HelpDesk.Models.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.BLL.Repository.Abstracts
{
    public interface IRepository<T, TId> where T : BaseEntity<TId>
    {
        List<T> GetAll();
        List<T> GetAll(Func<T, bool> predicate);
        T GetById(TId id);
        void Insert(T entity);
        void Delete(T entity);
        int Update(T entity);
        int Save();
    }
}
