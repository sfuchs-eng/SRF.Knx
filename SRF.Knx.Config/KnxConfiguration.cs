namespace SRF.Knx.Config;

/// <summary>
/// Formerly the single mixed KNX configuration class. Now split into:
/// <list type="bullet">
///   <item><see cref="KnxSystemConfigOptions"/> — file paths and config-generation settings, bound from <c>Knx:System</c>.</item>
///   <item><c>SRF.Network.Knx.KnxConnectionOptions</c> — per-connection transport settings, bound from <c>Knx:Connections:{name}</c>.</item>
///   <item><c>HomeCompanion.Integrations.Knx.KnxIntegrationOptions</c> — HomeCompanion integration behavior flags, bound from <c>Knx</c>.</item>
/// </list>
/// </summary>
[Obsolete("Use KnxSystemConfigOptions (Knx:System), KnxConnectionOptions (Knx:Connections:{name}), or KnxIntegrationOptions (Knx) instead.")]
public class KnxConfiguration
{
    public static readonly string SectionName = "Knx";
}
