using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace UPPrayerService
{
    public class Startup
    {
        private const string CORSPolicyName = "CORSPolicy";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CORSPolicyName, builder =>
                {
                    builder.WithOrigins("http://localhost:8080",
                        "https://localhost:8033",
                        "http://*.upmovement.org",
                        "https://*.upmovement.org",
                        "http://upmovement.org",
                        "https://upmovement.org")
                        .WithHeaders("Content-Type");
                });
            });
            services.AddDbContext<DataContext>(options=> options.UseSqlite(Configuration.GetConnectionString("SQLite")));
            services.AddScoped<Services.EmailService>();
            services.AddScoped<Services.EndorsementService>();
            services.AddScoped<Services.ReservationService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(CORSPolicyName);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet(".well-known/acme-challenge/{id}", Services.LetsEncryptService.HandleRequest);
                endpoints.MapControllers();
            });
        }
    }
}
