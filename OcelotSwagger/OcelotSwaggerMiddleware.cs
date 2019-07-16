namespace OcelotSwagger
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Distributed;

    using Newtonsoft.Json;

    using Ocelot.Configuration.Repository;

    using OcelotSwagger.Configuration;

    internal class OcelotSwaggerMiddleware
    {
        private static readonly Regex PathTemplateRegex = new Regex(
            @"\{[^\}]+\}",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly IDistributedCache _cache;

        private readonly OcelotSwaggerConfig _config;

        private readonly IInternalConfigurationRepository _internalConfiguration;

        private readonly JsonSerializer _jsonSerializer;

        private readonly RequestDelegate _next;

        public OcelotSwaggerMiddleware(
            RequestDelegate next,
            OcelotSwaggerConfig config,
            IInternalConfigurationRepository internalConfiguration,
            IDistributedCache cache)
        {
            this._next = next;
            this._config = config;
            this._internalConfiguration = internalConfiguration;
            this._cache = cache;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value;

            string cacheEntry = null;
            string cacheKey = null;

            if (this._config.Cache?.Enabled == true)
            {
                cacheKey = this._config.Cache.KeyPrefix + WebUtility.UrlEncode(path);
                cacheEntry = await this._cache.GetStringAsync(cacheKey);
            }

            if (cacheEntry != null)
            {
                CachedPathTemplate[] templates;

                using (var jsonReader = new JsonTextReader(new StringReader(cacheEntry)))
                {
                    templates = JsonConvert.DeserializeObject<CachedPathTemplate[]>(cacheEntry);
                }

                var newContent = await this.ReadContentAsync(httpContext);
                newContent = templates.Aggregate(
                    newContent,
                    (current, template) => current.Replace(
                        template.DownstreamPathTemplate,
                        template.UpstreamPathTemplate));
                await this.WriteContentAsync(httpContext, newContent);
            }
            else if (this._config.SwaggerEndPoints.Exists(i => i.Url == path))
            {
                var ocelotConfig = this._internalConfiguration.Get().Data;
                var matchedReRoute = (from i in ocelotConfig.ReRoutes
                                      from j in i.DownstreamReRoute
                                      where j.UpstreamPathTemplate.OriginalValue.Equals(
                                          path,
                                          StringComparison.OrdinalIgnoreCase)
                                      select j).ToList();
                if (matchedReRoute.Count > 0)
                {
                    var matchedHost = matchedReRoute.First().DownstreamAddresses.First();
                    var anotherReRoutes = (from i in ocelotConfig.ReRoutes
                                           from j in i.DownstreamReRoute
                                           where j.DownstreamAddresses.Exists(
                                               k => k.Host == matchedHost.Host && k.Port == matchedHost.Port)
                                           select j).ToList();

                    var templates = this._config.Cache?.Enabled == true
                                        ? new List<CachedPathTemplate>(anotherReRoutes.Count)
                                        : null;

                    var newContent = await this.ReadContentAsync(httpContext);

                    foreach (var downstreamReRoute in anotherReRoutes)
                    {
                        var newDownstreamPathTemplate = PathTemplateRegex.Replace(
                            downstreamReRoute.DownstreamPathTemplate.Value,
                            string.Empty);
                        var newUpstreamPathTemplate = PathTemplateRegex.Replace(
                            downstreamReRoute.UpstreamPathTemplate.OriginalValue,
                            string.Empty);
                        templates?.Add(new CachedPathTemplate(newDownstreamPathTemplate, newUpstreamPathTemplate));
                        newContent = newContent.Replace(newDownstreamPathTemplate, newUpstreamPathTemplate);
                    }

                    if (this._config.Cache?.Enabled == true)
                    {
                        await Task.WhenAll(
                            this._cache.SetStringAsync(
                                cacheKey,
                                JsonConvert.SerializeObject(templates),
                                new DistributedCacheEntryOptions
                                {
                                    SlidingExpiration = TimeSpan.FromSeconds(
                                            this._config.Cache.SlidingExpirationInSeconds)
                                }),
                            this.WriteContentAsync(httpContext, newContent));
                    }
                    else
                    {
                        await this.WriteContentAsync(httpContext, newContent);
                    }
                }
            }
            else
            {
                await this._next(httpContext);
            }
        }

        private async Task<string> ReadContentAsync([NotNull] HttpContext httpContext)
        {
            var existingBody = httpContext.Response.Body;
            using (var newBody = new MemoryStream())
            {
                // We set the response body to our stream so we can read after the chain of middlewares have been called.
                httpContext.Response.Body = newBody;

                await this._next(httpContext);

                // Reset the body so nothing from the latter middlewares goes to the output.
                httpContext.Response.Body = existingBody;

                newBody.Seek(0, SeekOrigin.Begin);
                var newContent = await new StreamReader(newBody).ReadToEndAsync();

                return newContent;
            }
        }

        private async Task WriteContentAsync([NotNull] HttpContext httpContext, string content)
        {
            httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(content);
            await httpContext.Response.WriteAsync(content);
        }
    }
}