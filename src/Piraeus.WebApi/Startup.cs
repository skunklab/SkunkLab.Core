using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Capl.Authorization.Matching;
using Capl.Authorization.Operations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Piraeus.Configuration;
using Piraeus.GrainInterfaces;
using Piraeus.WebApi.Security;
using SkunkLab.Security.Authentication;
//using Orleans.Clustering.Redis;
using Piraeus.Extensions.Configuration;
using SkunkLab.Storage;
using Microsoft.AspNetCore.HttpOverrides;

namespace Piraeus.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;           
        }

        private OrleansConfig config;
        private PiraeusConfig pconfig;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            config = GetOrleansConfig();
            pconfig = GetPiraeusConfig();
            //Capl.Authorization.AuthorizationPolicy authzPolicy = GetCaplPolicy();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest)
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

                        ValidIssuer = pconfig.ManagementApiIssuer,
                        ValidAudience = pconfig.ManagementApiAudience,
                        ClockSkew = TimeSpan.FromMinutes(5.0),
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(pconfig.ManagmentApiSymmetricKey))
                    };
                });

            PskStorageAdapter pskAdpater = GetPskAdapter();

           
            if(pskAdpater != null)
            {
                services.AddSingleton<PskStorageAdapter>(pskAdpater);
            }

            services.AddSingleton<PiraeusConfig>(pconfig);
            services.AddSingleton<IClusterClient>(CreateClusterClient);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddRouting();
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("Access", policy =>
            //        policy.Requirements.Add(new ApiAccessRequirement(authzPolicy)));
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //else
            //{
            //    app.UseHsts();
            //}

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            app.UseHttpsRedirection();
            //app.UseMvcWithDefaultRoute();
            //app.UseMvc();
            

            app.UseMvc(routes =>
            {
                routes.MapRoute("ExplictApi", "{controller}/{action}");
                routes.MapRoute("TokenApi", "{controller=Manage}/{id}");
            });
        }


        private OrleansConfig GetOrleansConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("./orleansconfig.json")
                .AddEnvironmentVariables("OR_");

            IConfigurationRoot root = builder.Build();
            OrleansConfig config = new OrleansConfig();
            ConfigurationBinder.Bind(root, config);

            return config;
        }

        private PiraeusConfig GetPiraeusConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("./piraeusconfig.json")
                .AddEnvironmentVariables("PI_");

            IConfigurationRoot root = builder.Build();
            PiraeusConfig pc = new PiraeusConfig();
            ConfigurationBinder.Bind(root, pc);

            return pc;
        }

        private PskStorageAdapter GetPskAdapter()
        {
            if(!string.IsNullOrEmpty(pconfig.PskRedisConnectionString))
            {
                return PskStorageAdapterFactory.Create(pconfig.PskRedisConnectionString);
            }

            if(!string.IsNullOrEmpty(pconfig.PskKeyVaultClientSecret) && !string.IsNullOrEmpty(pconfig.PskKeyVaultClientId) && !string.IsNullOrEmpty(pconfig.PskKeyVaultAuthority))
            {
                return PskStorageAdapterFactory.Create(pconfig.PskKeyVaultAuthority, pconfig.PskKeyVaultClientId, pconfig.PskKeyVaultClientSecret);
            }

            if(!string.IsNullOrEmpty(pconfig.PskKeys))
            {
                return PskStorageAdapterFactory.Create(pconfig.PskIdentities, pconfig.PskKeys);
            }

            return null;
        }

        private IClusterClient CreateClusterClient(IServiceProvider serviceProvider)
        {
            var log = serviceProvider.GetService<ILogger<Startup>>();
            if (config.Dockerized)
            {
                var clientBuilder = new ClientBuilder()
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPiSystem).Assembly))
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = config.ClusterId;
                        options.ServiceId = config.ServiceId;
                    });

                //client.UseRedisGatewayListProvider(options => options.ConnectionString = config.DataConnectionString);
                //clientBuilder.UseRedisGatewayListProvider(options => options.ConnectionString = config.DataConnectionString);
                
                clientBuilder.UseAzureStorageClustering(options => options.ConnectionString = config.DataConnectionString);
                //.UseRedisGatewayListProvider(logger, options =>
                //    options.ConnectionString = "piraeus.redis.cache.windows.net:6380,password=y4fGNTZkH+NI2Msz0yiH8Q+WFICgE1yO1FKysaL97oA=,ssl=True,abortConnect=False"
                //)
                var client = clientBuilder.Build();

                //.UseAzureStorageClustering(options => options.ConnectionString = config.OrleansDataConnectionString)

                client.Connect(RetryFilter).GetAwaiter().GetResult();
                return client;

                
            }
            else
            {
                var localClient = new ClientBuilder()
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPiSystem).Assembly))
                .UseLocalhostClustering()
                .Build();

                localClient.Connect(RetryFilter);
                return localClient;

                //var loggerFactory = new LoggerFactory().AddFilter(new Dictionary<string, LogLevel>
                //{
                //    { nameof(RedisGatewayListProvider) , LogLevel.Debug },
                //})
                //.AddConsole()
                //.CreateLogger(nameof(RedisGatewayListProvider));



                //ILoggerFactory loggerFactory = new LoggerFactory();
                //loggerFactory.AddConsole(LogLevel.Debug);
                //loggerFactory.AddDebug();
                //loggerFactory.AddDebug();
                //ILogger<RedisGatewayListProvider> logger = loggerFactory.CreateLogger<RedisGatewayListProvider>();



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
