using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.SiloHost.Core
{
    public class Program
    {
        private static ManualResetEventSlim done;

        static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            IServiceCollection services = new ServiceCollection();
            Startup startup = new Startup(null);
            startup.ConfigureServices(services);

            done = new ManualResetEventSlim(false);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };
            
            Console.WriteLine("Orleans silo is running...");
            done.Wait();

        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            //Restart the container because unobserved exception
            done.Set();
        }
    }
}
