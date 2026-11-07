using Microsoft.Playwright;
using NUnit.Framework.Interfaces;

namespace Framework.Web.Evidence;

/// <summary>Grava o trace do Playwright em todo teste e só salva o arquivo quando o teste falha.</summary>
public static class TraceRecorder
{
    public static Task StartAsync(IBrowserContext context) =>
        context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });

    public static async Task StopAsync(IBrowserContext context)
    {
        var test = TestContext.CurrentContext;

        if (test.Result.Outcome.Status != TestStatus.Failed)
        {
            await context.Tracing.StopAsync();
            return;
        }

        var path = Path.Combine(test.WorkDirectory, "playwright-traces", $"{Sanitize(test.Test.FullName)}.zip");
        await context.Tracing.StopAsync(new() { Path = path });
        TestContext.AddTestAttachment(path, "Playwright trace (abra com: pwsh bin/Debug/net10.0/playwright.ps1 show-trace <arquivo>)");
    }

    private static string Sanitize(string name) =>
        string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
