using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Domain.Contracts;
using Ordering.Infrastructure.NoSql.Repositories;

namespace Ordering.API.Extensions
{
    public static class NoSqlStoreExtensions
    {
        public static IServiceCollection RegisterNoSqlStore(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register repository class. Note how we pass connection information
            services.AddScoped<IOrderRepository, OrderRepository>(x =>
            {
                return new OrderRepository(
                    new DataStoreConfiguration(
                        configuration["CosmosEndpoint"],
                        configuration["cosmoskeysecret"]));
                        //configuration["CosmosPrimaryKey"]));
            });

            return services;
        }
    }
}
