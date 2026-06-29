namespace Respondr.Application;

using Microsoft.Extensions.DependencyInjection;
using Respondr.Application.Realtime;

public static class DependencyInjection
{
    public static IServiceCollection AddRespondrApplication(this IServiceCollection services)
    {
        services.AddSingleton<IRealtimePublisher, NoOpRealtimePublisher>();
        return services;
    }
}
