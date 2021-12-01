namespace Microsoft.Extensions.DependencyInjection;

using Exprazor;
using Exprazor.AspNetCore;
using Microsoft.AspNetCore.SignalR;
public static class ExprazorServiceExtentions
{
    public static void AddExprazor(this IServiceCollection services) {
        services.AddSignalRCore();
        services.AddScoped<ExprazorApp>();
    }
}

public static class ExprazorMiddlewares
{
    public static void UseExprazor(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ExprazorHub>("/exprazorhub");
        });
    }
}
