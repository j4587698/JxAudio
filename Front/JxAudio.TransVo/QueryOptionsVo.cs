namespace JxAudio.TransVo;

public class QueryOptionsVo
{
    public string? SortName { get; set; }
    
    public int SortOrder { get; set; }
    
    public int PageIndex { get; set; } = 1;
    
    public int PageItems { get; set; } = 20;
    
    public bool IsPage { get; set; }
    
    public DynamicFilterInfo? DynamicFilterInfo { get; set; }
}