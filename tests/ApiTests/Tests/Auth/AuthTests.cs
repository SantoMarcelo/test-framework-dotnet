using System.Net;
using ApiTests.Utils;
using Framework.Core.Api.Models.Auth;

namespace ApiTests.Tests.Auth;

public class AuthTests : BaseApiTest
{
    [Test]
    public async Task Login_WithCustomer_ShouldReturnToken()
    {
        var customer = Settings.Users.Customer;

        var response = await Api.Auth.LoginAsync(new LoginRequest(customer.Email, customer.Password));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data?.Token, Is.Not.Empty);
            Assert.That(response.Data?.User.Role, Is.EqualTo("CUSTOMER"));
        });
    }

    [Test]
    public async Task Me_WithAdminToken_ShouldReturnAdmin()
    {
        using var api = await ApiAsAsync(Settings.Users.Admin);

        var response = await api.Auth.MeAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data?.Email, Is.EqualTo(Settings.Users.Admin.Email));
            Assert.That(response.Data?.Role, Is.EqualTo("ADMIN"));
        });
    }

    [Test]
    public async Task Me_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await Api.Auth.MeAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
