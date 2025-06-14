using System.Text.Json;
using Skrapr.Domain.ValueObjects;

namespace Skrapr.Domain;

public interface IAgent
{
    Task<WebSiteInspectionResult> Inspect(WebSiteUrl url, UserDataJsonSchema schema, string? instruction);
}

public record WebSiteInspectionResult(JsonElement Data);
