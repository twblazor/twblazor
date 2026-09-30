# Security Policy

## Supported Versions

Security fixes are provided for the three most recent minor release lines of twblazor. Fixes ship as a new patch release of each supported line, so always upgrade to the latest patch of your line (for example `1.11.x`) to receive them.

| Version  | Supported          | .NET    |
| -------- | ------------------ | ------- |
| 1.11.x   | :white_check_mark: | .NET 10 |
| 1.10.x   | :white_check_mark: | .NET 10 |
| 1.9.x    | :white_check_mark: | .NET 10 |
| < 1.9    | :x:                | .NET 10 |

twblazor targets .NET 10 (`net10.0`) only. Other .NET versions are not supported, and issues that only occur on an unsupported .NET version will not receive a security fix.

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues, discussions or pull requests.**

Report them privately using GitHub's private vulnerability reporting:

1. Go to the [Security tab](https://github.com/TwBlazor/twblazor/security) of the repository.
2. Choose **Report a vulnerability**.
3. Describe the issue, including the affected twblazor version, your .NET version and Blazor render mode (Interactive Server or WebAssembly), and steps to reproduce it. A minimal example is very helpful.

## What to Expect

- You will receive an acknowledgement within **7 days**.
- Once the report is confirmed, you will be kept updated on progress towards a fix.
- Fixes are released as a patch version of every supported line that is affected, and credited in the release notes and the published security advisory unless you ask to remain anonymous.
- If a report is declined, for example because the affected version is unsupported or the behavior is not a vulnerability, you will be told why.

Please allow a fix to be released before disclosing the issue publicly.

## Scope

In scope: vulnerabilities in the twblazor NuGet package and its components, such as script injection through component parameters, unsafe handling of user-supplied content or unsafe defaults.

Out of scope: vulnerabilities in third-party dependencies that do not affect twblazor (please report those upstream), and issues in your own application code.
