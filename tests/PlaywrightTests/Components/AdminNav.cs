using Microsoft.Playwright;

namespace PlaywrightTests.Components;

/// <summary>Menu de navegação do admin, presente em todas as páginas /admin/*.</summary>
public class AdminNav(IPage page)
{
    public ILocator Layout => page.GetByTestId("admin-layout");
    public ILocator Nav => page.GetByTestId("admin-nav");
    public ILocator DashboardLink => page.GetByTestId("admin-nav-dashboard");
    public ILocator ProductsLink => page.GetByTestId("admin-nav-products");
    public ILocator StockLink => page.GetByTestId("admin-nav-stock");
    public ILocator OrdersLink => page.GetByTestId("admin-nav-orders");

    public async Task GoToDashboardAsync()
    {
        await DashboardLink.ClickAsync();
        await page.WaitForURLAsync("**/admin");
    }

    public async Task GoToProductsAsync()
    {
        await ProductsLink.ClickAsync();
        await page.WaitForURLAsync("**/admin/products");
    }

    public async Task GoToStockAsync()
    {
        await StockLink.ClickAsync();
        await page.WaitForURLAsync("**/admin/stock");
    }

    public async Task GoToOrdersAsync()
    {
        await OrdersLink.ClickAsync();
        await page.WaitForURLAsync("**/admin/orders");
    }
}
