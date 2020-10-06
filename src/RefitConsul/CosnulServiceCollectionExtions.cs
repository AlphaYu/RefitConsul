using System;
using Microsoft.Extensions.DependencyInjection;
using Consul;

namespace RefitConsul
{
    public static class CosnulServiceCollectionExtions
    {
        public static IServiceCollection AddConsulServiceProvider(this IServiceCollection services, string consulAddress)
        {
            services.AddTransient(provider =>
            {
                return new ConsulClient(x =>
                {
                    x.Address = new Uri(consulAddress);
                });
            });
            //services.AddTransient<ConsulDiscoveryDelegatingHandler>();
            return services;
        }
    }
}
