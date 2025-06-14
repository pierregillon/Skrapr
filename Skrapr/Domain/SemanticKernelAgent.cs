using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ModelContextProtocol.Client;
using Skrapr.Domain.ValueObjects;

#pragma warning disable SKEXP0001

namespace Skrapr.Domain;

public class SemanticKernelAgent(
    IOptions<AzureOpenAiConfiguration> configuration,
    IPlaywrightMcpClient playwrightMcpClient
) : IAgent
{
    private readonly AzureOpenAiConfiguration _configuration = configuration.Value;

    public async Task<WebSiteInspectionResult> Inspect(WebSiteUrl url, UserDataJsonSchema schema, string? instruction)
    {
        var agent =
            new ChatCompletionAgent
            {
                Name = "Skrapr",
                Instructions = Prompts.SystemPrompt,
                Kernel = await BuildKernel(),
                Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new FunctionChoiceBehaviorOptions
                    {
                        AllowStrictSchemaAdherence = true
                    }),
                    ResponseFormat = "json_object"
                })
            };

        var instructions = $"""
                            User instruction: {instruction}
                            Web site: {url}
                            Json schema: 
                            {schema}
                            """;

        var thread = new ChatHistoryAgentThread();
        
        var responses = await agent.InvokeAsync(instructions, thread).ToListAsync();

        var jsonResult = ToJsonElement(responses.Select(x => x.Message).ToList());

        return new WebSiteInspectionResult(jsonResult);
    }

    private async Task<Kernel> BuildKernel()
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.AddAzureOpenAIChatCompletion(
            _configuration.DeploymentName,
            _configuration.Endpoint,
            _configuration.ApiKey
        );

        var kernel = builder.Build();

        kernel.Plugins.AddFromFunctions("Playwright", await GetPlaywrightKernelFunctions());
        
        return kernel;
    }

    private async Task<IReadOnlyCollection<KernelFunction>> GetPlaywrightKernelFunctions()
    {
        var mcpClient = await playwrightMcpClient.BuildMcpClient();
        
        var tools = await mcpClient.ListToolsAsync();

        return tools
            .Select(aiFunction => aiFunction.AsKernelFunction())
            .ToList();
    }

    private static JsonElement ToJsonElement(IReadOnlyList<ChatMessageContent> textResult)
    {
        var utf8JsonReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(textResult.Single().Content!));
        return JsonElement.ParseValue(ref utf8JsonReader);
    }
}