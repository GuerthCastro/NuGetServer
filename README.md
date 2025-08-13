# NuGet Server

## [2025-08-12] NuGet v3 Compliance and Visual Studio Multi-Version Fix

- Registration endpoint now returns all package versions with correct canonical IDs and casing, ensuring full compatibility with Visual Studio 2022 and nuget.exe.
- All endpoints strictly use DTOs, no anonymous objects, and all required fields are present.
- Visual Studio and nuget.exe now display all historical versions in the version dropdown.

A lightweight, self-hosted NuGet server built with ASP.NET Core (.NET 9). This server provides a private NuGet feed that supports pushing, searching, and downloading NuGet packages through standard NuGet client tools.

---

**Security Notice:**

- Never commit real API keys, secrets, or credentials to this repository. Use environment variables or Docker runtime configuration for all secrets.
- All example API keys in this documentation use `<YOUR_API_KEY>` as a placeholder. Replace with your own value at runtime.
- This repository intentionally has **no CI/CD, build, or deployment automation**. All builds and deployments are managed by users locally (e.g., via Docker or `dotnet` CLI). See [docs/SECURITY_SETUP.md](docs/SECURITY_SETUP.md) for required manual GitHub security settings.


## ✨ Features

- 📦 **Push (publish) NuGet packages** via standard NuGet client
- 🔍 **Search and retrieve package metadata** (now always included, namespace-aware)
- 🌐 **Compatible with NuGet v3 API** protocol (Visual Studio and nuget.exe supported)
- 📊 **Download tracking** with per-package version statistics
- 📖 **OpenAPI/Swagger documentation** for easy API exploration
- 🐳 **Docker support** with multi-stage builds
- 🔧 **Configurable storage paths** and API keys
- 🛡️ **Health check endpoints** for monitoring
- 🧪 **Comprehensive unit tests** with high coverage

## ℹ️ Important Configuration Note

**The `NuGetIndex:ServiceUrl` setting in `appsettings.json` must match your public server root URL (e.g., `http://myserver:8080`), NOT including `/nuget`.**

If this is set incorrectly, Visual Studio and NuGet clients may show errors like `[Appserver003] The source does not have a Search service!`.

## 🛠️ Metadata Extraction Improvements

- Package metadata (description, authors) is now always included in all listings and search results.
- Metadata extraction is namespace-aware and compatible with all valid NuGet `.nuspec` files.
- Improved error handling and logging for missing or malformed metadata.

## 🛠️ Technologies Used

- **.NET 9** (ASP.NET Core)
- **Swashbuckle.AspNetCore** (Swagger/OpenAPI docs)
- **Microsoft.AspNetCore.OpenApi**
- **System.Text.Json** (JSON serialization for download tracking)
- **xUnit**, **Moq**, **FluentAssertions**, **Bogus** (Testing)
- **coverlet.collector** (Code coverage)

## � Download Tracking

The server tracks download counts for each package version. When a package is downloaded:

1. The server creates or updates a `download_count.json` file in the package version folder
2. Download counts are displayed in search results and package metadata
3. The tracking is case-insensitive, so package IDs are matched regardless of casing
4. Each version's download count is tracked separately

## �📁 Project Structure

```
NuGetServer/
├── Controllers/           # API endpoints for NuGet protocol
│   ├── PackagesController.cs          # Health, upload, download
│   ├── PackageSearchController.cs     # Search and listing
│   ├── PackagePublishController.cs    # Publishing and deletion
│   └── PackageMetadataController.cs   # Package metadata
├── Entities/             # Configuration and DTO classes
│   ├── Config/           # Service configuration classes
│   └── DTO/              # Data transfer objects
├── Extensions/           # Extension methods
├── Interfaces/           # Service abstractions
├── Services/             # Business logic and storage
├── Properties/           # Launch settings
├── appsettings.json      # Application configuration
└── Dockerfile           # Container configuration

NuGetServer.Tests/  # Unit tests and test data
├── Controllers/          # Controller tests
├── DataFakers/          # Test data generators
├── Interfaces/          # Test abstractions
└── Services/            # Service tests
```

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Docker (optional, for containerized deployment)

