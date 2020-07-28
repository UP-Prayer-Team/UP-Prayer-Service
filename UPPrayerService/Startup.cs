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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace UPPrayerService
{
    public class Startup
    {
        private const string CORSPolicyName = "CORSPolicy";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Ensure that the SQLite DB directory exists
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Configuration.GetSection("ConnectionStrings")["SQLite"].Split('=')[1]));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CORSPolicyName, builder =>
                {
                    builder.WithOrigins(
                        "http://localhost:8080",
                        "http://localhost:8081",
                        "http://*.upmovement.org",
                        "https://*.upmovement.org",
                        "http://stage.upmovement.org",
                        "https://stage.upmovement.org",
                        "http://upmovement.org",
                        "https://upmovement.org",
                        "http://www.upmovement.org",
                        "https://www.upmovement.org",
                        "https://admin.upmovement.org",
                        "https://admin.stage.upmovement.org")
                        .WithHeaders("Content-Type", "Authorization");
                });
            });
            services.AddDbContext<DataContext>(options=> options.UseSqlite(Configuration.GetConnectionString("SQLite")));
            services.AddScoped<Services.EmailService>();
            services.AddScoped<Services.EndorsementService>();
            services.AddScoped<Services.ReservationService>();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config =>
            {
                config.SaveToken = true;
                config.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidIssuer = Configuration["Tokens:Issuer"],
                    ValidAudience = Configuration["Tokens:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))
                };
            });
            services.AddIdentityCore<Models.User>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders()
                .AddSignInManager<SignInManager<Models.User>>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (IServiceScope serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                DataContext context = serviceScope.ServiceProvider.GetRequiredService<DataContext>();
                context.Database.EnsureCreated();
                context.Initialize(serviceScope.ServiceProvider.GetService<UserManager<Models.User>>(),
                    serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>());
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseCors(CORSPolicyName);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet(".well-known/acme-challenge/{id}", Services.LetsEncryptService.HandleRequest);
                endpoints.MapControllers();
            });
        }
    }
}
