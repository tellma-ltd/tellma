using Microsoft.Extensions.DependencyInjection;
using System;

namespace BSharp.Services.ClientInfo
{
    public static class ClientInfoServiceCollectionExtensions
    {
        public static IServiceCollection AddEmail(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddSingleton<IClientInfoAccessor, ClientInfoAccessor>();
        }
    }
}
