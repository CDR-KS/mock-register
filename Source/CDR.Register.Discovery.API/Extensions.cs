﻿using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Services;
using CDR.Register.Discovery.API.Business;
using CDR.Register.Domain.Repositories;
using CDR.Register.Repository;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;

namespace CDR.Register.Discovery.API
{
    public static class Extensions
    {
        public static IServiceCollection AddRegisterDiscovery(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRegisterDiscoveryRepository, RegisterDiscoveryRepository>();
            services.AddScoped<IDiscoveryService, DiscoveryService>();
            services.AddScoped<IDataRecipientStatusCheckService, DataRecipientStatusCheckService>();

            services.AddMediatR(typeof(Startup));

            // Add Authentication and Authorization
            services.AddAuthenticationAuthorization(configuration);

            return services;
        }

        public static IServiceCollection AddRegisterDiscoverySwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CDR Register Discovery API", Version = "v1" });
            });

            services.AddSwaggerGenNewtonsoftSupport();
            services.AddMvc().AddNewtonsoftJson(options => { options.SerializerSettings.Converters.Add(new StringEnumConverter()); });

            return services;
        }

        public static IApplicationBuilder UseRegisterDiscoverySwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CDR Register Discovery API v1"));

            return app;
        }
    }
}
