using PZPKRecorder.Data;

namespace PZPKRecorder.Services;

internal class VariantService
{
    static Dictionary<string, string?> cacheDict = new();

    public static List<VariantTable> GetAllVariants()
    {
        return SqlLiteHandler.Instance.DB.Table<VariantTable>().ToList();
    }

    public static string? GetVariant(string key)
    {
        if (cacheDict.ContainsKey(key)) return cacheDict[key];

        var v = SqlLiteHandler.Instance.DB.Find<VariantTable>(key);
        cacheDict.Add(key, v != null ? v.Value : null);

        if (v != null)
        {
            return v.Value;
        }
        return null;
    }
    public static void SetVariant(string key, string value)
    {
        if (cacheDict.ContainsKey(key) && cacheDict[key] == value) return;

        VariantTable v = new VariantTable() { Key = key, Value = value };
        SqlLiteHandler.Instance.DB.InsertOrReplace(v);

        cacheDict[key] = value;
    }
}
