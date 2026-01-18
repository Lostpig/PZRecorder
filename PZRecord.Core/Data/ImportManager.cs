using PZRecorder.Core.Common;
using PZRecorder.Core.Tables;
using SQLite;
using System.Reflection;
using System.Text.Json.Nodes;

namespace PZRecorder.Core.Data;

public class ImportManager(SqlHandler sql)
{
    private SqlHandler Sql { get; init; } = sql;

    public void ImportFromJson(string jsonFile, string? backup)
    {
        if (!File.Exists(jsonFile))
        {
            throw new Exception($"Import file \"{jsonFile}\" not exists");
        }

        var datas = ImportFromJsonFile(jsonFile);

        if (!string.IsNullOrEmpty(backup))
        {
            Sql.BackupTo(backup);
        }
        Sql.ResetDB();
        ExcuteImportData(datas);
    }
    public void ImportFromDB(string dbFile, string? backup)
    {
        if (!File.Exists(dbFile))
        {
            throw new Exception($"Import file \"{dbFile}\" not exists");
        }

        var datas = ImportFromDBFile(dbFile);
        if (!string.IsNullOrEmpty(backup))
        {
            Sql.BackupTo(backup);
        }
        Sql.ResetDB();
        ExcuteImportData(datas);
    }

    private void ExcuteImportData(ImportData[] datas)
    {
        Sql.Conn.RunInTransaction(() =>
        {
            foreach (var data in datas)
            {
                foreach (var values in data.ValueTexts)
                {
                    var cmd = $"insert into \"{data.TableName}\" ({data.ColumnsText}) values ({values})";
                    Sql.Conn.Execute(cmd);
                }
                if (data.HasAutoIncrement && data.MaxId > 0)
                {
                    string seqCmd = $"update sqlite_sequence set seq = {data.MaxId} where name = '{data.TableName}'";
                    Sql.Conn.Execute(seqCmd);
                }
            }
        });
    }

    private static ImportData[] ImportFromJsonFile(string path)
    {
        string jsonStr = File.ReadAllText(path);
        JsonNode root = JsonNode.Parse(jsonStr) ?? throw new NotSupportedException("Import file is not a json file");

        int version = root[Constant.DBVersionKey]?.GetValue<int>() ?? int.MaxValue;
        if (version > Constant.DBVersion)
        {
            throw new NotSupportedException("Not support json file to import");
        }

        // create import datas
        var tables = Index.AvailableTables();
        var importDatas = tables.Select(t => CreateImportList(root, t, version));

        return importDatas.ToArray();
    }
    private static ImportData CreateImportList(JsonNode root, TableMeta table, int dbVersion)
    {
        Type t = table.TableType;
        TableVersionAttribute? tverAttr = t.GetCustomAttribute<TableVersionAttribute>();
        if (tverAttr is not null)
        {
            if (tverAttr.MaxVersion < dbVersion || tverAttr.MinVersion > dbVersion)
            {
                return ImportData.Empty;
            }
        }
        JsonArray jTable = root[table.TableName]?.AsArray() ?? throw new InvalidDataException($"Import file invalid: table {table.TableName} not available");

        ImportData importData = new(table.SQLTabelName);
        List<ImportColumn> columns = CreateImportColumns(t, dbVersion, importData);
        importData.ColumnsText = string.Join(", ", columns.Select(c => c.ColumnName));

        foreach (var item in jTable)
        {
            if (item is null) continue;

            string values = string.Join(", ", columns.Select(c => c.CompatibleVersion ? GetValueText(item, c) : c.DefaultValue));
            if (importData.HasAutoIncrement)
            {
                int newId = item[importData.PK]?.GetValue<int>() ?? 0;
                importData.MaxId = MaxId(importData.MaxId, newId);
            }
            importData.ValueTexts.Add(values);
        }

        return importData;
    }

