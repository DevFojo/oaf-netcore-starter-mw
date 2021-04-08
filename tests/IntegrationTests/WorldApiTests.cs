using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Application.Common.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests
{
    public class WorldsApiTests : IClassFixture<ServerFixture>
    {
        private readonly HttpClient _testClient;
        private readonly IWorldRepository _worldRepo;
        private readonly ServerFixture _fixture;
        public WorldsApiTests(ServerFixture fixture)
        {
            using var scope = ServerFixture.serviceScopeFactory.CreateScope();
            _worldRepo = scope.ServiceProvider.GetService<IWorldRepository>();
            _testClient = fixture.client;
            _fixture = fixture;
        }

        [Fact]
        async void TestThatGetWorldsEndpointWorks()
        {
            //Arrange - Put the necessary data in the Database
            var fakeWorld = new World() { Name = "Mars", HasLife = true };
            var savedWorld = await _worldRepo.InsertDocument(fakeWorld);

            //Act - get all worlds from the API
            var response = await _testClient.GetAsync("/api/worlds");
            var responseString = await response.Content.ReadAsStringAsync();
            var expected = JsonSerializer.Deserialize<List<World>>(responseString);

            //Assert - check if the worlds returned are the same as the ones created
            response.StatusCode.Should().Equals(200);
            expected.Should().HaveCount(1);
        }

        [Fact]
        async void TestThatGetWorldEndpointWorks()
        {
           //Arrange - Put the necessary data in the Database
            var fakeWorld = new World() { Name = "Jupiter", HasLife = true };
            var savedWorld = await _worldRepo.InsertDocument(fakeWorld);

            //Act - get a world from the API
            var response = await _testClient.GetAsync($"/api/worlds?id={savedWorld.Id}");
            var responseString = await response.Content.ReadAsStringAsync();
            var expected = JsonSerializer.Deserialize<World>(responseString);

            //Assert - check if the worlds returned are the same as the ones created
            response.StatusCode.Should().Equals(200);
            expected.Should().BeEquivalentTo<World>(savedWorld);
        }

        [Fact]
        async void TestThatPostWorldsEndpointWorks()
        {
            //Arrange - Put the necessary data in the Database
            var fakeWorld = new World() { Name = "Earth", HasLife = true };
            HttpContent httpContent = new StringContent(JsonSerializer.Serialize(fakeWorld), Encoding.UTF8, "application/json");

            //Act - post a world from the API
            HttpResponseMessage response = await _testClient.PostAsync("/api/worlds", httpContent);
            string responseString = await response.Content.ReadAsStringAsync();
            var expected = JsonSerializer.Deserialize<World>(responseString);

            //Assert - check if the worlds returned are the same as the ones created
            response.StatusCode.Should().Equals(200);
            expected.Name.Should().Be(fakeWorld.Name);
            expected.HasLife.Should().Be(fakeWorld.HasLife);
        }
    }
}
