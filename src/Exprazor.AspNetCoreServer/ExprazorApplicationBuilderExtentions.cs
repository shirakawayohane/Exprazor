using Exprazor;
using Exprazor.AspNetCoreServer;
using MessagePack;
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
                    if (exprazorApp != null)
                    {
                        using (var webSocket = await context.WebSockets.AcceptWebSocketAsync())
                        {
                            await HandleConnection(context, webSocket, exprazorApp);
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

            async Task HandleConnection(HttpContext context, WebSocket webSocket, ExprazorApp app)
            {
                Func<IEnumerable<DOMCommand>,Task> commandHandler = async (commands) =>
                {
                    using (var memory = new MemoryStream()) {
#if DEBUG
                        Console.WriteLine(MessagePackSerializer.ConvertToJson(MessagePackSerializer.Serialize<FromServerCommand>(new HandleCommands(commands))));
#endif
                        await MessagePackSerializer.SerializeAsync<FromServerCommand>(memory, new HandleCommands(commands));
                        var deserialized = MessagePackSerializer.Deserialize<object>(memory.ToArray());
                        await webSocket.SendAsync(memory.ToArray(), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                };

                app.CommandHandler += commandHandler;
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

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                app.CommandHandler -= commandHandler;
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
        }
    }
}
