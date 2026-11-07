using Microsoft.Playwright;

namespace PlaywrightTests.Pages.Admin.Dashboard;

/// <summary>Elementos da tela de Dashboard do admin (rota /admin).</summary>
public class DashboardElements(IPage page)
{
    // Container e estados
    public ILocator Page => page.GetByTestId("admin-dashboard-page");
    public ILocator Title => page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard", Level = 1 });
    public ILocator Loading => page.GetByTestId("dashboard-loading");
    public ILocator Error => page.GetByTestId("dashboard-error");
    public ILocator ErrorMessage => page.GetByTestId("dashboard-error-message");
    public ILocator ErrorCode => page.GetByTestId("dashboard-error-code");
    public ILocator ErrorDetails => page.GetByTestId("dashboard-error-detail");

    // Cards de totais
    public ILocator TotalProducts => page.GetByTestId("dashboard-total-products");
    public ILocator InactiveProducts => page.GetByTestId("dashboard-inactive-products");
    public ILocator TotalOrders => page.GetByTestId("dashboard-total-orders");
    public ILocator LowStock => page.GetByTestId("dashboard-low-stock");
    public ILocator OutOfStock => page.GetByTestId("dashboard-out-of-stock");

    // Pedidos por status
    public ILocator OrdersByStatusTitle => page.GetByRole(AriaRole.Heading, new() { Name = "Pedidos por status", Level = 2 });
    public ILocator OrdersPending => OrdersByStatus("PENDING");
    public ILocator OrdersPaid => OrdersByStatus("PAID");
    public ILocator OrdersPaymentFailed => OrdersByStatus("PAYMENT_FAILED");
    public ILocator OrdersCancelled => OrdersByStatus("CANCELLED");

    /// <summary>Card de um status de pedido (PENDING, PAID, PAYMENT_FAILED, CANCELLED).</summary>
    public ILocator OrdersByStatus(string status) =>
        page.GetByTestId($"dashboard-orders-{status.ToLowerInvariant().Replace('_', '-')}");

    // Links
    public ILocator StockLink => page.GetByTestId("dashboard-stock-link");
}
