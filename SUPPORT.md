# Support

## Getting Help

Thank you for using NuGet Server! If you need help or have questions, there are several ways to get support:

### 📚 Documentation

Before seeking help, please check our comprehensive documentation:

- **[README.md](README.md)** - Complete setup and usage guide
- **[Configuration Guide](README.md#configuration)** - Detailed configuration options
- **[Docker Deployment](README.md#docker-deployment)** - Container deployment instructions
- **[API Reference](README.md#api-reference)** - Complete API documentation
- **[Troubleshooting](README.md#troubleshooting)** - Common issues and solutions

### 💬 Community Support

#### GitHub Discussions
For general questions, discussions, and community support:
- **[GitHub Discussions](https://github.com/GuerthCastro/NuGetServer/discussions)**

#### GitHub Issues
For bug reports and feature requests:
- **[Bug Reports](https://github.com/GuerthCastro/NuGetServer/issues/new?template=bug_report.md)**
- **[Feature Requests](https://github.com/GuerthCastro/NuGetServer/issues/new?template=feature_request.md)**
- **[Documentation Issues](https://github.com/GuerthCastro/NuGetServer/issues/new?template=documentation.md)**

### 🔍 Self-Help Resources

#### Common Questions

**Q: How do I change the API key?**
A: Update the `NuGetServer.ApiKey` value in your `appsettings.json` file or set the `NuGetServer__ApiKey` environment variable.

**Q: Why can't I push packages?**
A: Ensure you're using the correct API key and that the packages directory has write permissions.

**Q: How do I configure HTTPS?**
A: Set up a reverse proxy (nginx/Apache) or configure Kestrel with SSL certificates. See the deployment documentation.

**Q: Can I use cloud storage?**
A: Currently, the server supports local file storage. Cloud storage support is planned for future releases.

#### Troubleshooting Steps

1. **Check the logs** - Review application logs for error messages
2. **Verify configuration** - Ensure `appsettings.json` is correctly configured
3. **Test connectivity** - Use the health check endpoint: `GET /nuget/health`
4. **Check permissions** - Verify the packages directory has proper read/write permissions
5. **Update to latest** - Ensure you're using the latest version

### 🐛 Reporting Issues

When reporting issues, please include:

- **Environment details** (OS, .NET version, deployment method)
- **Configuration** (sanitized `appsettings.json`)
- **Error messages** and logs
- **Steps to reproduce** the issue
- **Expected vs actual behavior**

Use our issue templates to ensure you provide all necessary information:
- [Bug Report Template](https://github.com/GuerthCastro/NuGetServer/issues/new?template=bug_report.md)

### 💡 Feature Requests

We welcome feature requests! When suggesting new features:

- **Search existing requests** to avoid duplicates
- **Describe the use case** and problem you're solving
- **Provide detailed requirements** and expected behavior
- **Consider implementation complexity** and maintenance impact

Use our [Feature Request Template](https://github.com/GuerthCastro/NuGetServer/issues/new?template=feature_request.md)

### 🤝 Contributing

Want to help improve NuGet Server?

- **Code contributions** - See our [Contributing Guide](CONTRIBUTING.md)
- **Documentation** - Help improve our docs
- **Testing** - Report bugs and test new features
- **Community** - Help answer questions in discussions

### 📞 Contact Information

#### Project Maintainers
- **GitHub**: [@GuerthCastro](https://github.com/GuerthCastro)

#### Security Issues
For security vulnerabilities, please see our [Security Policy](SECURITY.md) and report privately.

#### Commercial Support
For commercial support, consulting, or custom development:
- **Email**: support@dragonflytech.solutions

### ⏱️ Response Times

This is a community-driven open source project. Response times may vary:

- **Critical security issues**: Within 48 hours
- **Bug reports**: Within 1 week
- **Feature requests**: When time allows
- **Questions/discussions**: Best effort from community

### 🌟 Supporting the Project

If NuGet Server is helpful to you, consider:

- ⭐ **Star the repository** to show your support
- 🐛 **Report bugs** and suggest improvements
- 📖 **Improve documentation** with pull requests
- 💝 **Sponsor the project** (see [FUNDING.yml](.github/FUNDING.yml))
- 📢 **Share with others** who might find it useful

### 📋 Support Guidelines

To help us provide better support:

#### Do ✅
- Search existing issues before creating new ones
- Provide complete information using our templates
- Be patient and respectful
- Follow up on your issues
- Help others when you can

#### Don't ❌
- Open duplicate issues
- Ask for unrealistic timelines
- Demand immediate responses
- Share sensitive information (API keys, passwords, etc.)
- Use issues for general support questions (use discussions instead)

### 🔗 Additional Resources

- **NuGet Documentation**: [docs.microsoft.com/nuget](https://docs.microsoft.com/en-us/nuget/)
- **Docker Documentation**: [docs.docker.com](https://docs.docker.com/)
- **.NET Documentation**: [docs.microsoft.com/dotnet](https://docs.microsoft.com/en-us/dotnet/)
- **ASP.NET Core**: [docs.microsoft.com/aspnet](https://docs.microsoft.com/en-us/aspnet/core/)

---

**Thank you for being part of the NuGet Server community!** 🙏

We appreciate your interest in the project and your patience as we work to provide the best possible experience for everyone.
