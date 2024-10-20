﻿using BootstrapBlazor.Components;
using FreeSql;
using JxAudio.Core.Attributes;
using JxAudio.Core.Entity;
using JxAudio.Core.Extensions;
using Microsoft.AspNetCore.Components;

namespace JxAudio.Core.Service;

[Scoped]
public class FreeSqlDataService<TModel> : DataServiceBase<TModel> where TModel : class, new()
{
    private readonly IFreeSql _db = BaseEntity.Orm;
    
    /// <summary>
    /// 删除方法
    /// </summary>
    /// <param name="models"></param>
    /// <returns></returns>
    public override async Task<bool> DeleteAsync(IEnumerable<TModel> models)
    {
        // 通过模型获取主键列数据
        // 支持批量删除
        await _db.Delete<TModel>(models).ExecuteAffrowsAsync();
        return true;
    }

    /// <summary>
    /// 保存方法
    /// </summary>
    /// <param name="model"></param>
    /// <param name="changedType"></param>
    /// <returns></returns>
    public override async Task<bool> SaveAsync(TModel model, ItemChangedType changedType)
    {
        await _db.GetRepository<TModel>().InsertOrUpdateAsync(model);
        return true;
    }

    /// <summary>
    /// 查询方法
    /// </summary>
    /// <param name="option"></param>
    /// <returns></returns>
    public override Task<QueryData<TModel>> QueryAsync(QueryPageOptions option)
    {
        var select = _db.Select<TModel>().WhereDynamicFilter(option.ToDynamicFilter())
            .OrderByPropertyNameIf(option.SortOrder != SortOrder.Unset, option.SortName,
                option.SortOrder == SortOrder.Asc)
            .IncludeByPropertyNameIf(typeof(TModel) == typeof(AlbumEntity), nameof(AlbumEntity.ArtistEntity))
            .IncludeByPropertyNameIf(typeof(TModel) == typeof(TrackEntity), nameof(TrackEntity.ArtistEntities))
            .IncludeByPropertyNameIf(typeof(TModel) == typeof(DirectoryEntity), nameof(DirectoryEntity.UserEntities))
            .Count(out var count);
        if (option.IsPage)
        {
            select = select.Page(option.PageIndex, option.PageItems);
        }
        var items = select.ToList();
        var ret = new QueryData<TModel>()
        {
            TotalCount = (int)count,
            Items = items,
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Any(),
            IsAdvanceSearch = option.AdvanceSearches.Any(),
            IsSearch = option.Searches.Any() || option.CustomerSearches.Any()
        };
        return Task.FromResult(ret);
    }
}