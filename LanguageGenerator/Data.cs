using System.Text.Json.Serialization;

namespace LanguageGenerator
{
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
}
