using System.ComponentModel.DataAnnotations;

namespace Skrapr.Domain;

public class AzureOpenAiConfiguration
{
    public const string SectionName = "AzureOpenAI";

    [Required] public string DeploymentName { get; set; } = string.Empty;

    [Required] public string Endpoint { get; set; } = string.Empty;

    [Required] public string ApiKey { get; set; } = string.Empty;
}