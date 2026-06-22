using Domain.Entities;

namespace Application.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
    string CreateToken(User user, int expiryMinutes);  // للـ guest أو custom expiry
    string GenerateRefreshToken();
}
