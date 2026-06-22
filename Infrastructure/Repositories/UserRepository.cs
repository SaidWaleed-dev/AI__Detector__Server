using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;


public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    
    public async Task<User?> GetByIdAsync(Guid id)
    {
        // ================= without spec =================
        // return await _context.Users
        //     .Include(u => u.Contents)
        //     .FirstOrDefaultAsync(u => u.Id == id);
        // ================================================

        // ================= with spec ====================
        var spec = new UserWithContentsSpecification(id);
        return await GetEntityWithSpecAsync(spec);
        // ================================================
    }

    
    public async Task<User?> GetByEmailAsync(string email)
    {
        // ================= without spec =================
        // return await _context.Users
        //     .FirstOrDefaultAsync(u => u.Email == email);
        // ================================================

        // ================= with spec ====================
        var spec = new UserWithContentsSpecification(email);
        return await GetEntityWithSpecAsync(spec);
        // ================================================
    }

    
    public async Task<User> AddAsync(User user)
    {
        return await base.AddAsync(user);
    }

    
    public async Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    
    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            DeleteUserPhysicalFiles(user);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task DeleteExpiredGuestsAsync(DateTime expiryTime)
    {
        var expiredGuests = await _context.Users
            .Include(u => u.Contents)
            .Where(u => u.Email.Contains("@temp.ai") && u.CreatedAt < expiryTime)
            .ToListAsync();

        if (expiredGuests.Any())
        {
            foreach (var guest in expiredGuests)
            {
                DeleteUserPhysicalFiles(guest);
            }
            _context.Users.RemoveRange(expiredGuests);
            await _context.SaveChangesAsync();
        }
    }

    private void DeleteUserPhysicalFiles(User user)
    {
        try
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "API", "wwwroot", "uploads");
            }

            foreach (var content in user.Contents)
            {
                if (content.Type != ContentType.Text && !string.IsNullOrEmpty(content.Data))
                {
                    var urlPart = content.Data.Split("|hash:")[0];
                    if (Uri.TryCreate(urlPart, UriKind.Absolute, out var uri))
                    {
                        var fileName = Path.GetFileName(uri.AbsolutePath);
                        var filePath = Path.Combine(uploadsPath, fileName);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting guest physical files: {ex.Message}");
        }
    }
}