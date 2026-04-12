using System.Text.Json;
using AsorAssistant.Application.Serialization;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Infrastructure.Http;

public static class AsorErrorParser
{
    public static IReadOnlyList<AsorError> Parse(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return [new AsorError { Error = "Empty response body" }];

        try
        {
            // Try VALIDATION_ERROR format first: { "error": "...", "errors": [...] }
            var validationResponse = JsonSerializer.Deserialize(
                responseBody, AsorJsonContext.Default.AsorValidationErrorResponse);

            if (validationResponse?.Errors is { Count: > 0 })
                return validationResponse.Errors;

            // Try single ERROR_MODEL format: { "error": "...", "code": "...", ... }
            var errorResponse = JsonSerializer.Deserialize(
                responseBody, AsorJsonContext.Default.AsorErrorResponse);

            if (errorResponse?.Error is not null)
            {
                return
                [
                    new AsorError
                    {
                        Error = errorResponse.Error,
                        Code = errorResponse.Code,
                        Field = errorResponse.Field,
                        Path = errorResponse.Path
                    }
                ];
            }

            return [new AsorError { Error = responseBody }];
        }
        catch (JsonException)
        {
            return [new AsorError { Error = responseBody }];
        }
    }
}
