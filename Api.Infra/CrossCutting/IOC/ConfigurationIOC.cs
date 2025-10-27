using Api.Application;
using Api.Application.Interfaces;
using Api.Application.Mappers;
using Api.Application.Services;
using Api.Core.Interfaces.Repositorys;
using Api.Core.Interfaces.Services;
using Api.Infra.Caching;
using Api.Infra.Data;
using Api.Infra.Data.Repositorys;
using Api.Services.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Infra.CrossCutting.IOC
{
    public static class ConfigurationIOC
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext com InMemory Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("ChallengeLevelUPDb"));

            // Application Services
            services.AddScoped<IRecommendationService, RecommendationService>();

            // Repositories
            services.AddScoped<IGameRecommendationRepository, GameRecommendationRepository>();

            // Service externa
            services.AddScoped<IRamEstimationService, RamEstimationService>();

            // HTTP Client para API externa
            services.AddHttpClient<IFreeToPlayApiClientService, FreeToPlayApiClientService>(client =>
            {
                client.BaseAddress = new Uri("https://www.freetogame.com/api/");
            });

            // Configuração do Redis
            services.AddScoped<IRedisCachingService, RedisCachingService>();

            // Configuração do Cache - Fallback automático
            var redisConnectionString = configuration["Redis:ConnectionString"];

            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                // Se tem connection string, tenta Redis
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = configuration["Redis:InstanceName"];
                });
            }
            else
            {
                // Fallback para Memory Cache (para desenvolvimento)
                services.AddDistributedMemoryCache();
            }

            return services;
        }
    }
}