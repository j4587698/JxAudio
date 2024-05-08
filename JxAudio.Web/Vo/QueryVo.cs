using BootstrapBlazor.Components;
using FreeSql.Internal.Model;

namespace JxAudio.Web.Vo;

public class QueryVo
{
    public QueryPageOptions? QueryPageOptions { get; set; }

    public DynamicFilterInfo? DynamicFilterInfo { get; set; }
}