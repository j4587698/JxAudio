using Jx.Toolbox.Cryptography;
using Jx.Toolbox.Extensions;
using Jx.Toolbox.Hash;
using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Subsonic;

namespace JxAudio.Core.Service;

[Transient]
public class UserService
{
    public void EncryptPassword(UserEntity userEntity)
    {
        if (userEntity.Iv.IsNullOrEmpty())
        {
            userEntity.Iv = AesEncryption.CreateIv();
        }
        userEntity.Password = AesEncryption.Encrypt(userEntity.Password, Constants.AesKey, userEntity.Iv);
    }
    
    public string DecryptPassword(UserEntity userEntity)
    {
        return AesEncryption.Decrypt(userEntity.Password, Constants.AesKey, userEntity.Iv);
    }
    
    public async Task<UserEntity?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await UserEntity.Where(x => x.UserName == username).FirstAsync(cancellationToken);
    }
    
    public async Task<UserEntity?> ValidatePasswordAsync(string username, string password, CancellationToken cancellationToken)
    {
        var user = await GetUserByUsernameAsync(username, cancellationToken);
        if (user == null)
        {
            return null;
        }

        if (password.Equals(DecryptPassword(user), StringComparison.OrdinalIgnoreCase) || user.IsGuest)
        {
            return user;
        }

        return null;
    }
    
    public async Task<UserEntity?> ValidatePasswordHexAsync(string username, string passwordHex, string salt, CancellationToken cancellationToken)
    {
        var user = await GetUserByUsernameAsync(username, cancellationToken);
        if (user == null)
        {
            return null;
        }

        if (passwordHex.Equals(MD5.MD5StringWithSalt(DecryptPassword(user), salt), StringComparison.OrdinalIgnoreCase) || user.IsGuest)
        {
            return user;
        }

        return null;
    }

    public async Task UpdatePassword(UserEntity userEntity, string password)
    {
        userEntity.Password = password;
        await userEntity.SaveAsync();
    }

    public async Task<UserEntity> GetUserByUsernameAsync(string username)
    {
        return await UserEntity.Where(x => x.UserName == username).FirstAsync();
    }
    
    
    public async Task<User> GetUserAsync(UserEntity userEntity, CancellationToken cancellationToken)
    {
        var ids = await DirectoryEntity
            .Where(x => x.IsAccessControlled == false || x.UserEntities!.Any(y => y.Id == userEntity.Id))
            .ToListAsync(x => x.Id, cancellationToken);
        
        return new User()
        {
            username = userEntity.UserName,
            email = userEntity.Email,
            scrobblingEnabled = true,
            maxBitRate = userEntity.MaxBitRate,
            maxBitRateSpecified = true,
            adminRole = false,
            settingsRole = !userEntity.IsGuest,
            downloadRole = true,
            uploadRole = false,
            playlistRole = true,
            coverArtRole = true,
            commentRole = false,
            podcastRole = false,
            streamRole = true,
            jukeboxRole = userEntity.CanJukebox,
            shareRole = false,
            videoConversionRole = false,
            avatarLastChanged = default,
            avatarLastChangedSpecified = false,
            folder = ids.ToArray(),
        };
    }

    public async Task<UserEntity?> GetUserById(Guid userId)
    {
        return await UserEntity.FindAsync(userId);
    }
}