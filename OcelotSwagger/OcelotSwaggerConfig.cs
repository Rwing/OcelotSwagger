using System;
using System.Collections.Generic;
using System.Text;

namespace OcelotSwagger
{
    public class OcelotSwaggerConfig
    {
	    public OcelotSwaggerConfig()
	    {
		    SwaggerEndPoints = new List<SwaggerEndPoint>();
	    }

	    public List<SwaggerEndPoint> SwaggerEndPoints { get; set; }
    }

	public class SwaggerEndPoint
	{
		public string Url { get; set; }
		public string Name { get; set; }
	}
}
