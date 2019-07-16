namespace OcelotSwagger.Configuration
{
    public class OcelotSwaggerCacheOptions
    {
        public bool Enabled { get; set; }

        public string KeyPrefix { get; set; } = "OcelotSwagger#";

        public int SlidingExpirationInSeconds { get; set; } = 300;
    }
}