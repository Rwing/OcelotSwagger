using Microsoft.AspNetCore.Http;
using Ocelot.Configuration.Repository;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OcelotSwagger
{
    using System.Collections.Generic;
    using System.Net;

    using FluentValidation.Validators;

    using JetBrains.Annotations;

    using Microsoft.Extensions.Caching.Distributed;

    using Newtonsoft.Json;

    internal class OcelotSwaggerMiddleware
    {
        private readonly OcelotSwaggerConfig _config;

        private readonly IInternalConfigurationRepository _internalConfiguration;

        private readonly IDistributedCache _cache;

        private readonly JsonSerializer _jsonSerializer;

        private readonly RequestDelegate _next;

        private const string CacheEntryTemplate = "9ea79994-9e87-4840-8e0b-dec288726978_{0}";

        public OcelotSwaggerMiddleware(RequestDelegate next, OcelotSwaggerConfig config, IInternalConfigurationRepository internalConfiguration, IDistributedCache cache)
        {
            _next = next;
            _config = config;
            _internalConfiguration = internalConfiguration;
            _cache = cache;
        }

        private static readonly Regex PathTemplateRegex = new Regex(@"\{[^\}]+\}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value;
            var cacheKey = string.Format(CacheEntryTemplate, WebUtility.UrlEncode(path));
            var cacheEntry = await _cache.GetStringAsync(cacheKey);

            if (cacheEntry != null)
            {
                CachedPathTemplate[] templates;

                using (var jsonReader = new JsonTextReader(new StringReader(cacheEntry)))
                {
                    templates = JsonConvert.DeserializeObject<CachedPathTemplate[]>(cacheEntry);
                }

                var newContent = await this.ReadContentAsync(httpContext);
                newContent = templates.Aggregate(newContent, (current, template) => current.Replace(template.DownstreamPathTemplate, template.UpstreamPathTemplate));
                await this.WriteContentAsync(httpContext, newContent);
            }
            else if (_config.SwaggerEndPoints.Exists(i => i.Url == path))
            {
                var ocelotConfig = _internalConfiguration.Get().Data;
                var matchedReRoute = (from i in ocelotConfig.ReRoutes
                                      from j in i.DownstreamReRoute
                                      where j.UpstreamPathTemplate.OriginalValue.Equals(path, StringComparison.OrdinalIgnoreCase)
                                      select j).ToList();
                if (matchedReRoute.Count > 0)
                {
                    var matchedHost = matchedReRoute.First().DownstreamAddresses.First();
                    var anotherReRoutes = (from i in ocelotConfig.ReRoutes
                                           from j in i.DownstreamReRoute
                                           where j.DownstreamAddresses.Exists(k => k.Host == matchedHost.Host && k.Port == matchedHost.Port)
                                           select j).ToList();

                    var templates = new List<CachedPathTemplate>(anotherReRoutes.Count);
                    var newContent = await this.ReadContentAsync(httpContext);

                    foreach (var downstreamReRoute in anotherReRoutes)
                    {
                        var newDownstreamPathTemplate = PathTemplateRegex.Replace(downstreamReRoute.DownstreamPathTemplate.Value, string.Empty);
                        var newUpstreamPathTemplate = PathTemplateRegex.Replace(downstreamReRoute.UpstreamPathTemplate.OriginalValue, string.Empty);
                        templates.Add(new CachedPathTemplate(newDownstreamPathTemplate, newUpstreamPathTemplate));
                        newContent = newContent.Replace(newDownstreamPathTemplate, newUpstreamPathTemplate);
                    }

                    await Task.WhenAll(
                        _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(templates), new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5) }),
                        this.WriteContentAsync(httpContext, newContent));
                }
            }
            else
            {
                await _next(httpContext);
            }
        }

        private async Task<string> ReadContentAsync([NotNull]HttpContext httpContext)
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

                return newContent;
            }
        }

        private async Task WriteContentAsync([NotNull]HttpContext httpContext, string content)
        {
            httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(content);
            await httpContext.Response.WriteAsync(content);
        }
    }
}