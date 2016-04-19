using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DemoApp.Middleware;

namespace DemoApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        
        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();

                //*POI
                //The CustomExceptioHandler middleware doesn't try to recreate the functionality of this middleware component.
                //It does a very good job of displaying the details of an exception.  So, any non-WebAPI exceptions that occur
                //in Debug mode are passed up the pipeline so they can be handled by this middleware component.  When not in
                //Debug mode the CustomExceptioHandler middleware redirects non-WebAPI exceptions to the specified error page.
                app.UseDeveloperExceptionPage();
            }

            //*POI
            //Add the CustomExceptionHandler middleware to the Http pipeline.  The only middleware components that should be above
            //this one are Debug exception handlers and logging components.
            app.UseCustomExceptionHandler(env, new string[] { "/API/" }, "/Home/Error", true, Configuration["Data:DefaultConnection:ConnectionString"]);

            app.UseIISPlatformHandler();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
