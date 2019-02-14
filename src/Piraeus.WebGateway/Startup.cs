using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Piraeus.Adapters;
using Piraeus.Configuration.Core;
using Piraeus.Configuration.Settings;
using Piraeus.GrainInterfaces;
using Piraeus.WebGateway.ContentFormatters;
using SkunkLab.Security.Authentication;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.WebGateway
{
    public class Startup
    {
        private OrleansConfig config;
        private ProtocolAdapter adapter;
        private PiraeusConfig pconfig;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            config = GetOrleansConfig();
            pconfig = GetPiraeusConfig();

            services.AddMvc()
                .AddXmlSerializerFormatters();
           
            services.AddMvc(o => 
            {
                o.InputFormatters.Add(new PlainTextInputFormatter());
                o.OutputFormatters.Add(new PlainTextOutputFormatter());
                o.InputFormatters.Add(new BinaryInputFormatter());
                o.OutputFormatters.Add(new BinaryOutputFormatter());
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            
            
            services.AddSingleton<PiraeusConfig>(pconfig);           
            services.AddSingleton<IClusterClient>(CreateClusterClient);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            


        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<WebSocketMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Connect}/{action=Index}/{id?}");
            });

            app.UseWebSockets();

            app.Use(async (http, next) =>
            {
                if (http.WebSockets.IsWebSocketRequest)
                {
                    BasicAuthenticator authn = new BasicAuthenticator();
                    http.Response.StatusCode = 101;
                    adapter = ProtocolAdapterFactory.Create(pconfig, http, CancellationToken.None, authn);
                    adapter.OnClose += Adapter_OnClose;
                    adapter.OnError += Adapter_OnError;
                    adapter.Init();
                    await next();
                
                }
                else
                {
                    Console.WriteLine("HTTP Request");
                    await next();
                }
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Log Web Host Service Running...");
            });

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseHsts();
            //}

            //app.UseHttpsRedirection();
            //app.UseMvc();
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            Console.WriteLine(e.Error.Message);
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            Console.WriteLine("closed");
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

        private static OrleansConfig GetOrleansConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(Environment.CurrentDirectory + "\\orleansconfig.json")
                .AddEnvironmentVariables("OR_");

            IConfigurationRoot root = builder.Build();
            OrleansConfig config = new OrleansConfig();
            ConfigurationBinder.Bind(root, config);

            return config;
        }

        private static PiraeusConfig GetPiraeusConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(Environment.CurrentDirectory + "\\piraeusconfig.json")
                .AddEnvironmentVariables("PI_");

            IConfigurationRoot root = builder.Build();
            PiraeusConfig config = new PiraeusConfig();
            ConfigurationBinder.Bind(root, config);

            return config;
        }
    }
}
