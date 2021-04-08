using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Api;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntegrationTests
{
    public class ServerFixture : IDisposable
    {
        public static IServiceScopeFactory serviceScopeFactory { get; private set; }
        public IConfigurationRoot _configuration { get; }
        public readonly HttpClient client;

        public ServerFixture()
        {
            string testSettings = "appsettings.Testing.json";
            string rootDir = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent}/src/config";
            var builder = new ConfigurationBuilder()
                .SetBasePath(rootDir)
                .AddJsonFile(testSettings);

            _configuration = builder.Build();

            var couchbaseOptions = _configuration.GetSection("Couchbase").Get<CouchbaseConfig>();

            var services = new ServiceCollection();

            var startup = new Startup(_configuration);

            services.AddLogging(l => l.AddProvider(NullLoggerProvider.Instance));
            services.AddSingleton<IConfiguration>(_configuration);
            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

            startup.ConfigureServices(services);

            serviceScopeFactory = services.BuildServiceProvider().GetService<IServiceScopeFactory>();
            client = new TestServer(new WebHostBuilder()
                .ConfigureAppConfiguration((context, builder) => {
                    builder.SetBasePath(rootDir);
                    builder.AddJsonFile(testSettings);
                }).UseStartup<Startup>()).CreateClient();
        }

        async public void Dispose()
        {
            await FlushBucket();
        }

        async public Task FlushBucket()
        {
            using var scope = serviceScopeFactory.CreateScope();
            var couchbaseContext = scope.ServiceProvider.GetService<ICouchbaseContext>();
            var couchbaseOptions = _configuration.GetSection("Couchbase").Get<CouchbaseConfig>();
            string query = $"DELETE from `{couchbaseOptions.BucketName}`";
            var cbResults = await couchbaseContext.Bucket.Cluster
                .QueryAsync<dynamic>(query);
        }

    }
}