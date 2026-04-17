using AsorAssistant.Domain.Models;

namespace AsorAssistant.Domain.Validation;

public static class AgentDefinitionValidator
{
    public static ValidationResult Validate(AgentDefinition definition)
    {
        var result = new ValidationResult();

        ValidateRequiredString(result, definition.Name, "Name", "Name is required.");
        ValidateRequiredString(result, definition.Description, "Description", "Description is required.");
        ValidateRequiredString(result, definition.Url, "Url", "Url is required.");
        ValidateRequiredString(result, definition.Version, "Version", "Version is required.");

        ValidateProvider(result, definition.Provider);
        ValidatePlatform(result, definition.Platform);

        if (definition.Capabilities is null)
            result.Errors.Add("Capabilities is required.");

        ValidateSkills(result, definition.Skills);
        ValidateWorkdayConfig(result, definition.WorkdayConfig, definition.Skills);

        return result;
    }

    private static void ValidateRequiredString(ValidationResult result, string? value, string fieldKey, string error)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.Errors.Add(error);
            result.FieldErrors[fieldKey] = error;
        }
    }

    private static void ValidateProvider(ValidationResult result, Provider? provider)
    {
        if (provider is null)
        {
            result.Errors.Add("Provider is required.");
            result.FieldErrors["Provider"] = "Provider is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(provider.Id))
        {
            result.Errors.Add("Provider id is required.");
            result.FieldErrors["Provider"] = "Provider id is required.";
        }
    }

    private static void ValidatePlatform(ValidationResult result, Platform? platform)
    {
        if (platform is null)
        {
            result.Errors.Add("Platform is required.");
            result.FieldErrors["Platform"] = "Platform is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(platform.Id))
        {
            result.Errors.Add("Platform id is required.");
            result.FieldErrors["Platform"] = "Platform id is required.";
        }
    }

    private static void ValidateSkills(ValidationResult result, List<AgentSkill>? skills)
    {
        if (skills is null || skills.Count == 0)
        {
            result.Errors.Add("At least one skill is required.");
            return;
        }

        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < skills.Count; i++)
        {
            var skill = skills[i];
            var prefix = $"Skills[{i}]";

            if (string.IsNullOrWhiteSpace(skill.Id))
            {
                result.Errors.Add($"{prefix}: id is required.");
                result.FieldErrors[$"{prefix}.Id"] = "Id is required.";
            }
            else if (!seenIds.Add(skill.Id))
            {
                result.Errors.Add($"{prefix}: duplicate skill id '{skill.Id}'.");
                result.FieldErrors[$"{prefix}.Id"] = $"Duplicate skill id '{skill.Id}'.";
            }

            if (string.IsNullOrWhiteSpace(skill.Name))
            {
                result.Errors.Add($"{prefix}: name is required.");
                result.FieldErrors[$"{prefix}.Name"] = "Name is required.";
            }

            if (string.IsNullOrWhiteSpace(skill.Description))
            {
                result.Errors.Add($"{prefix}: description is required.");
                result.FieldErrors[$"{prefix}.Description"] = "Description is required.";
            }

            // Tags are listed as required in the spec but are absent from real-world
            // payloads. Treat as optional to avoid false validation failures.
        }
    }

    private static void ValidateWorkdayConfig(
        ValidationResult result,
        List<AgentSkillResource>? workdayConfig,
        List<AgentSkill>? skills)
    {
        if (workdayConfig is null || workdayConfig.Count == 0)
            return;

        var skillIds = new HashSet<string>(StringComparer.Ordinal);
        if (skills is not null)
        {
            foreach (var skill in skills)
            {
                if (skill.Id is not null)
                    skillIds.Add(skill.Id);
            }
        }

        for (var i = 0; i < workdayConfig.Count; i++)
        {
            var resource = workdayConfig[i];
            var prefix = $"WorkdayConfig[{i}]";

            if (!ExecutionMode.IsValidId(resource.ExecutionMode?.Id))
            {
                result.Errors.Add($"{prefix}: executionMode must be '{ExecutionMode.Ambient}' or '{ExecutionMode.Delegate}'.");
                result.FieldErrors[$"{prefix}.ExecutionMode"] = "Must be Ambient or Delegate.";
            }

            if (!string.IsNullOrWhiteSpace(resource.SkillId) && !skillIds.Contains(resource.SkillId))
            {
                result.Errors.Add($"{prefix}: skillId '{resource.SkillId}' does not match any defined skill.");
                result.FieldErrors[$"{prefix}.SkillId"] = $"Skill '{resource.SkillId}' not found.";
            }
        }
    }
}