    private static ImportData[] ImportFromDBFile(string path)
    {
        var importConn = new SQLiteConnection(path);
        var vt = importConn.Get<VariantTable>(Constant.DBVersionKey);
        int version = vt != null ? int.Parse(vt.Value) : 0;

        if (version > Constant.DBVersion)
        {
            throw new NotSupportedException("Not support db version to import");
        }

        var tables = Index.AvailableTables();
        var importDatas = tables.Select(t => CreateImportList(importConn, t, version));

        return importDatas.ToArray();
    }
    private static ImportData CreateImportList(SQLiteConnection conn, TableMeta table, int dbVersion)
    {
        Type t = table.TableType;

        TableVersionAttribute? tverAttr = t.GetCustomAttribute<TableVersionAttribute>();
        if (tverAttr is not null)
        {
            if (tverAttr.MaxVersion < dbVersion || tverAttr.MinVersion > dbVersion)
            {
                return ImportData.Empty;
            }
        }

        ImportData importData = new(table.SQLTabelName);
        List<ImportColumn> columns = CreateImportColumns(t, dbVersion, importData);
        importData.ColumnsText = string.Join(", ", columns.Select(c => c.ColumnName));

        TableMapping mapping = new(t, CreateFlags.None);
        var items = conn.Query(mapping, $"SELECT * from {table.SQLTabelName}");
        ImportColumn? pkColumn = null;
        if (importData.HasAutoIncrement)
        {
            pkColumn = columns.Find(c => c.ColumnName == importData.PK);
        }

        foreach (var item in items)
        {
            string values = string.Join(", ", columns.Select(c => c.CompatibleVersion ? GetTableValueText(c.ColumnProp, item) : c.DefaultValue));
            if (pkColumn != null)
            {
                var newId = pkColumn.ColumnProp.GetValue(item);
                if (newId is int id)
                {
                    importData.MaxId = MaxId(importData.MaxId, id);
                }
            }
            
            importData.ValueTexts.Add(values);
        }

        return importData;
    }

    private static List<ImportColumn> CreateImportColumns(Type t, int dbVersion, ImportData importData)
    {
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

            ImportColumn column = new(colAttr.Name, pi, compatibleVersion, defaultValue);
            columns.Add(column);

            bool isPK = pi.GetCustomAttribute<PrimaryKeyAttribute>() is not null;
            bool isAutoInc = pi.GetCustomAttribute<AutoIncrementAttribute>() is not null;
            if (isPK && isAutoInc)
            {
                importData.PK = colAttr.Name;
                importData.HasAutoIncrement = true;
            }
        }

        return columns;
    }
    private static int MaxId(int currentMaxId, int newId)
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

    private static string GetValueText(JsonNode node, ImportColumn column)
    {
        var columnType = column.ColumnProp.PropertyType;
        // int string datetime enum
        if (columnType.IsEnum)
        {
            return node.IntValueText(column.ColumnName);
        }

        if (columnType == typeof(int))
        {
            return node.IntValueText(column.ColumnName);
        }

        if (columnType == typeof(string))
        {
            return node.StringValueText(column.ColumnName);
        }

        if (columnType == typeof(DateTime))
        {
            return node.DateTimeValueText(column.ColumnName);
        }

        if (columnType == typeof(bool))
        {
            return node.BoolValueText(column.ColumnName);
        }

        throw new NotSupportedException($"Not support property type {columnType.FullName} in import");
    }
    private static string GetValueText(PropertyInfo pi, object value)
    {
        if (pi.PropertyType.IsEnum)
        {
            return ((int)value).ToString();
        }
        if (pi.PropertyType == typeof(int))
        {
            return ((int)value).ToString();
        }
        if (pi.PropertyType == typeof(string))
        {
            return $"\"{(string)value}\"";
        }
        if (pi.PropertyType == typeof(DateTime))
        {
            return ((DateTime)value).Ticks.ToString();
        }
        if (pi.PropertyType == typeof(bool))
        {
            return (((bool)value) ? 1 : 0).ToString();
        }

        throw new NotSupportedException($"Not support property type {pi.PropertyType.FullName} in import");
    }
    private static string GetTableValueText(PropertyInfo pi, object tableObj)
    {
        var value = pi.GetValue(tableObj, null);
        return GetValueText(pi, value!);
    }
}
