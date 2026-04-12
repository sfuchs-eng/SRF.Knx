namespace SRF.Knx.Core.Master;

/// <summary>
/// Base class for KNX master data providers, responsible for loading and providing access to the master data.
/// Concrete implementation can derive e.g. configuration values via application specific dependency injection (DI).
/// </summary>
public abstract class KnxMasterDataProvider : IKnxMasterDataProvider
{
    /// <summary>
    /// Gets the KNX master data, loading it if necessary.
    /// Concrete implementations can implement caching or other optimizations as needed.
    /// </summary>
    public abstract KnxMasterData GetMasterData();

    /// <summary>
    /// File based master data provider implementation that loads the master data from a specified XML file path.
    /// </summary>
    public KnxMasterData GetMasterDataFromFile(string filePath)
    {
        return KnxMasterDataLoader.LoadFromFile(filePath);
    }
}