using System.IO.Compression;
using TwBlazor.Docs.Compiler;

namespace TwBlazor.Docs.Tests.Build;

public class SkillPackagerTests
{
    private static readonly AgentSkill[] _skills =
    [
        new("twblazor-theming", "Themes.", new Dictionary<string, string> { ["SKILL.md"] = "theming", ["references/class-merge.md"] = "merge" }),
        new("twblazor-setup", "Sets up.", new Dictionary<string, string> { ["SKILL.md"] = "setup" }),
    ];

    private static byte[] Zip(IEnumerable<AgentSkill> skills)
    {
        using var stream = new MemoryStream();
        SkillPackager.WriteZip(skills, stream);
        return stream.ToArray();
    }

    [Fact]
    public async Task WriteZip_PutsEachSkillInAFolderNamedAfterIt_AtTheRootOfTheZip()
    {
        using var archive = new ZipArchive(new MemoryStream(Zip(_skills)), ZipArchiveMode.Read);

        Assert.Equal(
            ["twblazor-setup/SKILL.md", "twblazor-theming/SKILL.md", "twblazor-theming/references/class-merge.md"],
            archive.Entries.Select(e => e.FullName));

        using var reader = new StreamReader(await archive.GetEntry("twblazor-theming/references/class-merge.md")!.OpenAsync(TestContext.Current.CancellationToken));
        Assert.Equal("merge", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public void WriteZip_WritesTheSameBytes_ForTheSameSkills()
    {
        Assert.Equal(Zip(_skills), Zip(_skills.Reverse()));
    }
}
