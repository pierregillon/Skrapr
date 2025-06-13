using System.ComponentModel.DataAnnotations;

namespace Skrapr;

public class PlaywrightMcpConfiguration
{
    public const string SectionName = "PlaywrightMcp";
    
    [Required] public string Endpoint { get; set; } = string.Empty;
}