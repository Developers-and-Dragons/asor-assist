using AsorAssistant.Core.Models;
using AsorAssistant.Domain.Models;
using AsorAssistant.Infrastructure.Persistence;

namespace AsorAssistant.Infrastructure.Tests.Persistence;

public class FileDraftStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileDraftStore _store;

    public FileDraftStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "asor-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _store = new FileDraftStore(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private static DraftEnvelope CreateTestEnvelope(string displayName = "Test Draft") => new()
    {
        Metadata = new DraftMetadata
        {
            Id = Guid.NewGuid().ToString("N"),
            DisplayName = displayName,
            Provider = "test-provider",
            Version = "1.0.0",
            RegionName = "US",
            TenantName = "test-tenant",
            LastModified = DateTimeOffset.UtcNow
        },
        Definition = new AgentDefinition
        {
            Name = "Test Agent",
            Description = "A test agent",
            Url = "https://example.com/agent",
            Version = "1.0.0",
            Provider = new Provider { Id = "test-provider" },
            Platform = new Platform { Id = "OTHER" },
            Capabilities = new Capabilities(),
            Skills =
            [
                new AgentSkill
                {
                    Id = "skill-1",
                    Name = "Test Skill",
                    Description = "A test skill",
                    Tags = [new SkillTag { Tag = "test" }]
                }
            ]
        }
    };

    [Fact]
    public async Task Save_then_load_roundtrips_correctly()
    {
        var envelope = CreateTestEnvelope();
        var saved = await _store.SaveAsync(envelope);
        var loaded = await _store.LoadAsync(saved.Metadata.Id);

        Assert.NotNull(loaded);
        Assert.Equal(envelope.Metadata.DisplayName, loaded.Metadata.DisplayName);
        Assert.Equal(envelope.Definition.Name, loaded.Definition.Name);
        Assert.Equal(envelope.Definition.Skills!.Count, loaded.Definition.Skills!.Count);
        Assert.Equal(envelope.Metadata.Provider, loaded.Metadata.Provider);
        Assert.Equal(envelope.Metadata.RegionName, loaded.Metadata.RegionName);
    }

    [Fact]
    public async Task List_returns_saved_draft_metadata()
    {
        var e1 = CreateTestEnvelope("Draft One");
        var e2 = CreateTestEnvelope("Draft Two");
        await _store.SaveAsync(e1);
        await _store.SaveAsync(e2);

        var list = await _store.ListAsync();

        Assert.Equal(2, list.Count);
        Assert.Contains(list, m => m.DisplayName == "Draft One");
        Assert.Contains(list, m => m.DisplayName == "Draft Two");
    }

    [Fact]
    public async Task Delete_removes_draft()
    {
        var envelope = CreateTestEnvelope();
        var saved = await _store.SaveAsync(envelope);

        var deleted = await _store.DeleteAsync(saved.Metadata.Id);
        Assert.True(deleted);

        var loaded = await _store.LoadAsync(saved.Metadata.Id);
        Assert.Null(loaded);

        var list = await _store.ListAsync();
        Assert.Empty(list);
    }

    [Fact]
    public async Task Delete_nonexistent_returns_false()
    {
        var result = await _store.DeleteAsync("nonexistent-id");
        Assert.False(result);
    }

    [Fact]
    public async Task Load_nonexistent_returns_null()
    {
        var loaded = await _store.LoadAsync("nonexistent-id");
        Assert.Null(loaded);
    }

    [Fact]
    public async Task Save_overwrites_existing_draft()
    {
        var envelope = CreateTestEnvelope();
        var saved = await _store.SaveAsync(envelope);

        saved.Definition.Name = "Updated Agent";
        saved.Metadata.DisplayName = "Updated Draft";
        await _store.SaveAsync(saved);

        var loaded = await _store.LoadAsync(saved.Metadata.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Updated Agent", loaded.Definition.Name);
        Assert.Equal("Updated Draft", loaded.Metadata.DisplayName);

        var list = await _store.ListAsync();
        Assert.Single(list);
    }

    [Fact]
    public async Task Duplicate_creates_independent_copy()
    {
        var envelope = CreateTestEnvelope("Original");
        var saved = await _store.SaveAsync(envelope);

        var duplicate = await _store.DuplicateAsync(saved.Metadata.Id, "Copy of Original");

        Assert.NotEqual(saved.Metadata.Id, duplicate.Metadata.Id);
        Assert.Equal("Copy of Original", duplicate.Metadata.DisplayName);
        Assert.Equal(saved.Definition.Name, duplicate.Definition.Name);

        // Modifying the duplicate should not affect the original
        duplicate.Definition.Name = "Modified Copy";
        await _store.SaveAsync(duplicate);

        var original = await _store.LoadAsync(saved.Metadata.Id);
        Assert.Equal("Test Agent", original!.Definition.Name);

        var list = await _store.ListAsync();
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task Filenames_are_sanitized()
    {
        var envelope = CreateTestEnvelope();
        // Use an id with characters that would be invalid in filenames
        envelope.Metadata.Id = "test/draft:with<bad>chars";
        var saved = await _store.SaveAsync(envelope);

        var loaded = await _store.LoadAsync(saved.Metadata.Id);
        Assert.NotNull(loaded);
        Assert.Equal(envelope.Definition.Name, loaded.Definition.Name);
    }

    [Fact]
    public async Task Save_updates_last_modified_timestamp()
    {
        var envelope = CreateTestEnvelope();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var saved = await _store.SaveAsync(envelope);
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        Assert.InRange(saved.Metadata.LastModified, before, after);
    }

    [Fact]
    public async Task List_returns_empty_when_no_drafts()
    {
        var list = await _store.ListAsync();
        Assert.Empty(list);
    }

    [Fact]
    public async Task Drafts_directory_created_if_missing()
    {
        var newDir = Path.Combine(_tempDir, "subdir", "drafts");
        var store = new FileDraftStore(newDir);

        var envelope = CreateTestEnvelope();
        await store.SaveAsync(envelope);

        Assert.True(Directory.Exists(newDir));
    }
}
