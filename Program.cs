var builder = WebApplication.CreateBuilder(args);

// Adiciona suporte a Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignora propriedades no C# que não vieram no JSON (ex: Intent no LaunchRequest)
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        // Ignora propriedades no JSON da Amazon que nós não mapeamos no C#
        options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; 
    });
    
// Adiciona os serviços do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura o pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // Define o Swagger para abrir na raiz (opcional, mas prático)
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Scrobbler API v1");
        c.RoutePrefix = string.Empty; 
    });
}

app.MapControllers();

app.Run();