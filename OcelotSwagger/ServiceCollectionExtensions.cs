using System;
using System.Collections.Generic;
using System.Text;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class MvcServiceCollectionExtensions
	{
		public static IServiceCollection AddOcelotSwagger(this IServiceCollection services)
		{
			services.AddSwaggerGen();
			return services;
		}
	}
}