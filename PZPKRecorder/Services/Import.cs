using Newtonsoft.Json.Linq;
using PZPKRecorder.Data;
using PZPKRecorder.Localization;
using SQLite;
using System.Reflection;

namespace PZPKRecorder.Services;

internal class ImportData
{
    public ImportData(string tableName)
    {
        PK = "";
        TableName = tableName;
    }
    public string TableName { get; init; }
    public List<string> Commands { get; init; } = [];
    public string PK { get; set; }
    public bool HasAutoIncrement { get; set; } = false;
    public int MaxId { get; set; } = 0;
}

internal class ImportColumn
{
    public string ColumnName { get; init; }
    public Type ColumnType { get; init; }
    public bool CompatibleVersion { get; init; }
    public string DefaultValue { get; init; }

    public ImportColumn(string columnName, Type columnType, bool compatibleVersion, string defaultValue)
    {
        ColumnName = columnName;
        ColumnType = columnType;
        CompatibleVersion = compatibleVersion;
        DefaultValue = defaultValue;
    }
}

internal class ImportService
{
    public static bool Import()
    {
        OpenFileDialog dlg = new()
        {
            Title = LocalizeDict.Import,
            Filter = "JSON Files (*.json)|*.json"
        };

        var result = dlg.ShowDialog();
        if (result == DialogResult.OK)
        {
            string filename = dlg.FileName;
            if (!File.Exists(filename))
            {
                throw new Exception($"Import file \"{filename}\" not exists");
            }
            ImportFromJsonFile(filename);

            return true;
        }
        else
        {
            return false;
        }
    }

    private static void ImportFromJsonFile(string path)
    {
        string jsonStr = File.ReadAllText(path);
        JObject jobj = JObject.Parse(jsonStr);

        int version = jobj.Value<int>("dbversion");
        if (!Helper.IsCompatibleVersion(version))
        {
            throw new NotSupportedException("Not support json file to import");
        }

        // backup before import
        SqlLiteHandler.Instance.BackupDB(SqlLiteHandler.DBPath);
        SqlLiteHandler.Instance.ResetDB();

        // create import datas
        List<ImportData> importDatas = [];
        importDatas.Add(CreateImportList<Kind>(jobj, "kinds", version));
        importDatas.Add(CreateImportList<Record>(jobj, "records", version));
        importDatas.Add(CreateImportList<Daily>(jobj, "dailies", version));
        importDatas.Add(CreateImportList<DailyWeek>(jobj, "dailyweeks", version));
        importDatas.Add(CreateImportList<ClockIn>(jobj, "clockin", version));
        importDatas.Add(CreateImportList<ClockInRecord>(jobj, "clockinrecords", version));
        importDatas.Add(CreateImportList<ProcessWatch>(jobj, "processwatches", version));
        importDatas.Add(CreateImportList<ProcessRecord>(jobj, "processrecords", version));

        ExcuteImportData(importDatas);

        // after import
        BroadcastService.Broadcast(BroadcastEvent.RemindStateChanged, string.Empty);
        ProcessWatchService.UpdateWatcher();
    }

