using Newtonsoft.Json.Linq;
using PZPKRecorder.Data;
using PZPKRecorder.Localization;
using SQLite;
using System.Reflection;

namespace PZPKRecorder.Services;

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

        var kinds = CreateImportList<Kind>(jobj, "kinds", version);
        var records = CreateImportList<Record>(jobj, "records", version);
        var dailies = CreateImportList<Daily>(jobj, "dailies", version);
        var dailyweeks = CreateImportList<Data.DailyWeek>(jobj, "dailyweeks", version);

        var clockIns = version >= 10005 ? CreateImportList<ClockIn>(jobj, "clockin", version) : [];
        var clockInRecords = version >= 10005 ? CreateImportList<ClockInRecord>(jobj, "clockinrecords", version) : [];

        var processWatches = version >= 10007 ? CreateImportList<ProcessWatch>(jobj, "processwatches", version) : [];
        var processRecords = version >= 10007 ? CreateImportList<ProcessRecord>(jobj, "processrecords", version) : [];

        SqlLiteHandler.Instance.ExcuteBatchInsert(
            kinds, records,
            dailies, dailyweeks,
            clockIns, clockInRecords,
            processWatches, processRecords,
            []);

        // after import
        BroadcastService.Broadcast(BroadcastEvent.RemindStateChanged, string.Empty);
        ProcessWatchService.UpdateWatcher();
    }

    private static IList<T> CreateImportList<T>(JObject jobj, string tableName, int dbVersion) where T : new()
    {
        Type t = typeof(T);
        TableVersionAttribute? tverAttr = t.GetCustomAttribute<TableVersionAttribute>();
        if (tverAttr is not null)
        {
            if (tverAttr.MaxVersion < dbVersion || tverAttr.MinVersion > dbVersion)
            {
                return [];
            }
        }

        JArray? jTable = jobj.Value<JArray>(tableName);
        if (jTable is null) throw new InvalidDataException($"Import file invalid: table {tableName} not available");

        List<T> list = new();
        foreach (var item in jTable)
        {
            T instance = new T();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                ColumnAttribute? colAttr = pi.GetCustomAttribute<ColumnAttribute>();
                if (colAttr is null) continue;

                FieldVersionAttribute? verAttr = pi.GetCustomAttribute<FieldVersionAttribute>();
                if (verAttr is not null)
                {
                    if (verAttr.MaxVersion < dbVersion || verAttr.MinVersion > dbVersion)
                    {
                        pi.SetValue(instance, verAttr.DefaultValue);
                        continue;
                    }
                }

                SetDataValue(instance, item, pi, colAttr.Name);
            }

            list.Add(instance);
        }

        return list;
    }

    private static void SetDataValue(object instance, JToken jtk, PropertyInfo pi, string columnName)
    {
        // int string datetime enum

        if (pi.PropertyType.IsEnum)
        {
            var val = Enum.ToObject(pi.PropertyType, jtk.Value<int>(columnName));
            pi.SetValue(instance, val);
            return;
        }

        if (pi.PropertyType == typeof(int))
        {
            pi.SetValue(instance, jtk.Value<int>(columnName));
            return;
        }

        if (pi.PropertyType == typeof(string))
        {
            pi.SetValue(instance, jtk.Value<string>(columnName));
            return;
        }

        if (pi.PropertyType == typeof(DateTime))
        {
            long modifyDateBin = jtk.Value<long>(columnName);
            pi.SetValue(instance, DateTime.FromBinary(modifyDateBin));
            return;
        }
    }
}
