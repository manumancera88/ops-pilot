
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using backend.Tests.Infrastructure;

namespace backend.Tests;

public class TenantIsolationTests
{
    private const string TestPassword = "TestPassword123!";

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private sealed record TestCompany(int Id, string Token);

    private static async Task<TestCompany> RegisterAndLoginAsync(
        HttpClient client,
        string suffix)
    {
        var email = $"usuario.{suffix}@test.local";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                CompanyName = $"Empresa {suffix} Test",
                TaxId = $"TEST-{suffix}",
                Name = $"Usuario {suffix}",
                Email = email,
                Password = TestPassword
            });

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        using var registerJson = JsonDocument.Parse(
            await registerResponse.Content.ReadAsStringAsync());

        var companyId = registerJson.RootElement
            .GetProperty("companyId")
            .GetInt32();

        var loginResponse = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new
            {
                Email = email,
                Password = TestPassword
            });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var loginJson = JsonDocument.Parse(
            await loginResponse.Content.ReadAsStringAsync());

        var token = loginJson.RootElement
            .GetProperty("token")
            .GetString();

        Assert.False(string.IsNullOrWhiteSpace(token));

        return new TestCompany(companyId, token!);
    }

    private static void UseToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task CompanyToken_CannotAccessAnotherCompany()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var companyA = await RegisterAndLoginAsync(client, "A");
        var companyB = await RegisterAndLoginAsync(client, "B");

        Assert.NotEqual(companyA.Id, companyB.Id);

        UseToken(client, companyA.Token);

        var ownCompanyResponse = await client.GetAsync(
            $"/api/Companies/{companyA.Id}");

        var otherCompanyResponse = await client.GetAsync(
            $"/api/Companies/{companyB.Id}");

        Assert.Equal(HttpStatusCode.OK, ownCompanyResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, otherCompanyResponse.StatusCode);
    }

    [Fact]
    public async Task CompanyToken_CannotReadUpdateOrDeleteAnotherCompanyCustomer()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var companyA = await RegisterAndLoginAsync(client, "CustomerA");
        var companyB = await RegisterAndLoginAsync(client, "CustomerB");

        // Criar um cliente pertencente à empresa B.
        UseToken(client, companyB.Token);

        var createResponse = await client.PostAsJsonAsync(
            "/api/Customers",
            new
            {
                Name = "Cliente Privado B",
                Email = "cliente.b@test.local",
                Phone = "11999990000",
                CompanyId = companyB.Id
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var customerJson = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());

        var customerId = customerJson.RootElement
            .GetProperty("id")
            .GetInt32();

        // Agora autenticar como empresa A e tentar acessar o cliente B.
        UseToken(client, companyA.Token);

        var getResponse = await client.GetAsync(
            $"/api/Customers/{customerId}");

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/Customers/{customerId}",
            new
            {
                Name = "Tentativa de alteração",
                Email = "alterado@test.local",
                Phone = "11000000000",
                IsActive = false
            });

        var deleteResponse = await client.DeleteAsync(
            $"/api/Customers/{customerId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        // Confirmar que o cliente continua existindo e sem alterações.
        UseToken(client, companyB.Token);

        var verifyResponse = await client.GetAsync(
            $"/api/Customers/{customerId}");

        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        var customer = await verifyResponse.Content
            .ReadFromJsonAsync<JsonElement>(JsonOptions);

        Assert.Equal(
            "Cliente Privado B",
            customer.GetProperty("name").GetString());
    }

    [Fact]
    public async Task CompanyToken_CannotReadUpdateOrDeleteAnotherCompanyAppointment()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var companyA = await RegisterAndLoginAsync(client, "AppointmentA");
        var companyB = await RegisterAndLoginAsync(client, "AppointmentB");

        // Criar o cliente necessário para a citação na empresa B.
        UseToken(client, companyB.Token);

        var customerResponse = await client.PostAsJsonAsync(
            "/api/Customers",
            new
            {
                Name = "Cliente da Empresa B",
                Email = "cliente.agenda.b@test.local",
                Phone = "11988887777",
                CompanyId = companyB.Id
            });

        Assert.Equal(HttpStatusCode.Created, customerResponse.StatusCode);

        using var customerJson = JsonDocument.Parse(
            await customerResponse.Content.ReadAsStringAsync());

        var customerId = customerJson.RootElement
            .GetProperty("id")
            .GetInt32();

        var startAt = DateTime.UtcNow.AddDays(2);
        var endAt = startAt.AddHours(1);

        var appointmentResponse = await client.PostAsJsonAsync(
            "/api/Appointments",
            new
            {
                Title = "Consulta Privada B",
                StartAt = startAt,
                EndAt = endAt,
                Notes = "Informação privada da empresa B",
                CompanyId = companyB.Id,
                CustomerId = customerId
            });

        Assert.Equal(
            HttpStatusCode.Created,
            appointmentResponse.StatusCode);

        using var appointmentJson = JsonDocument.Parse(
            await appointmentResponse.Content.ReadAsStringAsync());

        var appointmentId = appointmentJson.RootElement
            .GetProperty("id")
            .GetInt32();

        // A empresa A tenta ler, alterar e eliminar a consulta de B.
        UseToken(client, companyA.Token);

        var getResponse = await client.GetAsync(
            $"/api/Appointments/{appointmentId}");

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/Appointments/{appointmentId}",
            new
            {
                Title = "Tentativa de alteração",
                StartAt = startAt,
                EndAt = endAt,
                Notes = "Tentativa de invasão",
                IsCancelled = true
            });

        var deleteResponse = await client.DeleteAsync(
            $"/api/Appointments/{appointmentId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        // Confirmar que a consulta original continua intacta.
        UseToken(client, companyB.Token);

        var verifyResponse = await client.GetAsync(
            $"/api/Appointments/{appointmentId}");

        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        var appointment = await verifyResponse.Content
            .ReadFromJsonAsync<JsonElement>(JsonOptions);

        Assert.Equal(
            "Consulta Privada B",
            appointment.GetProperty("title").GetString());

        Assert.False(
            appointment.GetProperty("isCancelled").GetBoolean());
    }
}

