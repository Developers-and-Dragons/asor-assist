# ASOR Assistant

*A desktop tool for registering external agents against the Workday Agent System of Record (ASOR).*

Built by [Developers and Dragons](https://github.com/Developers-and-Dragons).

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge) ![Avalonia UI](https://img.shields.io/badge/Avalonia-12-blue?style=for-the-badge) [![Download](https://img.shields.io/badge/Download_Latest-orange?style=for-the-badge)](https://github.com/Developers-and-Dragons/asor-assist/releases)

---

## What it does

- **Visual editor** for authoring ASOR agent definitions with inline validation
- **JSON mode** for direct editing of the raw payload
- **Registration** — POST definitions directly to a Workday tenant
- **Fetch from tenant** — pull existing registered agents and edit them locally
- **Service operation lookups** — search for SOAP and REST WIDs
- **Local drafts** — save, load, and manage definitions locally
- **Contextual help** — right-side guidance panel for each section

---

## Quick Start

### 1. Download

Get the latest build from [GitHub Releases](https://github.com/Developers-and-Dragons/asor-assist/releases):

| Platform | File | Notes |
|----------|------|-------|
| Windows x64 | `AsorAssistant_Windows_x64.zip` | EV code signed |
| macOS ARM64 | `AsorAssistant_macOS_ARM64.zip` | Signed + notarized |

### 2. Install & Run

**Windows**

1. Download and extract the zip
2. Run `AsorAssistant.App.exe`

> **Windows SmartScreen Notice**
> Even with code signing, Windows SmartScreen may show "Windows protected your PC" until the app builds download reputation with Microsoft.
> This is normal for new or updated releases. Click **More info** → **Run anyway** to proceed.
> The warning will disappear as more users successfully run the signed app.

**macOS**

1. Download and extract the zip
2. Open `Asor Assistant.app`

> **macOS Gatekeeper Notice**
> The first time you open the app, macOS may show "App is from an unidentified developer."
> Right-click → **Open** once to approve; future launches will be trusted.

### 3. Get started

1. **Connection** — set your region and paste a bearer token from the Workday Developer Site (upper right corner — login to tenant and copy token)
2. **Open** — load a saved draft or fetch existing agents from a tenant
3. **Edit** — fill in required fields (Identity, Provider & Platform, Skills)
4. **Workday Config** — map skills to Workday resources and execution modes (use Lookups to find WIDs)
5. **Validate** — check the definition against the ASOR v1.2 spec
6. **Save** — save as a local draft
7. **Register** — POST to the Workday ASOR API

---

## ASOR Spec

This tool targets the [ASOR v1.2 specification](https://github.com/Workday/asor/blob/main/versions/v1.2.md).

**Regional endpoints:**

| Region | Endpoint |
|--------|----------|
| US | `https://us.agent.workday.com` |
| EU | `https://eu.agent.workday.com` |
| UK | `https://uk.agent.workday.com` |
| SIN | `https://sg.agent.workday.com` |
| IND | `https://in.agent.workday.com` |
| JPN | `https://jp.agent.workday.com` |

---

## Building from source

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet build AsorAssistant.slnx
dotnet run --project src/AsorAssistant.App
dotnet test AsorAssistant.slnx
```

---

## License

Licensed under the **MIT License** — see [LICENSE](LICENSE).
