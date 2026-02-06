using SRF.Knx.Config.Domain;
using SRF.Knx.Config.Domain.Legacy;
using SRF.Knx.Config.OpenHab.BaseConfig;
using SRF.Knx.Config.OpenHab.BaseConfig.Modifiers;
using SRF.Knx.Core;

namespace SRF.Knx.Config.OpenHab;

/// <summary>
/// Factory to create OpenHAB KNX configuration objects and files
/// based on <see cref="DomainConfiguration"/>.
/// </summary>
public interface IOpenHabKnxConfigFactory
{
    /// <summary>
    /// Load if possible, otherwise create new KNX OpenHAB configuration file based on provided domain configuration.
    /// </summary>
    public KnxOpenHabConfig GetKnxOpenHabConfig(DomainConfiguration domainConfig);
    public IEnumerable<IOpenHabKnxBaseConfigModifier> IdentifyConfigurationUpdates(DomainConfiguration domainConfig, KnxOpenHabConfig knxOpenHabConfig);
    public void ApplyConfigurationUpdates(IEnumerable<IOpenHabKnxBaseConfigModifier> updates, KnxOpenHabConfig knxOpenHabConfig);
    public void WriteOHConfigFiles(KnxOpenHabConfig knxOpenHabConfig);
    void SaveBaseConfig(KnxOpenHabConfig openHabConfig);
    IEnumerable<IOpenHabKnxBaseConfigModifier> OverrideWithLegacy(Domain.DomainConfiguration domainConfiguration, KnxOpenHabConfig cfg, List<KnxGroupAddressConfig> legacyGAC);
    OHKnxGroupAddress CreateOpenHabGAC(Core.GroupAddress groupAddress, DomainConfiguration domainConfig);
    public void OverrideConfigsFromLegacy(string legacyGroupAddressConfigFile, out DomainConfiguration domainConfiguration, out KnxOpenHabConfig openHabConfig);
}
