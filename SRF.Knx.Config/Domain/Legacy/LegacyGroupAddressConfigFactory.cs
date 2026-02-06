using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SRF.Knx.Config.Exceptions;

namespace SRF.Knx.Config.Domain.Legacy;

public class LegacyGroupAddressConfigFactory(ILogger<LegacyGroupAddressConfigFactory> logger)
{
    private readonly ILogger<LegacyGroupAddressConfigFactory> logger = logger;

    public List<KnxGroupAddressConfig> Load(string gacFileName)
    {
        List<KnxGroupAddressConfig> gacs = [];

        var xd = XDocument.Load(gacFileName);
        if ( xd.Root == null || !"GroupAddressItemConfigurations".Equals(xd.Root.Name.LocalName) )
            throw new KnxConfigurationException("Failed to load GroupAddressConfig items. GroupAddressConfigurations parent element not found.");

        var gacsE = xd.Root.DescendantNodes()
                        .OfType<XElement>()
                        .Where(n => "GroupAddressConfig".Equals(n.Name.LocalName));

        int goodCount = 0;
        int failCount = 0;
        foreach (XElement x in gacsE)
        {
            try
            {
                gacs.Add(new KnxGroupAddressConfig(x));
                goodCount++;
            }
            catch (Exception)
            {
                failCount++;
            }
        }
        logger.LogInformation("Loaded {count} GroupAddressConfig (failed with {failCount})", goodCount, failCount);

        KnxGroupAddressConfig.PopulateAdditionalGAC(gacs);
        return gacs;
    }
}
