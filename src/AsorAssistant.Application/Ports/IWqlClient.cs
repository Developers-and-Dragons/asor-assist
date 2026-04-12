using AsorAssistant.Application.Models;

namespace AsorAssistant.Application.Ports;

public interface IWqlClient
{
    Task<WqlResponse> QueryAsync(string bearerToken, string query);
}
