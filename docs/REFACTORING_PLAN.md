# Moton.CoAP Refactoring Plan

## Status: Tasks 2-5 COMPLETED ✅

**Completed on:** January 2, 2026

### Summary of Changes

| Task | Status | Description |
|------|--------|-------------|
| Task 1: SDK Update | ⏸️ Postponed | Will be done later |
| Task 2: CBOR Support | ✅ Complete | Added `ApplicationCbor = 60` and other formats |
| Task 3: Block Transfer | ✅ Complete | Implemented `CoapClientBlockTransferSender` for Block1 |
| Task 4: Rename to Moton.CoAP | ✅ Complete | All namespaces, projects, and files renamed |
| Task 5: Tests | ✅ Complete | 63 tests passing |

---

## Overview

This document outlines a comprehensive plan to refactor the forked CoAPnet project into **Moton.CoAP** - a modern, platform-independent CoAP library tailored for the ServoTuner application.

---

## Goals

| # | Goal | Priority |
|---|------|----------|
| 1 | Update to latest .NET SDK (net8.0/net9.0) and ensure platform independence | High |
| 2 | Add CBOR content format (60) support | High |
| 3 | Ensure block transfer support is complete and tested | High |
| 4 | Rename all projects, namespaces, and assemblies to `Moton.CoAP` | High |
| 5 | Ensure tests are runnable and comprehensive | High |

---

## Current Project Structure

```
Moton.CoAP/
├── Source/
│   ├── CoAPnet/                          # Core library
│   │   ├── Client/                       # High-level client
│   │   ├── Exceptions/                   # Custom exceptions
│   │   ├── Internal/                     # Internal utilities
│   │   ├── Logging/                      # Logging infrastructure
│   │   ├── LowLevelClient/               # Low-level UDP client
│   │   ├── MessageDispatcher/            # Message routing
│   │   ├── Protocol/                     # CoAP protocol
│   │   │   ├── BlockTransfer/            # RFC 7959 block transfer
│   │   │   ├── Encoding/                 # Message encoding/decoding
│   │   │   ├── Observe/                  # RFC 7641 observe
│   │   │   └── Options/                  # CoAP options
│   │   ├── Transport/                    # Transport layer abstraction
│   │   ├── CoapFactory.cs                # Factory for creating clients
│   │   └── CoAPnet.csproj                # Project file
│   │
│   ├── CoAPnet.Extensions.DTLS/          # DTLS extension
│   ├── CoAPnet.Tests/                    # Unit tests
│   ├── CoAP.TestClient/                  # Interactive test client
│   └── CoAPnet.sln                       # Solution file
│
├── Samples/
│   └── CoAPnet.Samples.csproj            # Sample application
│
└── Build/                                # Build scripts
```

---

## Phase 1: SDK and Target Framework Update

### 1.1 Target Framework Changes

**Current Target Frameworks:**
```xml
<!-- CoAPnet.csproj -->
<TargetFrameworks>net452;net5.0;net6.0;netstandard1.3;netstandard2.0</TargetFrameworks>

<!-- CoAPnet.Extensions.DTLS.csproj -->
<TargetFrameworks>net452;netstandard2.0;net5.0;net6.0</TargetFrameworks>

<!-- Tests/TestClient/Samples -->
<TargetFramework>net6.0</TargetFramework>
```

**New Target Frameworks:**
```xml
<!-- Core Library (Moton.CoAP.csproj) -->
<TargetFrameworks>net8.0;net9.0;netstandard2.0</TargetFrameworks>

<!-- Extensions (Moton.CoAP.Extensions.DTLS.csproj) -->
<TargetFrameworks>net8.0;net9.0;netstandard2.0</TargetFrameworks>

<!-- Tests/TestClient/Samples -->
<TargetFramework>net8.0</TargetFramework>
```

