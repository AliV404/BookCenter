using System.Net;

namespace BookCenter.API.Tests
{
    public class HealthTests : IAsyncLifetime
    {
        private readonly ApiFactory _factory = new();

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.DisposeAsync();

        [Fact]
        public async Task Health_endpoint_is_public_and_checks_the_database()
        {
            var response = await _factory.CreateClient().GetAsync("/api/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        }
    }
}
