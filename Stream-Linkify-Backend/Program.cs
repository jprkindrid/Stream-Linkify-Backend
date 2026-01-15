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


try
{
    if (!builder.Environment.IsProduction())
    {
        var keyVaultName = builder.Configuration["KeyVault:VaultName"];
        Console.WriteLine($"Loading Key Vault: {keyVaultName}");

        var keyVaultUrl = new Uri(
            $"https://{keyVaultName}.vault.azure.net/");

        builder.Configuration.AddAzureKeyVault(
            keyVaultUrl,
            new DefaultAzureCredential());

        Console.WriteLine("Key Vault loaded successfully");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR loading Key Vault: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");

    Console.WriteLine("WARNING: Continuing without Key Vault");
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
    .WithOrigins(
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "https://stream-linkify.pages.dev")
    .SetIsOriginAllowed(origin => true));

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
//test 
