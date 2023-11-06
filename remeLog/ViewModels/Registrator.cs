using Microsoft.Extensions.DependencyInjection;

namespace remeLog.ViewModels;

internal static class Registrator
{
    public static IServiceCollection RegisterViewModels(this IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();

        return services;
    }
}
