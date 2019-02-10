using AutoMapper;
using BSharp.Data;
using BSharp.Services.BlobStorage;
using BSharp.Services.Migrations;
using BSharp.Services.ModelMetadata;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Globalization;

namespace BSharp
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration config, IHostingEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register the admin context
            services.AddDbContext<AdminContext>(opt =>
                opt.UseSqlServer(_config.GetConnectionString(Constants.AdminConnection))
                .ReplaceService<IMigrationsSqlGenerator, CustomSqlServerMigrationsSqlGenerator>());


            // The application context contains the shardlets, and unlike the other contexts it acquires its connection
            // string dynamically using IShardResolver when it is constructed, therefore this context cannot be
            // be registered in the DI the usual way with AddDbContext<T>()
            services.AddScoped<ApplicationContext>();


            // Here we register a distributed cache, the default is SQL server unless
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
                    opt.ConnectionString = _config.GetConnectionString(Constants.AdminConnection);
                    opt.SchemaName = "dbo";
                    opt.TableName = "DistributedCache";
                    opt.ExpiredItemsDeletionInterval = TimeSpan.FromDays(15);
                });
            }

            // Add a blob service depending on wheter 
            var azureBlobConfig = _config.GetSection("AzureBlobStorage");
            var azureBlobConnString = azureBlobConfig["ConnectionString"];
            var azureBlobContainerName = azureBlobConfig["ContainerName"];
            if (!string.IsNullOrWhiteSpace(azureBlobConnString) && !string.IsNullOrWhiteSpace(azureBlobContainerName))
            {
                // If Azure blob storage info is provided use it
                // Note: This setup of using one azure blob storage may become a
                // bottleneck in the extremely far future we will worry about it then                
                services.AddAzureBlobStorage(opt =>
                {
                    opt.ConnectionString = azureBlobConnString;
                    opt.ContainerName = azureBlobContainerName;
                });
            }
            else
            {
                // If a connection string to an Azure blob storage is not provided use the sql db as blob storage
                // This uses the ApplicationContext itself to store blobs: more expensive but easier to set up on-premise
                services.AddSqlTableBlobStorage();
            }

            // Add all our custom services
            services.AddMultiTenancy();
            services.AddSharding();
            services.AddSqlLocalization();
            services.AddDynamicModelMetadata();

            // Setup an embedded instance of identity service in the same domain as the API if enabled in the configuration
            services.AddEmbeddedIdentityServerIfEnabled(_config, _env);

            // Add services for authenticating API calls against an OIDC authority, and helper services for accessing claims
            services.AddApiAuthentication(_config);

            // Register MVC using the most up to date version
            services.AddMvc(opt =>
            {
                opt.ModelMetadataDetailsProviders.Add(new ExcludeBindingMetadataProvider(typeof(Controllers.DTO.MeasurementUnit)));
            })
                .AddViewLocalization()
                .AddDataAnnotationsLocalization()
                .AddJsonOptions(options =>
                {
                    // The JSON options below instruct the serializer to keep property names in PascalCase, 
                    // even though this violates convention, it makes a few things easier since both client and server
                    // sides get to see and communicate identical property names, for example 'api/customers?orderby='Name'
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new DefaultNamingStrategy()
                    };

                    //   options.SerializerSettings.Converters.Insert(0, new TrimmingStringConverter());

                    // To response size
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // To allow a client that is hosted on another server
            services.AddCors();

            // Configure some custom behavior for API controllers
            services.Configure<ApiBehaviorOptions>(options =>
            {
                // This overrides the default behavior, when there are validation
                // errors we return a 422 unprocessable entity, instead of the default
                // 400 bad request, this makes it easier for clients to easily distinguish 
                // such kind of errors and handle them in a special way, for example:
                // by showing them on the fields with a red color
                options.InvalidModelStateResponseFactory = ctx =>
                {
                    return new UnprocessableEntityObjectResult(ctx.ModelState);
                };
            });


            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(config =>
            {
                config.RootPath = "ClientApp/dist";
            });

            // AutoMapper https://automapper.org/
            services.AddAutoMapper();
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Picks out the culture from the request string and sets it in the current thread
            app.UseRequestLocalization(opt =>
            {
                var defaultUICulture = _config.GetSection("Localization")["DefaultUICulture"] ?? "en";
                var defaultCulture = _config.GetSection("Localization")["DefaultCulture"] ?? "en-GB";

                opt.DefaultRequestCulture = new RequestCulture(defaultCulture, defaultUICulture);

                // Formatting numbers, dates, etc.
                opt.AddSupportedCultures(defaultCulture);

                // UI strings that we have localized.
                opt.SupportedUICultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            if (_env.IsDevelopment())
            {
                app.UseCors(builder =>
                {
                    // TODO: Read from settings for production
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("x-image-id")
                        .WithExposedHeaders("x-settings-version")
                        .WithExposedHeaders("x-permissions-version")
                        .WithExposedHeaders("x-user-settings-version");
                });
            }

            // Serves the identity server
            if(_config["EmbeddedIdentityServer:Enabled"]?.ToLower() == "true")
            {
                app.UseIdentityServer();
            }

            // Serves the API
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            // Serves the Angular client
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                if (_env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
