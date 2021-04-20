using System;
using ApiGateway.API.Extensions;
using ApiGateway.API.Filters;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using RestCommunication;
using ServiceDiscovery;

namespace ApiGateway.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddFeatureManagement(options =>
            //    {
            //        options.UseConfiguration(Configuration.GetSection("grpcFeatureFlags"));
            //    });

            // Register backing services
            services.RegisterTelemetryCollector(Configuration);
            services.AddFeatureManagement();

            //// Register telemetry initializer
            //services.AddSingleton<ITelemetryInitializer, ServiceNameTelemetryInitializer>();
            
            // Add custom exception filter to ASP.NET Core MVC
            services.AddMvc(
                config =>
                {
                    config.Filters.Add(typeof(GatewayCustomExceptionFilter));
                    config.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                    //config.InputFormatters.Add(new //XmlSerializerInputFormatter());
                    config.RespectBrowserAcceptHeader = true;
                }).AddNewtonsoftJson(options =>
                options.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver());
            ;

            services.AddControllers()
                .AddNewtonsoftJson();

            // Add Swagger (OpenId Connect) plumbing
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API Gateway Services API",
                    Version = "v1",
                    Description =
                        "API Gateway service for the Microsoft ActivateAzure with Microservices and Containers Workshop. It is the only service exposed to the client, encapsulating all other services behind it."
                });

                //// Set the comments path for the Swagger JSON and UI.
                //var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                //var xmlPath = Path.Combine(basePath, "ApiGateway.xml");
                //c.IncludeXmlComments(xmlPath);
            });


            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            // Add dependencies
            services.AddSingleton<IServiceLocator, ServiceLocator>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            //https://stackoverflow.com/questions/58052596/the-response-ended-prematurely-when-connecting-to-insecure-grpc-channel
            // Adds deep gRPC Logging 
            // https://docs.microsoft.com/en-us/aspnet/core/grpc/diagnostics?view=aspnetcore-3.0
            //services.AddGrpc(options =>
            //{
            //    //new GrpcChannelOptions { LoggerFactory = loggerFactory };

            //    var serviceProvider = services.BuildServiceProvider();
            //    var loggerFactory = serviceProvider.GetService<LoggerFactory>();

            //    new GrpcChannelOptions
            //    {
            //        LoggerFactory = loggerFactory
            //    };
            //});

            // Enable HTTPClient and resiliency support using HttpClientFactory
            //services.AddHttpServices(Configuration, _loggerFactory);
            //services.AddHttpServices<RestClient>(Configuration, loggerFactory);

            //************************************************************

            // https://stackoverflow.com/questions/31863981/how-to-resolve-instance-inside-configureservices-in-asp-net-core
            ////services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            //services.AddSingleton<LoggerFactory>(x => x.GetService<LoggerFactory>());
            //var sp1 = services.AddOptions<LoggerFactory>();
            //services.AddHttpServices<RestClient>(loggerFactory, Configuration);


            // This works. It's no ideal, but cannot find workaround.
            // Breaking change in .NET 3 resulted in this hack.
            ////var sp = services.BuildServiceProvider();
            ////var loggerFactory = sp.GetService<LoggerFactory>();


            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1
            // Add instance of LoggerFactory for HttpRestClientFactory to consume
            services.AddSingleton<LoggerFactory>();
            services.AddHttpServices<RestClient>(Configuration);

            services.AddApiVersioning(x =>
            {
                // Allows for API to return a version in the response header
                x.ReportApiVersions = true;
                // Default version for clients not specifying a version number
                x.AssumeDefaultVersionWhenUnspecified = true;
                // Specifies version to which to default. This is the version
                // to which you are routed if no version is specified
                x.DefaultApiVersion = new ApiVersion(1, 0);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            //loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
            //loggingBuilder.AddConsole();
            //loggingBuilder.AddDebug();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseSwagger();

            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway Services API V1"); });
        }
    }
}