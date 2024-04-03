using JxAudio.Core.Entity;

namespace AListProviderPlugin;

public class Constants
{
    public const string Key = "AList";
    
    public static string? Token { get; set; }

    public static async Task<AccountVo> GetAccount()
    {
        var values = await SettingsEntity.Where(x => x.GroupName == Key).ToListAsync();
        var account = new AccountVo();
        foreach (var value in values)
        {
            switch (value.SettingName)
            {
                case "UserName":
                    account.UserName = value.SettingValue;
                    break;
                case "Password":
                    account.Password = value.SettingValue;
                    break;
            }
        }
        
        return account;
    }
}