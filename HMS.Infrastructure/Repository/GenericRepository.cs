using HMS.Core.Contracts;
using HMS.Core.Entities;
using HMS.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HMS.Infrastructure.Repository
{
    public class GenericRepository<TEntity, TKey>(HotelDbContext _dbContext) :
        IGenericRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        public async Task AddAsync(TEntity entity) => await _dbContext.Set<TEntity>().AddAsync(entity);
        public void Delete(TEntity entity) => _dbContext.Set<TEntity>().Remove(entity);
        public void Update(TEntity entity) => _dbContext.Set<TEntity>().Update(entity);


        public async Task<IEnumerable<TEntity>> GetAllAsync() => await _dbContext.Set<TEntity>().ToListAsync();

        public async Task<IEnumerable<TEntity>> GetAllAsync(
                Expression<Func<TEntity, bool>>? filter = null,
                Expression<Func<TEntity, object>>? orderByExp = null,
                Expression<Func<TEntity, object>>? orderByDescExp = null)
        {
            var query = _dbContext.Set<TEntity>().AsQueryable();

            if (filter != null)
                query = query.Where(filter);

            if (orderByExp != null)
                query = query.OrderBy(orderByExp);

            if (orderByDescExp != null)
                query = query.OrderByDescending(orderByDescExp);

            return await query.ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(TKey id) => await _dbContext.Set<TEntity>().FindAsync(id);

        public Task<TEntity?> GetByIdAsync(TKey id,
            Expression<Func<TEntity, bool>>? filter = null,
            List<Expression<Func<TEntity, object>>>? includes = null
        )
        {
            var query = _dbContext.Set<TEntity>().AsQueryable();

            if (filter is not null)
                query = query.Where(filter);

            if (includes is not null)
                foreach (var item in includes)
                {
                    query = query.Include(item);
                }

            return query.FirstOrDefaultAsync(E => E.Id!.Equals(id));
        }

    }
}
