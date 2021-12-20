using Exprazor;
using Exprazor.AspNetCore;
using Exprazor.AspNetCore.Sandbox;
using Exprazor.AspNetCore.Sandbox.Examples;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole();
builder.Services.AddExprazor();

// For DI check.
builder.Services.AddTransient<IGetMessageService, GetMessageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapExprazor(router =>
{
    router.Route("/counter/?(\\d+)?", matches =>
    {
        if(matches == null || matches.Length == 0) return ExprazorApp.Create<Counter>(new CounterProps(0));
        return ExprazorApp.Create<Counter>(new CounterProps(int.Parse(matches![0])));
    });

    router.Route("/input/?", _ => ExprazorApp.Create<Input>(Unit.Instance));

    router.Route("/dicheck/?", _ => ExprazorApp.Create<DICheck>(Unit.Instance));

    router.Route("/tsinteropcheck/?", _ => ExprazorApp.Create<TSInteropCheck>(Unit.Instance));

    router.Route(".*", _ => ExprazorApp.Create<NotFound>(Unit.Instance));
});
app.UseExprazor();
app.MapFallbackToFile("index.html");

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.Run();