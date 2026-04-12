using System.Text.Json;
using AsorAssistant.Core.Models;
using AsorAssistant.Core.Ports;
using AsorAssistant.Core.Services;
using NSubstitute;

namespace AsorAssistant.Core.Tests.Services;

public class WqlLookupServiceTests
{
    private static WqlResponse ParseJsonToResponse(string json)
    {
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var data = new List<JsonElement>();
        if (root.TryGetProperty("data", out var dataArray))
        {
            foreach (var item in dataArray.EnumerateArray())
                data.Add(item.Clone());
        }

        var total = root.TryGetProperty("total", out var t) ? t.GetInt32() : 0;
        return new WqlResponse { Data = data, Total = total };
    }

    [Fact]
    public void ParseSoapResponse_flattens_nested_operations()
    {
        var json = """
        {
          "data": [
            {
              "webService": {
                "descriptor": "Recruiting (Public)",
                "id": "1bd838271d3c40ec93549d9b2ea513bb"
              },
              "webServiceOperation": [
                {
                  "webServiceOperationName": "Assess Candidate",
                  "workdayID": "31a364710b4510002be6be02ebdb00ef"
                },
                {
                  "webServiceOperationName": "Bulk Import Put Candidate",
                  "workdayID": "875d4ef4e00c10000f001159d7de0000"
                }
              ]
            },
            {
              "webService": {
                "descriptor": "Human Resources (Public)",
                "id": "aabbccdd"
              },
              "webServiceOperation": [
                {
                  "webServiceOperationName": "Get Workers",
                  "workdayID": "deadbeef1234"
                }
              ]
            }
          ],
          "total": 2
        }
        """;

        var response = ParseJsonToResponse(json);
        var results = WqlLookupService.ParseSoapResponse(response);

        Assert.Equal(3, results.Count);

        Assert.Equal("Assess Candidate", results[0].Name);
        Assert.Equal("31a364710b4510002be6be02ebdb00ef", results[0].WorkdayId);
        Assert.Equal("Recruiting (Public)", results[0].ServiceName);
        Assert.Equal(ServiceOperationType.Soap, results[0].Type);

        Assert.Equal("Bulk Import Put Candidate", results[1].Name);
        Assert.Equal("Recruiting (Public)", results[1].ServiceName);

        Assert.Equal("Get Workers", results[2].Name);
        Assert.Equal("Human Resources (Public)", results[2].ServiceName);
    }

    [Fact]
    public void ParseRestResponse_extracts_flat_operations()
    {
        var json = """
        {
          "data": [
            {
              "serviceOperation": {
                "descriptor": "api/workers/view (GET) (v1 -  )",
                "id": "94a39e71541b468fa895955508287acd"
              },
              "workdayID": "94a39e71541b468fa895955508287acd"
            },
            {
              "serviceOperation": {
                "descriptor": "api/absences/view (GET) (v1 -  )",
                "id": "bbbb1111"
              },
              "workdayID": "bbbb1111"
            }
          ],
          "total": 2
        }
        """;

        var response = ParseJsonToResponse(json);
        var results = WqlLookupService.ParseRestResponse(response);

        Assert.Equal(2, results.Count);

        Assert.Equal("api/workers/view (GET) (v1 -  )", results[0].Name);
        Assert.Equal("94a39e71541b468fa895955508287acd", results[0].WorkdayId);
        Assert.Null(results[0].ServiceName);
        Assert.Equal(ServiceOperationType.Rest, results[0].Type);

        Assert.Equal("api/absences/view (GET) (v1 -  )", results[1].Name);
    }

    [Fact]
    public void ParseSoapResponse_handles_empty_data()
    {
        var response = new WqlResponse { Data = [], Total = 0 };
        var results = WqlLookupService.ParseSoapResponse(response);
        Assert.Empty(results);
    }

    [Fact]
    public void ParseRestResponse_handles_empty_data()
    {
        var response = new WqlResponse { Data = [], Total = 0 };
        var results = WqlLookupService.ParseRestResponse(response);
        Assert.Empty(results);
    }

    [Fact]
    public void ParseSoapResponse_handles_null_data()
    {
        var response = new WqlResponse { Data = null, Total = 0 };
        var results = WqlLookupService.ParseSoapResponse(response);
        Assert.Empty(results);
    }

    [Fact]
    public void ParseSoapResponse_skips_entries_missing_operation_name()
    {
        var json = """
        {
          "data": [
            {
              "webService": { "descriptor": "Test", "id": "abc" },
              "webServiceOperation": [
                { "workdayID": "123" },
                { "webServiceOperationName": "Valid Op", "workdayID": "456" }
              ]
            }
          ],
          "total": 1
        }
        """;

        var response = ParseJsonToResponse(json);
        var results = WqlLookupService.ParseSoapResponse(response);

        Assert.Single(results);
        Assert.Equal("Valid Op", results[0].Name);
    }

    [Fact]
    public async Task LookupSoapOperationsAsync_sends_correct_query()
    {
        var mockClient = Substitute.For<IWqlClient>();
        mockClient.QueryAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new WqlResponse { Data = [], Total = 0 });

        var service = new WqlLookupService(mockClient);
        await service.LookupSoapOperationsAsync("test-token");

        await mockClient.Received(1).QueryAsync("test-token", WqlLookupService.SoapQuery);
    }

    [Fact]
    public async Task LookupRestOperationsAsync_sends_correct_query()
    {
        var mockClient = Substitute.For<IWqlClient>();
        mockClient.QueryAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(new WqlResponse { Data = [], Total = 0 });

        var service = new WqlLookupService(mockClient);
        await service.LookupRestOperationsAsync("test-token");

        await mockClient.Received(1).QueryAsync("test-token", WqlLookupService.RestQuery);
    }

    [Fact]
    public void Soap_query_constant_matches_expected()
    {
        Assert.Contains("publicWebServices", WqlLookupService.SoapQuery);
        Assert.Contains("webServiceOperation", WqlLookupService.SoapQuery);
    }

    [Fact]
    public void Rest_query_constant_matches_expected()
    {
        Assert.Contains("allPublicServiceOperations", WqlLookupService.RestQuery);
        Assert.Contains("serviceOperation", WqlLookupService.RestQuery);
    }
}
