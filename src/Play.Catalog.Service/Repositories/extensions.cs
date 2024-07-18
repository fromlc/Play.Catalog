using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Settings;

namespace Play.Catalog.Service.Repositories
{
    public static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {
            // store Id and CreatedDate as strings
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            // register services with service container

            // construct MongoDB client
            services.AddSingleton(serviceProvider =>
            {
                // retrieve service settings from appsettings.json
                var configuration = serviceProvider.GetService<IConfiguration>();
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>(); ;
                var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);

                return mongoClient.GetDatabase(serviceSettings.ServiceName);
            });

            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName)
            where T : IEntity
        {
            // register names of interface and type (class) that implements it
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                var database = serviceProvider.GetService<IMongoDatabase>();
                return new MongoRepository<T>(database, collectionName);
            });

            return services;
        }
    }
}


