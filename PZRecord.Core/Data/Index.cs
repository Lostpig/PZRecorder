using PZRecorder.Core.Tables;
using System.Reflection;

namespace PZRecorder.Core.Data;

internal class ImportData
{
    public ImportData(string tableName)
    {
        PK = "";
        TableName = tableName;
    }
    public string TableName { get; init; }
    public List<string> ValueTexts { get; init; } = [];
    public string ColumnsText { get; set; } = "";

    public string PK { get; set; }
    public bool HasAutoIncrement { get; set; } = false;
    public int MaxId { get; set; } = 0;

    public readonly static ImportData Empty = new("");
}

internal class ImportColumn
{
    public string ColumnName { get; init; }
    public PropertyInfo ColumnProp { get; init; }
    public bool CompatibleVersion { get; init; }
    public string DefaultValue { get; init; }

    public ImportColumn(string columnName, PropertyInfo columnProp, bool compatibleVersion, string defaultValue)
    {
        ColumnName = columnName;
        ColumnProp = columnProp;
        CompatibleVersion = compatibleVersion;
        DefaultValue = defaultValue;
    }
}

internal class TableMeta(Type type, string name)
{
    public string TableName { get; init; } = name;
    public Type TableType { get; init; } = type;
    public string SQLTabelName { get; init; } = GetSQLTableName(type);

    private static string GetSQLTableName(Type t)
    {
        var attr = t.GetCustomAttribute<SQLite.TableAttribute>() ?? throw new Exception($"Type {t.Name} is not a SQLite Table");
        return attr.Name;
    }
}

internal class Index
{
    private static List<TableMeta>? _tables;
    private static List<TableMeta> Tables
    {
        get
        {
            if (_tables == null)
            {
                _tables = [
                    new(typeof(Kind), "kinds"),
                    new(typeof(Record), "records"),
                    new(typeof(Daily), "dailies"),
                    new(typeof(DailyWeek), "dailyweeks"),
                    new(typeof(ClockIn), "clockin"),
                    new(typeof(ClockInRecord), "clockinrecords"),
                    new(typeof(ProcessWatch), "processwatches"),
                    new(typeof(ProcessRecord), "processrecords"),
                    new(typeof(TodoList), "todolist"),
                    new(typeof(CycleTaskKind), "cycletaskkind"),
                    new(typeof(CycleTask), "cycletask"),
                    new(typeof(CycleTaskItem), "cycletaskitem"),
                ];
            }
            return _tables;
        }
    }

    public static List<TableMeta> AvailableTables()
    {
        return Tables;
    }
}
