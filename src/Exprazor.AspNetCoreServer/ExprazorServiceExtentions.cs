using Exprazor.AspNetCoreServer;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExprazorServiceExtentions
{
    public static void AddExprazor(this IServiceCollection services)
    {
        services.AddSingleton<ExprazorRouter>();
    }
}