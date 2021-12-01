using Microsoft.AspNetCore.SignalR;
using System.Runtime.InteropServices;

namespace Exprazor.AspNetCore;
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

    public async Task SendAction(long actionPtr)
    {
        // UpdateAction
        var action = (((GCHandle)new IntPtr(actionPtr)).Target as Action);
    }
}