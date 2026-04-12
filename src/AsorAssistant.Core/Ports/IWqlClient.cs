using AsorAssistant.Core.Models;

namespace AsorAssistant.Core.Ports;

public interface IWqlClient
{
    Task<WqlResponse> QueryAsync(string bearerToken, string query);
}
