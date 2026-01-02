# Moton.CoAP Feature Analysis for Zephyr-Servo

This document analyzes the CoAP features required by the zephyr-servo project and evaluates Moton.CoAP's support status.

## Executive Summary

| Feature Category | Required by Zephyr-Servo | Moton.CoAP Support |
|-----------------|-------------------------|-------------------|
| Basic CoAP Methods | GET, PUT, POST, DELETE | ✅ Supported |
| Content Format (CBOR) | application/cbor (60) | ✅ Supported |
| Block-wise Transfer (Block2) | RFC 7959 for large responses | ✅ Supported |
| Block-wise Transfer (Block1) | RFC 7959 for large requests | ✅ Supported |
| Observe (RFC 7641) | Server-push notifications | ✅ Supported |
| URI Query Parameters | `?index=0x1234&subindex=0x00` | ✅ Supported |
| DTLS Security | Pre-Shared Key (PSK) | ✅ Supported |
| Connection Timeout | Handshake/request timeout | ✅ Supported |

---

## 1. CoAP Methods

### Zephyr-Servo Requirements

The CoAP API documentation shows usage of all standard CoAP methods:

| Method | Use Cases |
|--------|-----------|
| **GET** | Retrieve status, read data, download LUTs |
| **PUT** | Update SDO values, enable/disable NLC |
| **POST** | Start measurements, trigger calibrations |
| **DELETE** | Cancel operations, reset to defaults |

### Moton.CoAP Support

**Status: ✅ Fully Supported**

```csharp
// Moton.CoAP supports all methods via CoapRequestMethod
public enum CoapRequestMethod
{
    Get,
    Post,
    Put,
    Delete
}
```

---

## 2. Content Format - CBOR

### Zephyr-Servo Requirements

The servo uses **CBOR (Concise Binary Object Representation)** extensively:

- Content-Format: `application/cbor` (60)
- All measurement data (Rs, Ls, NLC) uses CBOR encoding
- CDDL schemas define the data structures (`sysid.cddl`, `foc.cddl`)

Example from API:
```
GET /sysid/rs/data
Content-Format: application/cbor (60)
Response: CBOR-encoded RsDataFullResponse
```

### Moton.CoAP Support

**Status: ✅ Fully Supported**

```csharp
// CoapMessageContentFormat enum includes CBOR
public enum CoapMessageContentFormat
{
    ApplicationCbor = 60,        // RFC 8949
    ApplicationCwt = 61,         // RFC 8392
    ApplicationSenmlCbor = 112,  // RFC 8428
    ApplicationSensmlCbor = 113, // RFC 8428
    // ... more
}

// Helper methods for CBOR detection
public static class CoapContentFormatHelper
{
    public static bool IsCborFormat(CoapMessageContentFormat format)
    {
        return format == CoapMessageContentFormat.ApplicationCbor ||
               format == CoapMessageContentFormat.ApplicationSenmlCbor ||
               // ...
    }
}
```

**Note:** Moton.CoAP provides transport-level CBOR support (content format negotiation). Actual CBOR serialization/deserialization should be handled by a separate library like `Spotflow.Cbor` or `System.Formats.Cbor`.

---

## 3. Block-wise Transfer (RFC 7959)

### Zephyr-Servo Requirements

Large data transfers use RFC 7959 block-wise transfer:

| Endpoint | Direction | Data Size |
|----------|-----------|-----------|
| `/sysid/rs/data` | Block2 (GET) | ~100KB+ (measurement samples) |
| `/sysid/ls/data` | Block2 (GET) | ~100KB+ (measurement samples) |
| `/foc/nlc/lut` | Block2 (GET) | ~64KB (16384 × int32) |
| `/foc/nlc/lut` | Block1 (PUT) | ~64KB (LUT upload) |
| `/foc/nlc/generation/data` | Block2 (GET) | ~64KB+ (raw data + flags) |

Configuration:
- Block size: 1024 bytes (SZX=6)
- Server config: `CONFIG_COAP_SERVER_BLOCK_SIZE=1024`

### Moton.CoAP Support

**Status: ✅ Fully Supported**

#### Block2 (Response Segmentation) - Receiving Large Responses

```csharp
// CoapClientBlockTransferReceiver handles automatic Block2 assembly
public sealed class CoapClientBlockTransferReceiver
{
    public static bool IsBlockTransfer(CoapMessage responseMessage)
    {
        return responseMessage.Options.Any(o => o.Number == CoapMessageOptionNumber.Block2);
    }

    public async Task<ArraySegment<byte>> ReceiveFullPayload(CancellationToken cancellationToken)
    {
        // Automatically requests and assembles all blocks
        while (receivedBlock2OptionValue.HasFollowingBlocks)
        {
            receivedBlock2OptionValue.Number++;
            var response = await _client.RequestAsync(requestMessage, cancellationToken);
            buffer.Write(response.Payload);
        }
        return buffer.GetBuffer();
    }
}
```

#### Block1 (Request Segmentation) - Sending Large Requests

