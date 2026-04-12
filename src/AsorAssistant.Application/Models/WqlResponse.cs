using System.Text.Json;

namespace AsorAssistant.Application.Models;

public class WqlResponse
{
    public List<JsonElement>? Data { get; set; }
    public int Total { get; set; }
}
