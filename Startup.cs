using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HangfireMemoryLeak
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddHangfire(x => {

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            var connectionString = "connection string";

            var opt = new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = false,
                SchemaName = "Hangfire_test",
                QueuePollInterval = TimeSpan.FromSeconds(1)
            };
            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString, opt);
            using (var connection = new SqlConnection(connectionString))
            {
                SqlServerObjectsInstaller.Install(connection, opt.SchemaName);
            }
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Queues = new[] { "test" }
            });

            app.UseMvc();
        }
    }
}
