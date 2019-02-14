using System;

namespace Piraeus.Silo
{
    public interface IPiraeusSiloHostBuilder
    {
        //IPiraeusSiloHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);
        IPiraeusSiloHostBuilder UseStartup<TStartup>() where TStartup : class;
        IPiraeusHost Build();
    }
}
