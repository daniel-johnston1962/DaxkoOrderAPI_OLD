using DaxkoOrderAPI.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;

namespace DaxkoOrderAPI
{
    public class Startup
    {
        private const string API_NAME = "DaxkoOrderAPI";
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public string ApplicationName { get; }
        public string ApplicationVersion { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
            ApplicationName = Assembly.GetExecutingAssembly().GetName().Name;
            ApplicationVersion = "v1";
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContexts(Configuration, Environment);
            services.AddServices(Configuration, Environment);
            services.AddHttpClients(Configuration);
            services.AddAuthentication(Configuration, API_NAME);
            services.AddValidatorServices();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(ApplicationVersion, new Info
                {
                    Version = ApplicationVersion,
                    Title = ApplicationName,
                    Description = $"{ApplicationName} Documentation"
                });

                //c.IncludeXmlComments(GetXmlCommentsPath());
                c.DescribeAllEnumsAsStrings();
            });
            services.AddCors();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.RollingFile("logs/" + ApplicationName + "_log_{Date}.log")
                .CreateLogger();

            app.UseGlobalExceptionHandling(loggerFactory);

            if (env.IsDevelopmentOrLocal())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(
                options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            );
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"../swagger/{ApplicationVersion}/swagger.json", $"{ApplicationName} {ApplicationVersion}");
            });
            app.UseMvc();
        }
    }
}
