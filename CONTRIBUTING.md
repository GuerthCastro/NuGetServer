# Contributing to NuGet Server

Thank you for your interest in contributing to NuGet Server! We welcome contributions from the community.

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/) (recommended)

### Setting up the Development Environment

1. **Fork the repository** on GitHub
2. **Clone your fork**:
   ```bash
   git clone https://github.com/yourusername/NuGetServer.git
   cd NuGetServer
   ```
3. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```
4. **Install dependencies**:
   ```bash
   dotnet restore
   ```
5. **Build the project**:
   ```bash
   dotnet build
   ```
6. **Run tests**:
   ```bash
   dotnet test
   ```

## 🛠️ Development Guidelines

### Code Style

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- Use meaningful names for variables, methods, and classes
- Write XML documentation comments for public APIs
- Keep methods small and focused on a single responsibility

### Testing

- Write unit tests for all new features
- Maintain or improve test coverage
- Use descriptive test names that explain what is being tested
- Follow the Arrange-Act-Assert (AAA) pattern
- Use the existing test frameworks: xUnit, Moq, FluentAssertions, Bogus

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): description

[optional body]

[optional footer]
```

Types:
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Changes that don't affect meaning (white-space, formatting, etc.)
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `test`: Adding missing tests or correcting existing tests
- `chore`: Changes to the build process or auxiliary tools

Examples:
```
feat(api): add package versioning endpoint
fix(storage): resolve null reference in package deletion
docs(readme): update installation instructions
test(controllers): add tests for error handling scenarios
```

## 📝 Pull Request Process

1. **Update documentation** if your changes affect the API or user interface
2. **Add or update tests** as appropriate
3. **Ensure all tests pass**:
   ```bash
   dotnet test
   ```
4. **Ensure the build succeeds**:
   ```bash
   dotnet build --configuration Release
   ```
5. **Update the README.md** if necessary
6. **Create a pull request** with:
   - Clear title and description
   - Reference to related issues (if any)
   - Screenshots (if UI changes)
   - Breaking changes noted (if any)

### Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing
- [ ] Tests pass locally
- [ ] New tests added for new functionality
- [ ] Updated existing tests as needed

## Checklist
- [ ] Code follows the project's style guidelines
- [ ] Self-review of code completed
- [ ] Code is commented, particularly in hard-to-understand areas
- [ ] Corresponding changes to documentation made
- [ ] No new warnings introduced
```

## 🐛 Reporting Bugs

When reporting bugs, please include:

1. **Clear title** and description
2. **Steps to reproduce** the issue
3. **Expected behavior**
4. **Actual behavior**
5. **Environment details**:
   - Operating System
   - .NET version
   - Docker version (if applicable)
6. **Logs or error messages**
7. **Screenshots** (if applicable)

## 💡 Suggesting Features

When suggesting new features:

1. **Check existing issues** to avoid duplicates
2. **Provide clear use case** and motivation
3. **Describe the proposed solution**
4. **Consider alternative solutions**
5. **Think about backward compatibility**

## 🔍 Areas for Contribution

We welcome contributions in these areas:

### High Priority
- **Security improvements** (authentication, authorization, input validation)
- **Performance optimizations** (caching, async operations)
- **Bug fixes** and stability improvements

### Medium Priority
- **Database support** (Entity Framework integration)
- **Additional NuGet protocol features**
- **Monitoring and observability** features
- **Web UI** for package management

### Low Priority
- **Documentation improvements**
- **Code quality** improvements
- **Additional testing** scenarios
- **Docker optimizations**

## 📚 Resources

- [NuGet Server API Documentation](https://docs.microsoft.com/en-us/nuget/api/overview)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Docker Documentation](https://docs.docker.com/)

## 🤝 Code of Conduct

This project adheres to a [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## 📞 Getting Help

- **GitHub Issues**: For bug reports and feature requests
- **Discussions**: For questions and general discussion
- **Documentation**: Check the README.md and inline code comments

## 🎉 Recognition

Contributors will be recognized in:
- GitHub contributors list
- Release notes (for significant contributions)
- README.md acknowledgments section

Thank you for contributing to NuGet Server! 🚀
