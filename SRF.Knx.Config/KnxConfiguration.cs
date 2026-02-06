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

    public string ConnectionString { get; set; } = "Type=IpRouting";

    public string EtsGAExportFile { get; set; } = "GroupAddressExport.xml";

    public string KnxDomainConfigFile { get; set; } = "KnxDomainConfig.json";

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
}
