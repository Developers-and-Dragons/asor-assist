# ASOR Assistant

A cross-platform desktop tool for registering external agents against the Workday Agent System of Record (ASOR) API v1.2.

Built by [Developers and Dragons](https://github.com/Developers-and-Dragons).

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4) ![Avalonia UI](https://img.shields.io/badge/Avalonia-12-blue) ![License](https://img.shields.io/github/license/Developers-and-Dragons/asor-assist)

## What it does

- **Visual editor** for authoring ASOR agent definitions with inline validation
- **JSON mode** for direct editing of the raw payload
- **Registration** — POST definitions directly to a Workday tenant's ASOR API
- **Fetch from tenant** — pull existing registered agents and edit them locally
- **Service operation lookups** — search for SOAP and REST WIDs via WQL
- **Local drafts** — save, load, and manage definitions locally
- **Contextual help** — right-side guidance panel for each section

## Download

See [Releases](https://github.com/Developers-and-Dragons/asor-assist/releases) for signed builds:

| Platform | File | Notes |
|----------|------|-------|
| Windows x64 | `AsorAssistant_Windows_x64.zip` | EV code signed |
| macOS ARM64 | `AsorAssistant_macOS_ARM64.zip` | Signed + notarized |

### Windows

1. Download and extract the zip
2. Run `AsorAssistant.App.exe`

### macOS

1. Download and extract the zip
2. Run `AsorAssistant.App`
3. If prompted about an unidentified developer: System Settings → Privacy & Security → Open Anyway

## Getting started

1. **Connection** — set your region and paste a bearer token from the Workday Developer Site (upper right corner — login to tenant and copy token)
2. **Open** — load a saved draft or fetch existing agents from a tenant
3. **Edit** — fill in required fields (Identity, Provider & Platform, Skills)
4. **Workday Config** — map skills to Workday resources and execution modes (use Lookups to find WIDs)
5. **Validate** — check the definition against the ASOR v1.2 spec
6. **Save** — save as a local draft
7. **Register** — POST to the Workday ASOR API

## ASOR spec

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

## Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Build and run

```bash
dotnet build AsorAssistant.slnx
dotnet run --project src/AsorAssistant.App
```

### Run tests

```bash
dotnet test AsorAssistant.slnx
```

### Project structure

```
src/
  AsorAssistant.App/            # Avalonia UI, ViewModels, DI
  AsorAssistant.Core/           # Use cases, serialization, ports
  AsorAssistant.Domain/         # Models, validation
  AsorAssistant.Infrastructure/ # HTTP clients, file persistence
tests/
  AsorAssistant.Domain.Tests/
  AsorAssistant.Core.Tests/
  AsorAssistant.Infrastructure.Tests/
```

### Architecture

```
Domain (zero dependencies)
  ↑
Core (serialization, ports, services)
  ↑
Infrastructure (HTTP, file storage)
  ↑
App (Avalonia UI, ViewModels, DI wiring)
```

## License

See [LICENSE](LICENSE) for details.
