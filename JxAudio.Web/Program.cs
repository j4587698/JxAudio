using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Net;
using BootstrapBlazor.Components;
using FreeSql;
using JxAudio.Core;
using JxAudio.Core.Extensions;
using JxAudio.Core.Options;
using JxAudio.Web.Components;
using JxAudio.Web.Middlewares;
using JxAudio.Web.Services;
using JxAudio.Web.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Serilog;
using Yarp.ReverseProxy.Forwarder;
using Console = System.Console;
using Constants = JxAudio.Core.Constants;
using SearchOption = System.IO.SearchOption;

Log.Logger = new LoggerConfiguration().WriteTo
    .File("./log/log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
    .WriteTo.Console().CreateLogger();

var builder = WebApplication.CreateBuilder(args).Inject(configOption =>
{
    configOption.ConfigSearchFolder = ["config"];
    configOption.DynamicPrefix = "/api/";
});
#if DEBUG
builder.Services.AddHttpForwarder();
#endif
builder.Host.UseSerilog();

var dbConfigOption = Application.GetValue<DbConfigOption>("Db");
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

Constants.AesKey = Application.GetValue("JxAudio:AesKey");

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

builder.Services.Configure<CookiePolicyOptions>(op =>
{
    op.CheckConsentNeeded = _ => true;
    op.MinimumSameSitePolicy = SameSiteMode.None;
    op.Secure = CookieSecurePolicy.None;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/denied";
    options.Cookie.Name = "JxAudio";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.Path = "/";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .WithOrigins("https://localhost:7077") // 允许任何来源
            .AllowAnyMethod() // 允许任何 HTTP 方法
            .AllowAnyHeader() // 允许任何头
            .AllowCredentials();
    });
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

app.UseCors("AllowAll");

//app.UseHttpsRedirection();
app.UseStatusCodePages();

app.UseStaticFiles();


app.UseMiddleware<InstallMiddleware>();

app.UseAntiforgery();

app.UseOpenApi();
app.UseSwaggerUi();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

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
#if DEBUG
var httpClient = new HttpMessageInvoker(new SocketsHttpHandler
{
    UseProxy = false,
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.None,
    UseCookies = false,
    EnableMultipleHttp2Connections = true,
    ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
    ConnectTimeout = TimeSpan.FromSeconds(15),
});

// Setup our own request transform class
var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
app.MapForwarder("{**catch-all}", "http://localhost:5077", requestOptions,HttpTransformer.Default,  httpClient);
#endif
app.Run();