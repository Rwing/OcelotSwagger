using System;
using System.Collections.Generic;
using System.Text;
using OcelotSwagger;

namespace Microsoft.AspNetCore.Builder
{
	public static class BuilderExtensions
	{
		public static IApplicationBuilder UseOcelotSwagger(this IApplicationBuilder app, Action<OcelotSwaggerConfig> configAction)
		{
			var config = new OcelotSwaggerConfig();
			configAction?.Invoke(config);

			app.UseSwagger();
			app.UseSwaggerUI(options =>
			{
				config.SwaggerEndPoints.ForEach(i => options.SwaggerEndpoint(i.Url, i.Name));
			});

			app.UseMiddleware<OcelotSwaggerMiddleware>(config);
			return app;
		}

	}
}
