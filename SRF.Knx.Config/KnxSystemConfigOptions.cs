namespace SRF.Knx.Config;

/// <summary>
/// System/file-management configuration for SRF.Knx.Config services.
/// </summary>
/// <remarks>
/// <para>
/// Bound from <c>Knx:System</c> via
/// <see cref="ExtensionsHosting.AddKnxConfig(Microsoft.Extensions.DependencyInjection.IServiceCollection, string?)"/>.
/// These options cover all file-path and config-generation concerns that are independent of
/// runtime KNX/IP transport connectivity.
/// </para>
/// <para>
/// Runtime KNX connection options (multicast address, port, KNX individual address, …) belong in
/// <see cref="SRF.Network.Knx.KnxConnectionOptions"/> bound under <c>Knx:Connections:{name}</c>.
/// </para>
/// </remarks>
public class KnxSystemConfigOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Knx:System";

    /// <summary>
    /// Path to the ETS Group Address export XML file.
    /// </summary>
    public string EtsGAExportFile { get; set; } = "GroupAddressExport.xml";

    /// <summary>
    /// Path to the KNX domain extra-config JSON file.
    /// </summary>
    public string KnxDomainConfigFile { get; set; } = "KnxDomainConfig.json";

    /// <summary>
    /// Folder containing <c>knx_master.xml</c> distributed with ETS.
    /// </summary>
    public string KnxMasterFolder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "knx-master");

    /// <summary>
    /// If true, generated <c>KnxValues.generated.cs</c> properties will include an OpenHAB bus
    /// mapping for initialization so that
    /// <c>HomeCompanion.Integrations.OpenHab</c> can populate the corresponding
    /// <see cref="HomeCompanion.Values.IValue"/> from the linked OpenHAB item state on startup.
    /// </summary>
    public bool LinkKnxValuesToOpenHabForInitialization { get; set; } = true;

    public HomeCompanionOptions HomeCompanion { get; set; } = new();

    public class HomeCompanionOptions
    {
        public string GeneratedValuesClassesNamespace { get; set; } = "HomeCompanion.Local.Values";

        public string KnxValuesClassName { get; set; } = "KnxValues";
        
        public string KnxValuesCodeGenFilePath { get; set; } = "KnxValues.generated.cs";

        public string OpenHabValuesClassName { get; set; } = "OpenHabValues";

        /// <summary>
        /// Path where <c>srf-network-cli kc --home-companion-code-gen</c> writes the generated
        /// <c>OpenHabValues.generated.cs</c> source file. Set this in your local <c>SRF.Network.json</c>
        /// to point at the <c>HomeCompanion.Local/Values/</c> project folder on your machine.
        /// </summary>
        public string OpenHabValuesCodeGenFilePath { get; set; } = "OpenHabValues.generated.cs";
    }

    /// <summary>
    /// OpenHAB configuration generation (Things, Channels, Items) for KNX Group Addresses.
    /// </summary>
    public OpenHabOptions OpenHab { get; set; } = new();

    /// <summary>
    /// OpenHAB KNX config-generation sub-options.
    /// </summary>
    public class OpenHabOptions
    {
        /// <summary>Base meta-configuration file path.</summary>
        public string BaseConfigFile { get; set; } = "OpenHabKnxMetaConfig.json";

        /// <summary>Folder containing templates and master-data JSON files.</summary>
        public string TemplatesFolder { get; set; } = "Resources";

        /// <summary>Item template definitions file (relative to <see cref="TemplatesFolder"/>).</summary>
        public string ItemTemplatesFile { get; set; } = "OpenHabItemTemplates.json";

        /// <summary>KNX-to-DPT mapping file (relative to <see cref="TemplatesFolder"/>).</summary>
        public string KnxDptMappings { get; set; } = "OpenHabDptMappings.json";

        /// <summary>OpenHAB config root directory (e.g. <c>/etc/openhab</c>).</summary>
        public string OHConfigRoot { get; set; } = "/etc/openhab";

        /// <summary>
        /// OpenHAB version for which configuration is generated. Supported values: <c>"3"</c>, <c>"5"</c>.
        /// </summary>
        public string OpenHabVersion { get; set; } = "5";

        /// <summary>Unit system configuration file (relative to <see cref="TemplatesFolder"/>).</summary>
        public string UnitSystemConfig { get; set; } = "UnitSystemConfig.json";

        /// <summary>
        /// Seconds to wait after renaming the Things file before writing the new one,
        /// giving OpenHAB time to unload existing Things.
        /// </summary>
        public int WaitTimeBeforeWritingThingsFileSec { get; set; } = 20;
    }
}
