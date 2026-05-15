using System.Security;

namespace SRF.Knx.Config;

/// <summary>
/// <para>Library configuration and locator for further configuration files.
/// It's foreseen to be used with <see cref="Microsoft.Extensions.Options"/>.</para>
/// <para>See <see cref="SRF.Knx.Config.Domain"/> and <see cref="IDomainConfigurationFactory"/>
/// for ETS / project specific configuration and loading thereof.</para>
/// </summary>
public class KnxConfiguration
{
    public static readonly string SectionName = "Knx";

    public bool Enable { get; set; } = true;

    /// <summary>
    /// If true, the application shall read the readable Group Addresses on startup to initialize the internal object cache.
    /// Set to false to prevent a background task sending out read requests for all readable GAs on startup.
    /// </summary>
    /// <value></value>
    public bool ReadGroupAddressesOnStartup { get; set; } = true;

    public string ConnectionString { get; set; } = "Type=IpRouting";

    public string EtsGAExportFile { get; set; } = "GroupAddressExport.xml";

    public string KnxDomainConfigFile { get; set; } = "KnxDomainConfig.json";

    /// <summary>
    /// Path where <c>srf-network-cli kc --home-companion-code-gen</c> writes the generated
    /// <c>KnxValues.generated.cs</c> source file. Set this in your local <c>SRF.Network.json</c>
    /// to point at the <c>HomeCompanion.Knx/</c> project folder on your machine.
    /// </summary>
    public string HomeCompanionCodeGenFile { get; set; } = "KnxValues.generated.cs";

    public string KnxMasterFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "knx-master");

    public class CommSecuritySettings
    {
        public bool UseCommSecurity => !string.IsNullOrEmpty(KeyRingFile);
        public string? KeyRingFile { get; set; }
        public SecureString? KeyRingPassword { get; set; }
        public string? SequenceControlFile { get; set; }
        public SecureString? SequenceControlPassword { get; set; }
    }
    public CommSecuritySettings CommSecurity { get; set; } = new CommSecuritySettings();

    /// <summary>
    /// OpenHAB configuration generation (Things, Channels, Items) for KNX Group Addresses.
    /// </summary>
    public class OpenHabOptions
    {
        public string BaseConfigFile { get; set; } = "OpenHabKnxMetaConfig.json";
        public string TemplatesFolder { get; set; } = "Resources";
        public string ItemTemplatesFile { get; set; } = "OpenHabItemTemplates.json";

        public string KnxDptMappings { get; set; } = "OpenHabDptMappings.json";
        public string OHConfigRoot { get; set; } = "/etc/openhab";

        /// <summary>
        /// The OpenHAB version for which the configuration is generated.
        /// Only "3" and "5" are supported.
        /// </summary>
        public string OpenHabVersion { get; set; } = "5";
        public string UnitSystemConfig { get; set; } = "UnitSystemConfig.json";
        public int WaitTimeBeforeWritingThingsFileSec { get; set; } = 20;
    }

    public OpenHabOptions OpenHab { get; set; } = new();
    
    /// <summary>
    /// Generated KNX value properties in <c>KnxValues.generated.cs</c> can optionally include an OpenHAB bus mapping for initialization.
    /// The feature uses HomeCompanion.Integrations.OpenHab that discovers the mappings and initializes the IValues accordingly on startup by reading the current state of the linked OpenHAB items.
    /// </summary>
    public bool LinkKnxValuesToOpenHabForInitialization { get; set; } = true;
}