```csharp
// CoapClientBlockTransferSender handles Block1 for large uploads
public sealed class CoapClientBlockTransferSender
{
    public const int DefaultBlockSize = 1024;
    public const int MaxBlockSize = 1024;

    public bool RequiresBlockTransfer(ArraySegment<byte> payload)
    {
        return payload.Count > _blockSize;
    }

    public async Task<CoapMessage> SendAsync(
        CoapMessage requestTemplate,
        ArraySegment<byte> payload,
        CancellationToken cancellationToken)
    {
        // Splits payload into blocks and sends sequentially
        // Handles server-requested block size changes
    }
}
```

---

## 4. CoAP Observe (RFC 7641)

### Zephyr-Servo Requirements

Server-push notifications for async operation status:

| Resource | Use Case |
|----------|----------|
| `/sysid/bootstrap/adc_bias/status` | ADC bias calibration progress |
| `/sysid/rs` | Rs measurement status |
| `/sysid/ls` | Ls measurement status |
| `/foc/nlc/generation` | NLC generation progress |

Pattern:
1. Client registers with `Observe: 0`
2. Server pushes status updates as CON messages
3. Client sends ACK for each notification
4. Client deregisters with `Observe: 1` when done

### Moton.CoAP Support

**Status: ✅ Fully Supported**

```csharp
// ICoapClient interface includes Observe support
public interface ICoapClient : IDisposable
{
    Task<CoapObserveResponse> ObserveAsync(CoapObserveOptions options, CancellationToken cancellationToken);
    Task StopObservationAsync(CoapObserveResponse observeResponse, CancellationToken cancellationToken);
}

// CoapClientObservationManager handles notifications
public sealed class CoapClientObservationManager
{
    public void Register(CoapMessageToken token, ICoapResponseHandler responseHandler);
    public void Deregister(CoapMessageToken token);
    
    public async Task<bool> TryHandleReceivedMessage(CoapMessage message)
    {
        // Handles incoming observe notifications
        // Sends ACK for Confirmable messages
        // Invokes registered response handlers
    }
}
```

Usage:
```csharp
var observeResponse = await client.ObserveAsync(new CoapObserveOptions
{
    Request = new CoapObserveRequest { /* ... */ },
    ResponseHandler = myHandler
}, cancellationToken);

// Later...
await client.StopObservationAsync(observeResponse, cancellationToken);
```

---

## 5. URI Query Parameters

### Zephyr-Servo Requirements

Several endpoints use query parameters:

```
GET /sdo?index=0x1234&subindex=0x00
GET /foc/adc?id=0
GET /mtpa/lut?chunk=0
GET /encoder/digital/0
```

### Moton.CoAP Support

**Status: ✅ Fully Supported**

```csharp
public sealed class CoapRequestOptions
{
    public string? UriPath { get; set; }
    
    public ICollection<string>? UriQuery { get; set; }
}

// Usage
var request = new CoapRequestBuilder()
    .WithMethod(CoapRequestMethod.Get)
    .WithPath("/sdo")
    .WithQuery("index=0x1234")
    .WithQuery("subindex=0x00")
    .Build();
```

---

## 6. DTLS Security (PSK)

### Zephyr-Servo Requirements

Optional DTLS with Pre-Shared Key authentication for secure communication.

### Moton.CoAP Support

**Status: ✅ Fully Supported (via Extension)**

```csharp
// Moton.CoAP.Extensions.DTLS project
public sealed class DtlsCoapTransportLayer : ICoapTransportLayer
{
    public IDtlsCredentials Credentials { get; set; }
    public DtlsVersion DtlsVersion { get; set; } = DtlsVersion.V1_2;
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(10);
}

public sealed class PreSharedKey : IDtlsCredentials
{
    public byte[] Identity { get; set; }
    public byte[] Key { get; set; }
}
```

---

## 7. Connection/Timeout Configuration

### Zephyr-Servo Requirements

- ACK timeout configuration (varies by endpoint: 1500ms - 5000ms)
- Handshake timeout for DTLS
- Retransmission control

### Moton.CoAP Support

**Status: ✅ Fully Supported**

```csharp
public sealed class CoapClientConnectOptions
{
    public string Host { get; set; }
    public int Port { get; set; } = CoapDefaultPort.Unsecured;
    public TimeSpan CommunicationTimeout { get; set; } = TimeSpan.FromSeconds(5);
    public Func<ICoapTransportLayer>? TransportLayerFactory { get; set; }
}

// DTLS-specific timeout
public sealed class DtlsCoapTransportLayer
{
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(10);
}
```

---

## 8. Feature Comparison: Current vs Moton.CoAP

| Feature | Current (CoAP.NET) | Moton.CoAP |
|---------|-------------------|------------|
| Target Framework | netstandard2.0 | **net10.0 + netstandard2.0** |
| CBOR Content Format | ✅ Via extension | ✅ Built-in enum |
| Block2 Transfer | ✅ Automatic | ✅ Automatic |
| Block1 Transfer | ⚠️ Not explicitly used (see note) | ✅ Supported |
| Observe | ✅ `Request.MarkObserve()` | ✅ `ObserveAsync()` |
| DTLS PSK | ❌ Not implemented | ✅ Full support |
| Nullable Reference Types | ❌ Not enabled | ✅ Enabled |
| Modern C# Features | ❌ Older patterns | ✅ Latest LangVersion |

