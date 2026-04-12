using System.Text.Json;
using AsorAssistant.Application.Models;
using AsorAssistant.Application.Ports;

namespace AsorAssistant.Application.Services;

public class WqlLookupService
{
    private readonly IWqlClient _wqlClient;

    public const string SoapQuery =
        "SELECT webService, webServiceOperation{webServiceOperationName, workdayID} FROM publicWebServices";

    public const string RestQuery =
        "SELECT serviceOperation, workdayID FROM allPublicServiceOperations";

    public WqlLookupService(IWqlClient wqlClient)
    {
        _wqlClient = wqlClient;
    }

    public async Task<IReadOnlyList<ServiceOperationLookup>> LookupSoapOperationsAsync(string bearerToken)
    {
        var response = await _wqlClient.QueryAsync(bearerToken, SoapQuery);
        return ParseSoapResponse(response);
    }

    public async Task<IReadOnlyList<ServiceOperationLookup>> LookupRestOperationsAsync(string bearerToken)
    {
        var response = await _wqlClient.QueryAsync(bearerToken, RestQuery);
        return ParseRestResponse(response);
    }

    internal static IReadOnlyList<ServiceOperationLookup> ParseSoapResponse(WqlResponse response)
    {
        if (response.Data is null)
            return [];

        var results = new List<ServiceOperationLookup>();

        foreach (var row in response.Data)
        {
            var serviceName = row.TryGetProperty("webService", out var ws)
                && ws.TryGetProperty("descriptor", out var desc)
                ? desc.GetString()
                : null;

            if (!row.TryGetProperty("webServiceOperation", out var operations)
                || operations.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var op in operations.EnumerateArray())
            {
                var name = op.TryGetProperty("webServiceOperationName", out var n) ? n.GetString() : null;
                var wid = op.TryGetProperty("workdayID", out var w) ? w.GetString() : null;

                if (name is not null && wid is not null)
                {
                    results.Add(new ServiceOperationLookup
                    {
                        Name = name,
                        WorkdayId = wid,
                        ServiceName = serviceName,
                        Type = ServiceOperationType.Soap
                    });
                }
            }
        }

        return results;
    }

    internal static IReadOnlyList<ServiceOperationLookup> ParseRestResponse(WqlResponse response)
    {
        if (response.Data is null)
            return [];

        var results = new List<ServiceOperationLookup>();

        foreach (var row in response.Data)
        {
            var name = row.TryGetProperty("serviceOperation", out var so)
                && so.TryGetProperty("descriptor", out var desc)
                ? desc.GetString()
                : null;

            var wid = row.TryGetProperty("workdayID", out var w) ? w.GetString() : null;

            if (name is not null && wid is not null)
            {
                results.Add(new ServiceOperationLookup
                {
                    Name = name,
                    WorkdayId = wid,
                    Type = ServiceOperationType.Rest
                });
            }
        }

        return results;
    }
}
