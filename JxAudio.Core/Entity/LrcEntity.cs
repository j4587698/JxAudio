using FreeSql;
using FreeSql.DataAnnotations;

namespace JxAudio.Core.Entity;

public class LrcEntity: BaseEntity<LrcEntity, int>
{
    public string? Artist { get; set; }

    public string? Title { get; set; }

    [Column(StringLength = -1)]
    public string? Lrc { get; set; }
    
}