### Local Development

1. **Clone the repository**
   ```sh
   git clone https://github.com/GuerthCastro/NuGetServer.git
   cd NuGetServer
   ```

2. **Build the solution**
   ```sh
   dotnet build
   ```

3. **Run the server**
   ```sh
   dotnet run --project NuGetServer
   ```

4. **Access the application**
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001
   - Swagger UI: http://localhost:5000/swagger

### Configuration

The server can be configured through `appsettings.json` or environment variables:

```json
{
   "NuGetServer": {
      "ApiKey": "<YOUR_API_KEY>",
      "PackagesPath": "/app/nuget-packages"
   },
   "NuGetIndex": {
      "ServiceName": "Dragonfly NuGet Server",
      "ServiceUrl": "http://localhost:5000/nuget"
   }
}
```

**Configuration Options:**
- `NuGetServer.ApiKey`: API key required for package publishing
- `NuGetServer.PackagesPath`: Directory where packages are stored
- `NuGetIndex.ServiceName`: Display name for the NuGet service
- `NuGetIndex.ServiceUrl`: Base URL for the NuGet service

## 📚 API Reference

### Endpoints

| Endpoint                                 | Method | Description                           |
|------------------------------------------|--------|---------------------------------------|
| `/nuget/health`                          | GET    | Health check endpoint                 |
| `/nuget/upload`                          | PUT    | Push (publish) a package              |
| `/nuget/download/{id}/{version}`         | GET    | Download a specific package           |
| `/nuget/index.json`                      | GET    | Service index (NuGet v3 protocol)     |
| `/nuget/search`                          | GET    | Search packages with query           |
| `/nuget/query`                           | GET    | Query packages (required by VS)       |
| `/nuget/list`                            | GET    | List all packages                    |
| `/nuget/autocomplete`                    | GET    | Package name autocomplete            |
| `/nuget/v3/registrations/{id}/index.json` | GET    | Get all versions of a package        |
| `/nuget/v3/registrations/{id}/{version}.json` | GET | Get specific package metadata        |
| `/nuget/v3-flatcontainer/{id}/index.json` | GET  | Package versions (flat container)     |
| `/nuget/v3-flatcontainer/{id}/{version}/{fileName}` | GET | Download package file    |
| `/nuget/{id}/{version}`                  | DELETE | Delete a specific package version    |

### API Documentation
- **Swagger UI**: Available at `/swagger` for interactive API exploration
- **OpenAPI Spec**: Available at `/swagger/v1/swagger.json`


## 📋 Usage Examples

### Push a Package
```sh
# Using NuGet CLI
nuget push MyPackage.1.0.0.nupkg -Source http://localhost:5000/nuget/upload -ApiKey <YOUR_API_KEY>

# Using dotnet CLI
dotnet nuget push MyPackage.1.0.0.nupkg --source http://localhost:5000/nuget/upload --api-key <YOUR_API_KEY>
```

### Configure NuGet Source
```sh
# Add the server as a package source
nuget sources add -Name "Dragonfly" -Source http://localhost:5000/nuget

# Install packages from the server
nuget install MyPackage -Source "Dragonfly"
```

### Search and Download
```sh
# Search packages
curl "http://localhost:5000/nuget/search?q=MyPackage"

# Download specific package
curl "http://localhost:5000/nuget/download/MyPackage/1.0.0" -o MyPackage.1.0.0.nupkg
```

## 🐳 Docker Deployment

**Note:** All builds and deployments are user-managed. This repository does not use any GitHub Actions, CI/CD, or automated build/deployment pipelines. You are responsible for building and running the server locally or in your own environment.

### Prerequisites
- Docker installed on your system
- Sufficient disk space for package storage

### Windows Deployment

1. **Build the Docker image**
   ```powershell
   docker build -t nugetserver:latest -f NuGetServer\Dockerfile .
   ```

