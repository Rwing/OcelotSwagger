using Microsoft.AspNetCore.Http;
using Ocelot.Configuration.Repository;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OcelotSwagger
{
    internal class OcelotSwaggerMiddleware
    {
        private readonly OcelotSwaggerConfig _config;
        private readonly IInternalConfigurationRepository _internalConfiguration;
        private readonly RequestDelegate _next;

        public OcelotSwaggerMiddleware(RequestDelegate next, OcelotSwaggerConfig config, IInternalConfigurationRepository internalConfiguration)
        {
            _next = next;
            _config = config;
            _internalConfiguration = internalConfiguration;
        }

        private static readonly Regex PathTemplateRegex = new Regex(@"\{[^\}]+\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value;
            if (_config.SwaggerEndPoints.Exists(i => i.Url == path))
            {
                var existingBody = httpContext.Response.Body;
                using (var newBody = new MemoryStream())
                {
                    // We set the response body to our stream so we can read after the chain of middlewares have been called.
                    httpContext.Response.Body = newBody;

                    await _next(httpContext);

                    // Reset the body so nothing from the latter middlewares goes to the output.
                    httpContext.Response.Body = existingBody;

                    newBody.Seek(0, SeekOrigin.Begin);
                    var newContent = await new StreamReader(newBody).ReadToEndAsync();

                    var ocelotConfig = _internalConfiguration.Get().Data;
                    foreach (var ocelotConfigReRoute in ocelotConfig.ReRoutes)
                    {
                        foreach (var downstreamReRoute in ocelotConfigReRoute.DownstreamReRoute)
                        {
                            var newDownstreamPathTemplate = PathTemplateRegex.Replace(downstreamReRoute.DownstreamDownstreamPathTemplate.Value, "");
                            var newUpstreamPathTemplate = PathTemplateRegex.Replace(downstreamReRoute.UpstreamPathTemplate.OriginalValue, "");
                            newContent = newContent.Replace(newDownstreamPathTemplate, newUpstreamPathTemplate);
                        }
                    }

                    httpContext.Response.ContentLength = newContent.Length;
                    await httpContext.Response.WriteAsync(newContent);
                }
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}