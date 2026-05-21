using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SRF.Knx.Config.Domain;
using SRF.Knx.Core;

namespace SRF.Knx.Config;

public static class ExtensionsHosting
{
    /// <summary>
    /// Extension method to add KNX configuration services to the dependency injection container.
    /// Registers <see cref="KnxSystemConfigOptions"/>, domain configuration, and DPT resolver services.
    /// Calls <see cref="AddKnxCore"/> to register core KNX services as well in the right order.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="sectionName">
    /// Override for the <c>Knx:System</c> configuration section name.
    /// Defaults to <see cref="KnxSystemConfigOptions.SectionName"/>.
    /// </param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddKnxConfig(this IServiceCollection services, string? sectionName = null)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddOptions<KnxSystemConfigOptions>().BindConfiguration(sectionName ?? KnxSystemConfigOptions.SectionName);

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

        services.TryAddSingleton<IKnxSystemConfiguration>(sp =>
            new KnxSystemConfigurationCached(
                GroupAddressConfiguration.FromDomainConfig(sp.GetRequiredService<DomainConfiguration>()),
                sp.GetRequiredService<IDptFactory>()));
        services.AddSingleton<IDptResolver>(sp => sp.GetRequiredService<IKnxSystemConfiguration>()); // error out in case IDptResolver is already registered. Libraries using KnxCore only get a simpler IDptResolver which is using TryAddSingleton.

        services.AddKnxCore();

        return services;
    }
}
