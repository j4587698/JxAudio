using BootstrapBlazor.Components;
using Front.WebAssembly.Data;
using Microsoft.AspNetCore.Components.Routing;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;

namespace Front.WebAssembly.Layout;

/// <summary>
/// 
/// </summary>
public sealed partial class MainLayout
{
    private bool UseTabSet { get; set; }

    private string Theme { get; set; } = "";

    private bool IsOpen { get; set; }

    private bool IsFixedHeader { get; set; } = true;

    private bool IsFixedFooter { get; set; } = true;

    private bool IsFullSide { get; set; } = true;

    private bool ShowFooter { get; set; } = true;

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

    private static List<MenuItem> GetIconSideMenuItems()
    {
        var menus = new List<MenuItem>
        {
            new() { Text = "返回组件库", Icon = "fa-solid fa-fw fa-home", Url = "https://www.blazor.zone/components" },
            new() { Text = "Index", Icon = "fa-solid fa-fw fa-flag", Url = "/", Match = NavLinkMatch.All },
            new() { Text = "Counter", Icon = "fa-solid fa-fw fa-check-square", Url = "/counter" },
            new() { Text = "Weather", Icon = "fa-solid fa-fw fa-database", Url = "/weather" },
            new() { Text = "Table", Icon = "fa-solid fa-fw fa-table", Url = "/table" },
            new() { Text = "花名册", Icon = "fa-solid fa-fw fa-users", Url = "/users" }
        };

        return menus;
    }
}