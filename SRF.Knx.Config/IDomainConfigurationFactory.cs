using SRF.Knx.Config.Domain;
using SRF.Knx.Config.Domain.ConfigModifiers;
using SRF.Knx.Config.ETS5;
using SRF.Knx.Core;

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

    /// <summary>
    /// Applies the given configuration updates to the provided domain configuration.
    /// No file updates, only in-memory updates. Use <see cref="UpdateConfigFiles"/> to persist the changes to disk.
    /// </summary>
    /// <param name="dcUpdates">The configuration updates to apply.</param>
    /// <param name="domainConfiguration">The domain configuration to update.</param>
    void ApplyConfigurationUpdates(IEnumerable<IDomainConfigModifier> dcUpdates, DomainConfiguration domainConfiguration);

    /// <summary>
    /// Associates a thing with the given group address in the domain configuration.
    /// </summary>
    /// <param name="groupAddress">The group address to associate the thing with.</param>
    /// <param name="domainConfig">The domain configuration to update.</param>
    /// <param name="isNewThing">Indicates if a new thing was created.</param>
    /// <param name="gotNewlyAssociated">Indicates if the thing was newly associated.</param>
    /// <returns>The associated thing.</returns>
    public Thing AssociateThing(GroupAddress groupAddress, DomainConfiguration domainConfig, out bool isNewThing, out bool gotNewlyAssociated);

    /// <summary>
    /// Associates a thing with the given ETS group address configuration in the domain configuration.
    /// </summary>
    /// <param name="gac">The ETS group address configuration.</param>
    /// <param name="gaec">The group address extra configuration.</param>
    /// <param name="extraConfig">The domain extra configuration.</param>
    /// <param name="isNewThing">Indicates if a new thing was created.</param>
    /// <param name="gotNewlyAssociated">Indicates if the thing was newly associated.</param>
    /// <returns>The associated thing.</returns>
    public Thing AssociateThing(EtsGroupAddressConfig gac, GroupAddressExtraConfig gaec, DomainExtraConfig extraConfig, out bool isNewThing, out bool gotNewlyAssociated);
}
