using Scalar.AspNetCore;
using SSharp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddGrpc();

// OpenAPI document (JSON schema generated from controllers + XML comments)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, ctx, ct) =>
    {
        doc.Info.Title       = "SSharp Eval API";
        doc.Info.Version     = "v1";
        doc.Info.Description =
            "Stateless evaluation API for the SSharp functional language. " +
            "Submit a code snippet and get back the captured output and type information. " +
            "No state is preserved between requests.";
        return Task.CompletedTask;
    });
});

// Configure Kestrel to handle both HTTP/1.1 (REST) and HTTP/2 (gRPC) on the same port.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});

var app = builder.Build();

// ── OpenAPI & Scalar UI ───────────────────────────────────────────────────────
// Raw OpenAPI JSON: GET /openapi/v1.json
app.MapOpenApi();

// Interactive Scalar UI: GET /scalar/v1
app.MapScalarApiReference(options =>
{
    options.Title           = "SSharp Eval API";
    options.Theme           = ScalarTheme.DeepSpace;
    options.DefaultHttpClient = new(ScalarTarget.Shell, ScalarClient.Curl);
});

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseRouting();
app.MapControllers();
app.MapGrpcService<EvalGrpcService>();

// ── Info endpoint ─────────────────────────────────────────────────────────────
app.MapGet("/", () => Results.Ok(new
{
    service = "SSharp Eval API",
    version = "0.1.0",
    endpoints = new[]
    {
        "POST /api/eval         — REST: evaluate a SSharp code snippet",
        "GET  /scalar/v1        — Scalar interactive API reference (Swagger UI)",
        "GET  /openapi/v1.json  — Raw OpenAPI 3.1 schema",
        "gRPC EvalService       — ssharp.EvalService/Eval"
    }
}));

app.Run();

