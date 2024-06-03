using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace JxAudio.Front.Components;

[CascadingTypeParameter(nameof(TItem))]
public partial class FrontTable<TItem> where TItem: class, new()
{
    [NotNull] public Table<TItem>? Table { get; set; }
    
    [NotNull] [Parameter] public RenderFragment<TItem>? TableColumns { get; set; }

    [Parameter] public Func<QueryPageOptions, Task<QueryData<TItem>>>? OnQueryAsync { get; set; }

    [Parameter] public Func<TItem, ItemChangedType, Task<bool>>? OnSaveAsync { get; set; }

    [Parameter] public bool ShowToolbar { get; set; }

    [Parameter] public bool ShowExtendButtons { get; set; }

    [Parameter] public RenderFragment? TableToolbarTemplate { get; set; }

    [Parameter] public RenderFragment<TItem>? RowButtonTemplate { get; set; }

    [Parameter] public bool ShowAddButton { get; set; } = true;
    [Parameter] public bool ShowDefaultButtons { get; set; } = true;
    [Parameter] public bool ShowDeleteButton { get; set; } = true;
    [Parameter] public bool ShowEditButton { get; set; } = true;
    [Parameter] public bool ShowExtendEditButton { get; set; } = true;
    [Parameter] public bool ShowExtendDeleteButton { get; set; } = true;
    [Parameter] public RenderFragment? TableExtensionToolbarTemplate { get; set; }
    [Parameter] public string? ConfirmDeleteContentText { get; set; }
    [Parameter] public Func<IEnumerable<TItem>, Task<bool>>? OnDeleteAsync { get; set; }
    [Parameter] public RenderFragment<TItem>? DetailRowTemplate { get; set; }
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    [Parameter] public bool IsPagination { get; set; } = true;

    public async Task QueryAsync()
    {
        await Table.QueryAsync();
    }
}