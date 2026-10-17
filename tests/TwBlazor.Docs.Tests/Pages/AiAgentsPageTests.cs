using Bunit;
using Microsoft.AspNetCore.Components.Web;
using TwBlazor.Docs.Generated;
using TwBlazor.Docs.Pages.Guides;
using TwBlazor.Docs.Services;

namespace TwBlazor.Docs.Tests.Pages;

public class AiAgentsPageTests : DocsTestBase
{
    [Fact]
    public async Task Page_HasItsOwnTitleAndCanonicalUrl()
    {
        // Arrange & Act
        var head = TestContext.Render<HeadOutlet>();
        var page = TestContext.Render<AiAgents>();
        await head.WaitForStateAsync(() => head.Markup.Contains("canonical"));

        // Assert
        var title = head.Find("title").TextContent;
        Assert.Contains("llms.txt", title);
        Assert.True(title.Length <= SiteMetadata.MaxTitleLength, title);
        Assert.Equal("https://twblazor.com/ai-agents", head.Find("link[rel='canonical']").GetAttribute("href"));
        Assert.Equal("AI Agents & Skills", page.Find("h1").TextContent);
    }

    [Fact]
    public void Page_ListsEveryGeneratedSkill_WithADescriptiveDownloadLink()
    {
        // Arrange & Act
        var page = TestContext.Render<AiAgents>();

        // Assert
        Assert.NotEmpty(AgentSkills.All);
        var rows = page.FindAll("#agent-skills tbody tr");
        Assert.Equal(AgentSkills.All.Length, rows.Count);

        foreach (var (row, skill) in rows.Zip(AgentSkills.All))
        {
            Assert.Equal(skill.Name, row.QuerySelector("th[scope='row']")!.TextContent);
            Assert.Contains(skill.Description, row.TextContent);

            var download = row.QuerySelector("a[download]")!;
            Assert.Equal($"/skills/{skill.Name}.zip", download.GetAttribute("href"));
            Assert.Equal($"Download the {skill.Name} skill as a zip", download.GetAttribute("aria-label"));
        }
    }

    [Fact]
    public void Page_OffersEverySkillInOneDownload()
    {
        // Arrange & Act
        var page = TestContext.Render<AiAgents>();

        // Assert
        var bundle = page.Find("#download-all-skills");
        Assert.Equal($"/skills/{AgentSkills.BundleName}.zip", bundle.GetAttribute("href"));
        Assert.True(bundle.HasAttribute("download"));
    }

    [Fact]
    public void Page_LinksToTheLlmsFiles_InANewTabThatScreenReadersAreToldAbout()
    {
        // Arrange & Act
        var page = TestContext.Render<AiAgents>();

        // Assert
        var links = page.FindAll("#llms-files a");
        Assert.Equal(["https://twblazor.com/llms.txt", "https://twblazor.com/llms-full.txt", "https://twblazor.com/button.md"], links.Select(a => a.GetAttribute("href")));
        Assert.All(links, a =>
        {
            Assert.Equal("_blank", a.GetAttribute("target"));
            Assert.Equal("noopener noreferrer", a.GetAttribute("rel"));
            Assert.Contains("opens in a new tab", a.TextContent);
        });
    }

    [Fact]
    public void Page_LabelsItsTablesForScreenReaders()
    {
        // Arrange & Act
        var page = TestContext.Render<AiAgents>();

        // Assert
        foreach (var table in page.FindAll("#llms-files, #agent-skills"))
        {
            var element = table.TagName == "TABLE" ? table : table.QuerySelector("table")!;
            Assert.NotEmpty(element.QuerySelector("caption")!.TextContent);
            Assert.All(element.QuerySelectorAll("thead th"), th => Assert.Equal("col", th.GetAttribute("scope")));
            Assert.All(element.QuerySelectorAll("tbody th"), th => Assert.Equal("row", th.GetAttribute("scope")));
        }
    }

    [Fact]
    public void Page_ShowsTheInstallCommandsForBothShells()
    {
        // Arrange & Act
        var page = TestContext.Render<AiAgents>();

        // Assert
        Assert.Contains("unzip -o twblazor-skills.zip -d .claude/skills", page.Markup);
        Assert.Contains("Expand-Archive twblazor-skills.zip -DestinationPath .claude/skills -Force", page.Markup);
    }
}