**Rationale:**
- `net8.0` - LTS version (supported until November 2026)
- `net9.0` - Latest STS version for new features
- `netstandard2.0` - Broad compatibility for legacy .NET Framework 4.6.1+ and Mono
- Remove `net452`, `net5.0`, `net6.0`, `netstandard1.3` (EOL or superseded)

### 1.2 LangVersion Update

**Current:**
```xml
<LangVersion>7.3</LangVersion>
```

**New:**
```xml
<LangVersion>latest</LangVersion>
```

### 1.3 SDK-Related Updates

| Item | Current | New |
|------|---------|-----|
| Nullable reference types | Not enabled | `<Nullable>enable</Nullable>` |
| Implicit usings | Not enabled | `<ImplicitUsings>enable</ImplicitUsings>` |
| Analyzers | Enabled | Keep enabled |
| SourceLink | GitHub | Keep as-is (update version) |

### 1.4 Package Reference Updates

| Package | Current Version | New Version | Notes |
|---------|-----------------|-------------|-------|
| Microsoft.SourceLink.GitHub | 1.1.1 | 8.0.0 | SourceLink for GitHub |
| Microsoft.NET.Test.Sdk | 17.0.0 | 17.11.1 | Test SDK |
| MSTest.TestAdapter | 2.2.8 | 3.6.3 | MSTest adapter |
| MSTest.TestFramework | 2.2.8 | 3.6.3 | MSTest framework |
| coverlet.collector | 3.1.0 | 6.0.2 | Code coverage |

### 1.5 Files to Modify

- [ ] `Source/CoAPnet/CoAPnet.csproj` → `Source/Moton.CoAP/Moton.CoAP.csproj`
- [ ] `Source/CoAPnet.Extensions.DTLS/CoAPnet.Extensions.DTLS.csproj` → `Source/Moton.CoAP.Extensions.DTLS/Moton.CoAP.Extensions.DTLS.csproj`
- [ ] `Source/CoAPnet.Tests/CoAPnet.Tests.csproj` → `Source/Moton.CoAP.Tests/Moton.CoAP.Tests.csproj`
- [ ] `Source/CoAP.TestClient/CoAP.TestClient.csproj` → `Source/Moton.CoAP.TestClient/Moton.CoAP.TestClient.csproj`
- [ ] `Samples/CoAPnet.Samples.csproj` → `Samples/Moton.CoAP.Samples.csproj`
- [ ] `Source/CoAPnet.sln` → `Source/Moton.CoAP.sln`

---

## Phase 2: CBOR Content Format Support

### 2.1 Current Content Format Enum

**File:** `Source/CoAPnet/Protocol/Options/CoapMessageContentFormat.cs`

```csharp
public enum CoapMessageContentFormat
{
    TextPlain = 0,
    ApplicationLinkFormat = 40,
    ApplicationXml = 41,
    ApplicationOctetStream = 42,
    ApplicationExi = 47,
    ApplicationJson = 50
}
```

### 2.2 Updated Content Format Enum

