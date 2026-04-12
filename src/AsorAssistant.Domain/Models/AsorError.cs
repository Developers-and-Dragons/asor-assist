namespace AsorAssistant.Domain.Models;

public class AsorError
{
    public string? Error { get; set; }
    public string? Code { get; set; }
    public string? Field { get; set; }
    public string? Path { get; set; }
}
