using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using Skrapr.Domain;

namespace Skrapr.Infra.Playwright;

public class RemotePlaywrightMcpClient(IOptions<PlaywrightMcpConfiguration> playwrightConfiguration)
    : IPlaywrightMcpClient
{
    private readonly PlaywrightMcpConfiguration _playwrightMcpConfiguration = playwrightConfiguration.Value;

    public Task<IMcpClient> BuildMcpClient() =>
        McpClientFactory.CreateAsync(
            new SseClientTransport(
                new SseClientTransportOptions
                {
                    Endpoint = new Uri(_playwrightMcpConfiguration.Endpoint),
                    Name = "Playwright sse",
                    TransportMode = HttpTransportMode.Sse,
                    ConnectionTimeout = TimeSpan.FromMinutes(1)
                })
        );
}