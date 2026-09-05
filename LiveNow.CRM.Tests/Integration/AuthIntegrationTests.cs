using System.Net;
using System.Net.Http.Json;
using LiveNow.CRM.Core.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LiveNow.CRM.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthIntegrationTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        using HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { UserOrEmail = "testadmin", Password = "Test-password-1234567890" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginResponseDto? body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { UserOrEmail = "testadmin", Password = "wrong-password" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SensitiveEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync("/api/customers");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Health_WithoutToken_RemainsPublic()
    {
        using HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync("/api/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
