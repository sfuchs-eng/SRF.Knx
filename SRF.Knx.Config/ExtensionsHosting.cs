using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SRF.Knx.Config.Domain;
using SRF.Knx.Core;

namespace SRF.Knx.Config;

public static class ExtensionsHosting
{
    /// <summary>
    /// Extension method to add KNX configuration services to the dependency injection container.
    /// Registers the KNX configuration, system configuration, and DPT resolver services.
    /// Calls <see cref="AddKnxCore"/> to register core KNX services as well in the right order.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="sectionName">The configuration section name. Defaults to <c>KnxConfiguration.SectionName</c>.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddKnxConfig(this IServiceCollection services, string? sectionName = null)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddOptions<KnxConfiguration>().BindConfiguration(sectionName ?? KnxConfiguration.SectionName);

        // Domain Config Services to base on, e.g. for running with Falcon SDK and SRF.Network.Knx
        services.TryAddSingleton<Domain.ILabelToNameConverter, Domain.DefaultLabelToNameConverter>();
        services.TryAddSingleton<IDomainConfigurationFactory, Domain.DomainConfigurationFactory>();
        services.TryAddSingleton<DomainConfiguration>((s) =>
        {
            var dcf = s.GetRequiredService<IDomainConfigurationFactory>();
            return dcf.Load();
        });

        // the library entry point
        services.TryAddSingleton<IKnxConfigFactory, KnxConfigFactory>();

        services.TryAddSingleton<IKnxMasterDataProvider, KnxMasterDataProvider>();

        services.TryAddSingleton<IKnxSystemConfiguration, KnxSystemConfigurationCached>();
        services.AddSingleton<IDptResolver>(sp => sp.GetRequiredService<IKnxSystemConfiguration>()); // error out in case IDptResolver is already registered. Libraries using KnxCore only get a simpler IDptResolver which is using TryAddSingleton.

        services.AddKnxCore();

        return services;
    }
}
