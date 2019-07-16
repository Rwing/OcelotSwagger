namespace OcelotSwagger
{
    using Microsoft.Extensions.DependencyInjection;

    public static class MvcServiceCollectionExtensions
    {
        public static IServiceCollection AddOcelotSwagger(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSwaggerGen();
            return services;
        }
    }
}