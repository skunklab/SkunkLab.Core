using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Piraeus.Configuration;
using Piraeus.GrainInterfaces;
using Piraeus.HttpGateway.Middleware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace Piraeus.HttpGateway
{
    public class Startup
    {
        private OrleansConfig config;
        private PiraeusConfig pconfig;

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseHsts();
            //}

            //app.UseAuthentication();
            app.UseMiddleware<PiraeusHttpMiddleware>();
            //app.UseStaticFiles();

            app.UseMvc();

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute("ApiRoute", "{controller=Connect}/{id}");
            //});

            //app.Use(async (context, next) =>
            //{
            //    var p = new PiraeusWebSocketMiddleware(null, pconfig);
            //    await p.Invoke(context);
            //    //await next.Invoke();                

            //    //try
            //    //{
            //    //    await next.Invoke();                 
            //    //}
            //    //catch (BadHttpRequestException ex) when (ex.StatusCode == StatusCodes.Status413RequestEntityTooLarge) { }
            //});


        }

        public void ConfigureServices(IServiceCollection services)
        {           
            pconfig = GetPiraeusConfig();
            config = GetOrleansConfig();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            if(config.Dockerized && !string.IsNullOrEmpty(pconfig.ClientIssuer))
            {
                services.AddAuthentication()
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = !string.IsNullOrEmpty(pconfig.ClientIssuer),
                            ValidateAudience = !string.IsNullOrEmpty(pconfig.ClientAudience),
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = pconfig.ClientIssuer,
                            ValidAudience = pconfig.ClientAudience,
                            ClockSkew = TimeSpan.FromMinutes(5.0),
                            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(pconfig.ClientSymmetricKey))
                        };
                    });
            }

           
            services.AddSingleton<PiraeusConfig>(pconfig);
            services.AddSingleton<IClusterClient>(CreateClusterClient);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddRouting();

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
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPiSystem).Assembly))
                .UseLocalhostClustering()
                .Build();

                localClient.Connect(RetryFilter);
                return localClient;
            }
            else
            {
                var client = new ClientBuilder()
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPiSystem).Assembly))
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = config.ClusterId;
                        options.ServiceId = config.ServiceId;
                    })
                    .UseAzureStorageClustering(options => options.ConnectionString = config.DataConnectionString)
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
