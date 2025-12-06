using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller للتعامل مع Authentication (تسجيل دخول وإنشاء حساب)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AuthController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// تسجيل مستخدم جديد
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // التحقق من البيانات
        if (string.IsNullOrWhiteSpace(dto.Email) || 
            string.IsNullOrWhiteSpace(dto.Password) || 
            string.IsNullOrWhiteSpace(dto.FullName))
        {
            return BadRequest(new { message = "All fields are required" });
        }

        // التحقق من عدم وجود البريد الإلكتروني مسبقاً
        if (await _userRepository.EmailExistsAsync(dto.Email))
        {
            return Conflict(new { message = "Email already exists" });
        }

        // تشفير كلمة المرور
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // إنشاء المستخدم
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // حفظ في قاعدة البيانات
        await _userRepository.AddAsync(user);

        return Ok(new 
        { 
            message = "User registered successfully",
            userId = user.Id,
            email = user.Email,
            fullName = user.FullName
        });
    }

    /// <summary>
    /// تسجيل الدخول
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // التحقق من البيانات
        if (string.IsNullOrWhiteSpace(dto.Email) || 
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        // البحث عن المستخدم
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // التحقق من كلمة المرور
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // التحقق من أن الحساب نشط
        if (!user.IsActive)
        {
            return Unauthorized(new { message = "Account is disabled" });
        }

        return Ok(new 
        { 
            message = "Login successful",
            userId = user.Id,
            email = user.Email,
            fullName = user.FullName
        });
    }

    /// <summary>
    /// الحصول على معلومات المستخدم
    /// </summary>
    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

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