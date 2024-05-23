namespace JxAudio.TransVo;

public class DynamicFilterInfo
{
    /// <summary>
    /// 属性名：Name<para></para>
    /// 导航属性：Parent.Name<para></para>
    /// 多表：b.Name<para></para>
    /// </summary>
    public string? Field { get; set; }

    /// <summary>操作符</summary>
    public DynamicFilterOperator Operator { get; set; }

    /// <summary>值</summary>
    public object? Value { get; set; }

    /// <summary>Filters 下的逻辑运算符</summary>
    public DynamicFilterLogic Logic { get; set; }

    /// <summary>
    /// 子过滤条件，它与当前的逻辑关系是 And<para></para>
    /// 注意：当前 Field 可以留空
    /// </summary>
    public List<DynamicFilterInfo>? Filters { get; set; }
}