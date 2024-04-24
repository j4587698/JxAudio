using Front.WebAssembly;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient();
    httpClient.BaseAddress = builder.HostEnvironment.IsDevelopment() ? 
        new Uri("http://localhost:5067/") : new Uri(builder.HostEnvironment.BaseAddress);
    httpClient.DefaultRequestHeaders.Add("credentials", "include");
    return httpClient;
});

builder.Services.AddBootstrapBlazor(options =>
{
    options.IgnoreLocalizerMissing = true;
    options.DefaultCultureInfo = "zh-CN";
});

await builder.Build().RunAsync();