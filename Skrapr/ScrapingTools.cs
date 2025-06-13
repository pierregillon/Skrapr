using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Skrapr.Domain;

#pragma warning disable SKEXP0001

namespace Skrapr;

[McpServerToolType]
[Description("""
             The pluging 'Skrapr'.
             Extract structured data from a website by simulating human-like navigation. 
             The tool uses a JSON Schema provided by the user to identify and return the relevant data points from the website.
             """)]
public class ScrapingTools(IAgent agent)
{
    [McpServerTool(
        Name = "scrape_with_schema",
        Title = "Extract structured data from a website using a JSON Schema.",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false
    )]
    [Description("""
                 Navigate and scrape the given website to extract data matching the structure of the provided JSON Schema.
                 This tool is ideal for retrieving structured content from arbitrary pages.
                 """)]
    public async Task<object> ScrapeWithSchema(
        [Required] [Description("The URL of the website to extract data from.")]
        string url,
        [Required] [Description("The JSON Schema describing the data structure to extract.")]
        string jsonSchema,
        [Description("Optional instructions to guide how the scraping should be performed.")]
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