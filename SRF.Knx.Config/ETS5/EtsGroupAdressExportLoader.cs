using Microsoft.Extensions.Logging;

namespace SRF.Knx.Config.ETS5;

public class EtsGroupAdressExportLoader(
    ILogger<EtsGroupAdressExportLoader> logger
)
{
    public GroupAddressExport? GroupAddressesExport { get; private set; }

    public IEnumerable<EtsGroupAddressConfig> GroupAddresses => GroupAddressesExport?.AllGroupAddresses
        ?? throw new InvalidOperationException("GroupAddressesExport is not loaded.");

    public EtsGroupAdressExportLoader Load(string exportFilePath)
    {
        logger.LogInformation("Loading ETS Group Address Export from {ExportFilePath}", exportFilePath);
        var xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(ETS5.GroupAddressExport));
        using var reader = System.IO.File.OpenRead(exportFilePath);
        GroupAddressesExport = (ETS5.GroupAddressExport)xmlSer.Deserialize(reader)!;
        return this;
    }

    private readonly ILogger<EtsGroupAdressExportLoader> logger = logger;
}
