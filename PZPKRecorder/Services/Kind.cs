using PZPKRecorder.Data;

namespace PZPKRecorder.Services;

internal class KindService
{
    public static IList<Kind> GetKinds()
    {
        return SqlLiteHandler.Instance.DB.Table<Kind>().ToList();
    }

    public static int InsertKind(Kind kind)
    {
        return SqlLiteHandler.Instance.DB.Insert(kind);
    }

    public static int DeleteKind(int id)
    {
        int usingCount = SqlLiteHandler.Instance.DB.Table<Record>().Where(t => t.Kind == id).Count();
        if (usingCount > 0)
        {
            throw new Exception("Kind is usage, cannot delete!");
        }

        return SqlLiteHandler.Instance.DB.Delete<Kind>(id);
    }

    public static int UpdateKind(Kind kind)
    {
        return SqlLiteHandler.Instance.DB.Update(kind);
    }
}
