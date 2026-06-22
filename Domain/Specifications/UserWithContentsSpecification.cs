using System;
using Domain.Entities;

namespace Domain.Specifications;


public class UserWithContentsSpecification : BaseSpecification<User>
{
    
    public UserWithContentsSpecification(Guid id) 
        : base(u => u.Id == id)
    {
        AddInclude(u => u.Contents);
    }

    
    public UserWithContentsSpecification(string email) 
        : base(u => u.Email == email)
    {
    }
}
