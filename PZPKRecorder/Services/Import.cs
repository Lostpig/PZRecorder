using Newtonsoft.Json.Linq;
using PZPKRecorder.Localization;
using PZPKRecorder.Data;
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
        switch (version)
        {
            case 0:
            case 10001:
            case 10002:
            case Helper.DataVersion:
                break;
            default: throw new NotSupportedException("Not support json file to import");
        }

        // backup before import
        SqlLiteHandler.Instance.BackupDB(SqlLiteHandler.dbPath);
        SqlLiteHandler.Instance.ResetDB();

        var kinds = CreateImportList<Kind>(jobj, "kinds", version);
        var records = CreateImportList<Record>(jobj, "records", version);
        var dailies = CreateImportList<Daily>(jobj, "dailies", version);
        var dailyweeks = CreateImportList<Data.DailyWeek>(jobj, "dailyweeks", version);

        SqlLiteHandler.Instance.ExcuteBatchInsert(kinds, records, dailies, dailyweeks, new List<VariantTable>());
    }

    private static IList<T> CreateImportList<T>(JObject jobj, string tableName, int dbVersion) where T : new()
    {
        JArray? jTable = jobj.Value<JArray>(tableName);
        if (jTable is null) throw new InvalidDataException("Import file invalid: kinds field not available");

        List<T> list = new();
        Type t = typeof(T);

        foreach (var item in jTable)
        {
            T instance = new T();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                SQLite.ColumnAttribute? colAttr = pi.GetCustomAttribute<SQLite.ColumnAttribute>();
                if (colAttr is null) continue;

                DataFieldAttribute? dfAttr = pi.GetCustomAttribute<DataFieldAttribute>();
                if (dfAttr is not null)
                {
                    if (dfAttr.MaxVersion < dbVersion || dfAttr.MinVersion > dbVersion) 
                    {
                        pi.SetValue(instance, dfAttr.DefaultValue);
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
