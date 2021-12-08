using Exprazor;
using Exprazor.AspNetCore.Sandbox;
using System.Net;
using System.Net.WebSockets;

namespace Microsoft.AspNetCore.Builder
{

    public static partial class ExprazorApplicationBuilderExtentions
    {
        public static void MapExprazor(this IApplicationBuilder app, Action<ExprazorRouter> router)
        {
            var expRouter = app.ApplicationServices.GetRequiredService<ExprazorRouter>();
            router(expRouter);
        }

        public static void UseExprazor(this IApplicationBuilder app)
        {
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
                Action<IEnumerable<DOMCommand>> commandHandler = (commands) =>
                {

                };
                app.CommandHandler += commandHandler;
                var buffer = new byte[4096];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (!result.CloseStatus.HasValue)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                app.CommandHandler -= commandHandler;
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
        }
    }
}
