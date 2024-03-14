using System.ComponentModel.DataAnnotations;
using BootstrapBlazor.Components;
using JxAudio.Core;
using JxAudio.Web.Components.Pages.Admin;
using JxAudio.Web.Vo;
using Microsoft.Extensions.Localization;

namespace JxAudio.Web.Validators;

public class RePasswordValidator: IValidator
{
    public void Validate(object? propertyValue, ValidationContext context, List<ValidationResult> results)
    {
        if (context.ObjectInstance is ResetPasswordVo resetPasswordVo)
        {
            var localizer = Application.GetRequiredService<IStringLocalizer<UserManager>>();
            switch (context.MemberName)
            {
                case nameof(ResetPasswordVo.RePassword):
                    if (resetPasswordVo.RePassword != resetPasswordVo.NewPassword)
                    {
                        results.Add(new ValidationResult(localizer!["RePassword"], new []{context.MemberName}));
                    }
                    break;
            }
            
        }
    }
}