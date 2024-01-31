using System.ComponentModel.DataAnnotations;

namespace JxAudio.Core.Attributes;

public class GuidRequiredAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not Guid guid)
        {
            return false;
        }
        
        return guid != Guid.Empty;
    }
}