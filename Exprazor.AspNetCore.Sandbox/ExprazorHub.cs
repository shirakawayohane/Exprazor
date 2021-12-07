using Microsoft.AspNetCore.SignalR;
using System.Runtime.InteropServices;

namespace Exprazor.AspNetCore;
using Id = System.Int64;
public class ExprazorHub : Hub
{
    ExprazorApp app { get; }
    public ExprazorHub(ExprazorApp app)
    {
        this.app = app;
    }

    public override Task OnConnectedAsync()
    {
        app.Initialize(commands =>
        {
            Clients.Caller.SendAsync("onCommands", commands);
        });

        return Task.CompletedTask;
    }

    public Task InvokeVoid(Id nodeId, string key)
    {
        app.InvokeVoidCallback(nodeId, key);

        return Task.CompletedTask;
    }
}