using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

#pragma warning disable SKEXP0001

namespace Skrapr;

[McpServerToolType]
[Description(
    """
    Extract specific data from a web page.
    """
)]
public class ExtractWebSiteDataTool(
    IOptions<AzureOpenAiConfiguration> configuration,
    IOptions<PlaywrightMcpConfiguration> playwrightConfiguration
)
{
    private readonly AzureOpenAiConfiguration _configuration = configuration.Value;
    private readonly PlaywrightMcpConfiguration _playwrightMcpConfiguration = playwrightConfiguration.Value;

    [McpServerTool(
        Name = "parse_web_site",
        Title = "Parse a web site and extract specific data.",
        ReadOnly = true,
        Idempotent = false,
        Destructive = false
    )]
    [Description("Parse a web site and extract specific data.")]
    public async Task<object> ParseWebSite(
        [Required] [Description("The web site url to browse.")] string url,
        [Required] [Description("The json schema to extract.")] string jsonSchema,
        [Description("Specific instruction to follow during the process.")] string? instruction = null
    )
    {
        try
        {
            var webSiteUrl = new WebSiteUrl(url);
            var userDataJsonSchema = new UserDataJsonSchema(jsonSchema);

            return await Process(webSiteUrl, userDataJsonSchema, instruction);
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

    private async Task<WebPageParsingResult> Process(WebSiteUrl url, UserDataJsonSchema schema, string? instruction)
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.AddAzureOpenAIChatCompletion(
            _configuration.DeploymentName,
            _configuration.Endpoint,
            _configuration.ApiKey
        );

        var kernel = builder.Build();

        await AddPlaywrightMcp(kernel);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();

        chatHistory.AddSystemMessage(Prompts.SystemPrompt);
        chatHistory.AddUserMessage(
            $"""
             User instruction: {instruction}
             Web site: {url}
             Json schema: {schema}
             """
        );

        var textResult = await chatCompletionService.GetChatMessageContentsAsync(
            chatHistory,
            new AzureOpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new FunctionChoiceBehaviorOptions
                {
                    AllowStrictSchemaAdherence = true
                }),
                ResponseFormat = "json_object"
            },
            kernel
        );

        var jsonResult = ToJsonElement(textResult);

        return new WebPageParsingResult(jsonResult);
    }

    private async Task AddPlaywrightMcp(Kernel kernel)
    {
        var mcpClient = await McpClientFactory.CreateAsync(
            new SseClientTransport(
                new SseClientTransportOptions
                {
                    Endpoint = new Uri(_playwrightMcpConfiguration.Endpoint),
                    Name = "Playwright tools",
                    TransportMode = HttpTransportMode.Sse
                })
        );

        var tools = await mcpClient.ListToolsAsync();

        var kernelFunctions = tools
            .Select(aiFunction => aiFunction.AsKernelFunction())
            .ToList();

        kernel.Plugins.AddFromFunctions("Playwright", kernelFunctions);
    }

    private static JsonElement ToJsonElement(IReadOnlyList<ChatMessageContent> textResult)
    {
        var utf8JsonReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(textResult.Single().Content!));
        return JsonElement.ParseValue(ref utf8JsonReader);
    }
}