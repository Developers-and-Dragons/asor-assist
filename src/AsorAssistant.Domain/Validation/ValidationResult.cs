namespace AsorAssistant.Domain.Validation;

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = [];

    /// <summary>
    /// Field-keyed errors for inline display. Key is a field path (e.g. "Name", "Skills[0].Id").
    /// </summary>
    public Dictionary<string, string> FieldErrors { get; } = new(StringComparer.Ordinal);

    public static ValidationResult Success() => new();

    public static ValidationResult Failure(params string[] errors)
    {
        var result = new ValidationResult();
        result.Errors.AddRange(errors);
        return result;
    }
}
