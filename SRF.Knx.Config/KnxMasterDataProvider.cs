using Microsoft.Extensions.Options;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Config;

/// <summary>
/// Loads KNX master data from the folder configured in <see cref="KnxConfiguration.KnxMasterFolder"/>.
/// </summary>
public sealed class KnxMasterDataProvider(IOptions<KnxConfiguration> options) : SRF.Knx.Core.Master.KnxMasterDataProvider
{
    private KnxMasterData? _cache;

    /// <inheritdoc/>
    public override KnxMasterData GetMasterData()
    {
        _cache ??= GetMasterDataFromFile(
            Path.Combine(options.Value.KnxMasterFolder, "knx_master.xml"));
        return _cache;
    }
}
