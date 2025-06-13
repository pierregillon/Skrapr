using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Json.Schema;
using Microsoft.Extensions.Options;

namespace Skrapr.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Extract_correct_data()
    {
        var tool = new ExtractWebSiteDataTool(
            new OptionsWrapper<AzureOpenAiConfiguration>(new AzureOpenAiConfiguration
            {
                DeploymentName = "",
                Endpoint = "",
                ApiKey = ""
            }),
            new OptionsWrapper<PlaywrightMcpConfiguration>(new PlaywrightMcpConfiguration
            {
                Endpoint = "http://localhost:8931"
            })
        );

        var jsonSchema = JsonDocument.Parse(
            """
            {
                "type": "object",
                "properties": {
                    "description": {
                        "type": "string"
                    },
                    "address": {
                        "type": "string"
                    },
                    "tickets": {
                        "type": "array",
                        "items": {
                            "type": "object",
                            "properties": {
                                "name": {
                                    "type": "string"
                                },
                                "short_description": {
                                    "type": "string"
                                },
                                "details": {
                                    "type": "string"
                                },
                                "price": {
                                    "type": "object",
                                    "properties": {
                                        "amount": {
                                            "type": "number"
                                        },
                                        "currency": {
                                            "type": "string",
                                            "description": "The currency of the price. In ISO 4217 format, for example USD, EUR, JPY, etc."
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            """);

        var webPageParsingResult = await tool.ParseWebPage(
            "https://snowcamp.io", 
            jsonSchema.RootElement.ToString(), 
            "Extract data in French or translate them in French."
        );

        webPageParsingResult
            .Data
            .Should()
            .NotBeNull();
        
        var schema = JsonSchema.FromText(jsonSchema.RootElement.ToString());

        var result = schema.Evaluate(webPageParsingResult.Data);

        result.IsValid.Should().BeTrue(result.Errors?.First().Value);
        
    }
}