
using Newtonsoft.Json;
using System.Text;

namespace PZPKRecorder.Services;

internal record LanguageItem(string Name, string Value)
{
    [JsonProperty("name")]
    public string Name { get; set; } = Name;

    [JsonProperty("value")]
    public string Value { get; set; } = Value;
}
internal class LanguageJson
{
    [JsonProperty("languages")]
    public List<LanguageItem> Languages { get; set; } = new();

    [JsonProperty("fields")]
    public List<string> Fields { get; set; } = new();

    [JsonProperty("default")]
    public string DefaultLanguage { get; set; } = string.Empty;
}

internal static class Translate
{
    const string DefaultLanguage = "zh-CN";
    public static string Current { get; private set; } = DefaultLanguage;
    private static readonly List<LanguageItem> _languages = new();
    public static IList<LanguageItem> Languages => _languages;

    public static void Init()
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string langFilePath = Path.Join(rootPath, "Localization", "languages.json");

        try
        {
            var langText = File.ReadAllText(langFilePath);
            var langJson = JsonConvert.DeserializeObject<LanguageJson>(langText);

            _languages.Clear();
            _languages.AddRange(langJson.Languages);

            string userSetLang = ReadLanguageSet();
            LoadLanguage(userSetLang);
            Current = userSetLang;
        }
        catch (Exception ex)
        {
            ExceptionProxy.CatchException(ex);
        }
    }

    public static void ChangeLanguage(string language)
    {
        try
        {
            LoadLanguage(language);
            Current = language;
            SaveLanguageSet(language);

            BroadcastService.Broadcast(BroadcastEvent.LanguageChanged, language);
        }
        catch (Exception ex)
        {
            ExceptionProxy.CatchException(ex);
        }
    }

    private static string ReadLanguageSet()
    {
        string? userSetCurrent = VariantService.GetVariant("language");
        if (String.IsNullOrEmpty(userSetCurrent))
        {
            userSetCurrent = DefaultLanguage;
            VariantService.SetVariant("language", userSetCurrent);
        }
        return userSetCurrent;
    }
    private static void SaveLanguageSet(string lang)
    {
        VariantService.SetVariant("language", lang);
    }

    private static void LoadLanguage(string lang)
    {
        string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        string langPath = Path.Join(rootPath, "Localization", $"{lang}.json");

        string langJson = File.ReadAllText(langPath, Encoding.UTF8);
        Dictionary<string, string>? map = JsonConvert.DeserializeObject<Dictionary<string, string>>(langJson);
        if (map != null) Localization.LocalizeDict.Update(map);
        else throw new Exception("Language file load error: language map is null");
    }
}

