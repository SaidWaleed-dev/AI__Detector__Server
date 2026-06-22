using System;
using Domain.Entities;

namespace Domain.Specifications;

public class ContentWithDetailsSpecification : BaseSpecification<Content>
{
    
    public ContentWithDetailsSpecification(Guid id) 
        : base(c => c.Id == id)
    {
        AddInclude(c => c.User);
        AddInclude("DetectionResults.AIModel");
    }

    
    public ContentWithDetailsSpecification(Guid userId, bool includeDetails) 
        : base(c => c.UserId == userId)
    {
        ApplyOrderByDescending(c => c.UploadedAt);
        if (includeDetails)
        {
            AddInclude("DetectionResults.AIModel");
        }
    }

    
    public ContentWithDetailsSpecification(Guid userId, int count) 
        : base(c => c.UserId == userId)
    {
        ApplyOrderByDescending(c => c.UploadedAt);
        ApplyPaging(0, count);
        AddInclude("DetectionResults.AIModel");
    }

    
    public ContentWithDetailsSpecification(Guid userId, string data, ContentType type) 
        : base(c => c.UserId == userId && 
                    c.Type == type && 
                    (c.Data.Trim() == data || c.Data.EndsWith("|" + data)))
    {
        AddInclude("DetectionResults.AIModel");
    }
}
