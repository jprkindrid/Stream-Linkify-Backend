using Scalar.AspNetCore;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Services;
using Stream_Linkify_Backend.Extensions;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });


if (!builder.Environment.IsDevelopment())
{
    var keyVaultName = builder.Configuration["KeyVault:VaultName"];

    if (string.IsNullOrEmpty(keyVaultName))
    {
        throw new InvalidOperationException(
            "KeyVault:VaultName not configured in appsettings.json");
    }

    var keyVaultUrl = new Uri(
        $"https://{keyVaultName}.vault.azure.net/");

    try
    {
        builder.Configuration.AddAzureKeyVault(
            keyVaultUrl,
            new DefaultAzureCredential());
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException(
            $"Failed to connect to Key Vault at {keyVaultUrl}: {ex.Message}", ex);
    }
}


// created services
builder.Services.AddScoped<IMusicServiceFactory, MusicServiceFactory>();

builder.Services.AddHttpClient();
builder.Services.AddSpotifyServices();
builder.Services.AddAppleServices();
builder.Services.AddTidalServices();
builder.Services.AddDeezerServices();
builder.Services.AddInputAndResolver();
builder.Services.AddFetcherServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.UseCors(x => x
.AllowAnyMethod()
.AllowAnyHeader()
.AllowCredentials()
.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
.SetIsOriginAllowed(origin => true));

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
//test 
