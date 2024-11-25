using System.ComponentModel.DataAnnotations;
using BootstrapBlazor.Components;
using JxAudio.TransVo;
using Microsoft.Extensions.Localization;

namespace JxAudio.Front.Validators;

public class RePasswordValidator: IValidator
{
    public void Validate(object? propertyValue, ValidationContext context, List<ValidationResult> results)
    {
        if (context.ObjectInstance is RePassVo resetPasswordVo)
        {
            switch (context.MemberName)
            {
                case nameof(RePassVo.ReNewPassword):
                    if (resetPasswordVo.ReNewPassword != resetPasswordVo.NewPassword)
                    {
                        results.Add(new ValidationResult("两次密码不一致", new []{context.MemberName}));
                    }
                    break;
            }
            
        }
    }
}