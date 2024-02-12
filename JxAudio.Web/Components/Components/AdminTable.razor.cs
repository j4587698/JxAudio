using System.Diagnostics.CodeAnalysis;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace JxAudio.Web.Components.Components;

[CascadingTypeParameter(nameof(TItem))]
public partial class AdminTable<TItem> where TItem : class, new()
{
    [NotNull]
    [Parameter]
    public RenderFragment<TItem>? TableColumns { get; set; }
    
    [Parameter] 
    public Func<QueryPageOptions,Task<QueryData<TItem>>>? OnQueryAsync { get; set; }
}