```csharp
public enum CoapMessageContentFormat
{
    /// <summary>
    /// text/plain; charset=utf-8 (RFC 7252)
    /// </summary>
    TextPlain = 0,

    /// <summary>
    /// application/link-format (RFC 6690)
    /// </summary>
    ApplicationLinkFormat = 40,

    /// <summary>
    /// application/xml (RFC 3023)
    /// </summary>
    ApplicationXml = 41,

    /// <summary>
    /// application/octet-stream (RFC 2045)
    /// </summary>
    ApplicationOctetStream = 42,

    /// <summary>
    /// application/exi (Efficient XML Interchange)
    /// </summary>
    ApplicationExi = 47,

    /// <summary>
    /// application/json (RFC 7159)
    /// </summary>
    ApplicationJson = 50,

    /// <summary>
    /// application/cbor (RFC 8949, IANA CoAP Content-Format 60)
    /// </summary>
    ApplicationCbor = 60,

    /// <summary>
    /// application/senml+json (RFC 8428)
    /// </summary>
    ApplicationSenmlJson = 110,

    /// <summary>
    /// application/sensml+json (RFC 8428)
    /// </summary>
    ApplicationSensmlJson = 111,

    /// <summary>
    /// application/senml+cbor (RFC 8428)
    /// </summary>
    ApplicationSenmlCbor = 112,

    /// <summary>
    /// application/sensml+cbor (RFC 8428)
    /// </summary>
    ApplicationSensmlCbor = 113,

    /// <summary>
    /// application/senml-exi (RFC 8428)
    /// </summary>
    ApplicationSenmlExi = 114,

    /// <summary>
    /// application/sensml-exi (RFC 8428)
    /// </summary>
    ApplicationSensmlExi = 115,

    /// <summary>
    /// application/coap-group+json (RFC 7390)
    /// </summary>
    ApplicationCoapGroupJson = 256,

    /// <summary>
    /// application/cose; cose-type="cose-encrypt0" (RFC 8152)
    /// </summary>
    ApplicationCoseEncrypt0 = 16,

    /// <summary>
    /// application/cose; cose-type="cose-mac0" (RFC 8152)
    /// </summary>
    ApplicationCoseMac0 = 17,

    /// <summary>
    /// application/cose; cose-type="cose-sign1" (RFC 8152)
    /// </summary>
    ApplicationCoseSign1 = 18
}
```

### 2.3 CBOR Helper Utilities (Optional)

Consider adding a helper class for CBOR content handling:

```csharp
namespace Moton.CoAP.Protocol.Options
{
    public static class CoapContentFormatHelper
    {
        /// <summary>
        /// Checks if the content format is CBOR-based.
        /// </summary>
        public static bool IsCborFormat(CoapMessageContentFormat format)
        {
            return format == CoapMessageContentFormat.ApplicationCbor ||
                   format == CoapMessageContentFormat.ApplicationSenmlCbor ||
                   format == CoapMessageContentFormat.ApplicationSensmlCbor;
        }

        /// <summary>
        /// Gets the IANA registered name for the content format.
        /// </summary>
        public static string GetMediaType(CoapMessageContentFormat format)
        {
            return format switch
            {
                CoapMessageContentFormat.TextPlain => "text/plain",
                CoapMessageContentFormat.ApplicationLinkFormat => "application/link-format",
                CoapMessageContentFormat.ApplicationXml => "application/xml",
                CoapMessageContentFormat.ApplicationOctetStream => "application/octet-stream",
                CoapMessageContentFormat.ApplicationExi => "application/exi",
                CoapMessageContentFormat.ApplicationJson => "application/json",
                CoapMessageContentFormat.ApplicationCbor => "application/cbor",
                CoapMessageContentFormat.ApplicationSenmlJson => "application/senml+json",
                CoapMessageContentFormat.ApplicationSensmlJson => "application/sensml+json",
                CoapMessageContentFormat.ApplicationSenmlCbor => "application/senml+cbor",
                CoapMessageContentFormat.ApplicationSensmlCbor => "application/sensml+cbor",
                _ => "application/octet-stream"
            };
        }
    }
}
```

### 2.4 Files to Modify

- [ ] `Source/Moton.CoAP/Protocol/Options/CoapMessageContentFormat.cs` - Add CBOR and other formats
- [ ] `Source/Moton.CoAP/Protocol/Options/CoapContentFormatHelper.cs` - New helper class (optional)
- [ ] `Source/Moton.CoAP.Tests/CoapMessageContentFormat_Tests.cs` - Add tests for new formats

---

## Phase 3: Block Transfer Verification

### 3.1 Current Block Transfer Implementation

The project already has block transfer support in:

**Files:**
- `Protocol/BlockTransfer/CoapBlockTransferOptionValue.cs` - Block option value structure
- `Protocol/BlockTransfer/CoapBlockTransferOptionValueEncoder.cs` - Encode block options
- `Protocol/BlockTransfer/CoapBlockTransferOptionValueDecoder.cs` - Decode block options
- `Client/CoapClientBlockTransferReceiver.cs` - Client-side block receive handling

