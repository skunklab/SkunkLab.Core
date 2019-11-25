using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Piraeus.Core.Logging;
using Piraeus.Extensions.Configuration;
using System;

namespace Piraeus.SiloHost.Core
{
    public class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("  ******** **  **            **      **                    **");
            Console.WriteLine(" **////// //  /**           /**     /**                   /**");
            Console.WriteLine("/**        ** /**  ******   /**     /**  ******   ****** ******");
            Console.WriteLine("/*********/** /** **////**  /********** **////** **//// ///**/");
            Console.WriteLine("////////**/** /**/**   /**  /**//////**/**   /**//*****   /**");
            Console.WriteLine("       /**/** /**/**   /**  /**     /**/**   /** /////**  /**");
            Console.WriteLine(" ******** /** ***//******   /**     /**//******  ******   //**");
            Console.WriteLine("////////  // ///  //////    //      //  //////  //////     //");
            Console.WriteLine("");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureLogging((builder) =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOrleansConfiguration();
                    services.AddSingleton<Logger>();    //add the logger
                    services.AddHostedService<SiloHostService>(); //start the silo host
                });
        //private static ManualResetEventSlim done;

        //static void Main(string[] args)
        //{
        //    //TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        //    //IServiceCollection services = new ServiceCollection();
        //    //Startup startup = new Startup(null);
        //    //startup.ConfigureServices(services);

        //    //done = new ManualResetEventSlim(false);

        //    //Console.CancelKeyPress += (sender, eventArgs) =>
        //    //{

        //    //    done.Set();
        //    //    eventArgs.Cancel = true;
        //    //};

        //    //Console.WriteLine("Orleans silo is running...");
        //    //done.Wait();

        //}

        //private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        //{
        //    //Restart the container because unobserved exception
        //    done.Set();
        //}
    }
}
