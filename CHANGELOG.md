# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.1] - 2025-08-15

### Added
- Initial release of NuGet Server
- NuGet v3 API protocol support
- Package upload, download, and search functionality
- Docker containerization support
- Comprehensive unit tests
- OpenAPI/Swagger documentation
- Health check endpoints
- Configurable storage and API keys

### Changed

### Deprecated

### Removed

### Fixed
- 🐛 Fixed NuGet v3 service index format with proper @context property
- 🔄 Changed service type names to PascalCase for better compatibility
- 🔍 Added query endpoint to complement the search endpoint (required by Visual Studio)
- 🧩 Configured JSON serialization to maintain @ symbols in property names
- 📦 Modified metadata controller route from "v3/metadata" to "v3/registrations"
- 📋 Improved search response to include all package versions, not just the latest one
- 🔢 Fixed version history display in Visual Studio and nuget.exe by adding download count to versions
- 🗃️ Updated list endpoint to group packages by ID with proper version history

### Security

## [1.0.0] - 2025-08-07

### Added
- 📦 **Push (publish) NuGet packages** via standard NuGet client
- 🔍 **Search and retrieve package metadata** with full text search
- 🌐 **Compatible with NuGet v3 API** protocol
- 📖 **OpenAPI/Swagger documentation** for easy API exploration
- 🐳 **Docker support** with multi-stage builds
- 🔧 **Configurable storage paths** and API keys
- 🛡️ **Health check endpoints** for monitoring
- 🧪 **Comprehensive unit tests** with high coverage
- **Technologies**: .NET 9, ASP.NET Core, Swashbuckle, xUnit, Moq, FluentAssertions, Bogus
- **API Endpoints**: Complete NuGet v3 protocol implementation
- **Configuration**: JSON-based configuration with environment variable support
- **Testing**: Unit tests for controllers, services, and data generation
- **Documentation**: Complete README with deployment and usage instructions
- **Container**: Multi-stage Docker build with optimized image size
- **Security**: API key authentication and configurable access controls

### Technical Details
- **.NET 9** with ASP.NET Core for modern web API development
- **NuGet v3 Protocol** compliance for broad client compatibility
- **File-based Storage** with configurable directory structure
- **Swagger/OpenAPI** integration for API documentation and testing
- **Docker** containerization with Windows and Linux support
- **Testing Framework** using xUnit, Moq, FluentAssertions, and Bogus
- **Health Checks** for monitoring and load balancer integration
- **Logging** with configurable levels and structured output

---

## Version History Template

### [X.Y.Z] - YYYY-MM-DD

#### Added
- New features

#### Changed
- Changes in existing functionality

#### Deprecated
- Soon-to-be removed features

#### Removed
- Now removed features

#### Fixed
- Any bug fixes

#### Security
- In case of vulnerabilities

---

## Contributing to Changelog

When submitting a PR, please add an entry to the "Unreleased" section following these guidelines:

1. **Categories**: Use the standard changelog categories (Added, Changed, Deprecated, Removed, Fixed, Security)
2. **Format**: Use clear, concise descriptions that explain the impact to users
3. **Links**: Include links to issues or PRs where relevant
4. **Breaking Changes**: Clearly mark any breaking changes
5. **Emojis**: Optional but can help with readability (📦 🔍 🐳 🛡️ etc.)

### Example Entry Format

```markdown
### Added
- 🚀 New feature that does X (#123)
- ✨ Enhancement to Y functionality (#124)

### Fixed
- 🐛 Fixed issue with Z causing crashes (#125)
- 🔧 Resolved configuration problem in Docker deployments (#126)

### Security
- 🔒 Updated dependency X to fix CVE-2024-1234 (#127)
```
