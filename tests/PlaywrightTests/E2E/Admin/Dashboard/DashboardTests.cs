using Framework.Web.Browser;
using PlaywrightTests.Pages.Admin.Dashboard;

namespace PlaywrightTests.E2E.Admin.Dashboard;

public class DashboardTests : BaseUiTest
{
    private DashboardActions _dashboard = null!;

    [SetUp]
    public async Task SetUp()
    {
        await LoginAsAsync(Settings.Users.Admin);
        _dashboard = new DashboardActions(Page);
    }

    [Test]
    public async Task Admin_ShouldSeeDashboard()
    {
        await _dashboard.GotoAsync();

        await Expect(_dashboard.Elements.Title).ToBeVisibleAsync();
        await Expect(_dashboard.Elements.TotalProducts).ToBeVisibleAsync();
    }
}
