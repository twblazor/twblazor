using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace TwBlazor.Docs.Compiler;

/// <summary>
/// Reads file modification dates from git history, so sitemap <c>lastmod</c> values and the "last
/// updated" line on each docs page reflect when the content actually changed rather than when it was built.
/// </summary>
public static class GitHistory
{
    /// <summary>
    /// Returns the date of the most recent commit that touched <paramref name="filePath"/>, or
    /// <see langword="null"/> when it can't be determined (git is missing, the directory isn't a
    /// repository, or the file has no history yet).
    /// </summary>
    /// <param name="repositoryRoot">The directory git is run from.</param>
    /// <param name="filePath">The file whose last commit date is wanted.</param>
    /// <returns>The commit's calendar date, or <see langword="null"/>.</returns>
    public static DateOnly? GetLastModified(string repositoryRoot, string filePath)
    {
        var git = ResolveGitExecutable();
        if (git is null)
        {
            return null;
        }

        var startInfo = new ProcessStartInfo(git)
        {
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("log");
        startInfo.ArgumentList.Add("-1");
        startInfo.ArgumentList.Add("--format=%cs");
        startInfo.ArgumentList.Add("--");
        startInfo.ArgumentList.Add(filePath);

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode == 0 ? ParseDate(output) : null;
        }
        catch (Win32Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Finds git on the <c>PATH</c> and returns its absolute path, so the process that runs is always a
    /// specific executable rather than whatever a bare command name happens to resolve to.
    /// </summary>
    /// <returns>The full path to the git executable, or <see langword="null"/> when git isn't installed.</returns>
    private static string? ResolveGitExecutable()
    {
        var fileName = OperatingSystem.IsWindows() ? "git.exe" : "git";
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

        return path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(directory => directory.Trim('"'))
            .Where(Path.IsPathRooted)
            .Select(directory => Path.Combine(directory, fileName))
            .FirstOrDefault(File.Exists);
    }

    /// <summary>
    /// Parses the short ISO date (<c>yyyy-MM-dd</c>) that <c>git log --format=%cs</c> prints.
    /// </summary>
    /// <param name="output">Raw git output, which may carry surrounding whitespace or be empty.</param>
    /// <returns>The parsed date, or <see langword="null"/> when the output isn't a date.</returns>
    public static DateOnly? ParseDate(string output) =>
        DateOnly.TryParseExact(output.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
}
