using Domain.Entities;

namespace Application.Interfaces;

public interface IOtpRepository
{
    Task AddAsync(OtpRecord otpRecord);
    Task<OtpRecord?> GetLatestOtpAsync(string email, OtpType type, bool onlyUnverified = true);
    Task UpdateAsync(OtpRecord otpRecord);
    Task DeleteExpiredOtpsAsync();
    Task<bool> IsRateLimitedAsync(string email);
}
