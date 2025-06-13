namespace Skrapr.Domain;

public interface IAgent
{
    Task<WebPageParsingResult> Inspect(WebSiteUrl url, UserDataJsonSchema schema, string? instruction);
}