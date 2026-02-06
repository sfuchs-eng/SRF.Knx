using SRF.Knx.Config.Domain;
using SRF.Knx.Config.Domain.ConfigModifiers;
using SRF.Knx.Config.ETS5;

namespace SRF.Knx.Config;

public interface IDomainConfigurationFactory
{
    /// <summary>
    /// Fresh load and cache update.
    /// </summary>
    public DomainConfiguration Load(bool createFreshExtraConfig = false);

    /// <summary>
    /// Cached configuration object if already loaded, otherwise fresh load.
    /// </summary>
    public DomainConfiguration Get();

    /// <summary>
    /// Initialize new DomainConfiguration from an ETS Group Address export and from a legacy "Group Address Config" file.
    /// </summary>
    public List<IDomainConfigModifier> UpdateWithLegacyGAC(DomainConfiguration domainConfig, IEnumerable<Domain.Legacy.KnxGroupAddressConfig> legacyGAC);

    /// <summary>
    /// Serialize the provided configuration to the configured files.
    /// Overwrites existing files.
    /// </summary>
    public void Save(DomainConfiguration domainConfiguration);

    /// <summary>
    /// Updates the domain configuration files, preserving json nodes that are not reflected
    /// by the classes into which the configuration is serialized into when calling <see cref="Load"/>.
    /// </summary>
    public void UpdateConfigFiles(IEnumerable<IDomainConfigModifier> domainConfigModifiers, DomainConfiguration? allowOverwriteWith = null);

    public Thing AssociateThing(KnxGroupAddress groupAddress, DomainConfiguration domainConfig, out bool isNewThing, out bool gotNewlyAssociated);
    public Thing AssociateThing(EtsGroupAddressConfig gac, GroupAddressExtraConfig gaec, DomainExtraConfig extraConfig, out bool isNewThing, out bool gotNewlyAssociated);

    void ApplyConfigurationUpdates(IEnumerable<IDomainConfigModifier> dcUpdates, DomainConfiguration domainConfiguration);
}
