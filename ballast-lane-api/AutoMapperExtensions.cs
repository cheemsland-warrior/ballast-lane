using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace API.Extensions
{
    public static class AutoMapperExtensions
    {
        public static IServiceCollection AddAutoMapperWithValidation(
            this IServiceCollection services,
            bool isDevelopment)
        {
            // Use the configuration expression callback to add the profile
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfiles>();
            });

            return services;
        }
    }
}