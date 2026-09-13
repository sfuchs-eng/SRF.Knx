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
    /// This is the default method to get the OpenHAB KNX configuration.<br/>
    /// Gets the cached OpenHAB KNX configuration if available. Loads or creates a fresh one if not yet cached.<br/>
    /// Updates are only applied upon load/creation. An existing cached configuration is not updated with new domain configuration updates. Use <see cref="Update(DomainConfiguration)"/> to update the cached configuration with new domain configuration updates.
    /// </summary>
    /// <returns></returns>
    KnxOpenHabConfig Get();

    /// <summary>
    /// Load if possible, otherwise create new KNX OpenHAB configuration file based on provided domain configuration.<br/>
    /// The cached/loaded/generated configuration is checked for updates and modified if needed. The updated configuration is returned and cached for subsequent calls to <see cref="Get()"/>.
    /// </summary>
    KnxOpenHabConfig Get(DomainConfiguration domainConfig);

    /// <summary>
    /// Updates the cached/loaded/generated OpenHAB KNX configuration with the provided configuration.<br/>
    /// The update configuration is cached for subsequent calls to <see cref="Get()"/>.<br/>
    /// The provided configuration is not persisted to disk. Use <see cref="Save(KnxOpenHabConfig)"/> to persist the configuration to disk.
    /// </summary>
    /// <param name="domainConfig">The domain configuration to update the cached OpenHAB KNX configuration with.</param>
    void Update(DomainConfiguration domainConfig);

    /// <summary>
    /// Set the cache to the provided OpenHAB KNX configuration. This will be used for subsequent calls to <see cref="Get()"/>.
    /// </summary>
    /// <param name="knxOpenHabConfig">The OpenHAB KNX configuration to set in the cache.</param>
    void Set(KnxOpenHabConfig knxOpenHabConfig);

    /// <summary>
    /// Create a new OpenHAB KNX configuration based on the provided domain configuration.<br/>
    /// The configuration is not cached or persisted to disk. Use <see cref="Set(KnxOpenHabConfig)"/> to cache the configuration and <see cref="Save(KnxOpenHabConfig)"/> to persist the configuration to disk.
    /// </summary>
    KnxOpenHabConfig Create(DomainConfiguration domainConfig);

    /// <summary>
    /// Save the meta configuration json files for OpenHAB KNX configuration to disk.
    /// This allows for manual editing of the configuration and reloading it later.
    /// </summary>
    /// <param name="openHabConfig">The OpenHAB KNX configuration to save to disk.</param>
    void Save(KnxOpenHabConfig openHabConfig);

    /// <summary>
    /// Save the meta configuration json files for OpenHAB KNX configuration to disk.
    /// This allows for manual editing of the configuration and reloading it later.
    /// </summary>
    /// <param name="openHabConfig">The OpenHAB KNX configuration to save to disk.</param>
    Task SaveAsync(KnxOpenHabConfig openHabConfig);

    Task<KnxOpenHabConfig> LoadAsync();

    /// <summary>
    /// Identify configuration updates that need to be applied to the OpenHAB KNX configuration.
    /// </summary>
    /// <param name="domainConfig"></param>
    /// <param name="knxOpenHabConfig"></param>
    /// <returns></returns>
    public IEnumerable<IOpenHabKnxBaseConfigModifier> IdentifyConfigurationUpdates(DomainConfiguration domainConfig, KnxOpenHabConfig knxOpenHabConfig);

    /// <summary>
    /// Apply the specified configuration updates to the provided OpenHAB KNX configuration.
    /// </summary>
    /// <param name="updates"></param>
    /// <param name="knxOpenHabConfig"></param>
    public void ApplyConfigurationUpdates(IEnumerable<IOpenHabKnxBaseConfigModifier> updates, KnxOpenHabConfig knxOpenHabConfig);

    /// <summary>
    /// Write the OpenHAB KNX configuration files to disk based on the provided configuration.
    /// This overwrites existing things and items files.
    /// </summary>
    /// <param name="knxOpenHabConfig"></param>
    public Task WriteOpenHabConfigFilesAsync(KnxOpenHabConfig knxOpenHabConfig);

    /// <summary>
    /// Override the OpenHAB KNX configuration with legacy group address configuration.
    /// </summary>
    /// <param name="domainConfiguration"></param>
    /// <param name="cfg"></param>
    /// <param name="legacyGAC"></param>
    /// <returns></returns>
    IEnumerable<IOpenHabKnxBaseConfigModifier> OverrideWithLegacy(Domain.DomainConfiguration domainConfiguration, KnxOpenHabConfig cfg, List<KnxGroupAddressConfig> legacyGAC);

    /// <summary>
    /// Create an OpenHAB specific group address configuration for the specified KNX group address.
    /// </summary>
    /// <param name="groupAddress"></param>
    /// <param name="domainConfig"></param>
    /// <returns></returns>
    OHKnxGroupAddress CreateOpenHabGAC(Core.GroupAddress groupAddress, DomainConfiguration domainConfig);

    /// <summary>
    /// Override the OpenHAB KNX configuration with legacy group address configuration from a file.
    /// </summary>
    /// <param name="groupAddress"></param>
    /// <param name="domainConfig"></param>
    /// <param name="ohConfig"></param>
    public void OverrideConfigsFromLegacy(string legacyGroupAddressConfigFile, out DomainConfiguration domainConfiguration, out KnxOpenHabConfig openHabConfig);
}
