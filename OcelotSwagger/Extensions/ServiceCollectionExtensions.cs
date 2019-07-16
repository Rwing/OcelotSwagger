namespace OcelotSwagger.Extensions
{
    using System;

    using Microsoft.Extensions.DependencyInjection;

    using OcelotSwagger.Configuration;

    public static class MvcServiceCollectionExtensions
    {
        public static IServiceCollection AddOcelotSwagger(this IServiceCollection services, Action<OcelotSwaggerOptions> configureOptions)
        {
            services.Configure(configureOptions);
            return services.AddOcelotSwagger();
        }

        public static IServiceCollection AddOcelotSwagger(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSwaggerGen();
            return services;
        }
    }
}