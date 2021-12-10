using Exprazor;
using Exprazor.AspNetCoreServer;
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
        static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new ClientCommandDeserializer(),
                new ServerCommandSerializer(),
            }
        };

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
                        JsonSerializer.Serialize<ServerCommand>(memory, new HandleCommands(commands), jsonSerializerOptions);
                        await webSocket.SendAsync(memory.ToArray(), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                };

                app.CommandHandler += commandHandler;
                var buffer = new byte[4096];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (!result.CloseStatus.HasValue)
                {
                    var slice = new ArraySegment<byte>(buffer, 0, result.Count);
                    var clientCommand = JsonSerializer.Deserialize<ClientCommand>(slice, jsonSerializerOptions);

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
