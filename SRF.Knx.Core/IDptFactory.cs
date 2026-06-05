using SRF.Knx.Core.DPT;

namespace SRF.Knx.Core;

/// <summary>
/// Provides the DPT object for a given DPT main and sub number.
/// The DPT factory uses the master data to determine the appropriate DPT type and its properties
/// and uses the PDT encoder factory to get the appropriate PDT encoder for the DPT type.
/// The DPT factory also uses the DPT numeric info factory to get the numeric information for
/// the DPT type, such as the range, resolution, unit, etc., which can be used by the DPT types to validate and format the values.
/// DPT objects created by the factory should be cached and reused by the application to avoid unnecessary object creation and to improve performance.
/// </summary>
/// <remarks>
/// <see cref="ExtensionsHosting.AddKnxCore(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/> registers <see cref="DptFactory"/>
/// wrapped in a <see cref="DptMemoryCache"/> as the default <see cref="IDptFactory"/> implementation in the dependency injection container.
/// The DPT factory is used by the <see cref="IDptResolver"/> implementation in the `SRF.Knx` package to resolve the DPT for a group address when processing incoming or outgoing group address events.
/// The DPT factory is also used by the `KnxValueContainerBase` class in the `HomeCompanion.Integrations.Knx` package to resolve the DPT for group addresses when initializing KNX value containers based on the ETS group address export in the domain configuration.
/// </remarks>
public interface IDptFactory
{
    DptBase Get(int main, int sub);
    DptBase Get(DataPointTypeId dpstId);
    DptBase Get(string dptId);
}
