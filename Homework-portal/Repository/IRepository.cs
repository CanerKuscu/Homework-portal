using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Homework_portal.Repository
{
    public interface IRepository<T> where T : class
    {
        T? Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null,
                              Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, bool tracked = false);
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);
        Task<IList<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null,
                                   Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, bool tracked = false);
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void Update(T entity);
    }
}