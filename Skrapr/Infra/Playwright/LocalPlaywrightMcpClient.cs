using ModelContextProtocol.Client;
using Skrapr.Domain;

namespace Skrapr.Infra.Playwright;

public class LocalPlaywrightMcpClient : IPlaywrightMcpClient
{
    public Task<IMcpClient> BuildMcpClient() =>
        McpClientFactory.CreateAsync(
            new StdioClientTransport(
                new StdioClientTransportOptions
                {
                    Name = "Playwright stdio",
                    Command = "npx",
                    Arguments = ["@playwright/mcp@latest"]
                })
        );
}