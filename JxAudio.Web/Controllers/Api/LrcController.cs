using ATL;
using JxAudio.Core;
using JxAudio.Core.Service;
using JxAudio.TransVo;
using Mapster;
using ResultVo = JxAudio.Web.Vo.ResultVo;

namespace JxAudio.Web.Controllers.Api;

public class LrcController(LrcService lrcService): DynamicControllerBase
{
    public async Task<object> Get(int id)
    {
        var lrc = await lrcService.GetLrcByIdAsync(id);
        var info = new LyricsInfo();
        info.ParseLRC(lrc.Lrc);
        var lyrics = info.SynchronizedLyrics;
        return ResultVo.Success(data: lyrics.Adapt<List<LrcVo>>());
    }
}