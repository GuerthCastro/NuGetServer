# Security Policy

## 🛡️ Supported Versions

We actively support the following versions of NuGet Server with security updates:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | ✅ Yes             |
| < 1.0   | ❌ No              |

## 🚨 Reporting a Vulnerability

We take the security of NuGet Server seriously. If you discover a security vulnerability, please follow these steps:

### 1. **Do NOT** create a public GitHub issue

Security vulnerabilities should not be reported publicly until they have been addressed.

### 2. Report via Private Channels

Please report security vulnerabilities through one of these methods:

#### GitHub Security Advisories (Preferred)
1. Go to the [Security tab](https://github.com/GuerthCastro/NuGetServer/security/advisories) of this repository
2. Click "Report a vulnerability"
3. Fill out the vulnerability details

#### Email (Alternative)
Send an email to: **security@dragonflytech.solutions**

Include the following information:
- Description of the vulnerability
- Steps to reproduce the issue
- Potential impact
- Suggested fix (if you have one)

### 3. What to Include

When reporting a vulnerability, please include:

- **Type of issue** (e.g., buffer overflow, SQL injection, cross-site scripting, etc.)
- **Full paths** of source files related to the manifestation of the issue
- **Location** of the affected source code (tag/branch/commit or direct URL)
- **Special configuration** required to reproduce the issue
- **Step-by-step instructions** to reproduce the issue
- **Proof-of-concept or exploit code** (if possible)
- **Impact** of the issue, including how an attacker might exploit it

## 🔄 Response Process

### Timeline
- **Initial Response**: Within 48 hours
- **Vulnerability Assessment**: Within 1 week
- **Fix Development**: Depends on severity and complexity
- **Public Disclosure**: After fix is released

### Our Commitment
1. We will confirm receipt of your vulnerability report
2. We will assess the issue and determine its severity
3. We will develop and test a fix
4. We will release the fix and notify users
5. We will publicly disclose the vulnerability after users have had time to update

## 🏆 Security Researchers

We appreciate the work of security researchers and will acknowledge your contribution when appropriate. If you would like to be credited in our security advisories, please let us know when you report the vulnerability.

## 🔒 Security Best Practices

### For Users

#### Production Deployment
- [ ] Change the default API key from the example configuration
- [ ] Use HTTPS/TLS for all communications
- [ ] Implement proper authentication and authorization
- [ ] Regularly update to the latest version
- [ ] Monitor logs for suspicious activity
- [ ] Use a reverse proxy (nginx, Apache) for additional security
- [ ] Implement rate limiting to prevent abuse
- [ ] Secure the package storage directory with appropriate file permissions

#### Network Security
- [ ] Use firewalls to restrict access to the NuGet server
- [ ] Consider using VPN or private networks for sensitive deployments
- [ ] Implement proper network segmentation

#### Package Security
- [ ] Validate packages before accepting them
- [ ] Consider implementing package signing verification
- [ ] Monitor for malicious packages
- [ ] Implement package scanning for vulnerabilities

### For Developers

#### Secure Coding Practices
- [ ] Follow OWASP secure coding guidelines
- [ ] Validate all input data
- [ ] Use parameterized queries to prevent injection attacks
- [ ] Implement proper error handling without information disclosure
- [ ] Use secure authentication mechanisms
- [ ] Implement proper logging for security events

#### Dependencies
- [ ] Regularly update dependencies to patch known vulnerabilities
- [ ] Use tools like `dotnet list package --vulnerable` to check for vulnerable packages
- [ ] Review security advisories for used packages

## 🎯 Scope

This security policy covers:
- The main NuGet Server application
- Configuration and deployment scripts
- Docker images and containers
- Documentation that could impact security

This policy does not cover:
- Third-party dependencies (report to their respective maintainers)
- Issues in older, unsupported versions
- General usage questions or non-security bugs

## 📚 Security Resources

### External Resources
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework)
- [Microsoft Security Development Lifecycle](https://www.microsoft.com/en-us/securityengineering/sdl/)
- [NuGet Security Best Practices](https://docs.microsoft.com/en-us/nuget/policies/security)

### Project Security Features
- Input validation and sanitization
- Secure file handling for package storage
- Authentication via API keys
- HTTPS support for secure communications
- Docker security best practices
- Comprehensive logging for security monitoring

## 📞 Contact

For any questions about this security policy, please contact:
- **Email**: security@dragonflytech.solutions
- **GitHub Issues**: For non-security related questions only

---

**Thank you for helping keep NuGet Server and our users safe!** 🙏
