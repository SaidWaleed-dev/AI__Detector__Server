using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
        var secretKey = _config["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
            throw new Exception("JWT SecretKey is missing in appsettings.json");
            
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    public string CreateToken(User user)
    {
        var defaultExpiry = double.Parse(_config["JwtSettings:ExpirationMinutes"] ?? "1440");
        return BuildToken(user, (int)defaultExpiry);
    }

    public string CreateToken(User user, int expiryMinutes)
    {
        return BuildToken(user, expiryMinutes);
    }

    private string BuildToken(User user, int expiryMinutes)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.FullName),
            new Claim(ClaimTypes.Role, user.Email.Contains("@temp.ai") ? "Guest" : "User")
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            SigningCredentials = creds,
            Issuer = _config["JwtSettings:Issuer"],
            Audience = _config["JwtSettings:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
