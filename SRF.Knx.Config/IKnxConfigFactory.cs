using HomeCompanion.Knx.Shared;
using SRF.Knx.Config.Domain;
using SRF.Knx.Config.ETS5;

namespace SRF.Knx.Config;

/// <summary>
/// Therre are several configuration stages:<br/>
/// - ETS Group Address export: loading only, accessible from <see cref="DomainConfiguration.GroupAddresses"/><br/>
/// - <see cref="KnxConfiguration"/> configured via <see cref="Microsoft.Extensions.Options"/>, e.g. from appsettings.json or other path as per <see cref="Microsoft.Extensions.Configuration"/>.<br/>
/// - <see cref="DomainExtraConfig"/> managed in dedicated config file. Loaded into <see cref="DomainConfiguration"/><br/>
/// - <see cref="OpenHab.BaseConfig.KnxOpenHabConfig"/> is generated from template patterns based on above configs and stored/loaded from files for manual intervention.<br/>
/// - OpenHAB configuration files generated using <see cref="OpenHab.OpenHabKnxConfigFactory"/> based on <see cref="OpenHab.BaseConfig.KnxOpenHabConfig"/>.
/// </summary>
public interface IKnxConfigFactory
{
    /// <summary>
    /// Consider injecting <see cref="DomainConfiguration"/> directly.
    /// </summary>
    DomainConfiguration GetDomainConfig();

    /// <summary>
    /// Consider getting it from <see cref="DomainConfiguration"/> directly.
    /// </summary>
    Dictionary<ushort, EtsGroupAddressConfig> GetEtsGroupAddressConfigs();

    void SaveDomainConfig(DomainConfiguration domainConfig);

    DomainConfiguration CreateDomainConfigFromEtsExport();

    /// <summary>Builds the HomeCompanionKnxAutoGen.json mapping from the loaded domain configuration.</summary>
    Dictionary<string, HomeCompanionAutoGenEntry> GenerateHomeCompanionAutoGen(DomainConfiguration config);

    /// <summary>Serializes the auto-gen mapping to <see cref="KnxConfiguration.HomeCompanionAutoGenFile"/>.</summary>
    void SaveHomeCompanionAutoGen(Dictionary<string, HomeCompanionAutoGenEntry> entries);
}