### 3.2 Block Transfer Gaps

| Feature | Status | Notes |
|---------|--------|-------|
| Block2 (response segmentation) | ✅ Implemented | Client can receive segmented responses |
| Block1 (request segmentation) | ❌ Missing | Client cannot send large payloads |
| Configurable block size | ⚠️ Partial | Hardcoded in some places |
| Block-wise observe | ⚠️ Partial | Basic support exists |

### 3.3 New Block1 Sender Implementation

Create `CoapClientBlockTransferSender.cs`:

```csharp
namespace Moton.CoAP.Client
{
    /// <summary>
    /// Handles sending large payloads using Block1 option (RFC 7959).
    /// </summary>
    public sealed class CoapClientBlockTransferSender
    {
        public const int DefaultBlockSize = 1024; // 1KB blocks (szx=6)
        public const int MinBlockSize = 16;       // 16 bytes (szx=0)
        public const int MaxBlockSize = 1024;     // 1KB (szx=6)

        private readonly CoapClient _client;
        private readonly CoapNetLogger _logger;
        private readonly int _blockSize;

        public CoapClientBlockTransferSender(
            CoapClient client, 
            CoapNetLogger logger, 
            int blockSize = DefaultBlockSize)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blockSize = Math.Clamp(blockSize, MinBlockSize, MaxBlockSize);
        }

        /// <summary>
        /// Determines if the payload requires block-wise transfer.
        /// </summary>
        public bool RequiresBlockTransfer(ArraySegment<byte> payload)
        {
            return payload.Count > _blockSize;
        }

        /// <summary>
        /// Sends a large payload using Block1 option.
        /// </summary>
        public async Task<CoapResponse> SendAsync(
            CoapMessage requestTemplate, 
            ArraySegment<byte> payload,
            CancellationToken cancellationToken)
        {
            // Implementation: Split payload into blocks and send sequentially
            // Return final response after all blocks acknowledged
        }
    }
}
```

### 3.4 Block Transfer Configuration

Add block transfer options to client connect options:

```csharp
public class CoapClientConnectOptions
{
    // ... existing properties ...

    /// <summary>
    /// Maximum block size for Block1/Block2 transfers (16, 32, 64, 128, 256, 512, 1024).
    /// </summary>
    public int PreferredBlockSize { get; set; } = 1024;

    /// <summary>
    /// Enable automatic block-wise transfer for large payloads.
    /// </summary>
    public bool EnableBlockTransfer { get; set; } = true;
}
```

### 3.5 Files to Create/Modify

- [ ] `Source/Moton.CoAP/Client/CoapClientBlockTransferSender.cs` - New Block1 sender
- [ ] `Source/Moton.CoAP/Client/CoapClientConnectOptions.cs` - Add block transfer config
- [ ] `Source/Moton.CoAP/Client/CoapClientConnectOptionsBuilder.cs` - Add builder methods
- [ ] `Source/Moton.CoAP/Client/CoapClient.cs` - Integrate Block1 sending
- [ ] `Source/Moton.CoAP.Tests/CoapBlockTransfer_Tests.cs` - Comprehensive tests

---

## Phase 4: Namespace and Project Renaming

### 4.1 Namespace Mapping

