using ScrobblerApi.Models.Config;
using ScrobblerApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Adiciona suporte a Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; 
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<LastFmSettings>(builder.Configuration.GetSection("LastFm"));
builder.Services.AddHttpClient();

builder.Services.AddSingleton<LastFmAuthService>();
builder.Services.AddSingleton<ScrobbleService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Scrobbler API v1");
        c.RoutePrefix = string.Empty; 
    });
}

app.MapControllers();

app.Run();