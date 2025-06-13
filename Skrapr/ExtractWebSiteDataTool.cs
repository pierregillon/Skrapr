using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ModelContextProtocol.Client;
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

    [McpServerTool]
    [Description("Parse a web page and extract specific data.")]
    public async Task<WebPageParsingResult> ParseWebPage(
        [Description("The url to browse.")] string url,
        [Description("The json schema to extract.")]
        string schema,
        [Description("Specific instruction to follow during the process.")]
        string? instruction = null
    )
    {
        CheckJsonSchema(schema);

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

    private static void CheckJsonSchema(string schema)
    {
        var utf8JsonReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(schema));
        _ = JsonElement.ParseValue(ref utf8JsonReader);
    }
}