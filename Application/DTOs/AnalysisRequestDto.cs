using Domain.Entities;

namespace Application.DTOs;

/// <summary>
/// DTO لطلب تحليل محتوى جديد
/// </summary>
public class AnalysisRequestDto
{
    // نوع المحتوى (نص، صورة، فيديو)
    public ContentType ContentType { get; set; }
    
    // المحتوى نفسه (نص أو Base64 للصور/فيديوهات)
    public string Content { get; set; } = string.Empty;
}