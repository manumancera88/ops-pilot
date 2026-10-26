
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using backend.Tests.Infrastructure;

namespace backend.Tests;

public class TenantIsolationTests
{
    [Fact]
    public async Task CompanyToken_CannotAccessAnotherCompany()
    {
        // Arrange: iniciar la API de prueba.
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        // Registrar la empresa A.
        var registerA = await client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                CompanyName = "Empresa A Test",
                TaxId = "TEST-TENANT-A",
                Name = "Usuario A",
                Email = "usuario.a@test.local",
                Password = "TestPassword123!"
            });

        Assert.Equal(HttpStatusCode.OK, registerA.StatusCode);

        using var jsonA = JsonDocument.Parse(
            await registerA.Content.ReadAsStringAsync());

        var companyAId = jsonA.RootElement
            .GetProperty("companyId")
            .GetInt32();

        // Registrar la empresa B.
        var registerB = await client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                CompanyName = "Empresa B Test",
                TaxId = "TEST-TENANT-B",
                Name = "Usuario B",
                Email = "usuario.b@test.local",
                Password = "TestPassword123!"
            });

        Assert.Equal(HttpStatusCode.OK, registerB.StatusCode);

        using var jsonB = JsonDocument.Parse(
            await registerB.Content.ReadAsStringAsync());

        var companyBId = jsonB.RootElement
            .GetProperty("companyId")
            .GetInt32();

        Assert.NotEqual(companyAId, companyBId);

        // Iniciar sesión como usuario de la empresa A.
        var login = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new
            {
                Email = "usuario.a@test.local",
                Password = "TestPassword123!"
            });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var loginJson = JsonDocument.Parse(
            await login.Content.ReadAsStringAsync());

        var token = loginJson.RootElement
            .GetProperty("token")
            .GetString();

        Assert.False(string.IsNullOrWhiteSpace(token));

        // Act: probar primero el acceso a la propia empresa.
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var ownCompanyResponse = await client.GetAsync(
            $"/api/Companies/{companyAId}");

        // Intentar acceder a la empresa B con el token de A.
        var otherCompanyResponse = await client.GetAsync(
            $"/api/Companies/{companyBId}");

        // Assert: acceso propio permitido, acceso ajeno denegado.
        Assert.Equal(
            HttpStatusCode.OK,
            ownCompanyResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.NotFound,
            otherCompanyResponse.StatusCode);
    }
}
