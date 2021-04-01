using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Bogus;
using Domain.Entities;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;

namespace Integration
{
    [Collection("Server collection")]
    public class ApiTest
    {
        private readonly HttpClient _testClient;

        private readonly TestWorldRepository _testWorldRepository;
        private Faker<World> _worldFaker;

        public ApiTest(ServerFixture serverFixture)
        {
            _testClient = serverFixture.client;
            _testWorldRepository = serverFixture.worldRepository;
            _worldFaker = serverFixture.worldFaker;
        }

        // This test is not passing as well
        [Fact]
        async Task Get_Worlds_ShouldReturn_Worlds()
        {
            World worldToSave = _worldFaker.Generate(1)[0];
            string name = worldToSave.Name;
            await _testWorldRepository.InsertDocument(worldToSave);

            HttpResponseMessage response = await _testClient.GetAsync("/api/worlds");
            string responseString = await response.Content.ReadAsStringAsync();

            var worlds = JsonConvert.DeserializeObject<List<World>>(responseString);
           worldToSave.Name.Should().Be(worlds[0].Name);
        }

        // The /api/worlds?id endpoint is not testable because there is no way to control how Guid's are being created
        // The Guid that user enters is not the same as the Guid that gets inserted in the db
        [Fact]
        async Task Get_WorldById_ShouldReturn_A_World()
        {
            World fakeWorld = _worldFaker.Generate(1)[0];

            HttpResponseMessage response = await _testClient.GetAsync($"/api/worlds?id={fakeWorld.Id}");
            string responseString = await response.Content.ReadAsStringAsync();

            World savedWorld = JsonConvert.DeserializeObject<World>(responseString);
            savedWorld.Id.Should().BeEquivalentTo(fakeWorld.Id);
        }

        [Fact]
        async Task Post_World_returns_World_Object()
        {
            World worldToSave = _worldFaker.Generate(1)[0];

            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(worldToSave), Encoding.UTF8, "application/json");
            HttpResponseMessage response = _testClient.PostAsync("/api/worlds", httpContent).Result;

            string responseString = await response.Content.ReadAsStringAsync();
            var savedWorld = JsonConvert.DeserializeObject<World>(responseString);
            savedWorld.Name.Should().BeEquivalentTo(worldToSave.Name);
        }
    }
}