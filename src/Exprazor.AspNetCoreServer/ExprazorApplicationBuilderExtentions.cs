﻿using Exprazor;
using Exprazor.AspNetCoreServer;
using MessagePack;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;

namespace Microsoft.AspNetCore.Builder
{
    internal class Exprazor { }

    public static partial class ExprazorBuilderExtentions
    {
        public static void MapExprazor(this IApplicationBuilder app, Action<ExprazorRouter> router)
        {
            var expRouter = app.ApplicationServices.GetRequiredService<ExprazorRouter>();
            router(expRouter);
        }

        public static void UseExprazor(this IApplicationBuilder app)
        {
            // Switch dev/prod of js client.
            app.UseRewriter(new RewriteOptions()
#if DEBUG
                .AddRedirect("_framework/exprazor.server.js", "_content/Exprazor.AspNetCoreServer/exprazor.server.dev.js")
#else
                .AddRedirect("_framework/exprazor.server.js", "_content/Exprazor.AspNetCoreServer/exprazor.server.js")
#endif
                );

            ExprazorApp.SetDIResolver(t => app.ApplicationServices.GetService(t)!);

            app.UseWebSockets();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value == null)
                {
                    await next();
                    return;
                }
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var exprazorApp = app.ApplicationServices.GetRequiredService<ExprazorRouter>().Get(context.Request.Path.Value);

                    var jsInvoker = app.ApplicationServices.GetRequiredService<IJSInvoker>();

                    if (exprazorApp != null)
                    {
                        using (var webSocket = await context.WebSockets.AcceptWebSocketAsync())
                        {
                            await HandleConnection(context, webSocket, exprazorApp, jsInvoker);
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    await next();
                }
            });

            async Task HandleConnection(HttpContext context, WebSocket webSocket, ExprazorApp app, IJSInvoker invoker)
            {
                Func<IEnumerable<DOMCommand>,Task> commandHandler = async (commands) =>
                {
                    using (var memory = new MemoryStream()) {
#if DEBUG
                        Console.WriteLine(MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize<FromServerCommand>(new HandleCommands(commands))));
#endif
                        await MessagePackSerializer.SerializeAsync<FromServerCommand>(memory, new HandleCommands(commands));
                        await webSocket.SendAsync(memory.ToArray(), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                };

                app.DOMCommandHandler += commandHandler;

                invoker.OnInvokeVoid += async (command) =>
                {
                    using (var memory = new MemoryStream())
                    {
                        var buffer = MessagePackSerializer.Serialize<FromServerCommand>(new InvokeClientSideVoid(command.FunctionName, command.Args));
                        await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                };

                var buffer = new byte[4096];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (!result.CloseStatus.HasValue)
                {
                    var slice = new ArraySegment<byte>(buffer, 0, result.Count);
                    var clientCommand = MessagePackSerializer.Deserialize<FromClientCommand>(slice);

                    if (clientCommand is Connected connected)
                    {
                        app.Start();
                    }
                    else if(clientCommand is InvokeVoid invokeVoid)
                    {
                        app.InvokeVoidCallback(invokeVoid.Id, invokeVoid.Key);
                    }
                    else if(clientCommand is InvokeWithString invokeWithString)
                    {
                        app.InvokeStringCallback(invokeWithString.Id, invokeWithString.Key, invokeWithString.Argument);
                    }

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                app.DOMCommandHandler -= commandHandler;
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
        }
    }
}
