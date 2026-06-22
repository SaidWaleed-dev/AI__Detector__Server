using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Specifications;

namespace Domain.Interfaces;


public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> ListAllAsync();
    
    // Specification Pattern methods
    Task<T?> GetEntityWithSpecAsync(ISpecification<T> spec);
    Task<IEnumerable<T>> ListAsync(ISpecification<T> spec);
    Task<int> CountAsync(ISpecification<T> spec);
    
    // Commands
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
