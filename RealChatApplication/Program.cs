using Microsoft.AspNetCore.Builder;
using RealChatApplication.Models;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
var webSOcketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromSeconds(2),
};

app.UseWebSockets(webSOcketOptions);

app.Map("/ws", appBuilder =>
{
    appBuilder.Use(async (context, next) =>
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var websocket = await context.WebSockets.AcceptWebSocketAsync();
            var handler = new WebSocketChatHandler();

            await handler.HandleAsync(context, websocket);
        }
        else
        {
            await next();
        }
    });
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
