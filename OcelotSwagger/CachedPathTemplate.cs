namespace OcelotSwagger
{
    public class CachedPathTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedPathTemplate"/> class.
        /// </summary>
        public CachedPathTemplate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedPathTemplate"/> class.
        /// </summary>
        /// <param name="downstreamPathTemplate">The downstream path template.</param>
        /// <param name="upstreamPathTemplate">The upstream path template.</param>
        public CachedPathTemplate(string downstreamPathTemplate, string upstreamPathTemplate)
        {
            this.DownstreamPathTemplate = downstreamPathTemplate;
            this.UpstreamPathTemplate = upstreamPathTemplate;
        }

        /// <summary>
        /// Gets or sets the downstream path template.
        /// </summary>
        /// <value>
        /// The downstream path template.
        /// </value>
        public string DownstreamPathTemplate { get; set; }

        /// <summary>
        /// Gets or sets the upstream path template.
        /// </summary>
        /// <value>
        /// The upstream path template.
        /// </value>
        public string UpstreamPathTemplate { get; set; }
    }
}
