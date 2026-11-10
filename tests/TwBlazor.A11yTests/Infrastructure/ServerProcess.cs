using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace TwBlazor.A11yTests.Infrastructure;

/// <summary>
/// Launches the actual TwBlazor.Server executable as a real child process on a free port, the
/// same way a developer or CI would run it. Interactive Server render mode needs a real
/// SignalR/WebSocket transport and a real static-file content root, and WebApplicationFactory's
/// in-process "deferred host" reflection trick (which re-executes Program.Main up to a captured
/// host reference) raced against this app's own `await app.RunAsync()` and tore the host down
/// mid-test - a real process sidesteps that entirely and is simpler to reason about.
/// </summary>
public sealed class ServerProcess : IAsyncDisposable
{
    private readonly Process _process;

    public Uri BaseAddress { get; }

    private ServerProcess(Process process, Uri baseAddress)
    {
        _process = process;
        BaseAddress = baseAddress;
    }

    public static async Task<ServerProcess> StartAsync()
    {
        var serverDllPath = FindServerDll();
        var baseAddress = new Uri($"http://127.0.0.1:{GetFreeTcpPort()}");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"\"{serverDllPath}\"",
            WorkingDirectory = Path.GetDirectoryName(serverDllPath),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.EnvironmentVariables["ASPNETCORE_URLS"] = baseAddress.ToString();
        startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start '{serverDllPath}'.");

        // Surface the child process's own diagnostics if it fails to come up, instead of a bare
        // "connection refused" from the first test to touch it.
        process.OutputDataReceived += (_, e) => { if (e.Data is not null) Console.WriteLine($"[TwBlazor.Server] {e.Data}"); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) Console.WriteLine($"[TwBlazor.Server:err] {e.Data}"); };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await WaitUntilReadyAsync(baseAddress, process);

        return new ServerProcess(process, baseAddress);
    }

    private static async Task WaitUntilReadyAsync(Uri baseAddress, Process process)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var deadline = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < deadline)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException($"TwBlazor.Server exited early with code {process.ExitCode} - see console output above.");
            }

            try
            {
                using var response = await client.GetAsync(baseAddress);
                return; // Any HTTP response at all means Kestrel is up.
            }
            catch (HttpRequestException)
            {
                await Task.Delay(250);
            }
        }

        throw new TimeoutException($"TwBlazor.Server did not become ready at {baseAddress} within 30s.");
    }

    private static string FindServerDll()
    {
        var configuration =
#if DEBUG
            "Debug";
#else
            "Release";
#endif

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "TwBlazor.slnx")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException($"Could not locate the repo root (TwBlazor.slnx) above '{AppContext.BaseDirectory}'.");
        }

        var serverDll = Path.Combine(dir.FullName, "docs", "TwBlazor.Server", "bin", configuration, "net11.0", "TwBlazor.Server.dll");
        if (!File.Exists(serverDll))
        {
            throw new FileNotFoundException($"TwBlazor.Server.dll not found at '{serverDll}'. Build TwBlazor.Server before running these tests.", serverDll);
        }

        return serverDll;
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
            await _process.WaitForExitAsync();
        }

        _process.Dispose();
    }
}
