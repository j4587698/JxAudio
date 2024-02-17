using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace JxAudio.Core.Components;

[CascadingTypeParameter(nameof(TItem))]
public partial class AdminTable<TItem> where TItem : class, new()
{
    [NotNull]
    [Parameter]
    public RenderFragment<TItem>? TableColumns { get; set; }
    
    [Parameter] 
    public Func<QueryPageOptions,Task<QueryData<TItem>>>? OnQueryAsync { get; set; }
    
    [Parameter] 
    public bool ShowToolbar { get; set; }
    
    [Parameter] 
    public bool ShowExtendButtons { get; set; }
}