using Domain.Entities;

namespace Application.DTOs;

public class DetectionResponseDto
{
    public Guid ContentId { get; set; }
    public ContentType ContentType { get; set; }
    public string Data { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    public List<ModelResultDto> Results { get; set; } = new();
}
