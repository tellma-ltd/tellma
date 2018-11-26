using BSharp.Data;
using BSharp.Data.Model.Identity;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;

namespace BSharp
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ////////// Register all DB contexts
            services.AddDbContext<ShardingContext>(opt =>
                opt.UseSqlServer(_config.GetConnectionString(Constants.ShardingConnection)));

            services.AddDbContext<ConfigurationContext>(opt =>
                opt.UseSqlServer(_config.GetConnectionString("ConfigurationConnection")));

            services.AddDbContext<IdentityContext>(opt =>
                opt.UseSqlServer(_config.GetConnectionString("IdentityConnection")));

            services.AddDbContext<LocalizationContext>(opt =>
                opt.UseSqlServer(_config.GetConnectionString("LocalizationConnection")));


            ////////// This DB context contains the shardlets, and unlike the others it acquires its connection string
            // dynamically when it is constructed using IShardResolver, therefore this context cannot be
            // be registered in the DI the usual way like the other contexts
            services.AddScoped<ApplicationContext>();


            ////////// Here we register a distributed cache, the default is SQL server unless
            // a Redis cache is specified in a configuration provider
            var redisConfig = _config.GetSection("RedisCache");
            if (redisConfig.Exists())
            {
                services.AddDistributedRedisCache(options =>
                {
                    options.Configuration = redisConfig["Configuration"];
                    options.InstanceName = redisConfig["InstanceName"];
                });
            }
            else
            {
                services.AddDistributedSqlServerCache(opt =>
                {
                    opt.ConnectionString = _config.GetConnectionString("LocalizationConnection");
                    opt.SchemaName = "dbo";
                    opt.TableName = "DistributedCache";
                    opt.ExpiredItemsDeletionInterval = TimeSpan.FromDays(15); 
                });
            }


            ////////// Add all our custom services
            services.AddMultiTenancy();
            services.AddSharding();
            services.AddSqlLocalization();


            ////////// Register identity based on IdentityContext
            services.AddIdentityCore<ApplicationUser>(opt =>
            {
                // Make the password requirement less annoying, and compensate by increasing required length
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequiredLength = 7;
            })
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();


            ////////// Register MVC using the most up to date version
            services.AddMvc()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            ////////// In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(config =>
            {
                config.RootPath = "ClientApp/dist";
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseRequestLocalization(opt =>
            {
                var defaultUICulture = _config.GetSection("Localization")["DefaultUICulture"];
                var defaultCulture = _config.GetSection("Localization")["DefaultCulture"];

                opt.DefaultRequestCulture = new RequestCulture(defaultCulture, defaultUICulture);

                // Formatting numbers, dates, etc.
                opt.AddSupportedCultures(defaultCulture);

                // UI strings that we have localized.
                opt.SupportedUICultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
