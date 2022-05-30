using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class RepositoryAsync<T> : IRepositoryAsync<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbset;
        public RepositoryAsync(ApplicationDbContext db)
        {

            _db = db;
            this.dbset = _db.Set<T>();


        }
        public async Task AddAsync(T entity)
        {
            await dbset.AddAsync(entity);
        }

        public async Task<T> GetAsync(int id)
        {
            return await dbset.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> Filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null)
        {
            IQueryable<T> query = dbset;
            if(Filter!=null)
            {
                query = query.Where(Filter);
            }
            if(includeProperties!=null)
            {
                foreach(var includeprop in includeProperties.Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeprop);
                }
            }
            if(orderBy!=null)
            {
                return await orderBy(query).ToListAsync();
            }
            return await query.ToListAsync();

        }

        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> Filter = null, string includeProperties = null)
        {
            IQueryable<T> query = dbset;
            if (Filter != null)
            {
                query = query.Where(Filter);
            }
            if (includeProperties != null)
            {
                foreach (var includeprop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeprop);
                }
            }

            return await query.FirstOrDefaultAsync();

        }

        public async Task RemoveAsync(int id)
        {
            T entity = await dbset.FindAsync(id);
           await RemoveAsync(entity);
            
        }

        public async Task RemoveAsync(T entity)
        {
             dbset.Remove(entity);
        }

        public async Task RemoveRangeAsync(IEnumerable<T> entity)
        {
            dbset.RemoveRange(entity);
        }

        
    }
}
