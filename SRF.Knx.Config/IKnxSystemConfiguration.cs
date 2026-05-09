using SRF.Knx.Core;

namespace SRF.Knx.Config;

/// <summary>
/// All configuration data, dominantly Group Addresses with DPT, names, descriptions and other metadata, that is required for the KNX system to operate.
/// It enriches loaded configuration data with additional functions (e.g. DPT objects for related functionality)
/// </summary>
/// <remarks>
/// Supposed to be injected as a singleton into the DI service catalog by the consumer application after loading it via <see cref="IKnxConfigFactory"/>.
/// Provides different dictionaries for efficient lookup of group address related information by different keys (e.g. by group address, by name, etc.).
/// Use only for static configuration data.<br/>
/// Default implementation is <see cref="KnxSystemConfigurationCached"/> which is registered in the DI container by <see cref="ExtensionsHosting.AddKnxConfig(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>, loading the configuration via <see cref="IKnxConfigFactory"/>.
/// </remarks>
public interface IKnxSystemConfiguration : IDptResolver
{
    GroupAddressMeta GetGroupAddressMeta(GroupAddress groupAddress);
    GroupAddressMeta GetGroupAddressMeta(string name);
    GroupAddressMeta? GetGroupAddressMetaOrNull(GroupAddress groupAddress);
    GroupAddressMeta? GetGroupAddressMetaOrNull(string name);
}
