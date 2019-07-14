namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using OcelotSwagger;

    public static class MvcServiceCollectionExtensions
	{
		public static IServiceCollection AddOcelotSwagger(this IServiceCollection services)
		{
            services.TryAddSingleton<IDistributedCache, NullDistributedCache>();
			services.AddSwaggerGen();
			return services;
		}
	}
}