using FastEndpoints;
using FastEndpoints.Swagger;
using StreamingLlmWebSocket.Services.Client;
using StreamingLlmWebSocket.Services.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddFastEndpoints()
    .AddSwaggerGen();

builder.Services.AddScoped<WsClientService>();
builder.Services.AddScoped<WsServerService>();

var app = builder.Build();
app.UseCors("Open");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseFastEndpoints()
    .UseSwaggerGen();

app.UseWebSockets();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
