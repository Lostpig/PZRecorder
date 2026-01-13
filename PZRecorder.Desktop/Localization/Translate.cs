using PZRecorder.Desktop.Common;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PZPKRecorder.Localization;

namespace PZRecorder.Desktop.Localization;

internal record LanguageItem(string Name, string Value)
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = Name;

    [JsonPropertyName("value")]
    public string Value { get; set; } = Value;
}
internal class LanguageJson
{
    [JsonPropertyName("languages")]
    public List<LanguageItem> Languages { get; set; } = new();

    [JsonPropertyName("fields")]
    public List<string> Fields { get; set; } = new();

    [JsonPropertyName("default")]
    public string DefaultLanguage { get; set; } = string.Empty;
}

internal static class Translate
{
    private static bool _initialized = false;
    private static List<LanguageItem> _languages = [];

    public static IReadOnlyList<LanguageItem> Languages => _languages;
    public static string Default { get; private set; } = "zh-CN";
    public static LanguageItem? Current { get; private set; }
    public static event Action? LanguageChanged;

    public static void Initialize()
    {
        if (_initialized) return;

        try
        {
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            string langFilePath = Path.Join(rootPath, "Localization", "languages.json");

            var langText = File.ReadAllText(langFilePath);
            var langJson = JsonSerializer.Deserialize<LanguageJson>(langText) ?? throw new Exception("languages.json deserialize failed");
            _languages.Clear();
            _languages.AddRange(langJson.Languages);

            Default = langJson.DefaultLanguage;
        }
        catch (Exception ex)
        {
            ErrorProxy.CatchException(ex);
        }
        finally
        {
            _initialized = true;
        }
    }

    public static void ChangeLanguage(string lang)
    {
        if (Current?.Value == lang) return;
        var langItem = _languages.FirstOrDefault(x => x.Value == lang);

        if (langItem != null)
        {
            Current = langItem;
            LoadLanguage(langItem);
            LanguageChanged?.Invoke();
        }
        else
        {
            throw new Exception($"Language {lang} not found!");
        }
    }
    private static void LoadLanguage(LanguageItem lang)
    {
        string rootPath = AppDomain.CurrentDomain.BaseDirectory;
        string langPath = Path.Join(rootPath, "Localization", $"{lang.Value}.json");

        string langJson = File.ReadAllText(langPath, Encoding.UTF8);
        var fields = JsonSerializer.Deserialize<Dictionary<string, string>>(langJson);
        if (fields != null) {
            LocalizeDict.Update(fields);
        }
        else throw new Exception("Language file load error: language map is null");
    }
}

