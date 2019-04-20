﻿using Goober.Core.Extensions;
using Goober.Logging;
using Goober.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;

namespace Goober.WebApi
{
    public abstract class GooberStartup
    {
        #region props

        protected IConfiguration Configuration { get; private set; }

        #endregion

        #region ctor

        public GooberStartup(IHostingEnvironment env, IConfiguration config)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        #endregion

        public void ConfigureServices(IServiceCollection services)
        {
            services.RegisterDateTimeService();
            services.AddCaching();
            services.AddSingleton(Configuration);
            services.AddSwaggerGenWithDocs();

            services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    });

            ConfigureServiceCollections(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory, IApplicationLifetime appLifetime)
        {
            app.UseExtenedLoggingVariables();

            UseRequestLocalizationByDefault(app);

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            app.UseNlogExceptionsHandling();

            ConfigurePipelineAfterExceptionsHandling(app);

            app.UseSwagger();

            app.UseSwaggerUIWithDocs();

            app.UseMvc();

            app.UseStaticFiles();

            ConfigurePipelineAfterMvc(app);
        }

        protected abstract void ConfigureServiceCollections(IServiceCollection services);

        protected abstract void ConfigurePipelineAfterExceptionsHandling(IApplicationBuilder app);

        protected abstract void ConfigurePipelineAfterMvc(IApplicationBuilder app);

        private static void UseRequestLocalizationByDefault(IApplicationBuilder app)
        {
            var supportedCultures = new[] {
                new CultureInfo("en"),
                new CultureInfo("ru"),
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("ru"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });
        }
    }
}
