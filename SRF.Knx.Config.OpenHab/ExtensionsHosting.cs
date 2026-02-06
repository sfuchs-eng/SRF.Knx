using Microsoft.Extensions.DependencyInjection;

namespace SRF.Knx.Config.OpenHab;

public static class ExtensionsHosting
{
    /// <summary>
    /// Ensure to call also <see cref="SRF.Knx.Config.ExtensionsHosting.AddKnxConfig(IServiceCollection, string?)"/>
    /// as this dependencies are not automatically registered by this method. This method only registers the OpenHAB KNX Config Factory, which is used to generate the OpenHAB KNX Config based on the KNX Config. The KNX Config management services are the <see cref="SRF.Knx.Config.ExtensionsHosting.AddKnxConfig(IServiceCollection, string?)"/> method.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static IServiceCollection AddKnxOpenHabConfig(this IServiceCollection services, string? sectionName = null)
    {
        // OpenHAB KNX Config Generator
        services.AddTransient<IOpenHabKnxConfigFactory, OpenHab.OpenHabKnxConfigFactory>();

       return services;
    }
}
