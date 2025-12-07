using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
    private readonly IFileService _fileService;
    private readonly IAiDetectionService _aiDetectionService;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        IAnalysisRepository analysisRepository,
        IUserRepository userRepository,
        IFileService fileService,
        IAiDetectionService aiDetectionService,
        ILogger<AnalysisController> logger)
    {
        _analysisRepository = analysisRepository;
        _userRepository = userRepository;
        _fileService = fileService;
        _aiDetectionService = aiDetectionService;
        _logger = logger;
    }

    /// <summary>
    /// تحليل محتوى جديد (نص، صورة، أو فيديو)
    /// </summary>
    /// <param name="userId">معرف المستخدم</param>
    /// <param name="contentType">نوع المحتوى (1=نص، 2=صورة، 3=فيديو)</param>
    /// <param name="textContent">محتوى النص (إذا كان نص)</param>
    /// <param name="file">الملف المراد تحليله (إذا كان صورة أو فيديو)</param>
    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze(
        [FromForm] Guid userId,
        [FromForm] string contentType,
        [FromForm] string? textContent,
        [FromForm] IFormFile? file)
    {
        try
        {
            _logger.LogInformation("Analyzing content for user {UserId}, type {ContentType}", userId, contentType);

            // التحقق من وجود المستخدم
            var user = await _userRepository.GetByIdAsync(userId);
            // تحويل contentType من string إلى int
            if (!int.TryParse(contentType, out int contentTypeInt))
            {
                return BadRequest(new { message = "Invalid content type format. Use 1 for Text, 2 for Image, 3 for Video" });
            }

            // التحقق من نوع المحتوى
            if (!Enum.IsDefined(typeof(ContentType), contentTypeInt))
            {
                return BadRequest(new { message = "Invalid content type. Use 1 for Text, 2 for Image, 3 for Video" });
            }

            var type = (ContentType)contentTypeInt;
            string content;
            Application.Models.AiDetectionResult detectionResult;

            // معالجة حسب نوع المحتوى
            switch (type)
            {
                case ContentType.Text:
                    // التحقق من وجود النص
                    if (string.IsNullOrWhiteSpace(textContent))
                    {
                        return BadRequest(new { message = "Text content is required for text analysis" });
                    }

                    content = textContent;
                    detectionResult = await _aiDetectionService.DetectTextAsync(textContent);
                    _logger.LogInformation("Text analysis completed: {Probability}%", detectionResult.AiProbability * 100);
                    break;

                case ContentType.Image:
                case ContentType.Video:
                    // التحقق من وجود الملف
                    if (file == null || file.Length == 0)
                    {
                        return BadRequest(new { message = $"{type} file is required for {type.ToString().ToLower()} analysis" });
                    }

                    // التحقق من نوع الملف
                    if (!_fileService.IsValidFileType(file.ContentType))
                    {
                        return BadRequest(new { message = $"Invalid file type: {file.ContentType}. Please upload a valid {type.ToString().ToLower()} file." });
                    }

                    // حفظ الملف
                    using (var stream = file.OpenReadStream())
                    {
                        content = await _fileService.SaveFileAsync(stream, file.FileName, file.ContentType);
                    }

                    _logger.LogInformation("File saved: {FilePath}", content);

                    // تحليل الملف
                    if (type == ContentType.Image)
                    {
                        // للصور: تحليل الصورة
                        var imagePath = GetLocalPathFromUrl(content);
                        detectionResult = await _aiDetectionService.DetectImageAsync(imagePath);
                    }
                    else
                    {
                        // للفيديوهات: تحليل الفيديو
                        var videoPath = GetLocalPathFromUrl(content);
                        detectionResult = await _aiDetectionService.DetectVideoAsync(videoPath);
                    }

                    _logger.LogInformation("{Type} analysis completed: {Probability}%", type, detectionResult.AiProbability * 100);
                    break;

                default:
                    return BadRequest(new { message = "Unsupported content type" });
            }

            // إنشاء سجل التحليل
            var analysis = new Analysis
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ContentType = type,
                Content = content,
                AiProbability = detectionResult.AiProbability,
                IsAiGenerated = detectionResult.IsAiGenerated,
                Details = JsonSerializer.Serialize(new
                {
                    detectionResult.Details,
                    detectionResult.Confidence,
                    detectionResult.Indicators,
                    detectionResult.ProcessingTimeMs
                }),
                AnalyzedAt = DateTime.UtcNow,
                Status = AnalysisStatus.Completed
            };

            // حفظ في قاعدة البيانات
            await _analysisRepository.AddAsync(analysis);
            _logger.LogInformation("Analysis saved to database: {AnalysisId}", analysis.Id);

            // إرجاع النتيجة
            var response = new AnalysisResponseDto
            {
                Id = analysis.Id,
                ContentType = analysis.ContentType,
                AiProbability = analysis.AiProbability,
                IsAiGenerated = analysis.IsAiGenerated,
                Details = detectionResult.Details,
                AnalyzedAt = analysis.AnalyzedAt,
                Status = analysis.Status
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing content for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while analyzing the content", error = ex.Message });
        }
    }

    /// <summary>
    /// الحصول على تاريخ التحليلات لمستخدم معين
    /// </summary>
    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetHistory(Guid userId)
    {
        try
        {
            _logger.LogInformation("Fetching analysis history for user {UserId}", userId);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching history for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while fetching history", error = ex.Message });
        }
    }

    /// <summary>
    /// الحصول على تحليل محدد
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnalysis(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching analysis {AnalysisId}", id);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching analysis {AnalysisId}", id);
            return StatusCode(500, new { message = "An error occurred while fetching the analysis", error = ex.Message });
        }
    }

    /// <summary>
    /// الحصول على المحتوى الأصلي لتحليل معين (للصور والفيديوهات)
    /// </summary>
    [HttpGet("content/{id}")]
    public async Task<IActionResult> GetContent(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching content for analysis {AnalysisId}", id);

            var analysis = await _analysisRepository.GetByIdAsync(id);
            
            if (analysis == null)
            {
                return NotFound(new { message = "Analysis not found" });
            }

            // إذا كان نص، إرجاع النص مباشرة
            if (analysis.ContentType == ContentType.Text)
            {
                return Ok(new { contentType = "text", content = analysis.Content });
            }

            // إذا كان صورة أو فيديو، إرجاع URL الملف
            return Ok(new { contentType = analysis.ContentType.ToString().ToLower(), fileUrl = analysis.Content });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching content for analysis {AnalysisId}", id);
            return StatusCode(500, new { message = "An error occurred while fetching the content", error = ex.Message });
        }
    }

    /// <summary>
    /// Helper method لتحويل URL إلى مسار محلي
    /// </summary>
    private string GetLocalPathFromUrl(string url)
    {
        // استخراج اسم الملف من URL
        var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
        
        // إنشاء المسار الكامل
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        return Path.Combine(uploadsPath, fileName);
    }
}