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

internal class Translate
{
    private readonly List<LanguageItem> _languages = [];
    public IReadOnlyList<LanguageItem> Languages => _languages;
    public string Default { get; private set; } = "zh-CN";
    public LanguageItem? Current { get; private set; }
    private BroadcastManager _broadcastManager;
    private ErrorProxy _errorProxy;

    public Translate(ErrorProxy errorProxy, BroadcastManager broadcastManager)
    {
        _broadcastManager = broadcastManager;
        _errorProxy = errorProxy;

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
            _errorProxy.CatchException(ex);
        }
    }

    public void ChangeLanguage(string lang)
    {
        if (Current?.Value == lang) return;
        var langItem = _languages.FirstOrDefault(x => x.Value == lang);

        if (langItem != null)
        {
            Current = langItem;
            LoadLanguage(langItem);
            _broadcastManager.Publish(BroadcastEvent.LanguageChanged);
        }
        else
        {
            _errorProxy.CatchException(new Exception($"Language {lang} not found!"));
        }
    }
    private void LoadLanguage(LanguageItem lang)
    {
        string rootPath = AppDomain.CurrentDomain.BaseDirectory;
        string langPath = Path.Join(rootPath, "Localization", $"{lang.Value}.json");

        string langJson = File.ReadAllText(langPath, Encoding.UTF8);
        var fields = JsonSerializer.Deserialize<Dictionary<string, string>>(langJson);
        if (fields != null)
        {
            LocalizeDict.Update(fields);
        }
        else
        {
            _errorProxy.CatchException(new Exception($"Language file {lang.Value} load error: language map is null"));
        }
    }
}

