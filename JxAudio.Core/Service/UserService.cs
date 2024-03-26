using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;

namespace JxAudio.Core.Service;

[Transient]
public class UserService
{
    public async Task<UserEntity?> GetUserByUsername(string username, CancellationToken cancellationToken)
    {
        return await UserEntity.Where(x => x.UserName == username).FirstAsync(cancellationToken);
    }
}