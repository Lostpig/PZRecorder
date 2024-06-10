using PZPKRecorder.Data;

namespace PZPKRecorder.Services;

internal class VariantService
{
    public static string? GetVariant(string key)
    {
        var v = SqlLiteHandler.Instance.DB.Find<VariantTable>(key);
        if (v != null)
        {
            return v.Value;
        }
        return null;
    }
    public static void SetVariant(string key, string value)
    {
        VariantTable v = new VariantTable() { Key = key, Value = value };
        SqlLiteHandler.Instance.DB.InsertOrReplace(v);
    }
}
