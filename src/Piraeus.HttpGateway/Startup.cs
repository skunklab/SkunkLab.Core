using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Piraeus.Configuration;
using Piraeus.Core.Logging;
using Piraeus.Extensions.Configuration;
using Piraeus.Extensions.Logging;
using Piraeus.Extensions.Orleans;
using Piraeus.HttpGateway.Formatters;
using Piraeus.HttpGateway.Middleware;
using System;


namespace Piraeus.HttpGateway
{
    public class Startup
    {
      
        //public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        //{
        //    //if (env.IsDevelopment())
        //    //{
        //    //    app.UseDeveloperExceptionPage();
        //    //}
        //    //else
        //    //{
        //    //    app.UseHsts();
        //    //}

        //    //app.UseAuthentication();
        //    app.UseMiddleware<PiraeusHttpMiddleware>();
        //    //app.UseStaticFiles();

        //    app.UseMvc();

        //    //app.UseMvc(routes =>
        //    //{
        //    //    routes.MapRoute("ApiRoute", "{controller=Connect}/{id}");
        //    //});

        //    //app.Use(async (context, next) =>
        //    //{
        //    //    var p = new PiraeusWebSocketMiddleware(null, pconfig);
        //    //    await p.Invoke(context);
        //    //    //await next.Invoke();                

        //    //    //try
        //    //    //{
        //    //    //    await next.Invoke();                 
        //    //    //}
        //    //    //catch (BadHttpRequestException ex) when (ex.StatusCode == StatusCodes.Status413RequestEntityTooLarge) { }
        //    //});


        //}
        public void Configure(IApplicationBuilder app)
        {

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });


            app.UseAuthentication();
            app.UseMiddleware<PiraeusHttpMiddleware>();
            

            //app.UseMvc();

        }

        public void ConfigureServices(IServiceCollection services)
        {
            //PiraeusConfig config;
            //OrleansConfig orleansConfig;
            services.AddPiraeusConfiguration(out PiraeusConfig config);
            services.AddOrleansConfiguration(out OrleansConfig orleansConfig);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingletonOrleansClusterClient(orleansConfig);
            LoggerType loggers = config.GetLoggerTypes();

            if (loggers.HasFlag(Piraeus.Configuration.LoggerType.AppInsights))
            {
                services.AddApplicationInsightsTelemetry(op =>
                {
                    op.InstrumentationKey = config.AppInsightsKey;
                    op.AddAutoCollectedMetricExtractor = true;
                    op.EnableHeartbeat = true;
                });
            }
            services.AddLogging(builder => builder.AddLogging(config));
            services.AddSingleton<Logger>();
            services.AddTransient<PiraeusHttpMiddleware>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = !string.IsNullOrEmpty(config.ClientIssuer),
                        ValidateAudience = !string.IsNullOrEmpty(config.ClientAudience),
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config.ClientIssuer,
                        ValidAudience = config.ClientAudience,
                        ClockSkew = TimeSpan.FromMinutes(5.0),
                        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(config.ClientSymmetricKey))
                    };
                });

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                options.InputFormatters.Add(new BinaryInputFormatter());
                options.InputFormatters.Add(new PlainTextInputFormatter());
                options.InputFormatters.Add(new XmlSerializerInputFormatter(options));
                options.OutputFormatters.Add(new BinaryOutputFormatter());
                options.OutputFormatters.Add(new PlainTextOutputFormatter());
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
            });
            services.AddRouting();
            services.AddMvcCore();
        }
        //public void ConfigureServices(IServiceCollection services)
        //{
        //    pconfig = GetPiraeusConfig();
        //    config = GetOrleansConfig();
        //    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

        //    if (config.Dockerized && !string.IsNullOrEmpty(pconfig.ClientIssuer))
        //    {
        //        services.AddAuthentication()
        //            .AddJwtBearer(options =>
        //            {
        //                options.TokenValidationParameters = new TokenValidationParameters
        //                {
        //                    ValidateIssuer = !string.IsNullOrEmpty(pconfig.ClientIssuer),
        //                    ValidateAudience = !string.IsNullOrEmpty(pconfig.ClientAudience),
        //                    ValidateLifetime = true,
        //                    ValidateIssuerSigningKey = true,

        //                    ValidIssuer = pconfig.ClientIssuer,
        //                    ValidAudience = pconfig.ClientAudience,
        //                    ClockSkew = TimeSpan.FromMinutes(5.0),
        //                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(pconfig.ClientSymmetricKey))
        //                };
        //            });
        //    }


        //    services.AddSingleton<PiraeusConfig>(pconfig);
        //    services.AddSingleton<IClusterClient>(CreateClusterClient);
        //    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        //    services.AddRouting();

        //}

        //private OrleansConfig GetOrleansConfig()
        //{
        //    var builder = new ConfigurationBuilder()
        //        .AddJsonFile(Environment.CurrentDirectory + "\\orleansconfig.json")
        //        .AddEnvironmentVariables("OR_");

        //    IConfigurationRoot root = builder.Build();
        //    OrleansConfig config = new OrleansConfig();
        //    ConfigurationBinder.Bind(root, config);

        //    return config;
        //}



        //private PiraeusConfig GetPiraeusConfig()
        //{
        //    var builder = new ConfigurationBuilder()
        //        .AddJsonFile(Environment.CurrentDirectory + "\\piraeusconfig.json")
        //        .AddEnvironmentVariables("PI_");

        //    IConfigurationRoot root = builder.Build();
        //    PiraeusConfig pc = new PiraeusConfig();
        //    ConfigurationBinder.Bind(root, pc);

        //    return pc;
        //}

        //private IClusterClient CreateClusterClient(IServiceProvider serviceProvider)
        //{
        //    var log = serviceProvider.GetService<ILogger<Startup>>();
        //    if (!config.Dockerized)
        //    {
        //        var localClient = new ClientBuilder()
        //        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPiSystem).Assembly))
        //        .UseLocalhostClustering()
        //        .Build();

        //        localClient.Connect(RetryFilter);
        //        return localClient;
        //    }
        //    else
        //    {
        //        var client = new ClientBuilder()
        //            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IPiSystem).Assembly))
        //            .Configure<ClusterOptions>(options =>
        //            {
        //                options.ClusterId = config.ClusterId;
        //                options.ServiceId = config.ServiceId;
        //            })
        //            .UseAzureStorageClustering(options => options.ConnectionString = config.DataConnectionString)
        //            .Build();

        //        client.Connect(RetryFilter).GetAwaiter().GetResult();
        //        return client;
        //    }
        //    async Task<bool> RetryFilter(Exception exception)
        //    {
        //        log?.LogWarning("Exception while attempting to connect to Orleans cluster: {Exception}", exception);
        //        await Task.Delay(TimeSpan.FromSeconds(2));
        //        return true;
        //    }
        //}
    }
}
