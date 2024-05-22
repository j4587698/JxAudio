using BootstrapBlazor.Components;

namespace JxAudio.Front.Filter;

public static class FreeSqlExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    public static DynamicFilterInfo ToDynamicFilter(this QueryPageOptions option)
    {
        var ret = new DynamicFilterInfo() { Filters = new() };

        // 处理模糊搜索
        if (option.Searches.Any())
        {
            ret.Filters.Add(new()
            {
                Logic = DynamicFilterLogic.Or,
                Filters = option.Searches.Select(i => i.ToDynamicFilter()).ToList()
            });
        }

        // 处理自定义搜索
        if (option.CustomerSearches.Any())
        {
            ret.Filters.AddRange(option.CustomerSearches.Select(i => i.ToDynamicFilter()));
        }

        // 处理高级搜索
        if (option.AdvanceSearches.Any())
        {
            ret.Filters.AddRange(option.AdvanceSearches.Select(i => i.ToDynamicFilter()));
        }

        // 处理表格过滤条件
        if (option.Filters.Any())
        {
            ret.Filters.AddRange(option.Filters.Select(i => i.ToDynamicFilter()));
        }

        return ret;
    }

    private static DynamicFilterInfo ToDynamicFilter(this IFilterAction filter)
    {
        var actions = filter.GetFilterConditions();
        var item = new DynamicFilterInfo();

        if (actions.Filters != null)
        {
            // TableFilter 最多仅两个条件
            if (actions.Filters.Count == 2)
            {
                item.Logic = actions.FilterLogic.ToDynamicFilterLogic();
                item.Filters = actions.Filters.Select(i => new DynamicFilterInfo()
                {
                    Field = i.FieldKey.ToFieldName(),
                    Value = i.FieldValue,
                    Operator = i.FilterAction.ToDynamicFilterOperator()
                }).ToList();
            }
            else
            {
                var c = actions.Filters.First();
                item.Field = c.FieldKey.ToFieldName();
                item.Value = c.FieldValue;
                item.Operator = c.FilterAction.ToDynamicFilterOperator();
            }
        }
        else
        {
            item.Field = actions.FieldKey.ToFieldName();
            item.Value = actions.FieldValue;
            item.Operator = actions.FilterAction.ToDynamicFilterOperator();
        }
        
        return item;
    }
    
    private static readonly Dictionary<string, string> _fieldMap = new()
    {
        { "Artists", "ArtistEntity.Name" },
        { "Artist", "ArtistEntity.Name" }
    };

    public static string? ToFieldName(this string? key)
    {
        return key == null ? key : _fieldMap.GetValueOrDefault(key, key);
    }

    private static DynamicFilterLogic ToDynamicFilterLogic(this FilterLogic logic) => logic switch
    {
        FilterLogic.And => DynamicFilterLogic.And,
        _ => DynamicFilterLogic.Or
    };

    private static DynamicFilterOperator ToDynamicFilterOperator(this FilterAction action) => action switch
    {
        FilterAction.Equal => DynamicFilterOperator.Equal,
        FilterAction.NotEqual => DynamicFilterOperator.NotEqual,
        FilterAction.Contains => DynamicFilterOperator.Contains,
        FilterAction.NotContains => DynamicFilterOperator.NotContains,
        FilterAction.GreaterThan => DynamicFilterOperator.GreaterThan,
        FilterAction.GreaterThanOrEqual => DynamicFilterOperator.GreaterThanOrEqual,
        FilterAction.LessThan => DynamicFilterOperator.LessThan,
        FilterAction.LessThanOrEqual => DynamicFilterOperator.LessThanOrEqual,
        _ => throw new System.NotSupportedException()
    };
}