| Old Namespace | New Namespace |
|---------------|---------------|
| `CoAPnet` | `Moton.CoAP` |
| `CoAPnet.Client` | `Moton.CoAP.Client` |
| `CoAPnet.Exceptions` | `Moton.CoAP.Exceptions` |
| `CoAPnet.Internal` | `Moton.CoAP.Internal` |
| `CoAPnet.Logging` | `Moton.CoAP.Logging` |
| `CoAPnet.LowLevelClient` | `Moton.CoAP.LowLevelClient` |
| `CoAPnet.MessageDispatcher` | `Moton.CoAP.MessageDispatcher` |
| `CoAPnet.Protocol` | `Moton.CoAP.Protocol` |
| `CoAPnet.Protocol.BlockTransfer` | `Moton.CoAP.Protocol.BlockTransfer` |
| `CoAPnet.Protocol.Encoding` | `Moton.CoAP.Protocol.Encoding` |
| `CoAPnet.Protocol.Observe` | `Moton.CoAP.Protocol.Observe` |
| `CoAPnet.Protocol.Options` | `Moton.CoAP.Protocol.Options` |
| `CoAPnet.Transport` | `Moton.CoAP.Transport` |
| `CoAPnet.Extensions.DTLS` | `Moton.CoAP.Extensions.DTLS` |

### 4.2 Project File Renaming

| Old Path | New Path |
|----------|----------|
| `Source/CoAPnet/CoAPnet.csproj` | `Source/Moton.CoAP/Moton.CoAP.csproj` |
| `Source/CoAPnet.Extensions.DTLS/CoAPnet.Extensions.DTLS.csproj` | `Source/Moton.CoAP.Extensions.DTLS/Moton.CoAP.Extensions.DTLS.csproj` |
| `Source/CoAPnet.Tests/CoAPnet.Tests.csproj` | `Source/Moton.CoAP.Tests/Moton.CoAP.Tests.csproj` |
| `Source/CoAP.TestClient/CoAP.TestClient.csproj` | `Source/Moton.CoAP.TestClient/Moton.CoAP.TestClient.csproj` |
| `Samples/CoAPnet.Samples.csproj` | `Samples/Moton.CoAP.Samples.csproj` |
| `Source/CoAPnet.sln` | `Source/Moton.CoAP.sln` |

### 4.3 Folder Renaming

```bash
# Core library
mv Source/CoAPnet Source/Moton.CoAP

# Extensions
mv Source/CoAPnet.Extensions.DTLS Source/Moton.CoAP.Extensions.DTLS

# Tests
mv Source/CoAPnet.Tests Source/Moton.CoAP.Tests

# Test client
mv Source/CoAP.TestClient Source/Moton.CoAP.TestClient
```

### 4.4 Project File Content Updates

**Moton.CoAP.csproj:**
```xml
<PropertyGroup>
    <AssemblyName>Moton.CoAP</AssemblyName>
    <RootNamespace>Moton.CoAP</RootNamespace>
    <PackageId>Moton.CoAP</PackageId>
    <Product>Moton.CoAP</Product>
    <Description>Moton CoAP library - A high performance .NET library for CoAP based communication.</Description>
    <Authors>Moton Contributors</Authors>
    <Copyright>Moton 2024-2026</Copyright>
    <PackageProjectUrl>https://github.com/panxxhub/zephyr-servo</PackageProjectUrl>
    <RepositoryUrl>https://github.com/panxxhub/zephyr-servo.git</RepositoryUrl>
</PropertyGroup>
```

### 4.5 Files to Modify (Namespace Changes)

All `.cs` files in the following directories need namespace updates:

**Core Library (Moton.CoAP):**
- [ ] `Client/*.cs` - ~25 files
- [ ] `Exceptions/*.cs` - ~1 file
- [ ] `Internal/*.cs` - ~3 files
- [ ] `Logging/*.cs` - ~2 files
- [ ] `LowLevelClient/*.cs` - ~5 files
- [ ] `MessageDispatcher/*.cs` - ~3 files
- [ ] `Protocol/*.cs` - ~5 files
- [ ] `Protocol/BlockTransfer/*.cs` - ~3 files
- [ ] `Protocol/Encoding/*.cs` - ~4 files
- [ ] `Protocol/Observe/*.cs` - ~1 file
- [ ] `Protocol/Options/*.cs` - ~9 files
- [ ] `Transport/*.cs` - ~2 files
- [ ] `CoapFactory.cs` - 1 file

**Extensions (Moton.CoAP.Extensions.DTLS):**
- [ ] All `.cs` files - ~12 files

