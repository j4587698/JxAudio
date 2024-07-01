using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Net.Http.Json;
using JxAudio.TransVo;

namespace JxAudio.Front.Layout;

/// <summary>
/// 
/// </summary>
public sealed partial class MainLayout
{
    private string Theme { get; set; } = "";

    private List<MenuItem>? Menus { get; set; }
    
    private User? User { get; set; }
    
    private string _avatar = "./images/logo.png";

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
        _avatar = User?.Avatar ?? "./images/logo.png";
    }

    private List<MenuItem> GetIconSideMenuItems()
    {
        var menus = new List<MenuItem>
        {
            new() { Text = StringLocalizer["Album"], Icon = "fa-solid fa-fw fa-flag", Url = "/Albums", Match = NavLinkMatch.All },
            new() { Text = StringLocalizer["Artist"], Icon = "fa-solid fa-fw fa-check-square", Url = "/Artists" },
            new() { Text = StringLocalizer["Track"], Icon = "fa-solid fa-fw fa-database", Url = "/Tracks" },
            new() { Text = StringLocalizer["Collection"], Icon = "fa-solid fa-fw fa-table", Items = [
                new MenuItem() {Text = StringLocalizer["Album"], Icon = "fa-solid fa-fw fa-flag", Url = "/Star/Album"},
                new MenuItem() {Text = StringLocalizer["Artist"], Icon = "fa-solid fa-fw fa-flag", Url = "/Star/Artist"},
                new MenuItem() {Text = StringLocalizer["Track"], Icon = "fa-solid fa-fw fa-flag", Url = "/Star/Track"},
            ]}
        };

        return menus;
    }
}