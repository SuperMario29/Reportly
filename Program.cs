using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Web;
using Reportly;
using Reportly.Services;
using Reportly.Models;
using Blazored.Modal;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

/* Load configuration
var config = new AppSettings
{
    Shopify = new()
    {
        StoreName = "your-shopify-store",
        ApiKey = "your-shopify-api-key",
        Password = "your-shopify-password"
    },
    Facebook = new()
    {
        AccessToken = "your-fb-access-token",
        AppId = "your-fb-app-id"
    },
    Klaviyo = new()
    {
        ApiKey = "your-klaviyo-api-key"
    }
};*/

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("Role:Admin", policy => 
        policy.RequireClaim(ClaimTypes.Role, "Admin"));
    
    options.AddPolicy("Role:Support", policy => 
        policy.RequireClaim(ClaimTypes.Role, "Support"));
    
    options.AddPolicy("Role:Client", policy => 
        policy.RequireClaim(ClaimTypes.Role, "Client"));
});

// Register custom auth state provider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

builder.Services.AddSingleton(config);
builder.Services.AddHttpClient();
builder.Services.AddScoped<ShopifyService>();
builder.Services.AddScoped<FacebookService>();
builder.Services.AddScoped<KlaviyoService>();
builder.Services.AddBlazoredModal();

/*RecurringJob.AddOrUpdate<SqlBackupService>(
    "daily-backup",
    x => x.PerformBackup(),
    Cron.Daily);*/

var app = builder.Build();
await app.RunAsync();

