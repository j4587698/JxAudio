using BootstrapBlazor.Components;
using JxAudio.Front.Data;
using JxAudio.TransVo;
using Microsoft.AspNetCore.Components;

namespace JxAudio.Front.Components;

public partial class TrackTable
{
    [Parameter] public List<TrackVo>? Tracks { get; set; }

    [Parameter] public RenderFragment<TrackVo>? TrackButton { get; set; }

    [Parameter] public bool ShowDefaultButton { get; set; } = true;
    [Parameter] public bool ShowDefaultToolbarButton { get; set; } = true;
    
    [Parameter] public bool ShowDefaultToolbar { get; set; } = true;

    [Parameter] public RenderFragment? ToolbarButtons { get; set; }

    [Parameter]
    public List<TrackVo>? SelectedRows { get; set; }

    [Parameter]
    public EventCallback<List<TrackVo>> SelectedRowsChanged { get; set; }

    [Parameter]
    public bool IsKeepSelectedRows { get; set; }

    private List<TrackVo>? _selectedRows;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _selectedRows = SelectedRows ?? [];
    }
    
    private void SelectedChanged(List<TrackVo> trackVos)
    {
        _selectedRows = trackVos;
        SelectedRowsChanged.InvokeAsync(_selectedRows);
    }

    private void Play(TrackVo trackVo)
    {
        DispatchService.Dispatch(new DispatchEntry<AddTrackMessage>()
        {
            Entry = new AddTrackMessage()
            {
                Tracks = [trackVo],
                Type = "replace"
            }
        });
    }

    private void AddToList(TrackVo trackVo)
    {
        DispatchService.Dispatch(new DispatchEntry<AddTrackMessage>()
        {
            Entry = new AddTrackMessage()
            {
                Tracks = [trackVo],
                Type = "add"
            }
        });
    }

    private Task PlayAll()
    {
        if (Tracks is { Count: > 0 })
        {
            DispatchService.Dispatch(new DispatchEntry<AddTrackMessage>()
            {
                Entry = new AddTrackMessage()
                {
                    Tracks = Tracks,
                    Type = "replace"
                }
            });
        }

        return Task.CompletedTask;
    }
}