using System.Text;
using Microsoft.Extensions.Logging;

namespace SRF.Knx.Config.OpenHab;

public class OpenHabValuesCodeGenerator(
    ILogger<OpenHabValuesCodeGenerator> logger
)
{
    private readonly ILogger<OpenHabValuesCodeGenerator> logger = logger;

    public string Generate(IEnumerable<OpenHabItemInfo> items, string className = "OpenHabValues", string nameSpace = "HomeCompanion.Local.Values")
    {
        var sb = new StringBuilder();
        sb.AppendLine("using HomeCompanion.Values;");
        sb.AppendLine("using HomeCompanion.Integrations.Knx;");
        sb.AppendLine("using HomeCompanion.Integrations.OpenHab;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine();
        sb.AppendLine($"namespace {nameSpace};");
        sb.AppendLine();
        sb.AppendLine($"public partial class {className}");
        sb.AppendLine("{");

        var usedNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in items)
        {
            var propName = MakeUnique(item.Name, usedNames);
            var summaryText = $"{EscapeXmlComment(item.Name)} (<c>{EscapeXmlComment(item.Type)}</c>)";
            string baseType = string.Empty;
            try
            {
                baseType = GetBaseType(item.Type);
            }
            catch (NotSupportedException ex)
            {
                logger.LogWarning(ex, "Skipping OpenHAB item '{itemName}' of type '{itemType}' because it is not supported for code generation.", item.Name, item.Type);
                continue;
            }
            if (string.IsNullOrWhiteSpace(baseType))
            {
                logger.LogWarning("Skipping OpenHAB item '{itemName}' of type '{itemType}' because the base type could not be determined.", item.Name, item.Type);
                continue;
            }

            sb.AppendLine($"    /// <summary>{summaryText}</summary>");
            sb.AppendLine($"    public ValueBase<{baseType}> {propName} {{ get; }} = new(loggerFactory.CreateLogger<ValueBase<{baseType}>>())");
            sb.AppendLine("    {");
            sb.AppendLine($"       Name = \"{propName}\",");
            sb.AppendLine($"       Label = {(string.IsNullOrWhiteSpace(item.Name) ? "null" : $"\"{DeCamelize(item.Name)}\"")},");
            sb.AppendLine("       BusMappings = new Dictionary<object, IValueBusEndpointMapping>");
            sb.AppendLine("       {");
            sb.AppendLine($"            [OpenHabBusEndpointMapping.BusId] = new OpenHabBusEndpointMapping(\"{item.Name}\") {{ Communication = BusCommunication.Receive | BusCommunication.Transmit }},");
            sb.AppendLine("       }");
            sb.AppendLine("    };");
            sb.AppendLine();
            }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string EscapeXmlComment(string name)
    {
        return System.Security.SecurityElement.Escape(name) ?? string.Empty;
    }

    private string MakeUnique(string name, HashSet<string> usedNames)
    {
        var uniqueName = name;
        var counter = 1;
        while (usedNames.Contains(uniqueName))
        {
            uniqueName = $"{name}_{counter}";
            counter++;
        }
        usedNames.Add(uniqueName);
        return uniqueName;
    }

    /// <summary>
    /// Derive a Label from the item Name.
    /// </summary>
    /// <param name="name">The camel-cased name of the OpenHAB item.</param>
    /// <returns>
    /// Rules applied to the input name:
    /// <list type="bullet">
    /// <item>Underscores are replaced with spaces.</item>
    /// <item>Uppercase letters that are alone (not first, no following, no preceding upper case character) are preceded by a space.</item>
    /// </list>
    /// </returns>
    private static string DeCamelize(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var sb = new StringBuilder();
        foreach (var c in name)
        {
            if (c == '_')
            {
                sb.Append(' ');
                continue;
            }

            if (char.IsUpper(c) && sb.Length > 0 && !char.IsUpper(sb[^1]) && sb[^1] != ' ')
            {
                sb.Append(' ');
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    private static string GetBaseType(string itemType)
    {
        var normalizedType = itemType.Trim();

        return normalizedType switch
        {
            "Switch" => "bool",
            "Contact" => "bool",
            "Dimmer" => "double",
            "Number" => "double",
            var type when type.StartsWith("Number:", StringComparison.Ordinal) => "double",
            "DateTime" => "global::System.DateTimeOffset",
            "String" => "string",
            "Color" => "string",
            "Location" => "string",
            "Player" => "string",
            "Rollershutter" => "double",
            "Call" => throw new NotSupportedException($"OpenHAB item type '{normalizedType}' handling is not implemented."),
            "Group" => throw new NotSupportedException($"OpenHAB item type '{normalizedType}' handling is not implemented."),
            "Image" => throw new NotSupportedException($"OpenHAB item type '{normalizedType}' handling is not implemented."),
            _ => throw new NotSupportedException($"OpenHAB item type '{normalizedType}' is not supported by the values generator.")
        };
    }
}

/// <summary>
/// Proxy class to hold information about an OpenHAB item, used for code generation.
/// Typically populated by deserializing JSON from the OpenHAB REST API / initialized from returned objects.
/// </summary>
public class OpenHabItemInfo
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
}