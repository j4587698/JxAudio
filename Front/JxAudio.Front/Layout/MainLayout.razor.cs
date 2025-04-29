using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Net.Http.Json;
using JxAudio.Front.Components;
using JxAudio.Front.Data;
using JxAudio.TransVo;
using Console = System.Console;

namespace JxAudio.Front.Layout;

/// <summary>
/// 
/// </summary>
public sealed partial class MainLayout
{
    private string Theme { get; set; } = "";

    private List<MenuItem>? Menus { get; set; }
    
    private User? User { get; set; }
    
    private string _avatar = "./Images/logo.png";

    private Modal? _modal;

    /// <summary>
    /// OnInitialized 方法
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Menus = GetIconSideMenuItems();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var res = await Http.GetAsync("/api/User/UserInfo");
        User = await res.Content.ReadFromJsonAsync<User>();
        _avatar = User?.Avatar ?? "./Images/logo.png";
    }

    private List<MenuItem> GetIconSideMenuItems()
    {
        var menus = new List<MenuItem>
        {
            new() { Text = StringLocalizer["Home"], Icon = "fa fa-fw fa-home", Url = "/", Match = NavLinkMatch.All },
            new() { Text = StringLocalizer["Album"], Icon = "fas fa-fw fa-compact-disc", Url = "/Albums", Match = NavLinkMatch.All },
            new() { Text = StringLocalizer["Artist"], Icon = "fas fa-fw fa-microphone", Url = "/Artists" },
            new() { Text = StringLocalizer["Track"], Icon = "fas fa-fw fa-music", Url = "/Tracks" },
            new() { Text = StringLocalizer["Playlist"], Icon = "fas fa-fw fa-clipboard-list", Url = "/Playlists" },
            new() { Text = StringLocalizer["Collection"], Icon = "fa-solid fa-fw fa-table", Items = [
                new MenuItem() {Text = StringLocalizer["Album"], Icon = "fas fa-fw fa-compact-disc", Url = "/Star/Album"},
                new MenuItem() {Text = StringLocalizer["Artist"], Icon = "fas fa-fw fa-microphone", Url = "/Star/Artist"},
                new MenuItem() {Text = StringLocalizer["Track"], Icon = "fas fa-fw fa-music", Url = "/Star/Track"},
            ]}
        };

        return menus;
    }
    
    private async Task RePass()
    {
        await _modal!.Toggle();
    }
}