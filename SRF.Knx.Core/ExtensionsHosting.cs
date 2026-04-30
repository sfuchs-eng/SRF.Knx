using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SRF.Knx.Core.DPT;

namespace SRF.Knx.Core;

public static class ExtensionsHosting
{
    /// <summary>
    /// Extension method to add KNX core services to the dependency injection container.
    /// Registers the DPT factory and its dependencies as singletons.
    /// Depends on a separately registered IKnxMasterDataProvider to supply the master data for DPT creation.
    /// </summary>
    public static IServiceCollection AddKnxCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IPdtEncoderFactory, PdtEncoderFactory>();
        services.TryAddSingleton<IDptNumericInfoFactory, DptNumericInfoFactory>();
        services.TryAddSingleton<IDptFactory>(sp =>
            new DptMemoryCache(
                ActivatorUtilities.CreateInstance<DptFactory>(sp)
            )
        );

        return services;
    }
}