using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SRF.Knx.Config.Domain;

namespace SRF.Knx.Config;

public static class ExtensionsHosting
{
    public static IServiceCollection AddKnxConfig(this IServiceCollection services, string? sectionName = null)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddOptions<KnxConfiguration>().BindConfiguration(sectionName ?? KnxConfiguration.SectionName);

        // Domain Config Services to base on, e.g. for running with Falcon SDK and SRF.Network.Knx
        services.AddSingleton<Domain.ILabelToNameConverter, Domain.DefaultLabelToNameConverter>();
        services.AddSingleton<IDomainConfigurationFactory, Domain.DomainConfigurationFactory>();
        services.AddSingleton<DomainConfiguration>((s) =>
        {
            var dcf = s.GetRequiredService<IDomainConfigurationFactory>();
            return dcf.Load();
        });

        // the library entry point
        services.AddSingleton<IKnxConfigFactory, KnxConfigFactory>();

       return services;
    }
}
