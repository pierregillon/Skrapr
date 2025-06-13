using Json.Schema;

namespace Skrapr;

public record UserDataJsonSchema
{
    private readonly string _jsonSchema;
    public JsonSchema Value { get; }
    
    public UserDataJsonSchema(string jsonSchema)
    {
        _jsonSchema = jsonSchema;
        try
        {
            var schema = JsonSchema.FromText(jsonSchema);
            if (schema.Keywords?.Count == 0 || schema.GetProperties()?.Count == 0)
            {
                throw new ArgumentException("The json schema is empty. At least one property is required.", nameof(jsonSchema));
            }
            Value = schema;
        }
        catch (System.Text.Json.JsonException e)
        {
            throw new ArgumentException("The json schema is malformed.", nameof(jsonSchema), e);
        }
    }
    
    public override string ToString() => _jsonSchema;
}