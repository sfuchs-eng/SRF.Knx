using SRF.Knx.Core.Master;

namespace SRF.Knx.Core;

public interface IKnxMasterDataProvider
{
    /// <summary>
    /// Gets the KNX master data containing datapoint types and property data types.
    /// E.g. derive implementations from <see cref="Master.KnxMasterDataProvider"/>.
    /// </summary>
    /// <returns>The KNX master data.</returns>
    KnxMasterData GetMasterData();
}