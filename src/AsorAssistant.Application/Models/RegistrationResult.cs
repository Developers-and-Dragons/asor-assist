using System.Net;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Application.Models;

public class RegistrationResult
{
    public required bool Success { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public AgentDefinition? Definition { get; init; }
    public IReadOnlyList<AsorError> Errors { get; init; } = [];
    public string? RawResponseBody { get; init; }

    public static RegistrationResult Succeeded(HttpStatusCode statusCode, AgentDefinition definition, string rawBody) =>
        new() { Success = true, StatusCode = statusCode, Definition = definition, RawResponseBody = rawBody };

    public static RegistrationResult Failed(HttpStatusCode statusCode, IReadOnlyList<AsorError> errors, string rawBody) =>
        new() { Success = false, StatusCode = statusCode, Errors = errors, RawResponseBody = rawBody };

    public static RegistrationResult NetworkError(string message) =>
        new() { Success = false, Errors = [new AsorError { Error = message }] };
}
