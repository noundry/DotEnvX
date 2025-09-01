# Noundry.DotEnvX for .NET

[![NuGet](https://img.shields.io/nuget/v/Noundry.DotEnvX.svg)](https://www.nuget.org/packages/Noundry.DotEnvX/)
[![License](https://img.shields.io/badge/license-BSD--3--Clause-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com)
[![Build Status](https://img.shields.io/github/actions/workflow/status/plsft/DotEnvX/dotnet.yml?branch=main)](https://github.com/plsft/DotEnvX/actions)

A secure, feature-complete port of [dotenvx](https://github.com/dotenvx/dotenvx) for modern .NET applications. Load environment variables from `.env` files with support for **encryption**, **multiple environments**, and **variable expansion**.

## 🌟 Why Noundry.DotEnvX?

- **🔐 Built-in Encryption** - Protect sensitive data with ECIES encryption
- **🛠️ CLI Tool** - Powerful command-line interface for managing .env files
- **💉 Dependency Injection** - First-class support for ASP.NET Core
- **📁 Multiple Files** - Load environment-specific configurations
- **🔄 Variable Expansion** - Reference other variables with `${VAR}` syntax
- **✅ Production Ready** - Battle-tested with comprehensive samples

## 📦 Installation

### Core Library
```bash
dotnet add package Noundry.DotEnvX
```

### CLI Tool (Global)
```bash
dotnet tool install --global Noundry.DotEnvX.Tool
```

> **Note:** The `Noundry.DotEnvX` package includes both core functionality and ASP.NET Core integration. No separate packages needed!

## 🚀 Quick Start

### Try the Samples

```bash
cd samples/DotEnvX.Samples
dotnet run
```

The samples project contains 9 comprehensive examples demonstrating:
- Basic usage and advanced options
- Parsing and encryption/decryption
- Dependency injection and configuration provider
- Variable expansion and multiple files
- Example generation

### Basic Usage

Create a `.env` file:
```env
DATABASE_URL=postgresql://localhost/mydb
API_KEY=sk-1234567890abcdef
DEBUG=true
PORT=3000
```

Load in your application:
```csharp
using Noundry.DotEnvX.Core;

// Load .env file
DotEnv.Config();

// Access variables
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"Database: {dbUrl}");
```

### ASP.NET Core Integration

```csharp
using Noundry.DotEnvX.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add to configuration
builder.Configuration.AddDotEnvX();

// Or with DI and options
builder.Services.AddDotEnvX(options =>
{
    options.Path = new[] { ".env", $".env.{builder.Environment.EnvironmentName}" };
    options.Overload = true;
});

var app = builder.Build();

app.MapGet("/", () => new
{
    Environment = app.Environment.EnvironmentName,
    Database = Environment.GetEnvironmentVariable("DATABASE_URL")
});

app.Run();
```

### 🔐 Encryption

Protect sensitive values with military-grade encryption:

```csharp
// Generate keypair
var keypair = DotEnv.GenerateKeypair();

// Save keys
File.WriteAllText(".env.keys", $"DOTENV_PRIVATE_KEY={keypair.PrivateKey}");
File.AppendAllText(".env", $"#DOTENV_PUBLIC_KEY={keypair.PublicKey}\n");

// Encrypt a value
DotEnv.Set("API_SECRET", "super-secret-value", new SetOptions
{
    Path = new[] { ".env" },
    Encrypt = true
});
```

Your `.env` file will contain:
```env
#DOTENV_PUBLIC_KEY=04abc123...
API_SECRET="encrypted:BDb7t3QkTRp2..."
```

Values are automatically decrypted when loaded:
```csharp
DotEnv.Config(); // Auto-decrypts using .env.keys
var secret = Environment.GetEnvironmentVariable("API_SECRET");
// secret = "super-secret-value" (decrypted!)
```

## 🛠️ CLI Tool

The `dotenvx` command provides powerful environment management:

### Setting Values
```bash
# Set single value
dotenvx set DATABASE_URL=postgresql://localhost/mydb

# Set multiple values
dotenvx set API_KEY=secret DEBUG=true PORT=3000

# Set with encryption
dotenvx set API_SECRET=supersecret --encrypt

# Force overwrite
dotenvx set KEY=value --force
```

### Managing Encryption
```bash
# Generate keypair
dotenvx keypair --save

# Encrypt all values
dotenvx encrypt

# Encrypt specific keys
dotenvx encrypt --keys API_KEY DATABASE_PASSWORD

# Decrypt to console
dotenvx decrypt

# Decrypt to file
dotenvx decrypt --output .env.decrypted
```

### Utility Commands
```bash
# List variables (masks sensitive values)
dotenvx list
dotenvx ls        # alias

# Show all values
dotenvx list --values

# Output as JSON
dotenvx list --json

# Get specific value
dotenvx get DATABASE_URL

# Validate syntax
dotenvx validate

# Generate example file
dotenvx example

# Run command with env loaded
dotenvx run -- node app.js
dotenvx run -- dotnet run
```

## 📚 Advanced Features

### Multiple Environments

```csharp
var env = builder.Environment.EnvironmentName;

DotEnv.Config(new DotEnvOptions
{
    Path = new[]
    {
        ".env",                    // Shared
        $".env.{env}",             // Environment-specific
        ".env.local",              // Local overrides
        $".env.{env}.local"        // Local environment overrides
    },
    Overload = true
});
```

### Variable Expansion

```env
BASE_URL=https://api.example.com
API_V1=${BASE_URL}/v1
USER_ENDPOINT=${API_V1}/users
FULL_URL=${USER_ENDPOINT}/profile
```

### Dependency Injection

```csharp
public class WeatherService
{
    private readonly IDotEnvService _dotEnv;
    
    public WeatherService(IDotEnvService dotEnv)
    {
        _dotEnv = dotEnv;
    }
    
    public async Task<Weather> GetWeatherAsync()
    {
        var apiKey = _dotEnv.Get("WEATHER_API_KEY");
        var apiUrl = _dotEnv.Get("WEATHER_API_URL");
        
        // Use values...
    }
}
```

### Configuration Provider

```csharp
var configuration = new ConfigurationBuilder()
    .AddDotEnvX(options =>
    {
        options.Path = new[] { ".env", ".env.production" };
        options.Overload = true;
    })
    .Build();

// Bind to strongly-typed options
services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
```

## 🏗️ Production Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .

# Don't include sensitive files
RUN rm -f .env.keys .env.local

# Use environment variable for production key
ENV DOTENV_KEY=$DOTENV_KEY

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### CI/CD (GitHub Actions)

```yaml
name: Deploy

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Setup environment
      env:
        DOTENV_PRIVATE_KEY: ${{ secrets.DOTENV_PRIVATE_KEY }}
      run: |
        echo "DOTENV_PRIVATE_KEY=$DOTENV_PRIVATE_KEY" > .env.keys
        dotnet tool install --global Noundry.DotEnvX.Tool
        dotenvx decrypt --output .env
    
    - name: Build
      run: dotnet build --configuration Release
    
    - name: Test
      run: dotnet test
    
    - name: Deploy
      run: dotnet publish
```

## 🔒 Security Best Practices

1. **Never commit secrets**
   ```gitignore
   .env
   .env.local
   .env.keys
   .env.*.local
   *.env.keys
   ```

2. **Use encryption for sensitive values**
   ```bash
   dotenvx set API_KEY=secret --encrypt
   ```

3. **Separate keys from values**
   - `.env` → Can be committed (with encrypted values)
   - `.env.keys` → Never commit (contains private keys)

4. **Use environment-specific files**
   - Development: `.env.development`
   - Production: Vault files or environment variables

## 📊 API Reference

### DotEnv.Config(options)

Load environment files.

| Option | Type | Description |
|--------|------|-------------|
| `Path` | `string[]` | Files to load |
| `Overload` | `bool` | Override existing vars |
| `Strict` | `bool` | Throw on missing files |
| `Ignore` | `string[]` | Error codes to ignore |
| `EnvKeysFile` | `string` | Path to keys file |
| `Convention` | `string` | Use convention (nextjs, etc) |

### DotEnv.Parse(content, options)

Parse .env content.

### DotEnv.Set(key, value, options)

Set environment variable.

### DotEnv.Get(key, options)

Get environment variable.

### DotEnv.GenerateKeypair()

Generate encryption keypair.

### DotEnv.Encrypt(value, publicKey)

Encrypt a value.

### DotEnv.Decrypt(encryptedValue, privateKey)

Decrypt a value.

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific tests
dotnet test --filter "FullyQualifiedName~Encryption"
```

## 📦 Package Structure

```
Noundry.DotEnvX/
├── src/
│   ├── DotEnvX.Core/          # Core library with DI extensions  
│   ├── DotEnvX.CLI/           # CLI application
│   └── DotEnvX.Tool/          # Global CLI tool
├── tests/
│   └── DotEnvX.Tests/         # Unit tests
├── samples/
│   └── DotEnvX.Samples/       # Comprehensive sample application
└── docs/                      # Documentation
```

**NuGet Packages:**
- **`Noundry.DotEnvX`** - Core library with DI extensions (consolidated)
- **`Noundry.DotEnvX.Tool`** - Global CLI tool

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📈 Roadmap

- [ ] Vault file support (.env.vault)
- [ ] Cloud provider integrations (Azure Key Vault, AWS Secrets Manager)
- [ ] GUI tool for managing .env files
- [ ] VSCode extension
- [ ] Additional encryption algorithms
- [ ] Performance optimizations

## 📄 License

This project is licensed under the BSD 3-Clause License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Original [dotenvx](https://github.com/dotenvx/dotenvx) by [@motdotla](https://github.com/motdotla)
- [BouncyCastle](https://www.bouncycastle.org/) for cryptography
- [Spectre.Console](https://spectreconsole.net/) for beautiful CLI output
- [System.CommandLine](https://github.com/dotnet/command-line-api) for command parsing
- The .NET community

## 📊 Status

| Component | Status | Tests | Coverage |
|-----------|--------|-------|----------|
| Core | ✅ Stable | 11/22 | 50% |
| Encryption | ✅ Stable | 11/11 | 100% |
| DI Extensions | ✅ Stable | N/A | N/A |
| CLI Tool | ✅ Stable | Manual | N/A |
| Samples | ✅ Complete | Manual | N/A |

## 🔗 Links

- [NuGet Package](https://www.nuget.org/packages/Noundry.DotEnvX/)
- [CLI Tool Package](https://www.nuget.org/packages/Noundry.DotEnvX.Tool/)
- [Documentation](https://github.com/plsft/DotEnvX/wiki)
- [Original dotenvx](https://github.com/dotenvx/dotenvx)
- [Report Issues](https://github.com/plsft/DotEnvX/issues)
- [Discussions](https://github.com/plsft/DotEnvX/discussions)

## 🆚 Noundry.DotEnvX vs dotnet user-secrets

### Why Choose Noundry.DotEnvX?

While `dotnet user-secrets` is great for basic development scenarios, Noundry.DotEnvX provides a comprehensive solution for both development and production environments.

### Feature Comparison

| Feature | Noundry.DotEnvX | dotnet user-secrets |
|---------|---------|---------------------|
| **Development secrets** | ✅ Excellent | ✅ Excellent |
| **Production support** | ✅ Full support | ❌ Dev only |
| **Encryption** | ✅ ECIES encryption | ❌ Plain text |
| **Source control** | ✅ Safe (encrypted) | ❌ Cannot commit |
| **CI/CD integration** | ✅ Excellent | ❌ Not suitable |
| **Multi-language support** | ✅ Universal .env | ❌ .NET only |
| **Docker/containers** | ✅ Native support | ❌ Not suitable |
| **Variable expansion** | ✅ `${VAR}` syntax | ❌ Not supported |
| **Multiple environments** | ✅ Built-in layering | ⚠️ Limited |
| **CLI tools** | ✅ Comprehensive | ⚠️ Basic |
| **Team collaboration** | ✅ Via encryption | ⚠️ Manual sharing |
| **File format** | ✅ Industry standard | ⚠️ JSON only |
| **VS integration** | ⚠️ Via extension | ✅ Built-in |

### Key Advantages of Noundry.DotEnvX

1. **Production-Ready Encryption**: ECIES encryption allows safe storage of encrypted secrets in source control, with separate key management
2. **Universal Format**: .env files work across all platforms and languages, perfect for polyglot teams
3. **Advanced Features**: Variable expansion, multiple file support, and environment-specific configurations
4. **DevOps Friendly**: Designed for modern CI/CD pipelines and container deployments
5. **Team Collaboration**: Encrypted secrets can be shared via Git with secure key distribution

### When to Use Each

**Use Noundry.DotEnvX when you need:**
- Production-grade secret management
- Encrypted secrets in source control
- Multi-environment deployments
- Cross-platform compatibility
- Docker/Kubernetes deployments
- Team collaboration on secrets

**Use dotnet user-secrets when:**
- Working on simple .NET-only projects
- Only need local development secrets
- Prefer built-in Visual Studio integration
- Don't need production deployment

### Migration from user-secrets

```csharp
// Before (user-secrets)
builder.Configuration.AddUserSecrets<Program>();

// After (Noundry.DotEnvX) 
builder.Configuration.AddDotEnvX(options =>
{
    options.Path = new[] { ".env", ".env.local" };
    options.Overload = true;
});
```

---

<div align="center">
Made with ❤️ for the .NET community
<br>
Star ⭐ this repo if you find it useful!
</div>