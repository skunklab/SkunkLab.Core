using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Silo
{
    public class PiraeusSiloHostBuilder : IPiraeusSiloHostBuilder
    {


        public IPiraeusHost Build()
        {
            
        }

        public IPiraeusSiloHostBuilder UseStartup<TStartup>() where TStartup : class
        {
            Activator.CreateInstance<TStartup>();
            return this;
        }
    }
}
