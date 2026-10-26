
using System.Net;
using backend.Tests.Infrastructure;

namespace backend.Tests;

public class ApiSmokeTests
{
    [Fact]
    public async Task Api_RespondsToHttpRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/Customers")]
    [InlineData("/api/Appointments")]
    public async Task ProtectedEndpoints_WithoutToken_Return401(
        string endpoint)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(endpoint);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}
