using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Promart.Api.Application.IntegrationEvents;
using Promart.Api.Infrastructure.AutofacModules;
using Promart.Api.Infrastructure.Filters;
using Promart.Api.Infrastructure.Services;
using Promart.Infrastructure;
using Serilog;
using Sunedu.BuildingBlocks.IntegrationEventLogEF;
using Sunedu.BuildingBlocks.IntegrationEventLogEF.Services;
using Sunedu.Siu.Infrastucture.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace Promart.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddCustomMvc(Configuration)
                .AddCustomDbContext(Configuration)
                .AddCustomSwagger(Configuration, Program.AppName)
                .AddCustomConfiguration(Configuration)
                .AddCustomIntegrations(Configuration)
                .AddIntegrationTracerServices(Configuration, Program.AppName);

            //configure autofac
            var container = new ContainerBuilder();
            container.Populate(services);

            container.RegisterModule(new MediatorModule());
            container.RegisterModule(new ApplicationModule(Configuration["ConnectionString"]));

            return new AutofacServiceProvider(container.Build());
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {

            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                loggerFactory.CreateLogger<Startup>().LogDebug("Using PATH BASE '{pathBase}'", pathBase);
                app.UsePathBase(pathBase);
            }

            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Promart.API V1");
                   c.OAuthAppName("Promart Swagger UI");
               });

            app.UseRouting();

            app.UseCors("CorsPolicy");

            ConfigureAuth(app);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();

            });

            //ConfigureEventBus(app);
        }

        private void ConfigureEventBus(IApplicationBuilder app)
        {
            #region Suscripción a eventos de otros servicios
            //var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            #endregion
        }

        protected virtual void ConfigureAuth(IApplicationBuilder app)
        {
          
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }


    static class CustomExtensionsMethods
    {

        public static IServiceCollection AddCustomMvc(this IServiceCollection services, IConfiguration configuration)
        {
            var CorsOriginAllowed = configuration.GetSection("AllowedOrigins").Get<List<string>>();
            ///TODO: Si no se registra un origen en [AllowedOrigins] se asigna [*] para responder a cualquier origen por defecto
            var origins = CorsOriginAllowed != null ? CorsOriginAllowed.ToArray() : new string[] { "*" };

            Log.Information("Configurando Origenes para ({CORS})...", origins);

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    //.AllowCredentials()
                    );
            });
            Log.Information("Fin de Configuración ({CORS})...");

            // Add framework services.
            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));

            })
                                   .AddNewtonsoftJson()
                                   .AddControllersAsServices(); //Injecting Controllers themselves thru DI
                                                                //For further info see: http://docs.autofac.org/en/latest/integration/aspnetcore.html#controllers-as-services


            return services;
        }


        public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PromartContext>(options =>
            {
                options.UseMySQL("Server=azr-promart-2022.mysql.database.azure.com;Initial Catalog=db_testpromart;Persist Security Info=False; Connection Timeout=30; User ID=devch@azr-promart-2022;Password=12345Aa@; port=3306");
                //    options.UseSqlServer(configuration["ConnectionString"],
                //        sqlServerOptionsAction: sqlOptions =>
                //        {
                //            sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                //            sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                //        });
                //},
                //           ServiceLifetime.Scoped  //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request)
            });
            //services.AddDbContext<IntegrationEventLogContext>(options =>
            //{
            //    options.UseSqlServer(configuration["ConnectionString"],
            //                         sqlServerOptionsAction: sqlOptions =>
            //                         {
            //                             sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
            //                             //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
            //                             sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            //                         });
            //});

            return services;
        }

        public static IServiceCollection AddCustomIntegrations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IIdentityService, IdentityService>();
            
            services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(
                sp => (DbConnection c) => new IntegrationEventLogService(c));

            services.AddTransient<IPromartIntegrationEventService, PromartIntegrationEventService>();

            return services;
        }

        public static IServiceCollection AddCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<AppSettings>(configuration);
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Instance = context.HttpContext.Request.Path,
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Por favor, consulte la propiedad de errores para más detalles."
                    };

                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json", "application/problem+xml" }
                    };
                };
            });

            return services;
        }
    }
}
