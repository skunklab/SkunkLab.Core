using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace Piraeus.TcpGateway
{
    class Program
    {
       
        private static ManualResetEventSlim done;


        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            Startup startup = new Startup(null);
            startup.ConfigureServices(services);
            

            done = new ManualResetEventSlim(false);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("TCP Gateway is ready...");
            done.Wait();
        }
    }
}
