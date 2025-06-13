using System.ComponentModel.DataAnnotations;

namespace Skrapr.Infra.Playwright;

public class PlaywrightMcpConfiguration
{
    public const string SectionName = "PlaywrightMcp";
    
    [Required] public string Endpoint { get; set; } = string.Empty;
}