**Tests (Moton.CoAP.Tests):**
- [ ] All test files - ~5 files

**Total: ~80 files**

### 4.6 Rename Script

```bash
#!/bin/bash
# Run from Moton.CoAP directory

# 1. Rename folders
mv Source/CoAPnet Source/Moton.CoAP
mv Source/CoAPnet.Extensions.DTLS Source/Moton.CoAP.Extensions.DTLS
mv Source/CoAPnet.Tests Source/Moton.CoAP.Tests
mv Source/CoAP.TestClient Source/Moton.CoAP.TestClient

# 2. Rename project files
mv Source/Moton.CoAP/CoAPnet.csproj Source/Moton.CoAP/Moton.CoAP.csproj
mv Source/Moton.CoAP.Extensions.DTLS/CoAPnet.Extensions.DTLS.csproj Source/Moton.CoAP.Extensions.DTLS/Moton.CoAP.Extensions.DTLS.csproj
mv Source/Moton.CoAP.Tests/CoAPnet.Tests.csproj Source/Moton.CoAP.Tests/Moton.CoAP.Tests.csproj
mv Source/Moton.CoAP.TestClient/CoAP.TestClient.csproj Source/Moton.CoAP.TestClient/Moton.CoAP.TestClient.csproj
mv Samples/CoAPnet.Samples.csproj Samples/Moton.CoAP.Samples.csproj

# 3. Rename solution
mv Source/CoAPnet.sln Source/Moton.CoAP.sln

# 4. Update namespaces in all .cs files
find Source Samples -name "*.cs" -exec sed -i 's/namespace CoAPnet/namespace Moton.CoAP/g' {} \;
find Source Samples -name "*.cs" -exec sed -i 's/using CoAPnet/using Moton.CoAP/g' {} \;

# 5. Update solution file
sed -i 's/CoAPnet/Moton.CoAP/g' Source/Moton.CoAP.sln
sed -i 's/CoAP\.TestClient/Moton.CoAP.TestClient/g' Source/Moton.CoAP.sln

# 6. Update project references in csproj files
find Source Samples -name "*.csproj" -exec sed -i 's/CoAPnet/Moton.CoAP/g' {} \;
find Source Samples -name "*.csproj" -exec sed -i 's/CoAP\.TestClient/Moton.CoAP.TestClient/g' {} \;
```

---

## Phase 5: Test Infrastructure

### 5.1 Current Test Status

**Test Project:** `Source/CoAPnet.Tests/CoAPnet.Tests.csproj`

**Existing Tests:**
- `CoapClient_Tests.cs`
- `CoapMessageCode_Tests.cs`
- `CoapMessageDecoder_Tests.cs`
- `CoapMessageEncoder_Tests.cs`
- `CoapMessageReader_Tests.cs`

### 5.2 Test Framework Update

```xml
<PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>

<ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="MSTest.TestAdapter" Version="3.6.3" />
    <PackageReference Include="MSTest.TestFramework" Version="3.6.3" />
    <PackageReference Include="coverlet.collector" Version="6.0.2">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
</ItemGroup>
```

### 5.3 New Test Files

| Test File | Coverage |
|-----------|----------|
| `CoapMessageContentFormat_Tests.cs` | CBOR and new content formats |
| `CoapBlockTransfer_Tests.cs` | Block1 and Block2 transfer |
| `CoapBlockTransferSender_Tests.cs` | Block1 sender unit tests |
| `CoapClientIntegration_Tests.cs` | End-to-end client tests |
| `CoapObserve_Tests.cs` | Observe pattern tests |

### 5.4 Sample Test Structure

