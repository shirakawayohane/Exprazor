namespace Microsoft.Extensions.DependencyInjection;

using Exprazor;
using Exprazor.AspNetCore;
using Exprazor.AspNetCore.Sandbox;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;

public static class ExprazorServiceExtentions
{
    public static void AddExprazor(this IServiceCollection services)
    {
        services.AddSingleton<ExprazorRouter>();
    }
}