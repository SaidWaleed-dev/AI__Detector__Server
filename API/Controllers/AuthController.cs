using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Text.Json;

namespace API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserRepository userRepository, 
        IOtpRepository otpRepository,
        ITokenService tokenService, 
        IEmailService emailService, 
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _tokenService = tokenService;
        _emailService = emailService;
        _configuration = configuration;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || 
            string.IsNullOrWhiteSpace(dto.Password) || 
            string.IsNullOrWhiteSpace(dto.FullName))
        {
            return BadRequest(new { message = "All fields are required" });
        }

        if (await _userRepository.EmailExistsAsync(dto.Email))
        {
            return Conflict(new { message = "Email already exists" });
        }

        if (await _otpRepository.IsRateLimitedAsync(dto.Email))
        {
            return BadRequest(new { message = "Please wait a minute before requesting another code." });
        }

        Random generator = new Random();
        var code = generator.Next(100000, 999999).ToString();

        var metadata = JsonSerializer.Serialize(new { 
            FullName = dto.FullName, 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password) 
        });

        var otpRecord = new OtpRecord
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            OtpCode = code,
            Type = OtpType.Registration,
            Metadata = metadata,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        await _otpRepository.AddAsync(otpRecord);
        Console.WriteLine($" OTP record saved for {dto.Email}, code: {code}");

        try {
            await _emailService.SendVerificationCodeAsync(dto.Email, dto.FullName, code);
            Console.WriteLine($" Email send completed for {dto.Email}");
        } catch (Exception ex) {
            Console.WriteLine($" Email send FAILED for {dto.Email}: {ex.Message}");
            return BadRequest(new { message = "Could not send verification email. Please ensure the email address is correct." });
        }

        return Ok(new { message = "Verification code sent to your email. It expires in 5 minutes." });
    }

    
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.OtpCode))
            return BadRequest(new { message = "Email and code are required" });

        var type = dto.Type.ToLower() == "registration" ? OtpType.Registration : OtpType.PasswordReset;
        var otp = await _otpRepository.GetLatestOtpAsync(dto.Email, type);

        if (otp == null || otp.OtpCode != dto.OtpCode || otp.IsExpired)
        {
            return BadRequest(new { message = "Invalid or expired verification code." });
        }

        if (type == OtpType.Registration)
        {
            if (string.IsNullOrEmpty(otp.Metadata))
            {
                return BadRequest(new { message = "Registration data lost. Please register again." });
            }

            var data = JsonSerializer.Deserialize<JsonElement>(otp.Metadata);
            var fullName = data.TryGetProperty("FullName", out var fn) ? fn.GetString() : "User";
            var passwordHash = data.TryGetProperty("PasswordHash", out var ph) ? ph.GetString() : null;

            if (string.IsNullOrEmpty(passwordHash))
            {
                return BadRequest(new { message = "Invalid registration metadata." });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName!,
                Email = otp.Email,
                PasswordHash = passwordHash!,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.AddAsync(user);
            
            otp.IsVerified = true;
            await _otpRepository.UpdateAsync(otp);

            // Auto login after verification
            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            return Ok(new AuthResponseDto
            {
                Token = _tokenService.CreateToken(user),
                RefreshToken = user.RefreshToken,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName
            });
        }
        else
        {
            otp.IsVerified = true;
            await _otpRepository.UpdateAsync(otp);
            return Ok(new { message = "Code verified successfully. You can now reset your password." });
        }
    }


    
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        var user = await _userRepository.GetByEmailAsync(dto.Email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = "Account is disabled" });
        }

        
        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        return Ok(new AuthResponseDto
        {
            Token = _tokenService.CreateToken(user),
            RefreshToken = user.RefreshToken,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName
        });
    }

    
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        if (user == null || user.RefreshToken != dto.RefreshToken || user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        
        var newAccessToken = _tokenService.CreateToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        return Ok(new AuthResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName
        });
    }

    
    [HttpPost("google")]
    public async Task<ActionResult<AuthResponseDto>> GoogleAuth([FromBody] GoogleAuthDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.IdToken))
            return BadRequest(new { message = "Google ID Token is required" });

        try 
        {
            // Verify Google Token securely with Client ID validation
            var googleSettings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { _configuration["Google:ClientId"] ?? "" }
            };
            
            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, googleSettings);
            
            var email = payload.Email;
            var fullName = payload.Name;
            var providerId = payload.Subject;

            var user = await _userRepository.GetByEmailAsync(email);
            
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = fullName,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Provider = "Google",
                    ProviderId = providerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                await _userRepository.AddAsync(user);
            }
            else 
            {
                if (!user.IsActive)
                    return Unauthorized(new { message = "Account is disabled" });

                // Update provider info if it was a standard user before
                if (string.IsNullOrEmpty(user.Provider))
                {
                    user.Provider = "Google";
                    user.ProviderId = providerId;
                    await _userRepository.UpdateAsync(user);
                }
            }

            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            return Ok(new AuthResponseDto
            {
                Token = _tokenService.CreateToken(user),
                RefreshToken = user.RefreshToken,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName
            });
        }
        catch (InvalidJwtException)
        {
            return Unauthorized(new { message = "Invalid Google token" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Google authentication failed: " + ex.Message });
        }
    }

    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "Email is required" });

        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return Ok(new { message = "If the email exists, a security code will be sent." });

        if (await _otpRepository.IsRateLimitedAsync(dto.Email))
        {
            return BadRequest(new { message = "Please wait a minute before requesting another code." });
        }

        Random generator = new Random();
        var code = generator.Next(100000, 999999).ToString();

        var otpRecord = new OtpRecord
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            OtpCode = code,
            Type = OtpType.PasswordReset,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        await _otpRepository.AddAsync(otpRecord);
        await _emailService.SendPasswordResetCodeAsync(user.Email, user.FullName, code);

        return Ok(new { message = "Security code sent successfully. It expires in 5 minutes." });
    }


    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "Email, code, and new password are required" });

        var otp = await _otpRepository.GetLatestOtpAsync(dto.Email, OtpType.PasswordReset, onlyUnverified: false);
        
        if (otp == null || otp.OtpCode != dto.Token || !otp.IsVerified || otp.IsExpired)
        {
            return BadRequest(new { message = "Invalid or unverified security code." });
        }

        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null) return NotFound(new { message = "User not found" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);

        // Delete the used OTP
        otp.ExpiresAt = DateTime.UtcNow.AddMinutes(-1); // Expire it
        await _otpRepository.UpdateAsync(otp);

        return Ok(new { message = "Password has been reset successfully" });
    }

    
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "Old password and new password are required" });

        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null) return NotFound(new { message = "User not found" });

        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "Incorrect old password" });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdateAsync(user);

        return Ok(new { message = "Password changed successfully" });
    }

    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto? dto)
    {
        if (dto != null && !string.IsNullOrEmpty(dto.Email))
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user != null)
            {
                if (user.Email.Contains("@temp.ai"))
                {
                    await _userRepository.DeleteAsync(user.Id);
                    Console.WriteLine($"Guest user {user.Id} and associated content deleted on logout.");
                }
                else
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiry = null;
                    await _userRepository.UpdateAsync(user);
                }
            }
        }
        return Ok(new { message = "Logout successful." });
    }

    
    [HttpPost("guest")]
    [AllowAnonymous]
    public async Task<IActionResult> ContinueAsGuest()
    {
        var userId = Guid.NewGuid();
        var guestUser = new User
        {
            Id = userId,
            FullName = "Guest User",
            Email = $"guest_{userId:N}@temp.ai",
            PasswordHash = string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await _userRepository.AddAsync(guestUser);

        return Ok(new AuthResponseDto
        {
            Token = _tokenService.CreateToken(guestUser, 120),
            RefreshToken = string.Empty,
            UserId = guestUser.Id,
            Email = guestUser.Email,
            FullName = guestUser.FullName,
            IsGuest = true
        });
    }

    
    [HttpGet("user/{id}")]
    [Authorize]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound(new { message = "User not found" });

        return Ok(new 
        { 
            userId = user.Id,
            email = user.Email,
            fullName = user.FullName,
            createdAt = user.CreatedAt,
            isActive = user.IsActive
        });
    }
}



