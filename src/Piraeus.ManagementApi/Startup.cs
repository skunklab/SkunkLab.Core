using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Rest.Azure;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Piraeus.Configuration.Core;
using Piraeus.Configuration.Settings;
using System;
using System.Threading.Tasks;

namespace Piraeus.ManagementApi
{
    public class Startup
    {
        private OrleansConfig config;
        private PiraeusConfig pconfig;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {

            config = GetOrleansConfig();
            pconfig = GetPiraeusConfig();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddXmlSerializerFormatters();


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = pconfig.Security.WebApi.Issuer,
                        ValidAudience = pconfig.Security.WebApi.Audience,
                        ClockSkew = TimeSpan.FromMinutes(5.0),
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(pconfig.Security.WebApi.SymmetricKey))
                    };
                });

            services.AddSingleton<PiraeusConfig>(pconfig);
            services.AddSingleton<IClusterClient>(CreateClusterClient);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseHsts();
            }

            app.UseAuthentication();
            //app.UseHttpsRedirection();
            app.UseMvc();

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute("ExplictApi", "{controller}/{action}");
            //    routes.MapRoute("TokenApi", "{controller=Manage}/{id}");
            //});


        }

        private OrleansConfig GetOrleansConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(Environment.CurrentDirectory + "\\orleansconfig.json")
                .AddEnvironmentVariables("OR_");

            IConfigurationRoot root = builder.Build();
            OrleansConfig config = new OrleansConfig();
            ConfigurationBinder.Bind(root, config);

            return config;
        }

        private PiraeusConfig GetPiraeusConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(Environment.CurrentDirectory + "\\piraeusconfig.json")
                .AddEnvironmentVariables("PI_");

            IConfigurationRoot root = builder.Build();
            PiraeusConfig pc = new PiraeusConfig();
            ConfigurationBinder.Bind(root, pc);

            return pc;
        }

        private IClusterClient CreateClusterClient(IServiceProvider serviceProvider)
        {
            var log = serviceProvider.GetService<ILogger<Startup>>();
            if (!config.Dockerized)
            {
                var localClient = new ClientBuilder()
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
                .UseLocalhostClustering()
                .Build();

                localClient.Connect(RetryFilter);
                return localClient;
            }
            else
            {
                var client = new ClientBuilder()
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = config.OrleansClusterId;
                        options.ServiceId = config.OrleansServiceId;
                    })
                    .UseAzureStorageClustering(options => options.ConnectionString = config.OrleansDataConnectionString)
                    .Build();

                client.Connect(RetryFilter).GetAwaiter().GetResult();
                return client;
            }
            async Task<bool> RetryFilter(Exception exception)
            {
                log?.LogWarning("Exception while attempting to connect to Orleans cluster: {Exception}", exception);
                await Task.Delay(TimeSpan.FromSeconds(2));
                return true;
            }
        }
    }
}
