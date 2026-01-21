using RealTimeDemo.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("dev", policy =>
        policy.WithOrigins(
            "http://localhost:5024",
            "http://localhost:4200",
            "http://localhost:3000",
            "null"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors("dev");
app.MapHub<ChatHub>("/hubs/chat");
app.MapGet("/", () => "Hello");

app.Run();