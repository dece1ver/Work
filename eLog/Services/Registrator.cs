using eLog.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace eLog.Services
{
    internal static class Registrator
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<IUserDialogService, WindowsUserDialogService>();

            return services;
        }
    }
}