using System.IO;
using System.Net.Http;
using Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Xunit;
using Bogus;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using Application.Common.Interfaces;

namespace Integration
{
    public class ServerFixture : IDisposable
    {
        public readonly HttpClient testClient;
        public Faker<World> fakeWorldsFactory;
        public IConfiguration Configuration;
        public TestServer testServer;
        public IWorldRepository worldRepository;
        private string rootDir = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent}/src/config";

        public ServerFixture()
        {
            Configuration = new ConfigurationBuilder()
            .SetBasePath(rootDir)
            .AddJsonFile("appsettings.Testing.json")
            .Build();

            testServer = new TestServer(new WebHostBuilder()
                .UseConfiguration(Configuration)
                .UseStartup<Startup>());

            testClient = testServer.CreateClient();
            worldRepository = testServer.Host.Services.GetRequiredService<IWorldRepository>();
        }

        public void Dispose()
        {
            var results = worldRepository.RemoveAllDocuments();
            testClient.Dispose();
        }

        public Faker<World> FakeWorldFactory()
        {
            return new Faker<World>()
            .RuleFor(p => p.Id, f => f.Random.Guid().ToString())
            .RuleFor(p => p.Name, f => f.PickRandom<string>(Configuration["Worlds"].Split(',')))
            .RuleFor(p => p.HasLife, true)
            .RuleFor(p => p.Entity, "World");
        }
    }

    [CollectionDefinition("Server collection")]
    public class ServerCollection : ICollectionFixture<ServerFixture>
    {
    }
}