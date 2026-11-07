using Microsoft.Playwright;

namespace PlaywrightTests.Pages.Admin.Dashboard;

/// <summary>Ações da tela de Dashboard do admin. Requer sessão de ADMIN autenticada.</summary>
public class DashboardActions(IPage page)
{
    public const string Path = "/admin";

    public DashboardElements Elements { get; } = new(page);

    /// <summary>Abre o dashboard e aguarda o fim do carregamento.</summary>
    public async Task GotoAsync()
    {
        await page.GotoAsync(Path);
        await WaitForLoadAsync();
    }

    /// <summary>Aguarda a página renderizar e o loading sumir.</summary>
    public async Task WaitForLoadAsync()
    {
        await Elements.Page.WaitForAsync();
        await Elements.Loading.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    /// <summary>Lê os cinco cards de totais.</summary>
    public async Task<DashboardTotals> GetTotalsAsync() => new(
        TotalProducts: await ReadIntAsync(Elements.TotalProducts),
        InactiveProducts: await ReadIntAsync(Elements.InactiveProducts),
        TotalOrders: await ReadIntAsync(Elements.TotalOrders),
        LowStock: await ReadIntAsync(Elements.LowStock),
        OutOfStock: await ReadIntAsync(Elements.OutOfStock));

    /// <summary>Lê a quantidade de pedidos de um status (PENDING, PAID, PAYMENT_FAILED, CANCELLED).</summary>
    public Task<int> GetOrdersByStatusAsync(string status) => ReadIntAsync(Elements.OrdersByStatus(status));

    /// <summary>Clica em "Gerenciar estoque" e aguarda a navegação para /admin/stock.</summary>
    public async Task GoToStockManagementAsync()
    {
        await Elements.StockLink.ClickAsync();
        await page.WaitForURLAsync("**/admin/stock");
    }

    private static async Task<int> ReadIntAsync(ILocator locator) =>
        int.Parse((await locator.InnerTextAsync()).Trim());
}

public record DashboardTotals(int TotalProducts, int InactiveProducts, int TotalOrders, int LowStock, int OutOfStock);
