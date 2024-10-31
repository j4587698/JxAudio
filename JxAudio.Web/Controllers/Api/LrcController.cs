using JxAudio.Core;
using JxAudio.Core.Service;
using JxAudio.TransVo;
using JxAudio.Web.Utils;
using ResultVo = JxAudio.Web.Vo.ResultVo;

namespace JxAudio.Web.Controllers.Api;

public class LrcController(LrcService lrcService): DynamicControllerBase
{
    public async Task<object> Get(int id)
    {
        var lrc = await lrcService.GetLrcByIdAsync(id);
        return ResultVo.Success(data: MappingConfig.LrcEntityToLrcVo(lrc));
    }
}