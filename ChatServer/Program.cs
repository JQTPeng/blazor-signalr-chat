using ChatServer.Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR().AddJsonProtocol(opts => { opts.PayloadSerializerOptions.PropertyNamingPolicy = null; });
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("CorsPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:8000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors("CorsPolicy");
app.UseRouting();
// Add authentication or authorization between routing & endpoint
app.MapGet("/", () => "Change 13");
app.MapHub<ChatHub>("/chat");
app.Run();