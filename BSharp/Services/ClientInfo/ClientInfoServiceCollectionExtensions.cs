using BSharp.Services.ClientInfo;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClientInfoServiceCollectionExtensions
    {
        public static IServiceCollection AddClientInfo(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddSingleton<IClientInfoAccessor, ClientInfoAccessor>();
        }
    }
}
