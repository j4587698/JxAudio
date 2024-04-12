using System.DirectoryServices.Protocols;
using BootstrapBlazor.Components;
using FreeSql;
using JxAudio.Core;
using JxAudio.Core.Extensions;
using JxAudio.Core.Options;
using JxAudio.Web.Components;
using JxAudio.Web.Middlewares;
using JxAudio.Web.Services;
using JxAudio.Web.Utils;
using Microsoft.Extensions.Options;
using Serilog;
using Console = System.Console;
using SearchOption = System.IO.SearchOption;

Log.Logger = new LoggerConfiguration().WriteTo
    .File("./log/log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
    .WriteTo.Console().CreateLogger();

var builder = WebApplication.CreateBuilder(args).Inject(configOption =>
{
    configOption.ConfigSearchFolder = ["config"];
    configOption.DynamicPrefix = "/api/";
});

builder.Host.UseSerilog();

var dbConfigOption = builder.Configuration.GetSection("Db").Get<DbConfigOption>();
if (dbConfigOption != null)
{
    var ret = Setup.SetupDb(dbConfigOption);
    if (!ret.isSuccess)
    {
        throw new Exception(ret.msg);
    }

    builder.Services.Configure<DbConfigOption>(x => { });
}
else if (Util.IsInstalled)
{
    throw new Exception("Init Db fail");
}

builder.Services.AddTaskServices();
builder.Services.AddHostedService<JobHostedService>();
builder.Services.AddServiceController();
builder.Services.AddOpenApiDocument();
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

var app = builder.Build().Use();
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

app.UseStaticFiles();

app.UseMiddleware<InstallMiddleware>();

app.UseAntiforgery();

app.UseOpenApi();
app.UseReDoc();

app.MapDefaultControllerRoute();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.UseExceptionHandler(applicationBuilder =>
{
    applicationBuilder.Run(context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return Task.CompletedTask;
    });
});
app.Run();