2. **Run the container**
   ```powershell
   docker run -d `
     --name nugetserver `
     -p 5000:8080 `
     -p 5001:8081 `
     -v E:\NuGetPackages:/app/nuget-packages `
     -e ASPNETCORE_ENVIRONMENT=Production `
     nugetserver:latest
   ```

3. **Access the server**
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001
   - Swagger: http://localhost:5000/swagger

### Linux Deployment

1. **Build the Docker image**
   ```bash
   docker build -t nugetserver:latest -f NuGetServer/Dockerfile .
   ```

2. **Run the container**
   ```bash
   docker run -d \
     --name nugetserver \
     --restart=always \
     -p 5000:8080 \
     -p 5001:8081 \
     -v /var/nuget-packages:/app/nuget-packages \
     -e ASPNETCORE_ENVIRONMENT=Production \
     nugetserver:latest
   ```

3. **Access the server**
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001
   - Swagger: http://localhost:5000/swagger

### Docker Compose Deployment

For easier management, you can use the included `docker-compose.yml`:

```bash
# Start the service
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the service
docker-compose down
```

### Environment Variables

The Docker container supports the following environment variables:

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | ASP.NET Core environment |
| `ASPNETCORE_URLS` | `http://+:8080;https://+:8081` | URLs to bind to |
| `NuGetServer__ApiKey` | - | Override API key via environment |
| `NuGetServer__PackagesPath` | `/app/nuget-packages` | Package storage path |

### Volume Mounts

- **Package Storage**: Mount a host directory to `/app/nuget-packages` to persist packages
- **Configuration**: Optionally mount custom `appsettings.json` to `/app/appsettings.json`

### Health Checks

The container includes a health check endpoint:

```bash
# Check container health
docker exec dragonfly-nugetserver curl -f http://localhost:8080/nuget/health || exit 1
```

### Container Management

```bash
# View container logs
docker logs dragonfly-nugetserver

# Update the container
docker pull dragonfly-nugetserver:latest
docker stop dragonfly-nugetserver
docker rm dragonfly-nugetserver
# Run the new container with same parameters

# Backup packages
docker cp dragonfly-nugetserver:/app/nuget-packages ./backup/

# Access container shell
docker exec -it dragonfly-nugetserver /bin/bash
```

### Raspberry Pi Deployment with SSL

This section covers deploying the NuGet server on a Raspberry Pi with proper SSL certificate configuration for secure access.

#### 1. Create SSL Certificates

First, create a directory for your certificates and generate self-signed certificates:

```bash
# Create certificates directory
mkdir -p /home/username/certs && cd /home/username/certs 

# Generate self-signed certificate
openssl req -x509 -newkey rsa:2048 -sha256 -days 3650 -nodes \
  -keyout key.pem -out cert.pem -subj "/CN=servername"

# Create PFX file for ASP.NET Core
openssl pkcs12 -export -out nuget.pfx -inkey key.pem -in cert.pem \
  -passout pass:yourpassword

# Export certificate for client import (optional)
openssl x509 -in cert.pem -outform der -out nuget.cer
```

Replace `servername` with your Raspberry Pi hostname or IP address, and `yourpassword` with a secure password.

#### 2. Build the Docker Image

Build the Docker image on your Raspberry Pi:

```bash
# Navigate to project directory
cd /path/to/NuGetServer

# Build the image
docker build -t dragonfly-nugetserver:latest -f NuGetServer/Dockerfile .
```

#### 3. Run the Container with SSL

Run the container with the SSL certificate mounted:

```bash
docker run -d \
  --name dragonfly-nugetserver \
  --restart=always \
  -p 5000:8080 \
  -p 5001:8081 \
  -v /home/username/nuget-server-data:/app/nuget-packages \
  -v /home/username/certs/nuget.pfx:/https/nuget.pfx:ro \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS="http://+:8080;https://+:8081" \
  -e Kestrel__Certificates__Default__Path=/https/nuget.pfx \
  -e Kestrel__Certificates__Default__Password='yourpassword' \
  -e NuGetIndex__ServiceUrl="https://servername:5001" \
  dragonfly-nugetserver:latest
```