### Note on Block1 Transfer for NLC LUT Upload

The **`PUT /foc/nlc/lut`** endpoint is designed for RFC 7959 Block1 transfer on the server side:

```c
// foc_api.c - Server explicitly handles Block1
struct nlc_lut_block1_ctx {
    uint8_t *buffer;           // Reassembly buffer
    size_t buffer_size;        // Allocated size
    size_t received;           // Bytes received so far
    bool active;               // Transfer in progress
    int64_t last_access_ms;    // Timeout tracking
    struct coap_block_context block_ctx;
};
```

**Payload size calculation:**
- LUT size: 16384 entries × 4 bytes (int32) = **65,536 bytes**
- Plus CBOR overhead: ~65.5 KB total
- CoAP MTU: typically 1024 bytes per block

**Current client behavior:**
```csharp
// CoapClientManager.cs - PutNlcLutCborAsync
var payload = NlcLutCborEncoder.Encode(storeToFile, data);  // ~65KB CBOR
request.Payload = payload;  // Sent as single request
```

The current CoAP.NET library may handle Block1 **implicitly** when the payload exceeds the MTU, but this is not explicitly coded. The server handles both:
1. Single-block requests (if client sends everything in one packet)
2. Multi-block requests (RFC 7959 Block1 reassembly)

**With Moton.CoAP**, Block1 is **explicit and configurable**:
```csharp
// Moton.CoAP - Explicit Block1 handling
var sender = new CoapClientBlockTransferSender(client, blockSize: 1024);
if (sender.RequiresBlockTransfer(payload))
{
    response = await sender.SendAsync(request, payload, cancellationToken);
}
```

---

## 9. Migration Considerations

### API Differences

| Current Pattern | Moton.CoAP Pattern |
|-----------------|-------------------|
| `new Request(Method.GET)` | `new CoapRequestBuilder().WithMethod(CoapRequestMethod.Get)` |
| `request.URI = new Uri(...)` | `.WithHost(...).WithPath(...)` |
| `request.Send()` | `await client.ConnectAsync(...); await client.RequestAsync(...)` |
| `request.WaitForResponse()` | Returns `Task<CoapResponse>` |
| `request.MarkObserve()` | `await client.ObserveAsync(...)` |

### Block Transfer

Current implementation uses a custom `CborBlockTransfer` helper class. With Moton.CoAP, block transfer is transparent:

```csharp
// Current (explicit block handling)
var cborData = await CborBlockTransfer.GetBlockWiseAsync(uri, 5000);

// Moton.CoAP (automatic)
var response = await client.RequestAsync(request, cancellationToken);
// Block transfer is handled automatically if response has Block2 option
```

### CBOR Serialization

Both approaches use external CBOR libraries. No change needed:
```csharp
// Works with both
var payload = CborSerializer.Serialize(request);
var response = CborSerializer.Deserialize<T>(bytes);
```

---

## 10. Recommendations

1. **Use Moton.CoAP** - All required features are supported with modern C# patterns

2. **Leverage Built-in Block Transfer** - Remove custom `CborBlockTransfer` class; Moton.CoAP handles this automatically

3. **Simplify Observe Pattern** - Use `ICoapClient.ObserveAsync()` with proper async/await instead of event callbacks

4. **Add DTLS Support** - Enable secure communication using `Moton.CoAP.Extensions.DTLS`

5. **Consider Wrapper Service** - Create a thin wrapper to maintain backward compatibility:
   ```csharp
   public class MotonCoapClientManager : ICoapClientManager
   {
       private readonly ICoapClient _client;
       // Implement interface using Moton.CoAP
   }
   ```

---

## 11. Test Coverage in Moton.CoAP

| Test File | Coverage |
|-----------|----------|
| `CoapBlockTransfer_Tests.cs` | Block1/Block2 encoding/decoding |
| `CoapClient_Tests.cs` | End-to-end client tests |
| `CoapContentFormatHelper_Tests.cs` | Content format detection |
| `CoapMessageCode_Tests.cs` | Response code handling |
| `CoapMessageEncoder_Tests.cs` | Message serialization |
| `CoapMessageDecoder_Tests.cs` | Message deserialization |

**Total: 66 tests passing**

---

## Conclusion

**Moton.CoAP fully supports all CoAP features required by the zephyr-servo project:**

- ✅ All HTTP-like methods (GET, PUT, POST, DELETE)
- ✅ CBOR content format (60)
- ✅ RFC 7959 Block-wise transfer (both Block1 and Block2)
- ✅ RFC 7641 Observe for server-push notifications
- ✅ URI path and query parameters
- ✅ DTLS with Pre-Shared Key authentication
- ✅ Configurable timeouts and retries
- ✅ Modern .NET 10 support with netstandard2.0 compatibility

The library is ready for integration with the ServoTuner application.
