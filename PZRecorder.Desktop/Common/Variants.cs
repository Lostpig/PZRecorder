using PZRecorder.Core;
using PZRecorder.Core.Tables;

namespace PZRecorder.Desktop.Common;

internal static class VariantFields 
{
    public static string Language = "language";
    public static string DBVersion = "dbversion";
    public static string Theme = "theme";
}

internal class VariantsManager
{
    private readonly Dictionary<string, string> _cache = [];
    private readonly SqlHandler _sql;

    public VariantsManager(SqlHandler sql)
    {
        _sql = sql;
    }

    public VariantTable[] GetAll()
    {
        return _sql.Conn.Table<VariantTable>().ToArray();
    }
    public string GetVariant(string key)
    {
        if (_cache.TryGetValue(key, out string? value)) return value;

        var tb = _sql.Conn.Find<VariantTable>(key);
        value = tb?.Value ?? "";
        _cache.Add(key, value);

        return value;
    }
    public void SetVariant(string key, string value)
    {
        if (_cache.TryGetValue(key, out string? orgVal) && orgVal == value) return;

        var v = new VariantTable() { Key = key, Value = value };
        _sql.Conn.InsertOrReplace(v);

        _cache[key] = value;
    }
}