Replace:
- `/home/username/nuget-server-data` with your desired storage path
- `/home/username/certs/nuget.pfx` with the path to your certificate
- `yourpassword` with the certificate password you set earlier
- `servername` with your actual server name or IP address

#### 4. Import the Certificate on Client Machines (Windows)

To trust the self-signed certificate on Windows clients:

1. Copy the `nuget.cer` file to your Windows machine
2. Double-click the file and select "Install Certificate"
3. Choose "Local Machine" and click "Next"
4. Select "Place all certificates in the following store"
5. Click "Browse" and select "Trusted Root Certification Authorities"
6. Click "Next" and then "Finish"

#### 5. Configure NuGet Client

On your development machine:

```bash
# Add the NuGet source
nuget sources add -Name "Raspberry NuGet" -Source "https://servername:5001/nuget" -NonInteractive

# Test the connection
nuget list -Source "Raspberry NuGet"
```

## 🧪 Testing & Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Run tests in watch mode during development
dotnet watch test --project NuGetServer.Tests
```

### Test Structure

The project includes comprehensive unit tests:

- **Controller Tests**: Test API endpoints and HTTP responses
- **Service Tests**: Test business logic and storage operations
- **Data Fakers**: Generate realistic test data using Bogus
- **Integration Tests**: Test complete request/response cycles

### Development Setup

1. **Install development tools**
   ```bash
   # Install EF Core tools (if needed for future database features)
   dotnet tool install --global dotnet-ef
   
   # Install code coverage tools
   dotnet tool install --global dotnet-reportgenerator-globaltool
   ```

2. **Run in development mode**
   ```bash
   # Set development environment
   $env:ASPNETCORE_ENVIRONMENT="Development"  # Windows PowerShell
   export ASPNETCORE_ENVIRONMENT=Development  # Linux/macOS
   
   # Run with hot reload
   dotnet watch run --project NuGetServer
   ```

3. **Debug in VS Code**
   - Use the included launch configurations
   - Set breakpoints in controllers and services
   - Use the integrated terminal for package operations

## 🔧 Configuration & Customization

### Advanced Configuration

```json
{
  "Swagger": {
    "Title": "Dragonfly NuGet Server",
    "Version": "v1", 
    "Description": "Simple private NuGet feed"
  },
  "NuGetServer": {
    "ApiKey": "your-secure-api-key-here",
    "PackagesPath": "/app/nuget-packages"
  },
  "NuGetIndex": {
    "ServiceName": "NuGet Server",
    "ServiceUrl": "http://localhost:5000/nuget"
  },
  "ServiceConfig": {
    "ServiceName": "NuGetServer",
    "Environment": "Production"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Security Considerations

- **API Key**: Change the default API key in production
- **HTTPS**: Enable HTTPS for production deployments
- **Authentication**: Consider implementing additional authentication layers
- **Network Security**: Restrict access using firewalls or reverse proxies
- **Storage Security**: Ensure package storage directory has appropriate permissions

### Performance Tuning

- **Storage**: Use SSD storage for better I/O performance
- **Memory**: Allocate sufficient memory for large package operations
- **Network**: Configure appropriate timeout values for large uploads
- **Caching**: Consider implementing response caching for metadata endpoints

## 🔍 Monitoring & Maintenance

### Health Monitoring

```bash
# Check service health
curl http://localhost:5000/nuget/health

# Monitor container resources
docker stats dragonfly-nugetserver

# View application logs
docker logs -f dragonfly-nugetserver
```

### Backup and Recovery

```bash
# Backup packages directory
tar -czf nuget-packages-backup-$(date +%Y%m%d).tar.gz /path/to/nuget-packages

# Restore packages
tar -xzf nuget-packages-backup-20250801.tar.gz -C /path/to/restore/
```

### Troubleshooting

#### Common Issues

1. **Port Already in Use**
   ```bash
   # Check what's using the port
   netstat -tulpn | grep :5000
   
   # Use different ports
   docker run -p 5002:8080 -p 5003:8081 ...
   ```

2. **Permission Issues with Volume Mounts**
   ```bash
   # Fix permissions on Linux
   sudo chown -R 1000:1000 /path/to/nuget-packages
   
   # On Windows, ensure the drive is shared in Docker Desktop
   ```

3. **Package Upload Failures**
   - Verify API key is correct
   - Check available disk space
   - Ensure package is valid (.nupkg format)
   - Check server logs for detailed error messages

4. **Network Connectivity Issues**
   - Verify firewall settings
   - Check Docker network configuration
   - Ensure DNS resolution is working

## 🤝 Contributing

Contributions are welcome! We appreciate your help in making this project better.

### How to Contribute

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes**
   - Follow the existing code style and conventions
   - Add unit tests for new functionality
   - Update documentation as needed
4. **Test your changes**
   ```bash
   dotnet test
   dotnet build
   ```
5. **Commit and push**
   ```bash
   git commit -m "feat: add your feature description"
   git push origin feature/your-feature-name
   ```
6. **Submit a pull request**

### Development Guidelines

- Follow C# coding conventions and .NET best practices
- Write unit tests for new features and bug fixes
- Update documentation for API changes
- Ensure all tests pass before submitting PR
- Use meaningful commit messages (conventional commits preferred)

### Areas for Enhancement

- **Authentication**: Implement JWT or OAuth2 authentication
- **Cloud Storage**: Add support for Azure Blob Storage, AWS S3
- **Database**: Add database support for metadata storage
- **Caching**: Implement Redis or in-memory caching
- **Metrics**: Add Prometheus metrics and monitoring
- **UI**: Create a web interface for package management

## 🔄 Extension Points

The project is designed to be extensible:

### Custom Storage Providers
Implement `IPackageStorageService` to add support for:
- Cloud blob storage (Azure, AWS, GCP)
- Database-backed storage
- Distributed file systems

### Authentication Middleware
Add custom authentication by:
- Implementing ASP.NET Core authentication middleware
- Adding JWT token validation
- Integrating with identity providers (Azure AD, Auth0)

### Custom Package Processing
Extend package handling for:
- Package validation and scanning
- Virus scanning integration
- Custom metadata extraction
- Package signing verification

## 🛡️ Security & Production Readiness

### Security Checklist

- [ ] Change default API key
- [ ] Enable HTTPS in production
- [ ] Implement proper authentication
- [ ] Configure CORS policies
- [ ] Set up proper logging and monitoring
- [ ] Regular security updates
- [ ] Network access controls
- [ ] Package content validation

### Production Deployment Tips

1. **Use Environment Variables** for sensitive configuration
2. **Set up SSL/TLS** with proper certificates
3. **Configure Reverse Proxy** (nginx, Apache) for better security
4. **Implement Rate Limiting** to prevent abuse
5. **Set up Monitoring** and alerting
6. **Regular Backups** of package storage
7. **Update Dependencies** regularly for security patches

## 📊 Performance Considerations

- **Disk I/O**: Use SSD storage for package files
- **Memory Usage**: Monitor memory consumption for large packages
- **Network**: Configure appropriate timeouts for uploads/downloads
- **Concurrent Requests**: Tune Kestrel settings for your load
- **Package Size**: Consider implementing size limits

## 🐛 Error Handling

The server implements comprehensive error handling:

- **HTTP Status Codes**: Standard REST API status codes
- **Validation Errors**: Return 400 Bad Request with details
- **Authentication**: Return 401 Unauthorized for invalid API keys
- **Not Found**: Return 404 for missing packages/versions
- **Server Errors**: Return 500 with logging for debugging

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- NuGet protocol implementation based on [NuGet Server v3 API](https://docs.microsoft.com/en-us/nuget/api/overview)
- Testing framework using [xUnit](https://xunit.net/) and [Moq](https://github.com/moq/moq4)
- API documentation powered by [Swagger/OpenAPI](https://swagger.io/)

---

**Happy packaging! 📦✨**

For issues, feature requests, or questions, please [open an issue](https://github.com/GuerthCastro/NuGetServer/issues) on GitHub.
