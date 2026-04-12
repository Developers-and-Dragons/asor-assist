namespace AsorAssistant.Domain.Validation;

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = [];

    public static ValidationResult Success() => new();

    public static ValidationResult Failure(params string[] errors)
    {
        var result = new ValidationResult();
        result.Errors.AddRange(errors);
        return result;
    }
}
