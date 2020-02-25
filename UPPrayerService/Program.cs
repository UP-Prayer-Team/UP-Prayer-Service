using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UPPrayerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureWebHost(configure => configure.UseKestrel(options =>
                {
                    options.ListenAnyIP(443, listenOptions =>
                    {
                        string certPath = config.GetSection("Certificate").GetValue<string>("Path");
                        string certPassword = config.GetSection("Certificate").GetValue<string>("Password");
                        if (certPassword == "")
                        {
                            throw new Exception("Fill in Certificate.Password in appsettings.json!");
                        }
                        listenOptions.UseHttps(new X509Certificate2(certPath, certPassword));
                    });
                }));
        }
    }
}
