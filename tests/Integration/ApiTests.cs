using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Bogus;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Newtonsoft.Json;

namespace Integration
{
    [Collection("Server collection")]
    public class ApiTest
    {
        private readonly HttpClient _testClient;
        private readonly IWorldRepository _worldRepository;
        private readonly Faker<World> fakeWorldsFactory;
        private readonly TestServer _testServer;

        public ApiTest(ServerFixture serverFixture)
        {
            _testClient = serverFixture.testClient;
            _testServer = serverFixture.testServer;
            fakeWorldsFactory = serverFixture.FakeWorldFactory();
            _worldRepository = serverFixture.worldRepository;
        }

        [Fact]
        async Task Get_Worlds_ShouldReturn_Worlds()
        {
            List<World> worldsToSave = fakeWorldsFactory.Generate(2);
            foreach(World world in worldsToSave) await _worldRepository.InsertDocument(world);

            var response = await _testClient.GetAsync("/api/worlds");
            string responseString = await response.Content.ReadAsStringAsync();

            var savedWorlds = JsonConvert.DeserializeObject<List<World>>(responseString);
            savedWorlds.Count.Should().BeGreaterOrEqualTo(worldsToSave.Count);
        }

        [Fact]
        async Task Get_WorldById_ShouldReturn_A_World()
        {
            World insertedWorld = await _worldRepository.InsertDocument(fakeWorldsFactory.Generate(1)[0]);

            HttpResponseMessage response = await _testClient.GetAsync($"/api/worlds?id={insertedWorld.Id}");
            string responseString = await response.Content.ReadAsStringAsync();

            World world = JsonConvert.DeserializeObject<World>(responseString);
            world.Id.Should().Be(insertedWorld.Id);
        }

        [Fact]
        async Task Post_World_returns_World_Object()
        {
            World worldToSave = fakeWorldsFactory.Generate(1)[0];
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(worldToSave), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _testClient.PostAsync("/api/worlds", httpContent);
            string responseString = await response.Content.ReadAsStringAsync();

            World savedWorld = JsonConvert.DeserializeObject<World>(responseString);
            savedWorld.Name.Should().Be(worldToSave.Name);
        }
    }
}