using ApiMES.Application.Configurations;
using ApiMES.Application.DTOs.Logs;
using ApiMES.Application.Services.Users;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data;
using System.Text;

namespace ApiMES.Infrastructure.DAOs.Mongo
{
    public class MongoDao
    {
        private readonly IMongoCollection<ProcessLog> _collection;
        private readonly AppSettings _settings;
        private readonly ILogger<MongoDao> _logger;
        private readonly UserApplicationService _userService;

        public MongoDao(IOptions<AppSettings> settings, IMongoClient client, ILogger<MongoDao> logger, UserApplicationService userService)
        {
            _settings = settings.Value;
            var database = client.GetDatabase(_settings.MongoDatabase);
            _collection = database.GetCollection<ProcessLog>(_settings.MongoCollection4Form);
            _logger = logger;
            _userService = userService;
        }

        public async Task<List<ProcessDTO>> GetProcessLogsAsync(string processId, string cId = "")
        {
            // 1. Truy vấn MongoDB
            var filter = string.IsNullOrEmpty(cId)
                ? Builders<ProcessLog>.Filter.Eq(e => e.ProcessInstanceId, processId)
                : Builders<ProcessLog>.Filter.In(e => e.ProcessInstanceId, new[] { processId, cId });

            var queryResult = await _collection.Find(filter).ToListAsync();

            if (!queryResult.Any())
                return new List<ProcessDTO>();

            // 2. Tiền xử lý: ánh xạ UserName, TaskName, KeyName, và normalize HistoryField
            var firstNonNullKeyName = queryResult.FirstOrDefault(q => !string.IsNullOrEmpty(q.KeyName))?.KeyName ?? string.Empty;

            foreach (var log in queryResult)
            {
                log.UserName = await GetUserNameAsync(log.UserId);
                log.TaskName ??= "起始表单";
                log.KeyName ??= firstNonNullKeyName;

                if (log.HistoryField != null)
                {
                    foreach (var field in log.HistoryField)
                    {
                        if (field.Value == "False") field.Value = string.Empty;
                    }
                }
            }

            // 3. Nhóm theo tháng và trả về ProcessDTO
            return queryResult
                .OrderByDescending(log => log.Stamp)
                .GroupBy(log => log.FormatStampMonthly)
                .Select(g => new ProcessDTO
                {
                    Month = g.Key,
                    Logs = g.ToList(),
                    KeyName = g.FirstOrDefault()?.KeyName ?? string.Empty
                })
                .ToList();
        }

        private async Task<string> GetUserNameAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return string.Empty;

            // Normalize user ID
            var normalizedId = userId.Contains("fepv", StringComparison.OrdinalIgnoreCase)
                ? userId.ToUpper()
                : userId;

            var employees = await _userService.LoadEmployeesAsync();
            var employee = employees.FirstOrDefault(e => e.EmployeeID == normalizedId);

            return employee?.Name ?? normalizedId;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var database = _collection.Database;
                var collectionName = _collection.CollectionNamespace.CollectionName;
                var dbName = database.DatabaseNamespace.DatabaseName;
                var serverAddress = database.Client.Settings.Server;

                var command = new BsonDocument("ping", 1);
                var result = await database.RunCommandAsync<BsonDocument>(command);

                var logMessage = new StringBuilder();
                logMessage.AppendLine("✅ MongoDB Connection Check:");
                logMessage.AppendLine($"🔹 Server Address: {serverAddress.Host}:{serverAddress.Port}");
                logMessage.AppendLine($"🔹 Database Name : {dbName}");
                logMessage.AppendLine($"🔹 Collection    : {collectionName}");
                logMessage.AppendLine($"🔹 Ping Result   : {result}");

                _logger.LogInformation(logMessage.ToString());
                return result.Contains("ok") && result["ok"] == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<(bool IsConnected, string Message, List<ProcessLog> Data)> CheckMongoAndQueryAsync(string processId)
        {
            try
            {
                var database = _collection.Database;
                var collectionName = _collection.CollectionNamespace.CollectionName;
                var dbName = database.DatabaseNamespace.DatabaseName;
                var serverAddress = database.Client.Settings.Server;

                // Thực hiện truy vấn
                var filter = Builders<ProcessLog>.Filter.Eq(p => p.ProcessInstanceId, processId);
                var result = await _collection.Find(filter).ToListAsync();

                // Log chi tiết
                var logMessage = new StringBuilder();
                logMessage.AppendLine("✅ MongoDB Query Check:");
                logMessage.AppendLine($"🔹 Server Address  : {serverAddress.Host}:{serverAddress.Port}");
                logMessage.AppendLine($"🔹 Database Name   : {dbName}");
                logMessage.AppendLine($"🔹 Collection Name : {collectionName}");
                logMessage.AppendLine($"🔹 Process ID      : {processId}");
                logMessage.AppendLine($"🔹 Records Found   : {result.Count}");

                _logger.LogInformation(logMessage.ToString());
                return (true, logMessage.ToString(), result);
            }
            catch (Exception ex)
            {
                var errorMsg = $"❌ MongoDB Query FAILED: {ex.Message}";
                _logger.LogInformation(errorMsg);
                return (false, errorMsg, new List<ProcessLog>());
            }
        }
    }
}
