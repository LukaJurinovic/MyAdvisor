using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyAdvisor.Client;
using MyAdvisor.Client.Handlers;
using MyAdvisor.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = new Uri("http://localhost:5027");

builder.Services.AddScoped(sp =>
    new AuthHandler(
        sp.GetRequiredService<Microsoft.JSInterop.IJSRuntime>(),
        sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>(),
        apiBase));

builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler) { BaseAddress = apiBase };
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DiaryService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<AiImportService>();
builder.Services.AddScoped<ChatService>();

await builder.Build().RunAsync();
