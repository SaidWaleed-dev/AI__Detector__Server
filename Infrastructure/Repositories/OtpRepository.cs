using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OtpRepository : GenericRepository<OtpRecord>, IOtpRepository
{
    public OtpRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task AddAsync(OtpRecord otpRecord)
    {
        await base.AddAsync(otpRecord);
    }

    public async Task<OtpRecord?> GetLatestOtpAsync(string email, OtpType type, bool onlyUnverified = true)
    {
        // ================= without spec =================
        // var query = _context.OtpRecords
        //     .Where(o => o.Email == email && o.Type == type);
        //     
        // if (onlyUnverified)
        // {
        //     query = query.Where(o => !o.IsVerified);
        // }
        // 
        // return await query
        //     .OrderByDescending(o => o.CreatedAt)
        //     .FirstOrDefaultAsync();
        // ================================================

        // ================= with spec ====================
        var spec = new OtpRecordSpecification(email, type, onlyUnverified);
        return await GetEntityWithSpecAsync(spec);
        // ================================================
    }

    public async Task UpdateAsync(OtpRecord otpRecord)
    {
        _context.OtpRecords.Update(otpRecord);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredOtpsAsync()
    {
        var expired = await _context.OtpRecords
            .Where(o => o.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();
            
        if (expired.Any())
        {
            _context.OtpRecords.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsRateLimitedAsync(string email)
    {
        // Prevent sending OTP more than once per minute
        
        // ================= without spec =================
        // var lastOtp = await _context.OtpRecords
        //     .Where(o => o.Email == email)
        //     .OrderByDescending(o => o.CreatedAt)
        //     .FirstOrDefaultAsync();
        // ================================================
            
        // ================= with spec ====================
        var spec = new OtpRecordSpecification(email);
        var lastOtp = await GetEntityWithSpecAsync(spec);
        // ================================================
            
        if (lastOtp == null) return false;
        
        return (DateTime.UtcNow - lastOtp.CreatedAt).TotalSeconds < 60;
    }
}
