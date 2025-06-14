using System.ComponentModel.DataAnnotations;

namespace Skrapr.Infra.Playwright;

public class PlaywrightMcpConfiguration
{
    public const string SectionName = "PlaywrightMcp";
    
    public string Endpoint { get; set; } = string.Empty;
    public bool IsLocal { get; set; } = false;

    public void EnsureValid()
    {
        var isValid = IsLocal && string.IsNullOrWhiteSpace(Endpoint) || !IsLocal && !string.IsNullOrWhiteSpace(Endpoint);
        
        if (!isValid)
        {
            throw new ValidationException("Playwright must be local OR having an endpoint.");
        }
    }
}