using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
#pragma warning disable SKEXP0001

namespace Skrapr.Infra.Playwright;

public class RemoteMcpServerPlaywrightConfigurator(IOptions<PlaywrightMcpConfiguration> playwrightConfiguration) 
    : IPlaywrightConfigurator
{
    private readonly PlaywrightMcpConfiguration _playwrightMcpConfiguration = playwrightConfiguration.Value;
    
    public async Task Configure(Kernel kernel)
    {
        var mcpClient = await McpClientFactory.CreateAsync(
            new SseClientTransport(
                new SseClientTransportOptions
                {
                    Endpoint = new Uri(_playwrightMcpConfiguration.Endpoint),
                    Name = "Playwright tools",
                    TransportMode = HttpTransportMode.Sse,
                    ConnectionTimeout = TimeSpan.FromMinutes(1)
                })
        );

        var tools = await mcpClient.ListToolsAsync();

        var kernelFunctions = tools
            .Select(aiFunction => aiFunction.AsKernelFunction())
            .ToList();

        kernel.Plugins.AddFromFunctions("Playwright", kernelFunctions);
    }
}