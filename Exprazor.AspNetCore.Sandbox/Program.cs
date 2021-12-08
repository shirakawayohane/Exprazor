using Exprazor;
using Exprazor.AspNetCore;
using Exprazor.AspNetCore.Sandbox;
using Exprazor.AspNetCore.Sandbox.Examples;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalRCore();
builder.Services.AddExprazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapExprazor(router =>
{
    router.Route("/counter/(\\d+)", matches =>
    {
        return ExprazorApp.Create<Counter>(new CounterProps(int.Parse(matches![0])));
    });
});
app.UseExprazor();
app.MapFallbackToFile("index.html");
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.Run();
