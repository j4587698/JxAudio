using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;

namespace JxAudio.Core.Service;

[Transient]
public class LrcService
{
    public async Task<string> GetLrcAsync(string? artist, string? title)
    {
        var lrc = await LrcEntity.Where(x => x.Title == title && x.Artist == artist).FirstAsync();
        if (lrc == null)
        {
            return "";
        }
        var song = new LrcParser.Parser.Lrc.LrcParser().Decode(lrc.Lrc!);
        var lrcText = string.Join("\n", song.Lyrics.Select(x => x.Text));
        return lrcText;
    }

    public async Task<LrcEntity> GetLrcByIdAsync(int lrcId)
    {
        var lrc = await LrcEntity.FindAsync(lrcId);
        return lrc;
    }
}