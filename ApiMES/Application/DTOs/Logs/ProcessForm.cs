using ApiMES.Shared.Utilities;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;

namespace ApiMES.Application.DTOs.Logs;

public class ProcessForm
{
    [BsonElement("name")]
    [BsonSerializer(typeof(StringInt32BoolSerializer))]
    public required string Name { get; set; }

    [BsonElement("value")]
    [BsonSerializer(typeof(StringInt32BoolSerializer))]
    public required string Value { get; set; }

    [BsonIgnore]
    public bool IsJson => TryParseJsonArray(Value, out _);

    [BsonIgnore]
    public JArray? JsonValue => TryParseJsonArray(Value, out var result) ? result : null;

    private static bool TryParseJsonArray(string input, out JArray? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        try
        {
            result = JArray.Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

