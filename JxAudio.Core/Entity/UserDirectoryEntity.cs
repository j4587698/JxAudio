using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

public class UserDirectoryEntity
{
    [Column(IsPrimary = true)]
    public Guid UserId { get; set; }

    [Navigate(nameof(UserId))]
    public UserEntity? UserEntity { get; set; }

    [Column(IsPrimary = true)]
    public long DirectoryId { get; set; }

    [Navigate(nameof(DirectoryId))]
    public DirectoryEntity? DirectoryEntity { get; set; }
}