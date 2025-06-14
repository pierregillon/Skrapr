namespace Skrapr.Domain;

public interface IAgent
{
    Task<WebSiteInspectionResult> Inspect(WebSiteUrl url, UserDataJsonSchema schema, string? instruction);
}