```csharp
namespace Moton.CoAP.Tests
{
    [TestClass]
    public class CoapMessageContentFormat_Tests
    {
        [TestMethod]
        public void ApplicationCbor_Should_HaveCorrectValue()
        {
            Assert.AreEqual(60, (int)CoapMessageContentFormat.ApplicationCbor);
        }

        [TestMethod]
        public void IsCborFormat_Should_ReturnTrue_ForCborFormats()
        {
            Assert.IsTrue(CoapContentFormatHelper.IsCborFormat(CoapMessageContentFormat.ApplicationCbor));
            Assert.IsTrue(CoapContentFormatHelper.IsCborFormat(CoapMessageContentFormat.ApplicationSenmlCbor));
        }

        [TestMethod]
        public void IsCborFormat_Should_ReturnFalse_ForNonCborFormats()
        {
            Assert.IsFalse(CoapContentFormatHelper.IsCborFormat(CoapMessageContentFormat.ApplicationJson));
            Assert.IsFalse(CoapContentFormatHelper.IsCborFormat(CoapMessageContentFormat.TextPlain));
        }
    }

    [TestClass]
    public class CoapBlockTransfer_Tests
    {
        [TestMethod]
        public void BlockSize_Encoding_Should_BeCorrect()
        {
            // Test SZX encoding: szx=0 -> 16 bytes, szx=6 -> 1024 bytes
            var value = new CoapBlockTransferOptionValue
            {
                Number = 0,
                Size = 1024,
                HasFollowingBlocks = true
            };
            
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            var decoded = CoapBlockTransferOptionValueDecoder.Decode(encoded);

            Assert.AreEqual(value.Size, decoded.Size);
            Assert.AreEqual(value.Number, decoded.Number);
            Assert.AreEqual(value.HasFollowingBlocks, decoded.HasFollowingBlocks);
        }
    }
}
```

### 5.5 Running Tests

```bash
# Run all tests
dotnet test Source/Moton.CoAP.Tests/Moton.CoAP.Tests.csproj

# Run with coverage
dotnet test Source/Moton.CoAP.Tests/Moton.CoAP.Tests.csproj --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~CoapBlockTransfer"
```

---

## Implementation Order

### Step 1: Rename and Restructure (1-2 days)
1. Run rename script
2. Fix any path/reference issues
3. Verify build succeeds

### Step 2: SDK Update (0.5 days)
1. Update target frameworks
2. Update package references
3. Update LangVersion
4. Enable nullable and implicit usings
5. Verify build succeeds

### Step 3: CBOR Support (0.5 days)
1. Update `CoapMessageContentFormat` enum
2. Add helper class (optional)
3. Add tests

### Step 4: Block Transfer Completion (1-2 days)
1. Implement `CoapClientBlockTransferSender`
2. Add block transfer configuration
3. Integrate into `CoapClient`
4. Add comprehensive tests

### Step 5: Test Verification (0.5 days)
1. Update test project
2. Run all tests
3. Fix any failures
4. Verify coverage

---

## Verification Checklist

### Build Verification
- [ ] `dotnet build Source/Moton.CoAP.sln` succeeds
- [ ] All target frameworks build successfully
- [ ] No compiler warnings

### Test Verification
- [ ] `dotnet test Source/Moton.CoAP.Tests/Moton.CoAP.Tests.csproj` succeeds
- [ ] All existing tests pass
- [ ] New tests pass

### Feature Verification
- [ ] CBOR content format (60) is available
- [ ] Block1 sending works for large payloads
- [ ] Block2 receiving works for large responses
- [ ] Observe pattern works

### Integration Verification
- [ ] ServoTuner can reference `Moton.CoAP`
- [ ] Existing `CoapClientManager` works with new library
- [ ] No breaking changes to public API

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Breaking changes in API | Keep public interfaces stable, only internal refactoring |
| Test failures after rename | Run tests incrementally after each change |
| Missing dependencies | Verify all package references after rename |
| DTLS breaking | Keep DTLS extension as optional, test separately |

---

## Post-Refactoring Tasks

