using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers;

/// <summary>
/// Controller for handling content detection using the normalized schema.
/// </summary>
[ApiController]
[Route("api/[controller]")] // -> api/detection
[AllowAnonymous] // مفيش حاجة للتوكين في الـ Detection
public class DetectionController : ControllerBase
{
    private readonly IDetectionRepository _detectionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFileService _fileService;
    private readonly IAiDetectionService _aiDetectionService;
    private readonly IWebHostEnvironment _env;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
    private readonly ILogger<DetectionController> _logger;
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, System.Threading.SemaphoreSlim> _userLocks = new();

    public DetectionController(
        IDetectionRepository detectionRepository,
        IUserRepository userRepository,
        IFileService fileService,
        IAiDetectionService aiDetectionService,
        ILogger<DetectionController> logger,
        IWebHostEnvironment env,
        Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _detectionRepository = detectionRepository;
        _userRepository = userRepository;
        _fileService = fileService;
        _aiDetectionService = aiDetectionService;
        _logger = logger;
        _env = env;
        _cache = cache;
    }

    
    [HttpPost("detect")]
    [HttpPost("/api/detect")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Detect(
        [FromForm] Guid userId,
        [FromForm] int contentType,
        [FromForm] string? textContent,
        [FromForm] IFormFile? file)
    {
        var semaphore = _userLocks.GetOrAdd(userId, _ => new System.Threading.SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();

        try
        {
            _logger.LogInformation("Detecting content for user {UserId}, type {ContentType}", userId, contentType);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            if (!Enum.IsDefined(typeof(ContentType), contentType))
                return BadRequest(new { message = "Invalid content type" });

            var type = (ContentType)contentType;

            // Enforce guest limit of 3 scans per content type
            bool isGuest = user.Email.Contains("@temp.ai");
            if (isGuest)
            {
                var usageCount = await _detectionRepository.GetUsageCountAsync(userId, type);
                if (usageCount >= 3)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        message = "Guest limit exceeded. Guests are allowed a maximum of 3 scans per content type.",
                        messageAr = "تم تجاوز الحد المسموح به للزائرين. يُسمح للزوار بحد أقصى 3 عمليات فحص لكل نوع محتوى."
                    });
                }
            }

            string data;
            Application.Models.AiDetectionResult serviceResult;

            
            switch (type)
            {
                case ContentType.Text:
                    if (string.IsNullOrWhiteSpace(textContent))
                        return BadRequest(new { message = "Text content is required" });
                    data = textContent.Trim(); // Trim whitespace

                    
                    _logger.LogInformation("Checking duplicate text for user {UserId}", userId);
                    var existingTextContent = await _detectionRepository.GetContentByDataAsync(userId, data, ContentType.Text);
                    if (existingTextContent != null && existingTextContent.DetectionResults.Any())
                    {
                        _logger.LogInformation("Duplicate text found. Returning existing result {ContentId}", existingTextContent.Id);
                        return Ok(MapToResponseDto(existingTextContent));
                    }

                    serviceResult = await _aiDetectionService.DetectTextAsync(textContent);
                    break;

                case ContentType.Image:
                case ContentType.Video:
                case ContentType.Audio:
                    if (file == null || file.Length == 0)
                        return BadRequest(new { message = "File is required" });

                    if (!_fileService.IsValidFileType(file.ContentType))
                        return BadRequest(new { message = "Invalid file type" });

                    
                    string fileHash;
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            var hashBytes = md5.ComputeHash(stream);
                            fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                        }
                    }

                    
                    _logger.LogInformation("Checking duplicate file hash {Hash} for user {UserId}", fileHash, userId);
                    var existingFileContent = await _detectionRepository.GetContentByDataAsync(userId, $"hash:{fileHash}", type);
                    if (existingFileContent != null && existingFileContent.DetectionResults.Any())
                    {
                        _logger.LogInformation("Duplicate file found. Returning existing result {ContentId}", existingFileContent.Id);
                        return Ok(MapToResponseDto(existingFileContent));
                    }

