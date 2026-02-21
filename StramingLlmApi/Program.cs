using System.ClientModel;
using FastEndpoints;
using OpenAI.Chat;
using StramingLlmApi.Services;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

var config = builder.Configuration;

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFastEndpoints();

builder.Services.AddSingleton<AiChatService>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddSingleton(new ChatClient(
    model: "gpt-5",
    credential: new ApiKeyCredential(config["ApiKey"]!)
));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseWebSockets();

app.UseAuthorization();
app.UseFastEndpoints();
app.MapControllers();

app.Run();
