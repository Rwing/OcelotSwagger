namespace OcelotSwagger.Extensions
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using OcelotSwagger.Configuration;

    public static class BuilderExtensions
    {
        public static IApplicationBuilder UseOcelotSwagger(this IApplicationBuilder app)
        {
            var optionsAccessor = app.ApplicationServices.GetRequiredService<IOptionsMonitor<OcelotSwaggerOptions>>();

            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                    {
                        optionsAccessor.CurrentValue.SwaggerEndPoints.ForEach(
                            i => options.SwaggerEndpoint(i.Url, i.Name));
                    });

            app.UseMiddleware<OcelotSwaggerMiddleware>();
            return app;
        }
    }
}