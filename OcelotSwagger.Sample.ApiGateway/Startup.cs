using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace OcelotSwagger.Sample.ApiGateway
{
    using Microsoft.Extensions.Configuration;

    using OcelotSwagger.Configuration;
    using OcelotSwagger.Extensions;

    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // Load options from code
            //services.AddOcelotSwagger(c =>
            //    {
            //        c.Cache.Enabled = true;
            //        c.SwaggerEndPoints.Add(new SwaggerEndPoint { Name = "PET API", Url = "/up/swagger.json" });
            //    });

            // Load options from appsettings.json
            services.Configure<OcelotSwaggerOptions>(this.configuration.GetSection(nameof(OcelotSwaggerOptions)));
            services.AddOcelotSwagger();

            services.AddOcelot();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseOcelotSwagger();

            app.UseOcelot().Wait();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
