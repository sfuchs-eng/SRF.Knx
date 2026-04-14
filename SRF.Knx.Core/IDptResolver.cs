using SRF.Knx.Core.DPT;

namespace SRF.Knx.Core;

/// <summary>
/// Resolves the Data Point Type (DPT) for a given group address.
/// E.g. required upon receiving or transmitting a group address event to determine the correct DPT for encoding, decoding or formatting the payload.
/// Preferrably DPT objects are cached and reused by the resolver. Also, DPT objects shall only be created and added to the cache when they are actually needed by an application.</br>
/// Implementations would typically inject an <see cref="IKnxMasterDataProvider"/> and an <see cref="IDptFactory"/>
/// while loading the Address vs. DPT mapping from an ETS group address export. The latter is provided by the `SRF.Knx.Config` package.
/// </summary>
public interface IDptResolver
{
    public DptBase GetDpt(GroupAddress groupAddress);
}
