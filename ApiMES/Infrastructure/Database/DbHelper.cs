using System.Data;
using System.Reflection;
using System.Text.Json;

namespace ApiMES.Infrastructure.Database
{
    public class DbHelper
    {
        public List<T> ConvertList<T>(DataTable dt) where T : new()
        {
            var result = new List<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (DataRow row in dt.Rows)
            {
                var item = new T();
                foreach (var prop in properties)
                {
                    if (!row.Table.Columns.Contains(prop.Name) || !prop.CanWrite)
                        continue;

                    var value = row[prop.Name];
                    if (value is DBNull) continue;

                    try
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        var safeValue = Convert.ChangeType(value, targetType);
                        prop.SetValue(item, safeValue);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Error converting property '{prop.Name}': {ex.Message}", ex);
                    }
                }

                result.Add(item);
            }

            return result;
        }

        public object ConvertJson(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                rows.Add(dict);
            }

            var json = JsonSerializer.Serialize(rows, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            return JsonSerializer.Deserialize<object>(json)!;
        }

        public List<string> GetPropertyList(object obj)
        {
            var propertyList = new List<string>();
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);

                var formatted = value switch
                {
                    null => "",
                    bool b => b ? "是" : "否",
                    _ => value?.ToString() ?? ""
                };

                propertyList.Add(formatted);
            }

            return propertyList;
        }

        public string NormalizeUsername(string username)
        {
            return username.Contains("fepv", StringComparison.OrdinalIgnoreCase)
                ? username.ToUpper()
                : username;
        }
    }
}
