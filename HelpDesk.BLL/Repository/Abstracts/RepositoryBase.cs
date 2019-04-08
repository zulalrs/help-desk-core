using HelpDesk.DAL;
using HelpDesk.Models.Abstracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.BLL.Repository.Abstracts
{
    public abstract class RepositoryBase<T, TId> : IRepository<T, TId> where T : BaseEntity<TId>
    {
        internal readonly MyContext DbContext;
        internal readonly DbSet<T> DbObject;

        internal RepositoryBase(MyContext dbContext)
        {
            DbContext = dbContext;
            DbObject = DbContext.Set<T>();
        }
        public List<T> GetAll()
        {
            return DbObject.ToList();
        }

        public List<T> GetAll(Func<T, bool> predicate)
        {
            return DbObject.Where(predicate).AsQueryable().ToList();
        }

        public T GetById(TId id)
        {
            return DbObject.Find(id);
        }

        public virtual void Insert(T entity)
        {
            DbObject.Add(entity);
            this.Save();
        }

        public virtual void Delete(T entity)
        {
            DbObject.Remove(entity);
            this.Save();
        }

        public virtual int Update(T entity)
        {
            DbObject.Attach(entity);
            DbContext.Entry(entity).State = EntityState.Modified;
            //entity.UpdatedDate = DateTime.Now;
            return Save();
        }

        public int Save()
        {
           return DbContext.SaveChanges();
        }
    }
}
