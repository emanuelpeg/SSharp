# SSharp.Api

Stateless evaluation API for the SSharp language, exposing two transports over the same port:

| Transport | Endpoint | Protocol |
|-----------|----------|----------|
| REST | `POST /api/eval` | HTTP/1.1 + HTTP/2 |
| gRPC | `ssharp.EvalService/Eval` | HTTP/2 |
| Scalar UI | `GET /scalar/v1` | HTTP/1.1 |
| OpenAPI JSON | `GET /openapi/v1.json` | HTTP/1.1 |

Every request is fully independent — no session state is preserved between calls. For a stateful interactive session, use the [REPL console mode](../SSharp.CLI/README.md) instead.

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Running locally

```sh
dotnet run --project SSharp.Api
```

The server starts on **port 5000** and accepts both HTTP/1.1 (REST) and HTTP/2 (gRPC) on the same port without TLS — suitable for local development.

You can verify the server is up with:

```sh
curl http://localhost:5000/
```

```json
{
  "service": "SSharp Eval API",
  "version": "0.1.0",
  "endpoints": [
    "POST /api/eval         — REST: evaluate a SSharp code snippet",
    "GET  /scalar/v1        — Scalar interactive API reference (Swagger UI)",
    "GET  /openapi/v1.json  — Raw OpenAPI 3.1 schema",
    "gRPC EvalService       — ssharp.EvalService/Eval"
  ]
}
```

---

## Interactive API Reference (Scalar)

Once the server is running, open **[http://localhost:5000/scalar/v1](http://localhost:5000/scalar/v1)** in your browser.

Scalar is a modern OpenAPI explorer that lets you:
- Browse all endpoints with full request/response schemas
- Send live requests directly from the browser
- Copy ready-to-use `curl` snippets

The raw **OpenAPI 3.1 JSON** schema is also available at:
```
GET http://localhost:5000/openapi/v1.json
```

---

## REST API

### `POST /api/eval`

Evaluates a SSharp code snippet and returns the captured output, any errors, and the inferred type of the last top-level declaration.

#### Request

```
POST /api/eval
Content-Type: application/json
```

```json
{
  "code": "<SSharp source code>"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `code` | `string` | ✓ | SSharp source code to evaluate |

#### Response `200 OK`

```json
{
  "success":   true,
  "output":    "Hello, SSharp!",
  "errors":    [],
  "elapsedMs": 42,
  "typeInfo":  "Unit"
}
```

| Field | Type | Description |
|-------|------|-------------|
| `success` | `bool` | `true` if all pipeline stages passed and execution did not throw |
| `output` | `string` | Everything written to stdout during execution |
| `errors` | `string[]` | Lex / parse / type / runtime error messages (empty on success) |
| `elapsedMs` | `number` | Wall-clock time of the eval phase in milliseconds |
| `typeInfo` | `string \| null` | SSharp type of the last top-level expression (e.g. `"Int"`, `"List[String]"`) |

#### Examples

**List Construction with `::` and `Nil`**

```sh
curl -s -X POST http://localhost:5000/api/eval \
  -H "Content-Type: application/json" \
  -d '{"code":"1 :: 2 :: 3 :: Nil"}'
```

```json
{
  "success": true,
  "output": "",
  "errors": [],
  "elapsedMs": 15,
  "typeInfo": "List[Int]"
}
```

**Simple expression**

```sh
curl -s -X POST http://localhost:5000/api/eval \
  -H "Content-Type: application/json" \
  -d '{"code":"val x = 2 + 2\nprintln(x)"}'
```

```json
{
  "success": true,
  "output": "4",
  "errors": [],
  "elapsedMs": 38,
  "typeInfo": "Int"
}
```

**Function definition and call**

```sh
curl -s -X POST http://localhost:5000/api/eval \
  -H "Content-Type: application/json" \
  -d '{
    "code": "def factorial(n: Int): Int =\n    if (n <= 1) 1 else n * factorial(n - 1)\nprintln(factorial(10))"
  }'
```

```json
{
  "success": true,
  "output": "3628800",
  "errors": [],
  "elapsedMs": 51,
  "typeInfo": null
}
```

**Type error**

```sh
curl -s -X POST http://localhost:5000/api/eval \
  -H "Content-Type: application/json" \
  -d '{"code":"val x: Int = \"oops\""}'
```

```json
{
  "success": false,
  "output": "",
  "errors": ["[1:15] Type Error: Type mismatch: Val 'x' expected Int, but got String."],
  "elapsedMs": 0,
  "typeInfo": null
}
```

---

## gRPC API

The proto definition lives at [`Protos/eval.proto`](Protos/eval.proto).

```protobuf
service EvalService {
  rpc Eval (EvalRequest) returns (EvalResponse);
}

message EvalRequest {
  string code = 1;
}

message EvalResponse {
  bool            success    = 1;
  string          output     = 2;
  repeated string errors     = 3;
  int64           elapsed_ms = 4;
  string          type_info  = 5;
}
```

### Using `grpcurl`

```sh
# List available services
grpcurl -plaintext localhost:5000 list

# Call Eval
grpcurl -plaintext -d '{"code":"val x = 42\nprintln(x)"}' \
  localhost:5000 ssharp.EvalService/Eval
```

### .NET client example

```csharp
using Grpc.Net.Client;
using SSharp.Api; // generated from proto

var channel  = GrpcChannel.ForAddress("http://localhost:5000");
var client   = new EvalService.EvalServiceClient(channel);

var response = await client.EvalAsync(new EvalRequest
{
    Code = "val x = 42\nprintln(x)"
});

Console.WriteLine($"Success:  {response.Success}");
Console.WriteLine($"Output:   {response.Output}");
Console.WriteLine($"TypeInfo: {response.TypeInfo}");
```

---

## Pipeline

Every request (REST or gRPC) runs the full SSharp compiler pipeline in-process, entirely in memory — no files are written to disk:

```
Input code
    │
    ▼
 Lexer          → lex errors → 400-style error response
    │
    ▼
 Parser         → parse errors
    │
    ▼
 TypeChecker    → type errors
    │
    ▼
 CodeGenerator  → C# source (in memory)
    │
    ▼
 Roslyn         → .NET assembly (in memory, CollectibleAssemblyLoadContext)
    │
    ▼
 Execute        → stdout captured → response
```

---

## Project Structure

```
SSharp.Api/
├── Controllers/
│   └── EvalController.cs   # REST POST /api/eval
├── Protos/
│   └── eval.proto          # gRPC service definition
├── Services/
│   └── EvalGrpcService.cs  # gRPC EvalService implementation
├── Program.cs              # ASP.NET Core host setup
└── SSharp.Api.csproj
```

---

## Security Note

> [!WARNING]
> The API executes arbitrary SSharp code in-process with no sandboxing beyond what .NET provides.
> It is intended for **local development and trusted internal use only**.
> Do not expose this service to the public internet without adding authentication and resource limits.
