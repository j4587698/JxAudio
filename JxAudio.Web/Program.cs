using BootstrapBlazor.Components;
using FreeSql;
using JxAudio.Core.Extensions;
using JxAudio.Core.Service;
using JxAudio.Web.Components;
using JxAudio.Web.Services;
using Microsoft.Extensions.Options;
using Serilog;
using Console = System.Console;

IFreeSql fsql = new FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=freedb.db")
    .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句
    .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
    .Build();
BaseEntity.Initialization(fsql, null);

Log.Logger = new LoggerConfiguration().WriteTo
    .File("./log/log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
    .WriteTo.Console().CreateLogger();

var builder = WebApplication.CreateBuilder(args).Inject();
builder.Host.UseSerilog();
builder.Services.AddTaskServices();
builder.Services.AddHostedService<JobHostedService>();
builder.Services.AddServiceController();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBootstrapBlazor(null, option =>
{
    option.AdditionalJsonFiles = Directory.GetFiles("./Locales", "*.json", SearchOption.AllDirectories);
});

// 增加多语言支持配置信息
builder.Services.AddRequestLocalization<IOptionsMonitor<BootstrapBlazorOptions>>((localizerOption, blazorOption)=>
{
    blazorOption.OnChange(Invoke);
    Invoke(blazorOption.CurrentValue);

    void Invoke(BootstrapBlazorOptions option)
    {
        var supportedCultures = option.GetSupportedCultures();
        localizerOption.SupportedCultures = supportedCultures;
        localizerOption.SupportedUICultures = supportedCultures;
    }
});

var app = builder.Build();
app.UseSerilogRequestLogging();
// 启用本地化
var option = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (option != null)
{
    app.UseRequestLocalization(option.Value);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStatusCodePages();

app.UsePathBase("/rest");
app.UseStaticFiles();
app.UseAntiforgery();
app.UseExceptionHandler(applicationBuilder =>
{
    applicationBuilder.Run(context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return Task.CompletedTask;
    });
});
app.MapDefaultControllerRoute();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();