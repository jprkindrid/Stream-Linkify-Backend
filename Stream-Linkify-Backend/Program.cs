using Azure.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Scalar.AspNetCore;
using StackExchange.Redis;
using Stream_Linkify_Backend.Extensions;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddAzureWebAppDiagnostics();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

try
{
    var keyVaultName = builder.Configuration["KeyVault:VaultName"];
    Console.WriteLine($"Loading Key Vault: {keyVaultName}");

    var keyVaultUrl = new Uri($"https://{keyVaultName}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUrl, new DefaultAzureCredential());
    Console.WriteLine("Key Vault loaded successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR loading Key Vault: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
    throw;
}


if (builder.Environment.IsDevelopment())
{
    Console.WriteLine(builder.Configuration["Redis:ConnectionString"]);
    builder.Services.AddStackExchangeRedisCache(o =>
    {
        o.Configuration = builder.Configuration["Redis:ConnectionString"];
    });
}
else
{

    var redisHost = builder.Configuration["Redis:Host"];
    var config = ConfigurationOptions.Parse($"{redisHost}:6380,ssl=True,abortConnect=False");

    await config.ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());
    builder.Services.AddStackExchangeRedisCache(o =>
    {
        o.ConfigurationOptions = config;
    });
}
builder.Services.AddScoped<IMusicServiceFactory, MusicServiceFactory>();
builder.Services.AddHttpClient();
builder.Services.AddSpotifyServices();
builder.Services.AddAppleServices();
builder.Services.AddTidalServices();
builder.Services.AddDeezerServices();
builder.Services.AddSoundcloudServices();
builder.Services.AddInputAndResolver();
builder.Services.AddFetcherServices();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

    Console.WriteLine($"IDistributedCache impl: {cache.GetType().FullName}");

    await cache.SetStringAsync(
        "debug:cache:ping",
        "1",
        new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        }
    );
}

Console.WriteLine($"ASPNETCORE_ENVIRONMENT={builder.Environment.EnvironmentName}");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .WithOrigins(
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "https://stream-linkify.pages.dev"));

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.Run();