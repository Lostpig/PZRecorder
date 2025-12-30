using PZRecorder.Core;
using PZRecorder.Core.Tables;


namespace PZRecorder.Core.Managers;

public class ProcessMonitorManager(SqlHandler db)
{
    private SqlHandler DB { get; init; } = db;
    internal event Action? WatchChanged;

    public List<ProcessWatch> GetAllWatches()
    {
        return DB.Conn.Table<ProcessWatch>().OrderBy(pw => pw.OrderNo).ToList();
    }
    public ProcessWatch GetWatch(int id)
    {
        return DB.Conn.Find<ProcessWatch>(id);
    }
    public int UpdateWatch(ProcessWatch item)
    {
        var res = DB.Conn.Update(item);
        WatchChanged?.Invoke();
        return res;
    }
    public int InsertWatch(ProcessWatch item)
    {
        var res = DB.Conn.Insert(item);
        WatchChanged?.Invoke();
        return res;
    }
    public int DeleteWatch(int id)
    {
        int result = 0;
        DB.Conn.RunInTransaction(() =>
        {
            DB.Conn.Table<ProcessRecord>().Delete(pr => pr.Pid == id);
            result = DB.Conn.Delete<ProcessWatch>(id);
        });

        WatchChanged?.Invoke();
        return result;
    }
    public bool CheckDuplicate(string processName)
    {
        var exists = DB.Conn.Table<ProcessWatch>().Where(pw => pw.ProcessName == processName).FirstOrDefault();
        return exists == null;
    }

    public int InsertRecord(ProcessRecord record)
    {
        return DB.Conn.Insert(record);
    }
    public List<ProcessRecord> GetRecords(int pid, int startDate, int endDate)
    {
        return DB.Conn.Table<ProcessRecord>()
            .Where(pr => pr.Pid == pid && pr.Date >= startDate && pr.Date <= endDate)
            .OrderByDescending(pr => pr.Date)
            .ToList();
    }
    public List<ProcessRecord> GetAllRecords()
    {
        return DB.Conn.Table<ProcessRecord>().ToList();
    }
}
