using SRF.Knx.Core.DPT;

namespace SRF.Knx.Core;

/// <summary>
/// Resolves the Data Point Type (DPT) for a given group address.
/// E.g. required upon receiving or transmitting a group address event to determine the correct DPT for encoding, decoding or formatting the payload.
/// Preferrably DPT objects are cached and reused by the resolver. Also, DPT objects shall only be created and added to the cache when they are actually needed by an application.</br>
/// Implementations would typically inject an <see cref="IKnxMasterDataProvider"/> and an <see cref="IDptFactory"/>
/// while loading the Address vs. DPT mapping from an ETS group address export. The latter is provided by the `SRF.Knx.Config` package.
/// </summary>
/// <remarks>
/// The resolver is used by the KNX connectivity provider in the `SRF.Knx` package to resolve the DPT for a group address when processing incoming or outgoing group address events.
/// The resolver is also used by the `KnxValueContainerBase` class in the `HomeCompanion.Integrations.Knx` package to resolve the DPT for group addresses when initializing KNX value containers based on the ETS group address export in the domain configuration.
/// As the IDptResolver is used in performance-sensitive code paths (e.g. processing incoming group address events), it should implement caching to avoid repeated lookups of the same group address. The cache should be thread-safe as group address events may be processed concurrently.
/// Ensure calling <see cref="ClearCache"/> when the cache needs to be invalidated due to changes in the underlying ETS group address export or DPT factory logic. It's primarily intended for use with constant KNX configurations.
/// </remarks>
public interface IDptResolver
{
    public DptBase GetDpt(GroupAddress groupAddress);

    public void ClearCache();
}
