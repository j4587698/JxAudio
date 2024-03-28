﻿using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Subsonic;

namespace JxAudio.Core.Service;

[Transient]
public class UserService
{
    public async Task<UserEntity?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await UserEntity.Where(x => x.UserName == username).FirstAsync(cancellationToken);
    }

    public async Task UpdatePassword(UserEntity userEntity, string password)
    {
        userEntity.Password = password;
        await userEntity.SaveAsync();
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
}