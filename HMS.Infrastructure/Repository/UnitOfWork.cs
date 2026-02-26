using HMS.Core.Contracts;
using HMS.Core.Entities;
using HMS.Infrastructure.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Infrastructure.Repository
{
    public class UnitOfWork(HotelDbContext _dbContext) : IUnitOfWork
    {

        private Dictionary<Type, object> _repositories = [];
        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
        {
            //Get Type Name
            var type = typeof(TEntity);

            if (_repositories.TryGetValue(type, out var repository))
                return (IGenericRepository<TEntity, TKey>)repository;

            //create repo
            var newRepository = new GenericRepository<TEntity, TKey>(_dbContext);

            // Add to container
            _repositories[type] = newRepository;

            //return the Object
            return newRepository;
        }

        public async Task<int> SaveChangesAsync() => await _dbContext.SaveChangesAsync();
    }
}
