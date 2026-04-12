using System.Text.Json;
using AsorAssistant.Core.Models;
using AsorAssistant.Core.Ports;
using AsorAssistant.Core.Serialization;

namespace AsorAssistant.Infrastructure.Persistence;

public class FileDraftStore : IDraftStore
{
    private readonly string _draftsDirectory;

    public FileDraftStore(string draftsDirectory)
    {
        _draftsDirectory = draftsDirectory;
    }

    public async Task<DraftEnvelope> SaveAsync(DraftEnvelope envelope)
    {
        Directory.CreateDirectory(_draftsDirectory);

        envelope.Metadata.LastModified = DateTimeOffset.UtcNow;

        var path = GetFilePath(envelope.Metadata.Id);
        var json = JsonSerializer.Serialize(envelope, AsorJsonContext.Default.DraftEnvelope);
        await File.WriteAllTextAsync(path, json);

        return envelope;
    }

    public async Task<DraftEnvelope?> LoadAsync(string id)
    {
        var path = GetFilePath(id);
        if (!File.Exists(path))
            return null;

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize(json, AsorJsonContext.Default.DraftEnvelope);
    }

    public Task<IReadOnlyList<DraftMetadata>> ListAsync()
    {
        if (!Directory.Exists(_draftsDirectory))
            return Task.FromResult<IReadOnlyList<DraftMetadata>>([]);

        var files = Directory.GetFiles(_draftsDirectory, "*.json");
        var metadata = new List<DraftMetadata>(files.Length);

        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var envelope = JsonSerializer.Deserialize(json, AsorJsonContext.Default.DraftEnvelope);
                if (envelope?.Metadata is not null)
                    metadata.Add(envelope.Metadata);
            }
            catch (JsonException)
            {
                // Skip malformed files
            }
        }

        return Task.FromResult<IReadOnlyList<DraftMetadata>>(
            metadata.OrderByDescending(m => m.LastModified).ToList());
    }

    public Task<bool> DeleteAsync(string id)
    {
        var path = GetFilePath(id);
        if (!File.Exists(path))
            return Task.FromResult(false);

        File.Delete(path);
        return Task.FromResult(true);
    }

    public async Task<DraftEnvelope> DuplicateAsync(string sourceId, string newDisplayName)
    {
        var source = await LoadAsync(sourceId)
            ?? throw new InvalidOperationException($"Draft '{sourceId}' not found.");

        var duplicate = new DraftEnvelope
        {
            Metadata = new DraftMetadata
            {
                Id = Guid.NewGuid().ToString("N"),
                DisplayName = newDisplayName,
                Provider = source.Metadata.Provider,
                Version = source.Metadata.Version,
                RegionName = source.Metadata.RegionName,
            },
            Definition = DeepCopy(source.Definition)
        };

        return await SaveAsync(duplicate);
    }

    private string GetFilePath(string id)
    {
        var sanitized = SanitizeFileName(id);
        return Path.Combine(_draftsDirectory, sanitized + ".json");
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new char[name.Length];
        for (var i = 0; i < name.Length; i++)
            sanitized[i] = Array.IndexOf(invalid, name[i]) >= 0 ? '_' : name[i];
        return new string(sanitized);
    }

    private static Domain.Models.AgentDefinition DeepCopy(Domain.Models.AgentDefinition source)
    {
        var json = JsonSerializer.Serialize(source, AsorJsonContext.Default.AgentDefinition);
        return JsonSerializer.Deserialize(json, AsorJsonContext.Default.AgentDefinition)!;
    }
}
