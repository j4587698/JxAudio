using System.ComponentModel.DataAnnotations;
using BootstrapBlazor.Components;
using Jx.Toolbox.Extensions;
using JxAudio.Core;
using JxAudio.Core.Enums;
using JxAudio.Core.Options;
using JxAudio.Web.Vo;
using Microsoft.Extensions.Localization;

namespace JxAudio.Web.Validators;

public class InstallValidator : ValidatorBase
{
    /// <summary>
    /// 校验规则
    /// </summary>
    /// <param name="propertyValue"></param>
    /// <param name="context"></param>
    /// <param name="results"></param>
    public override void Validate(object? propertyValue, ValidationContext context, List<ValidationResult> results)
    {
        var stringLocalizer = Application.GetRequiredService<IStringLocalizer<InstallValidator>>();
        if (context.ObjectInstance is UserInstallVo infoVo)
        {
            switch (context.MemberName)
            {
                case nameof(infoVo.RePassword):
                    if (infoVo.RePassword != infoVo.Password)
                    {
                        results.Add(new ValidationResult(stringLocalizer!["PasswordNotEqual"], new []{context.MemberName}));
                    }
                    break;
            }
        }
        // else if (context.ObjectInstance is CronInstallVo cronInstallVo)
        // {
        //     if (context.MemberName.IsNullOrEmpty())
        //     {
        //         return;
        //     }
        //     var cron = propertyValue as string;
        //     if (cron.IsNullOrEmpty())
        //     {
        //         results.Add(new ValidationResult("Cron表达式不能为空", new[] { context.MemberName }!));
        //         return;
        //     }
        //
        //     var cronTab = Crontab.TryParse(cron, CronStringFormat.WithSeconds);
        //     if (cronTab == default)
        //     {
        //         results.Add(new ValidationResult("Cron表达式格式不正确", new[] { context.MemberName }!));
        //     }
        // }
    }
}