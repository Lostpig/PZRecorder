using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using PZPKRecorder.Localization;

namespace PZPKRecorder.Services;

internal class ExportService
{
    public static string? Export(bool indented)
    {
        SaveFileDialog dlg = new()
        {
            Title = LocalizeDict.Export,
            FileName = $"export-{DateTime.Now:yyyyMMddHHmmss}.json",
            Filter = "JSON Files (*.json)|*.json"
        };

        var result = dlg.ShowDialog();
        if (result == DialogResult.OK)
        {
            string filename = dlg.FileName;
            if (File.Exists(filename))
            {
                throw new Exception($"Export file \"{filename}\" already exists");
            }

            Formatting formatting = indented ? Formatting.Indented : Formatting.None;
            string json = CreateExportData(formatting);
            File.WriteAllText(filename, json);

            return filename;
        }
        else
        {
            return null;
        }
    }

    private static string CreateExportData(Formatting formatting)
    {
        JObject jobj = [];
        
        // db version
        var dbVersion = VariantService.GetVariant(SqlLiteHandler.dbVersionKey);
        int dbv = int.Parse(dbVersion ?? "0");
        jobj.Add(new JProperty("dbversion", dbv));
        // kinds
        var kinds = KindService.GetKinds();
        jobj.Add(new JProperty("kinds", CreateExportJsonArray(kinds)));
        // records
        var records = RecordService.GetAllRecords();
        jobj.Add(new JProperty("records", CreateExportJsonArray(records)));
        // daily & daily_of_month
        var dailies = DailyService.GetDailies(null);
        jobj.Add(new JProperty("dailies", CreateExportJsonArray(dailies)));
        var dailyweeks = DailyService.GetDailyDatas(null);
        jobj.Add(new JProperty("dailyweeks", CreateExportJsonArray(dailyweeks)));

        return jobj.ToString(formatting);
    }
    private static JArray CreateExportJsonArray<T>(IList<T> list)
    {
        Type t = typeof(T);
        JArray jarr = [];

        foreach (T item in list)
        {
            JObject jitem = [];

            foreach (PropertyInfo pi in t.GetProperties())
            {
                Attribute? attr = pi.GetCustomAttribute(typeof(SQLite.ColumnAttribute));
                if (attr is SQLite.ColumnAttribute column)
                {
                    string columnName = column.Name;
                    var val = pi.GetValue(item);
                    if (val is DateTime dt)
                    {
                        val = dt.ToBinary();
                    }

                    jitem.Add(new JProperty(columnName, val));
                }
            }

            jarr.Add(jitem);
        }

        return jarr;
    }
}
