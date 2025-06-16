using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Globalization;

namespace ApiMES.Shared.Utilities;

public class StringInt32BoolSerializer : SerializerBase<string>
{
    public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var reader = context.Reader;
        var bsonType = reader.GetCurrentBsonType();

        return bsonType switch
        {
            BsonType.Null => ReadNull(reader),
            BsonType.String => reader.ReadString(),
            BsonType.Int32 => reader.ReadInt32().ToString(CultureInfo.InvariantCulture),
            BsonType.Boolean => reader.ReadBoolean().ToString(CultureInfo.InvariantCulture),
            BsonType.Double => reader.ReadDouble().ToString(CultureInfo.InvariantCulture),
            _ => throw new BsonSerializationException(
                $"Cannot deserialize string from BsonType {bsonType}.")
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
    {
        var writer = context.Writer;

        if (value != null)
        {
            writer.WriteString(value);
        }
        else
        {
            writer.WriteNull();
        }
    }

    private static string ReadNull(IBsonReader reader)
    {
        reader.ReadNull();
        return string.Empty;
    }
}
