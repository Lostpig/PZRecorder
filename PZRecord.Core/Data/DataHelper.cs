using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace PZRecorder.Core.Data;

internal static class NodeExtensions
{
    public static string IntValueText(this JsonNode node, string columnName)
    {
        var value = node[columnName]?.GetValue<int>() ?? 0; 
        return value.ToString();
    }
    public static string StringValueText(this JsonNode node, string columnName)
    {
        var value = node[columnName]?.GetValue<string>() ?? "";
        return $"\"{value}\"";
    }
    public static string DateTimeValueText(this JsonNode node, string columnName)
    {
        var value = node[columnName]?.GetValue<long>() ?? 0;
        return DateTime.FromBinary(value).Ticks.ToString();
    }
    public static string BoolValueText(this JsonNode node, string columnName)
    {
        var value = node[columnName]?.GetValue<bool>() ?? false;
        return (value ? 1 : 0).ToString();
    }
}
