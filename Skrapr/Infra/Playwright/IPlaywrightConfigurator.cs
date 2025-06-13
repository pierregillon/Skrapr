using Microsoft.SemanticKernel;

namespace Skrapr.Infra.Playwright;

public interface IPlaywrightConfigurator
{
    Task Configure(Kernel kernel);
}