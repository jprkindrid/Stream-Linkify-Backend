using Scalar.AspNetCore;
using Stream_Linkify_Backend.Interfaces;
using Stream_Linkify_Backend.Services;
using Stream_Linkify_Backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Logging.AddConsole();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });


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

app.UseHttpsRedirection();

app.UseCors(x => x
.AllowAnyMethod()
.AllowAnyHeader()
.AllowCredentials()
//.WithOrigins("http://localhost:44351")
.SetIsOriginAllowed(origin => true));

app.UseAuthorization();

app.MapControllers();

app.Run();
