namespace OcelotSwagger.Configuration
{
    using System.Collections.Generic;

    public class OcelotSwaggerConfig
    {
        public OcelotSwaggerCacheOptions Cache { get; set; } = new OcelotSwaggerCacheOptions();

        public List<SwaggerEndPoint> SwaggerEndPoints { get; set; } = new List<SwaggerEndPoint>();
    }
}