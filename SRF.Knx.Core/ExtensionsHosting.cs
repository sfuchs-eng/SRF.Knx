using Microsoft.Extensions.DependencyInjection;
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
        // Register the DPT factory and its dependencies as singletons
        services.AddSingleton<IPdtEncoderFactory, PdtEncoderFactory>();
        services.AddSingleton<IDptNumericInfoFactory, DptNumericInfoFactory>();
        services.AddSingleton<IDptFactory, DptFactory>();

        return services;
    }
}