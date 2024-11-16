using Microsoft.Extensions.DependencyInjection;

namespace GamestatsBase;
public static class GamestatsServiceExtensions
{
    public static IServiceCollection AddGamestatsBaseServices(this IServiceCollection services)
    {
        services.AddSingleton<GamestatsSessionManager>();

        return services;
    }
}
