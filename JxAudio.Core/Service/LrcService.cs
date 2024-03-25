using ATL;
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
        var info = new LyricsInfo();
        info.ParseLRC(lrc.Lrc);
        var lrcText = string.Join("\n", info.SynchronizedLyrics.Select(x => x.Text));
        return lrcText;
    }
}