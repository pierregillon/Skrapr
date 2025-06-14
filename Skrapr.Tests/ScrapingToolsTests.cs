using System.Text.Json;
using FluentAssertions;
using Json.Schema;
using ModelContextProtocol.Client;
using Skrapr.Domain;
using Xunit.Abstractions;

namespace Skrapr.Tests;

public class ScrapingToolsTests(ITestOutputHelper output) : IDisposable, IAsyncDisposable
{
    private const string ScrapeWithSchemaToolName = "scrape_with_schema";

    private readonly TestApplication _application = new(output);

    [Fact]
    public async Task A_single_tool_for_web_site_extraction_is_available()
    {
        var client = await _application.SafeCreateClient();

        var mcpClient = await BuildMcpClient(client);

        var tools = await mcpClient.ListToolsAsync();

        tools.Should().HaveCount(1);
        tools.Single().Name.Should().Be(ScrapeWithSchemaToolName);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Cannot_scrape_an_empty_web_site_url(string? url)
    {
        var action = () => ScrapeWithSchema(url!);

        await action
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("Url cannot be null or whitespace. (Parameter 'url')");
    }

    [Fact]
    public async Task Cannot_scrape_an_invalid_web_site_url()
    {
        var action = () => ScrapeWithSchema("toto");

        await action
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("The url must be an absolute http or https url. (Parameter 'url')");
    }

    [Theory]
    [InlineData("{}}")]
    [InlineData("""
                {
                    "type": "object",
                    "properties": {
                        "description": {
                            "type": "string"
                        }
                    },,,,,,
                }
                """)]
    public async Task Cannot_scrape_a_web_site_with_an_invalid_schema(string invalidSchema)
    {
        var action = () => ScrapeWithSchema(schema: invalidSchema);
        ;

        await action
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("The json schema is malformed. (Parameter 'jsonSchema')");
    }

    [Theory]
    [InlineData("""
                {
                }
                """)]
    [InlineData("""
                {
                  "type": "object",
                  "properties": {}
                }
                """)]
    public async Task Cannot_scrape_a_web_page_with_an_empty_schema(string emptySchema)
    {
        var action = () => ScrapeWithSchema(schema: emptySchema);
        ;

        await action
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("The json schema is empty. At least one property is required. (Parameter 'jsonSchema')");
    }

    [Fact]
    public async Task Scrape_web_site()
    {
        var jsonSchema =
            """
            {
                "type": "object",
                "properties": {
                    "description": {
                        "type": "string"
                    },
                    "address": {
                        "type": "string"
                    },
                    "tickets": {
                        "type": "array",
                        "items": {
                            "type": "object",
                            "properties": {
                                "name": {
                                    "type": "string"
                                },
                                "short_description": {
                                    "type": "string"
                                },
                                "details": {
                                    "type": "string"
                                },
                                "price": {
                                    "type": "object",
                                    "properties": {
                                        "amount": {
                                            "type": "number"
                                        },
                                        "currency": {
                                            "type": "string",
                                            "description": "The currency of the price. In ISO 4217 format, for example USD, EUR, JPY, etc."
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                "required": ["description", "address", "tickets"]
            }
            """;

        var webPageParsingResult = await ScrapeWithSchema(
            "https://snowcamp.io",
            jsonSchema,
            "Extract data in French or translate them in French."
        );

        webPageParsingResult.Data.Should().NotBeNull();

        var schema = JsonSchema.FromText(jsonSchema);

        var result = schema.Evaluate(webPageParsingResult.Data);

        result.IsValid.Should().BeTrue();
    }

    private async Task<WebSiteInspectionResult> ScrapeWithSchema(
        string webSite = "https://www.google.com",
        string schema = "{}",
        string? instruction = null
    )
    {
        var client = await _application.SafeCreateClient();

        var mcpClient = await BuildMcpClient(client);

        var response = await mcpClient.CallToolAsync(
            ScrapeWithSchemaToolName,
            new Dictionary<string, object?>
            {
                { "url", webSite },
                { "jsonSchema", schema },
                { "instruction", instruction }
            },
            cancellationToken: CancellationToken.None
        );

        if (response.IsError)
        {
            throw new Exception(response.Content.Single().Text);
        }

        var document = JsonDocument.Parse(response.Content.Single().Text!);

        var root = document.RootElement.EnumerateObject().First().Value;

        return new WebSiteInspectionResult(root);
    }

    private static async Task<IMcpClient> BuildMcpClient(HttpClient client)
    {
        var mcpClient = await McpClientFactory.CreateAsync(
            new SseClientTransport(
                new SseClientTransportOptions { Endpoint = new Uri(client.BaseAddress!.AbsoluteUri + "sse") },
                client
            )
        );
        return mcpClient;
    }

    public void Dispose() =>
        _application.Dispose();

    public async ValueTask DisposeAsync() =>
        await _application.DisposeAsync();
}