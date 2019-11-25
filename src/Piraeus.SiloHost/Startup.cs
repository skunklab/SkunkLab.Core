using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Piraeus.Configuration;
using Piraeus.Extensions.Configuration;
using System;

namespace Piraeus.SiloHost
{
    public class Startup
    {
        private Host host;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            OrleansConfig oconfig = null;

            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddOrleansConfiguration(out oconfig);
            ServiceDescriptor sd = new ServiceDescriptor(typeof(Host), new Host());
            services.Add(sd);

            IServiceProvider sp = services.BuildServiceProvider();

            host = sp.GetRequiredService<Host>();
            host.Init();
        }
    }
}
