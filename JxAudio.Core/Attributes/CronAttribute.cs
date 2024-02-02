using System.ComponentModel.DataAnnotations;
using Cronos;

namespace JxAudio.Core.Attributes;

public class CronAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string cron)
        {
            return false;
        }

        try
        {
            CronExpression.Parse(cron, CronFormat.IncludeSeconds);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}