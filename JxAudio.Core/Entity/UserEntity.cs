﻿using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

public class UserEntity : BaseEntity<UserEntity, Guid>
{
    public string? UserName { get; set; }

    public string? Password { get; set; }

    public int MaxBitRate { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsGuest { get; set; }

    public bool CanJukebox { get; set; }

    [Navigate(ManyToMany = typeof(UserDirectoryEntity))]
    public ICollection<DirectoryEntity>? DirectoryEntities { get; set; }
}