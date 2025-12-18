using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Homework_portal.Data;
using Microsoft.EntityFrameworkCore;

namespace Homework_portal.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> DbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            DbSet = _db.Set<T>();
        }

        
        public void Add(T entity) => DbSet.Add(entity);
        public void AddRange(IEnumerable<T> entities) => DbSet.AddRange(entities);
        public void Remove(T entity) => DbSet.Remove(entity);
        public void RemoveRange(IEnumerable<T> entities) => DbSet.RemoveRange(entities);
        public void Update(T entity) => DbSet.Update(entity);

        public T? Get(
            Expression<Func<T, bool>> filter,
            string? includeProperties = null,
            bool tracked = false)
        {
            var query = BuildQuery(filter, includeProperties, tracked);
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool tracked = false)
        {
            var query = BuildQuery(filter, includeProperties, tracked);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return query.ToList();
        }

        // Async versions (recommended)
        public async Task<T?> GetAsync(
            Expression<Func<T, bool>> filter,
            string? includeProperties = null,
            bool tracked = false)
        {
            var query = BuildQuery(filter, includeProperties, tracked);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IList<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool tracked = false)
        {
            var query = BuildQuery(filter, includeProperties, tracked);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

        private IQueryable<T> BuildQuery(
            Expression<Func<T, bool>>? filter,
            string? includeProperties,
            bool tracked)
        {
            IQueryable<T> query = DbSet;

            if (!tracked)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                var includes = includeProperties
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (includes.Length > 1)
                {
                    query = query.AsSplitQuery();
                }

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query;
        }
    }
}