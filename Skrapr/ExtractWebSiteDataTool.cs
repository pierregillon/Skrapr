using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Skrapr.Domain;

#pragma warning disable SKEXP0001

namespace Skrapr;

[McpServerToolType]
[Description(
    """
    Extract specific data from a web page.
    """
)]
public class ExtractWebSiteDataTool(IAgent agent)
{
    [McpServerTool(
        Name = "parse_web_site",
        Title = "Parse a web site and extract specific data.",
        ReadOnly = true,
        Idempotent = false,
        Destructive = false
    )]
    [Description("Parse a web site and extract specific data.")]
    public async Task<object> ParseWebSite(
        [Required] [Description("The web site url to browse.")]
        string url,
        [Required] [Description("The json schema to extract.")]
        string jsonSchema,
        [Description("Specific instruction to follow during the process.")]
        string? instruction = null
    )
    {
        try
        {
            var webSiteUrl = new WebSiteUrl(url);
            var userDataJsonSchema = new UserDataJsonSchema(jsonSchema);

            return await agent.Inspect(webSiteUrl, userDataJsonSchema, instruction);
        }
        catch (ArgumentException e)
        {
            return new CallToolResponse
            {
                Content =
                [
                    new Content
                    {
                        Type = "text",
                        Text = e.Message
                    }
                ],
                IsError = true
            };
        }
    }
}