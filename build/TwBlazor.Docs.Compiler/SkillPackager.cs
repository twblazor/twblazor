using System.IO.Compression;
using System.Text;

namespace TwBlazor.Docs.Compiler;

/// <summary>
/// Writes agent skills as zip files: one per skill, for apps that take a single uploaded skill, and one with
/// every skill, to extract straight into a <c>skills</c> folder.
/// </summary>
/// <remarks>
/// Each skill sits in a folder named after it at the root of the zip, the layout skill uploads and
/// <c>.claude/skills/</c> both expect. Entries carry a fixed timestamp and a stable order, so an unchanged
/// skill produces the same bytes on every build.
/// </remarks>
public static class SkillPackager
{
    private static readonly DateTimeOffset _timestamp = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Writes <paramref name="skills"/> into <paramref name="destination"/> as a zip.
    /// </summary>
    /// <param name="skills">The skills to include.</param>
    /// <param name="destination">The stream to write the zip to. It is left open.</param>
    public static void WriteZip(IEnumerable<AgentSkill> skills, Stream destination)
    {
        using var archive = new ZipArchive(destination, ZipArchiveMode.Create, leaveOpen: true);

        foreach (var skill in skills.OrderBy(s => s.Name, StringComparer.Ordinal))
        {
            foreach (var (path, text) in skill.Files.OrderBy(f => f.Key, StringComparer.Ordinal))
            {
                var entry = archive.CreateEntry($"{skill.Name}/{path}", CompressionLevel.Optimal);
                entry.LastWriteTime = _timestamp;

                using var stream = entry.Open();
                var bytes = Encoding.UTF8.GetBytes(text);
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
