# AI agent skills

Using Claude Code or another AI coding agent? twblazor publishes four skills that teach it to set up, theme, localize and build with twblazor using the real components and parameters, instead of guessing or borrowing another library's API:

- `twblazor-setup`: adds twblazor to a Blazor Server or WebAssembly app.
- `twblazor-theming`: creates and changes your `Theme.cs` through the Theme builder.
- `twblazor-components`: writes markup using only the parameters each component has.
- `twblazor-localization`: translations, date and time formats and right to left layouts.

## Installing the skills

A skill is a folder the agent reads only when a task needs it. The commands below download all four as one zip and extract them into `.claude/skills`, the folder Claude Code loads skills from. Run them from your project's root folder (the one holding your `.sln` or `.csproj`), then start a new Claude Code session.

macOS and Linux:

```bash
# Download every twblazor skill as one zip
curl -L -o twblazor-skills.zip https://twblazor.com/skills/twblazor-skills.zip

# Extract it into .claude/skills, creating the folder if needed and replacing older copies
mkdir -p .claude/skills
unzip -o twblazor-skills.zip -d .claude/skills

# Delete the downloaded zip
rm twblazor-skills.zip
```

Windows PowerShell:

```pwsh
# Download every twblazor skill as one zip
Invoke-WebRequest https://twblazor.com/skills/twblazor-skills.zip -OutFile twblazor-skills.zip

# Extract it into .claude/skills, creating the folder if needed and replacing older copies
Expand-Archive twblazor-skills.zip -DestinationPath .claude/skills -Force

# Delete the downloaded zip
Remove-Item twblazor-skills.zip
```

This installs the skills for that project only. Commit the `.claude/skills` folder and everyone working on the project gets them too. To have them in every project on your machine instead, use `~/.claude/skills` as the destination in place of `.claude/skills`. Run the commands again after upgrading twblazor to get skills that match the new release.

## Other assistants

Assistants without skills support can read [twblazor.com/llms.txt](https://twblazor.com/llms.txt), which lists the real components, setup steps and Theme builder.

See [AI Agents & Skills](https://twblazor.com/ai-agents) for what each skill covers, using them in the Claude apps and pointing other assistants at `llms.txt`.

[Back to the README](README.md)