                    using (var stream = file.OpenReadStream())
                    {
                        data = await _fileService.SaveFileAsync(stream, file.FileName, file.ContentType);
                    }

                    var localPath = GetLocalPathFromUrl(data);
                    
                    if (type == ContentType.Image)
                        serviceResult = await _aiDetectionService.DetectImageAsync(localPath);
                    else if (type == ContentType.Video)
                        serviceResult = await _aiDetectionService.DetectVideoAsync(localPath);
                    else
                        serviceResult = await _aiDetectionService.DetectAudioAsync(localPath);
                    
                    
                    data = $"{data}|hash:{fileHash}";
                    break;

                default:
                    return BadRequest(new { message = "Unsupported type" });
            }

            
            var model = await _detectionRepository.GetModelByNameAsync("DistilBERT-AI-Detector");
            if (model == null)
            {
                model = new AIModel { Id = Guid.NewGuid(), Name = "DistilBERT-AI-Detector", Version = "2.0", IsActive = true };
                await _detectionRepository.AddModelAsync(model);
            }

            
            var contentEntity = new Content
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Data = data,
                UploadedAt = DateTime.UtcNow
            };
            await _detectionRepository.AddContentAsync(contentEntity);

            var resultEntity = new AIDetectionResult
            {
                Id = Guid.NewGuid(),
                ContentId = contentEntity.Id,
                AIModelId = model.Id,
                AiProbability = serviceResult.AiProbability,
                IsAiGenerated = serviceResult.IsAiGenerated,
                Details = JsonSerializer.Serialize(new
                {
                    serviceResult.Details,
                    serviceResult.Confidence,
                    serviceResult.Indicators,
                    serviceResult.ProcessingTimeMs
                }),
                AnalyzedAt = DateTime.UtcNow
            };
            await _detectionRepository.AddResultAsync(resultEntity);

            
            var finalContent = await _detectionRepository.GetContentByIdAsync(contentEntity.Id);
            return Ok(MapToResponseDto(finalContent!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during detection");
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
        finally
        {
            semaphore.Release();
        }
    }

    private DetectionResponseDto MapToResponseDto(Content c)
    {
        string cleanData = c.Data;
        if (c.Type != ContentType.Text && cleanData.Contains("|hash:"))
        {
            cleanData = cleanData.Split("|hash:")[0];
        }

        return new DetectionResponseDto
        {
            ContentId = c.Id,
            ContentType = c.Type,
            Data = cleanData,
            UploadedAt = c.UploadedAt,
            Results = c.DetectionResults.Select(r => new ModelResultDto
            {
                ResultId = r.Id,
                ModelName = r.AIModel?.Name ?? "Unknown",
                AiProbability = r.AiProbability,
                IsAiGenerated = r.IsAiGenerated,
                Details = r.Details,
                AnalyzedAt = r.AnalyzedAt
            }).ToList()
        };
    }

    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetHistory(Guid userId)
    {
        var contents = await _detectionRepository.GetRecentContentsByUserIdAsync(userId);
        var response = contents.Select(MapToResponseDto).ToList();
        return Ok(response);
    }

    [HttpDelete("history/{contentId}")]
    public async Task<IActionResult> DeleteHistoryItem(Guid contentId)
    {
        _logger.LogInformation("Deleting history item {ContentId}", contentId);
        var deleted = await _detectionRepository.DeleteContentAsync(contentId);
        if (!deleted)
        {
            return NotFound(new { message = "History record not found" });
        }
        return Ok(new { message = "Record deleted successfully" });
    }

    [HttpDelete("history/clear/{userId}")]
    public async Task<IActionResult> ClearHistory(Guid userId)
    {
        _logger.LogInformation("Clearing history for user {UserId}", userId);
        var deleted = await _detectionRepository.DeleteAllUserContentAsync(userId);
        return Ok(new { message = "History cleared successfully" });
    }

    private string GetLocalPathFromUrl(string url)
    {
        var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
        return Path.Combine(uploadsPath, fileName);
    }
}
