using ModelContextProtocol.Client;

namespace Skrapr.Domain;

public interface IPlaywrightMcpClient
{
    Task<IMcpClient> BuildMcpClient();
}