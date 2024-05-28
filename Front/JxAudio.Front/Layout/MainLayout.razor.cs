﻿using BootstrapBlazor.Components;
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

    private static List<MenuItem> GetIconSideMenuItems()
    {
        var menus = new List<MenuItem>
        {
            new() { Text = "专辑", Icon = "fa-solid fa-fw fa-flag", Url = "/Albums", Match = NavLinkMatch.All },
            new() { Text = "歌手", Icon = "fa-solid fa-fw fa-check-square", Url = "/Artists" },
            new() { Text = "歌曲", Icon = "fa-solid fa-fw fa-database", Url = "/Tracks" },
            new() { Text = "Table", Icon = "fa-solid fa-fw fa-table", Url = "/table" },
            new() { Text = "花名册", Icon = "fa-solid fa-fw fa-users", Url = "/users" }
        };

        return menus;
    }
}