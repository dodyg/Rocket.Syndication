# AGENTS.md

This document provides essential information for AI agents (including GitHub Copilot, Claude, and other AI assistants) working on the Rocket.Syndication project.

## Project Overview

**Rocket.Syndication** is a .NET 10 library for consuming RSS and Atom feeds with full specification compliance, modern async patterns, and enterprise-ready features.

- **Language:** C# 14.0
- **Target Framework:** .NET 10.0
- **Package ID:** SyndicationClient
- **Key Features:** RSS/Atom feed parsing, HTTP client management, caching, authentication, discovery, and dependency injection support

## Project Structure

```
Rocket.Syndication/
├── src/
│   └── SyndicationClient/           # Main library project
│       ├── Authentication/          # Feed authentication components
│       ├── Caching/                 # Caching mechanisms
│       ├── DependencyInjection/     # DI extensions
│       ├── Discovery/               # Feed discovery features
│       ├── Http/                    # HTTP client handling
│       ├── Models/                  # Data models for feeds
│       ├── Parsing/                 # RSS/Atom parsing logic
│       ├── ISyndicationClient.cs    # Main interface
│       └── SyndicationClient.cs     # Main implementation
└── tests/
    └── SyndicationClient.Tests/     # Unit and integration tests
```

## Development Workflow

### Build Commands

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Format code (MUST run after changes)
dotnet format
```

### Required Workflow

After any code changes:
1. Run `dotnet format` for consistent style
2. Run `dotnet build` to verify compilation
3. Run `dotnet test` to verify correctness

## Code Style Guidelines

- Follow official C# coding conventions: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
- Use nullable reference types (enabled)
- Use implicit usings (enabled)
- Maintain async/await patterns throughout
- Follow dependency injection patterns using Microsoft.Extensions

## Key Dependencies

- **Microsoft.Extensions.Http** - HTTP client factory
- **Microsoft.Extensions.Http.Polly** - Resilience and transient fault handling
- **Microsoft.Extensions.Options** - Options pattern
- **Microsoft.Extensions.Logging.Abstractions** - Logging infrastructure
- **Microsoft.Extensions.DependencyInjection.Abstractions** - DI abstractions
- **Polly** (v8.5.0) - Resilience policies

## Technical Specifications

- Refer to `SPEC-RSS.md` for RSS/Atom feed specification details
- The library implements full RSS 2.0 and Atom 1.0 compliance
- Uses modern async patterns throughout
- Implements enterprise features: caching, authentication, resilience policies

## Architecture Patterns

- **Options Pattern:** Configuration via `SyndicationClientOptions`
- **Factory Pattern:** HTTP client management
- **Dependency Injection:** Full DI support via extension methods
- **Interface-based Design:** `ISyndicationClient` abstraction
- **Async/Await:** All I/O operations are asynchronous

## Testing Guidelines

- Unit tests are located in `tests/SyndicationClient.Tests/`
- Ensure all new features have corresponding tests
- Run full test suite before committing changes
- Test both RSS and Atom feed parsing scenarios

## Important Notes for AI Agents

1. **Always run `dotnet format`** after making code changes
2. **Verify builds and tests** before considering work complete
3. **Maintain async patterns** - don't introduce blocking calls
4. **Preserve nullability annotations** - the project uses nullable reference types
5. **Follow DI patterns** - use constructor injection and options pattern
6. **Update tests** - ensure test coverage for new functionality
7. **Check specifications** - refer to SPEC-RSS.md for feed format details

## Common Tasks

### Adding a new feature
1. Identify appropriate namespace/folder
2. Implement with interfaces and async patterns
3. Add DI registration if needed
4. Write tests
5. Run format, build, and test commands

### Fixing a bug
1. Write a failing test that reproduces the issue
2. Implement the fix
3. Verify the test passes
4. Run full test suite
5. Format and verify build

### Refactoring
1. Ensure tests exist for the area being refactored
2. Make incremental changes
3. Run tests after each significant change
4. Verify builds and tests at completion
5. Run dotnet format

## Contact & Resources

- **Repository:** /mnt/d/GitHub/Rocket.Syndication
- **Solution File:** SyndicationClient.sln
- **Documentation:** README.md, SPEC-RSS.md, CLAUDE.md
