using System;
using Domain.Entities;

namespace Domain.Specifications;


public class OtpRecordSpecification : BaseSpecification<OtpRecord>
{
    
    public OtpRecordSpecification(string email, OtpType type, bool onlyUnverified)
        : base(o => o.Email == email && 
                    o.Type == type && 
                    (!onlyUnverified || !o.IsVerified))
    {
        ApplyOrderByDescending(o => o.CreatedAt);
    }

    
    public OtpRecordSpecification(string email)
        : base(o => o.Email == email)
    {
        ApplyOrderByDescending(o => o.CreatedAt);
    }
}
