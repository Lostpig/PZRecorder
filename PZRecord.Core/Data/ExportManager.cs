using PZRecorder.Core.Common;
using PZRecorder.Core.Tables;
using SQLite;
using System.Reflection;
using System.Text.Json.Nodes;

namespace PZRecorder.Core.Data;

public class ExportManager(SqlHandler sql)
{
    private SqlHandler Sql { get; init; } = sql;
    public void ExportJson(string jsonFile, bool indented)
    {
        if (File.Exists(jsonFile))
        {
            throw new Exception($"Export file \"{jsonFile}\" already exists");
        }

        string json = CreateExportJsonData(Sql.Conn, indented);
        File.WriteAllText(jsonFile, json);
    }
    public void ExportDB(string dbFile)
    {
        if (File.Exists(dbFile))
        {
            throw new Exception($"Export file \"{dbFile}\" already exists");
        }

        Sql.BackupTo(dbFile);
    }

    private static string CreateExportJsonData(SQLiteConnection conn, bool indented)
    {
        JsonObject jsonObject = [];

        // db version
        var vt = conn.Get<VariantTable>(Constant.DBVersionKey);
        int version = vt != null ? int.Parse(vt.Value) : 0;
        jsonObject.Add(Constant.DBVersionKey, version);

        var tables = Index.AvailableTables();
        foreach (var table in tables)
        {
            var mapping = new TableMapping(table.TableType, CreateFlags.None);
            var items = conn.Query(mapping, $"SELECT * FROM {table.SQLTabelName}");
            var jarr = CreateExportJsonArray(items, table.TableType);

            jsonObject.Add(table.TableName, jarr);
        }

        return jsonObject.ToJsonString(new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = indented
        });
    }
    private static JsonArray CreateExportJsonArray(List<object> list, Type tableType)
    {
        JsonArray jarr = [];

        foreach (var item in list)
        {
            JsonObject jitem = [];

            foreach (PropertyInfo pi in tableType.GetProperties())
            {
                var attr = pi.GetCustomAttribute<ColumnAttribute>();
                if (attr != null)
                {
                    string columnName = attr.Name;
                    var val = pi.GetValue(item);
                    SetValue(jitem, columnName, pi, val);
                }
            }

            jarr.Add(jitem);
        }

        return jarr;
    }
    private static void SetValue(JsonObject jobj, string columnName, PropertyInfo prop, object? value)
    {

        if (value == null)
        {
            jobj.Add(columnName, null);
            return; 
        }

        var columnType = prop.PropertyType;
        // int string datetime enum
        if (columnType.IsEnum)
        {
            jobj.Add(columnName, (int)value!);
            return;
        }

        if (columnType == typeof(int))
        {
            jobj.Add(columnName, (int)value!);
            return;
        }

        if (columnType == typeof(string))
        {
            jobj.Add(columnName, value.ToString());
            return;
        }

        if (columnType == typeof(DateTime))
        {
            var bin = ((DateTime)value).ToBinary();
            jobj.Add(columnName, bin);
            return;
        }

        if (columnType == typeof(bool))
        {
            jobj.Add(columnName, (bool)value!);
            return;
        }

        throw new NotSupportedException($"Not support property type {columnType.FullName} in export");
    }
}
