using Newtonsoft.Json;

namespace PZPKRecorderGenerator.Helpers;

internal class LocalizationHelper
{
    public static LanguageJson DeserializeLanguage(string languagesText)
    {
        return JsonConvert.DeserializeObject<LanguageJson>(languagesText);
    }
}

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
    public List<LanguageItem> Languages { get; set; } = new ();

    [JsonProperty("fields")]
    public List<string> Fields { get; set; } = new();

    [JsonProperty("default")]
    public string DefaultLanguage { get; set; } = string.Empty;
}
