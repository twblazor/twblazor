using System.Diagnostics;
using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Build;

public sealed class GitHistoryTests : IDisposable
{
    private readonly string _repoPath = Path.Combine(Path.GetTempPath(), "twblazor-git-" + Guid.NewGuid().ToString("N"));

    public GitHistoryTests() => Directory.CreateDirectory(_repoPath);

    public void Dispose()
    {
        // git marks object files read-only on some platforms, which blocks a plain recursive delete.
        foreach (var file in Directory.EnumerateFiles(_repoPath, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(_repoPath, recursive: true);
    }

    [Theory]
    [InlineData("2026-09-04\n", 2026, 9, 4)]
    [InlineData("  2026-01-12  ", 2026, 1, 12)]
    public void ParseDate_ReadsTheShortIsoDateGitPrints(string output, int year, int month, int day)
    {
        // Act & Assert
        Assert.Equal(new DateOnly(year, month, day), GitHistory.ParseDate(output));
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("not a date")]
    [InlineData("04/09/2026")]
    public void ParseDate_ReturnsNull_WhenTheOutputIsNotADate(string output)
    {
        // Act & Assert
        Assert.Null(GitHistory.ParseDate(output));
    }

    [Fact]
    public void GetLastModified_ReturnsTheDateOfTheLastCommitThatTouchedTheFile()
    {
        // Arrange
        RequireGit();
        Git("init", "--quiet");
        File.WriteAllText(Path.Combine(_repoPath, "old.razor"), "old");
        File.WriteAllText(Path.Combine(_repoPath, "new.razor"), "new");
        Commit("add old", "2026-03-01T12:00:00Z", "old.razor");
        Commit("add new", "2026-04-15T12:00:00Z", "new.razor");

        // Act & Assert
        Assert.Equal(new DateOnly(2026, 3, 1), GitHistory.GetLastModified(_repoPath, Path.Combine(_repoPath, "old.razor")));
        Assert.Equal(new DateOnly(2026, 4, 15), GitHistory.GetLastModified(_repoPath, Path.Combine(_repoPath, "new.razor")));
    }

    [Fact]
    public void GetLastModified_ReturnsNull_WhenTheFileHasNoHistory()
    {
        // Arrange
        RequireGit();
        Git("init", "--quiet");
        File.WriteAllText(Path.Combine(_repoPath, "untracked.razor"), "x");

        // Act & Assert
        Assert.Null(GitHistory.GetLastModified(_repoPath, Path.Combine(_repoPath, "untracked.razor")));
    }

    [Fact]
    public void GetLastModified_ReturnsNull_WhenTheDirectoryIsNotARepository()
    {
        // Arrange
        RequireGit();
        File.WriteAllText(Path.Combine(_repoPath, "a.razor"), "x");

        // Act & Assert - the temp directory sits outside any repository, so git has nothing to report.
        Assert.Null(GitHistory.GetLastModified(_repoPath, Path.Combine(_repoPath, "a.razor")));
    }

    private static void RequireGit()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo("git", "--version") { RedirectStandardOutput = true, UseShellExecute = false });
            process!.WaitForExit();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            Assert.Skip("git is not available on this machine.");
        }
    }

    private void Commit(string message, string isoDate, string file)
    {
        Git("add", file);
        Git(new Dictionary<string, string> { ["GIT_COMMITTER_DATE"] = isoDate, ["GIT_AUTHOR_DATE"] = isoDate },
            "-c", "user.name=Test", "-c", "user.email=test@example.com", "-c", "commit.gpgsign=false", "commit", "--quiet", "-m", message);
    }

    private void Git(params string[] args) => Git([], args);

    private void Git(Dictionary<string, string> environment, params string[] args)
    {
        var startInfo = new ProcessStartInfo("git") { WorkingDirectory = _repoPath, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        foreach (var (key, value) in environment)
        {
            startInfo.Environment[key] = value;
        }

        using var process = Process.Start(startInfo)!;
        process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Assert.True(process.ExitCode == 0, $"git {string.Join(' ', args)} failed: {error}");
    }
}
