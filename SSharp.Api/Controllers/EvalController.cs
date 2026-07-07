using Microsoft.AspNetCore.Mvc;
using SSharp.Api.Services;

namespace SSharp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EvalController : ControllerBase
{
    /// <summary>
    /// Evaluates a SSharp code snippet and returns the result.
    /// </summary>
    /// <remarks>
    /// This endpoint is stateless — no context is preserved between requests.
    ///
    /// Example request body:
    ///
    ///     { "code": "val x = 42\nprintln(x)" }
    ///
    /// </remarks>
    /// <param name="request">The code snippet to evaluate.</param>
    /// <returns>Evaluation result with output, errors, and type info.</returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(EvalResponseDto), 200)]
    public IActionResult Post([FromBody] EvalRequestDto request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { error = "Field 'code' is required." });

        var grpcResponse = EvalGrpcService.EvalCore(request.Code);

        return Ok(new EvalResponseDto(
            grpcResponse.Success,
            grpcResponse.Output,
            grpcResponse.Errors.ToList(),
            grpcResponse.ElapsedMs,
            string.IsNullOrEmpty(grpcResponse.TypeInfo) ? null : grpcResponse.TypeInfo));
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record EvalRequestDto(string Code);

public record EvalResponseDto(
    bool Success,
    string Output,
    IReadOnlyList<string> Errors,
    long ElapsedMs,
    string? TypeInfo);
