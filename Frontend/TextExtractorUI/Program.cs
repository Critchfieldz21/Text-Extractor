using TextExtractorUI.Components;
using TextExtractorUI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorBootstrap();

// Configure HTTP client for backend API communication
var backendUrl = builder.Configuration["BackendUrl"] ?? "http://localhost:5000";
builder.Services.AddHttpClient<BackendApiService>(client =>
{
    client.BaseAddress = new Uri(backendUrl);
    client.Timeout = TimeSpan.FromSeconds(300); // 5 minute timeout for large PDF processing
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
