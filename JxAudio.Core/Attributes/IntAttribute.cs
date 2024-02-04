using System.ComponentModel.DataAnnotations;

namespace JxAudio.Core.Attributes;

public class IntAttribute: ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value != null && int.TryParse(value.ToString(), out _);
    }
}