1. Update `servo-tuner` to reference `Moton.CoAP` instead of `CoAP` NuGet package
2. Remove old `CoAP` package reference from `ServoTuner.Core.csproj`
3. Update `CoapClientManager` to use new namespace
4. Document migration in project README
5. Consider publishing `Moton.CoAP` as internal NuGet package

---

## Appendix A: Complete File List

### Core Library Files (to be renamed/modified)

```
Source/Moton.CoAP/
├── Moton.CoAP.csproj
├── CoapFactory.cs
├── Client/
│   ├── CoapClient.cs
│   ├── CoapClientBlockTransferReceiver.cs
│   ├── CoapClientBlockTransferSender.cs      # NEW
│   ├── CoapClientConfigurationInvalidException.cs
│   ├── CoapClientConnectOptions.cs
│   ├── CoapClientConnectOptionsBuilder.cs
│   ├── CoapClientObservationManager.cs
│   ├── CoapMessageIdProvider.cs
│   ├── CoapMessageToResponseConverter.cs
│   ├── CoapMessageToken.cs
│   ├── CoapMessageTokenProvider.cs
│   ├── CoapObserveOptions.cs
│   ├── CoapObserveRequest.cs
│   ├── CoapObserveRequestBuilder.cs
│   ├── CoapObserveResponse.cs
│   ├── CoapRequest.cs
│   ├── CoapRequestBuilder.cs
│   ├── CoapRequestMethod.cs
│   ├── CoapRequestOptions.cs
│   ├── CoapRequestToMessageConverter.cs
│   ├── CoapResponse.cs
│   ├── CoapResponseOptions.cs
│   ├── CoapResponseStatusCode.cs
│   ├── HandleResponseContext.cs
│   ├── ICoapClient.cs
│   └── ICoapResponseHandler.cs
├── Protocol/
│   ├── CoapDefaultPort.cs
│   ├── CoapMessage.cs
│   ├── CoapMessageCode.cs
│   ├── CoapMessageCodes.cs
│   ├── CoapMessageType.cs
│   ├── BlockTransfer/
│   │   ├── CoapBlockTransferOptionValue.cs
│   │   ├── CoapBlockTransferOptionValueDecoder.cs
│   │   └── CoapBlockTransferOptionValueEncoder.cs
│   ├── Encoding/
│   │   ├── CoapMessageDecoder.cs
│   │   ├── CoapMessageEncoder.cs
│   │   ├── CoapMessageReader.cs
│   │   └── CoapMessageWriter.cs
│   ├── Observe/
│   │   └── CoapObserveOptionValue.cs
│   └── Options/
│       ├── CoapContentFormatHelper.cs        # NEW
│       ├── CoapMessageContentFormat.cs
│       ├── CoapMessageOption.cs
│       ├── CoapMessageOptionEmptyValue.cs
│       ├── CoapMessageOptionFactory.cs
│       ├── CoapMessageOptionNumber.cs
│       ├── CoapMessageOptionOpaqueValue.cs
│       ├── CoapMessageOptionStringValue.cs
│       ├── CoapMessageOptionUintValue.cs
│       └── CoapMessageOptionValue.cs
└── ...
```

---

## Appendix B: Package Reference Summary

### Moton.CoAP.csproj
```xml
<ItemGroup>
    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All" />
</ItemGroup>
```

### Moton.CoAP.Extensions.DTLS.csproj
```xml
<ItemGroup>
    <PackageReference Include="Portable.BouncyCastle" Version="1.9.0" />
    <ProjectReference Include="..\Moton.CoAP\Moton.CoAP.csproj" />
</ItemGroup>
```

### Moton.CoAP.Tests.csproj
```xml
<ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="MSTest.TestAdapter" Version="3.6.3" />
    <PackageReference Include="MSTest.TestFramework" Version="3.6.3" />
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
    <ProjectReference Include="..\Moton.CoAP\Moton.CoAP.csproj" />
    <ProjectReference Include="..\Moton.CoAP.Extensions.DTLS\Moton.CoAP.Extensions.DTLS.csproj" />
</ItemGroup>
```