    private static ImportData CreateImportList<T>(JObject jobj, string tableName, int dbVersion) where T : new()
    {
        Type t = typeof(T);
        SQLite.TableAttribute? slTable = t.GetCustomAttribute<SQLite.TableAttribute>() ?? throw new InvalidOperationException($"Type {t.FullName} is not a SQLite table");
        JArray? jTable = jobj.Value<JArray>(tableName) ?? throw new InvalidDataException($"Import file invalid: table {tableName} not available");

        ImportData importData = new(slTable.Name);
        TableVersionAttribute? tverAttr = t.GetCustomAttribute<TableVersionAttribute>();
        if (tverAttr is not null)
        {
            if (tverAttr.MaxVersion < dbVersion || tverAttr.MinVersion > dbVersion)
            {
                return importData;
            }
        }

        List<ImportColumn> columns = [];
        foreach (PropertyInfo pi in t.GetProperties())
        {
            ColumnAttribute? colAttr = pi.GetCustomAttribute<ColumnAttribute>();
            if (colAttr is null) continue;

            IgnoreAttribute? ignAttr = pi.GetCustomAttribute<IgnoreAttribute>();
            if (ignAttr is not null) continue;

            bool compatibleVersion = true;
            string defaultValue = "NULL";
            FieldVersionAttribute? verAttr = pi.GetCustomAttribute<FieldVersionAttribute>();
            if (verAttr is not null)
            {
                if (verAttr.MaxVersion < dbVersion || verAttr.MinVersion > dbVersion)
                {
                    compatibleVersion = false;
                    defaultValue = GetValueText(pi, verAttr.DefaultValue);
                }
            }

            ImportColumn column = new(colAttr.Name, pi.PropertyType, compatibleVersion, defaultValue);
            columns.Add(column);

            bool isPK = pi.GetCustomAttribute<PrimaryKeyAttribute>() is not null;
            bool isAutoInc = pi.GetCustomAttribute<AutoIncrementAttribute>() is not null;
            if (isPK && isAutoInc)
            {
                importData.PK = colAttr.Name;
                importData.HasAutoIncrement = true;
            }
        }

        string columnNames = string.Join(", ", columns.Select(c => c.ColumnName));
        foreach (var item in jTable)
        {
            string values = string.Join(", ", columns.Select(c => c.CompatibleVersion ? GetValueText(item, c.ColumnType, c.ColumnName) : c.DefaultValue));
            var commandText = string.Format($"insert into \"{slTable.Name}\" ({columnNames}) values ({values})");

            if (importData.HasAutoIncrement)
            {
                int newId = item.Value<int>(importData.PK);
                importData.MaxId = UpdateMaxId(importData.MaxId, newId);
            }
            importData.Commands.Add(commandText);
        }

        return importData;
    }

    private static void ExcuteImportData(List<ImportData> datas)
    {
        SqlLiteHandler.Instance.DB.RunInTransaction(() =>
        {
            foreach (var data in datas)
            {
                foreach (var cmd in data.Commands)
                {
                    SqlLiteHandler.Instance.DB.Execute(cmd);
                }
                if (data.HasAutoIncrement && data.MaxId > 0)
                {
                    string seqCmd = $"update sqlite_sequence set seq = {data.MaxId} where name = '{data.TableName}'";
                    SqlLiteHandler.Instance.DB.Execute(seqCmd);
                }
            }
        });
    }

    private static int UpdateMaxId(int currentMaxId, int newId)
    {
        if (newId > currentMaxId)
        {
            return newId;
        }
        else
        {
            return currentMaxId;
        }
    }

    private static string GetValueText(JToken jtk, Type columnType, string columnName)
    {
        // int string datetime enum
        if (columnType.IsEnum)
        {
            return jtk.Value<int>(columnName).ToString();
        }

        if (columnType == typeof(int))
        {
            return jtk.Value<int>(columnName).ToString();
        }

        if (columnType == typeof(string))
        {
            return $"\"{jtk.Value<string>(columnName)}\"";
        }

        if (columnType == typeof(DateTime))
        {
            long modifyDateBin = jtk.Value<long>(columnName);
            return DateTime.FromBinary(modifyDateBin).Ticks.ToString();
        }

        if (columnType == typeof(bool))
        {
            bool value = jtk.Value<bool>(columnName);
            return (value ? 1 : 0).ToString();
        }

        throw new NotSupportedException($"Not support property type {columnType.FullName} in import");
    }

    private static string GetValueText(PropertyInfo pi, object defaultValue)
    {
        if (pi.PropertyType.IsEnum)
        {
            return ((int)defaultValue).ToString();
        }
        if (pi.PropertyType == typeof(int))
        {
            return ((int)defaultValue).ToString();
        }
        if (pi.PropertyType == typeof(string))
        {
            return $"\"{(string)defaultValue}\"";
        }
        if (pi.PropertyType == typeof(DateTime))
        {
            return ((DateTime)defaultValue).Ticks.ToString();
        }
        if (pi.PropertyType == typeof(bool))
        {
            return (((bool)defaultValue) ? 1 : 0).ToString();
        }

        throw new NotSupportedException($"Not support property type {pi.PropertyType.FullName} in import");
    }
}
