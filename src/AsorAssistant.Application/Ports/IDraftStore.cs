using AsorAssistant.Application.Models;

namespace AsorAssistant.Application.Ports;

public interface IDraftStore
{
    Task<DraftEnvelope> SaveAsync(DraftEnvelope envelope);
    Task<DraftEnvelope?> LoadAsync(string id);
    Task<IReadOnlyList<DraftMetadata>> ListAsync();
    Task<bool> DeleteAsync(string id);
    Task<DraftEnvelope> DuplicateAsync(string sourceId, string newDisplayName);
}
