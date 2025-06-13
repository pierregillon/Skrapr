using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Skrapr.Domain;
using Skrapr.Infra.Playwright;

#pragma warning disable SKEXP0001

namespace Skrapr.Infra;

public class SemanticKernelAgent(
    IOptions<AzureOpenAiConfiguration> configuration,
    IPlaywrightConfigurator playwrightConfigurator
) : IAgent
{
    private readonly AzureOpenAiConfiguration _configuration = configuration.Value;

    public async Task<WebPageParsingResult> Inspect(WebSiteUrl url, UserDataJsonSchema schema, string? instruction)
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.AddAzureOpenAIChatCompletion(
            _configuration.DeploymentName,
            _configuration.Endpoint,
            _configuration.ApiKey
        );

        var kernel = builder.Build();

        await playwrightConfigurator.Configure(kernel);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var textResult = await chatCompletionService.GetChatMessageContentsAsync(
            BuildChatHistory(url, schema, instruction),
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

    private static ChatHistory BuildChatHistory(WebSiteUrl url, UserDataJsonSchema schema, string? instruction)
    {
        var chatHistory = new ChatHistory();

        chatHistory.AddSystemMessage(Prompts.SystemPrompt);
        chatHistory.AddUserMessage(
            $"""
             User instruction: {instruction}
             Web site: {url}
             Json schema: {schema}
             """
        );
        
        return chatHistory;
    }

    private static JsonElement ToJsonElement(IReadOnlyList<ChatMessageContent> textResult)
    {
        var utf8JsonReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(textResult.Single().Content!));
        return JsonElement.ParseValue(ref utf8JsonReader);
    }
}