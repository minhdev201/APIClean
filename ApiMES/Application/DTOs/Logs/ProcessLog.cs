using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ApiMES.Shared.Utilities;

namespace ApiMES.Application.DTOs.Logs
{
    [BsonIgnoreExtraElements]
    public class ProcessLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string Id { get; set; }

        [BsonElement("key")]
        [BsonSerializer(typeof(StringInt32BoolSerializer))]
        public required string Key { get; set; }

        [BsonElement("keyname")]
        [BsonSerializer(typeof(StringInt32BoolSerializer))]
        public required string KeyName { get; set; }

        [BsonElement("processInstanceId")]
        [BsonSerializer(typeof(StringInt32BoolSerializer))]
        public required string ProcessInstanceId { get; set; }

        [BsonElement("taskid")]
        [BsonSerializer(typeof(StringInt32BoolSerializer))]
        public required string TaskId { get; set; }

        [BsonElement("name")]
        [BsonSerializer(typeof(StringInt32BoolSerializer))]
        public required string TaskName { get; set; }

        [BsonElement("description")]
        [BsonSerializer(typeof(StringInt32BoolSerializer))]
        public required string TaskDescription { get; set; }

        [BsonElement("username")]
        [BsonSerializer(typeof(StringInt32BoolSerializer))]
        public required string UserId { get; set; }

        [BsonElement("userName")]
        [BsonSerializer(typeof(StringInt32BoolSerializer))]
        public required string UserName { get; set; }

        [BsonElement("historyField")]
        public List<ProcessForm> HistoryField { get; set; } = new();

        [BsonElement("sync")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Stamp { get; set; }

        [BsonIgnore]
        public string FormatStamp => Stamp.ToString("yyyy-MM-dd HH:mm:ss");

        [BsonIgnore]
        public string FormatStampMonthly => Stamp.ToString("yyyy-MM");
    }
}
