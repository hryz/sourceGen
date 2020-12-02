using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using zipkin4net;
using zipkin4net.Tracers;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;
using ILogger = zipkin4net.ILogger;

namespace net5
{
    public class Program
    {
        
        public static async Task Main(string[] args)
        {
            //await DumpCacheDemo.Run();
            //await ConcurrencyLab.Run();

            await ZipkinFun.RunDemo();
            
            return;
            await CreateHostBuilder(args).Build().RunAsync();
        }
        
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(x => x.UseStartup<Startup>());
        
    }
}
