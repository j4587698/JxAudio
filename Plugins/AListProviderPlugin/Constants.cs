using System.IdentityModel.Tokens.Jwt;
using FreeSql;
using JxAudio.Core.Entity;

namespace AListProviderPlugin;

public class Constants
{
    public const string Key = "AList";
    
    public static string? Token { get; set; }
    
    public static AccountVo? Account { get; set; }

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
                case nameof(AccountVo.ServerUrl):
                    account.ServerUrl = value.SettingValue;
                    break;
            }
        }
        
        return account;
    }
    
    public static bool IsJwtExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        // 确保token是有效的JWT格式
        if (!handler.CanReadToken(token))
        {
            return false;
        }
    
        var jwtToken = handler.ReadJwtToken(token);
        // 获取exp声明
        var expiry = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

        if (expiry != null)
        {
            // 将exp声明的值转换为Unix时间戳，并转换为DateTime
            var expiryDateUnix = long.Parse(expiry);
            var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expiryDateUnix).DateTime;
            return expiryDate < DateTime.UtcNow; // 转换为本地时间
        }

        return false; // 如果没有exp声明，返回null
    }
}