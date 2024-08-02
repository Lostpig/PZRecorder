using Newtonsoft.Json.Linq;
using PZPKRecorder.Localization;
using PZPKRecorder.Data;

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

        JArray? kinds = jobj.Value<JArray>("kinds");
        JArray? records = jobj.Value<JArray>("records");
        JArray? dailies = jobj.Value<JArray>("dailies");
        JArray? dailyweeks = jobj.Value<JArray>("dailyweeks");

        #region add kinds
        if (kinds is null) throw new InvalidDataException("Import file invalid: kinds field not available");
        List<Kind> kindList = new();
        foreach (var kind in kinds)
        {
            Kind k = new Kind();
            k.Id = kind.Value<int>("id");
            k.Name = kind.Value<string>("name")!;
            if (version >= 10001) k.OrderNo = kind.Value<int>("order_no");
            else k.OrderNo = 0;
            kindList.Add(k);
        }
        #endregion

        #region add records
        if (records is null) throw new InvalidDataException("Import file invalid: records field not available");
        List<Record> recordList = new();
        foreach (var record in records)
        {
            Record r = new Record();
            r.Id = record.Value<int>("id");
            r.Name = record.Value<string>("name")!;
            r.Alias = record.Value<string>("alias") ?? "";
            r.Remark = record.Value<string>("remark") ?? "";
            r.Episode = record.Value<int>("episode");
            r.EpisodeCount = record.Value<int>("episode_count");
            r.Kind = record.Value<int>("kind");
            r.PublishYear = record.Value<int>("publish_year");
            r.PublishMonth = record.Value<int>("publish_month");
            r.State = (RecordState)record.Value<int>("state");

            long modifyDateBin = record.Value<long>("modify_date");
            r.ModifyDate = DateTime.FromBinary(modifyDateBin);

            recordList.Add(r);
        }
        #endregion

        #region add daily
        if (dailies is null) throw new InvalidDataException("Import file invalid: dailies field not available");
        List<Daily> dailyList = new();
        foreach (var daily in dailies)
        {
            Daily d = new Daily();
            d.Id = daily.Value<int>("id");
            d.Name = daily.Value<string>("name")!;
            d.Alias = daily.Value<string>("alias") ?? "";
            d.Remark = daily.Value<string>("remark") ?? "";
            d.State = (DailyState)daily.Value<int>("state");

            long modifyDateBin = daily.Value<long>("modify_date");
            d.ModifyDate = DateTime.FromBinary(modifyDateBin);

            if (version >= 10001) d.OrderNo = daily.Value<int>("order_no");
            else d.OrderNo = 0;

            dailyList.Add(d);
        }
        #endregion

        #region add dailyweek
        if (dailyweeks is null) throw new InvalidDataException("Import file invalid: dailyweeks field not available");
        List<Data.DailyWeek> dwList = new();
        foreach (var dailyweek in dailyweeks)
        {
            Data.DailyWeek dw = new();

            dw.MondayDay = dailyweek.Value<int>("monday_day");
            dw.DailyId = dailyweek.Value<int>("daily_id");
            dw.Id = Data.DailyWeek.GenerateId(dw.DailyId, dw.MondayDate);

            dw.Day1 = dailyweek.Value<int>("d1");
            dw.Day2 = dailyweek.Value<int>("d2");
            dw.Day3 = dailyweek.Value<int>("d3");
            dw.Day4 = dailyweek.Value<int>("d4");
            dw.Day5 = dailyweek.Value<int>("d5");
            dw.Day6 = dailyweek.Value<int>("d6");
            dw.Day7 = dailyweek.Value<int>("d7");

            dwList.Add(dw);
        }
        #endregion

        SqlLiteHandler.Instance.ExcuteBatchInsert(kindList, recordList, dailyList, dwList, new List<VariantTable>());
    }


}
