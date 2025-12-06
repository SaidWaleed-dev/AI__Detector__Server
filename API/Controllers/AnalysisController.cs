using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller للتعامل مع تحليل المحتوى
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly IAnalysisRepository _analysisRepository;
    private readonly IUserRepository _userRepository;

    public AnalysisController(
        IAnalysisRepository analysisRepository,
        IUserRepository userRepository)
    {
        _analysisRepository = analysisRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// تحليل محتوى جديد
    /// </summary>
    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze(
        [FromBody] AnalysisRequestDto dto,
        [FromQuery] Guid userId)
    {
        // التحقق من وجود المستخدم
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // هنا هنضيف لوجيك الـ AI Detection لاحقاً
        // دلوقتي هنعمل mock result
        var random = new Random();
        var aiProbability = random.NextDouble(); // رقم عشوائي من 0 إلى 1

        // إنشاء التحليل
        var analysis = new Analysis
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ContentType = dto.ContentType,
            Content = dto.Content,
            AiProbability = aiProbability,
            IsAiGenerated = aiProbability > 0.5,
            Details = $"Mock analysis for {dto.ContentType}",
            AnalyzedAt = DateTime.UtcNow,
            Status = AnalysisStatus.Completed
        };

        // حفظ في قاعدة البيانات
        await _analysisRepository.AddAsync(analysis);

        // إرجاع النتيجة
        var response = new AnalysisResponseDto
        {
            Id = analysis.Id,
            ContentType = analysis.ContentType,
            AiProbability = analysis.AiProbability,
            IsAiGenerated = analysis.IsAiGenerated,
            Details = analysis.Details,
            AnalyzedAt = analysis.AnalyzedAt,
            Status = analysis.Status
        };

        return Ok(response);
    }

    /// <summary>
    /// الحصول على تاريخ التحليلات لمستخدم معين
    /// </summary>
    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetHistory(Guid userId)
    {
        var analyses = await _analysisRepository.GetByUserIdAsync(userId);
        
        var response = analyses.Select(a => new AnalysisResponseDto
        {
            Id = a.Id,
            ContentType = a.ContentType,
            AiProbability = a.AiProbability,
            IsAiGenerated = a.IsAiGenerated,
            Details = a.Details,
            AnalyzedAt = a.AnalyzedAt,
            Status = a.Status
        });

        return Ok(response);
    }

    /// <summary>
    /// الحصول على تحليل محدد
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnalysis(Guid id)
    {
        var analysis = await _analysisRepository.GetByIdAsync(id);
        
        if (analysis == null)
        {
            return NotFound(new { message = "Analysis not found" });
        }

        var response = new AnalysisResponseDto
        {
            Id = analysis.Id,
            ContentType = analysis.ContentType,
            AiProbability = analysis.AiProbability,
            IsAiGenerated = analysis.IsAiGenerated,
            Details = analysis.Details,
            AnalyzedAt = analysis.AnalyzedAt,
            Status = analysis.Status
        };

        return Ok(response);
    }
}