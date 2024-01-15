using FreeSql;
using JxAudio.Web.Components;

IFreeSql fsql = new FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=freedb.db")
    .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句
    .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
    .Build();
BaseEntity.Initialization(fsql, null);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
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