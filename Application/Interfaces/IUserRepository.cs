using Domain.Entities;

namespace Application.Interfaces;


public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    
    Task<User?> GetByEmailAsync(string email);
    
    
    Task<User> AddAsync(User user);
    
    
    Task UpdateAsync(User user);
    
    Task DeleteAsync(Guid id);
    
    Task<bool> EmailExistsAsync(string email);

    Task DeleteExpiredGuestsAsync(DateTime expiryTime);
}