using SRF.Knx.Core;
using SRF.Knx.Core.DPT;
using SRF.Knx.Core.Master;

namespace SRF.Knx.Test.Core;

public static class KnxMasterDataUtils
{
    public static bool TryGetKnxMasterData(out string filename, out KnxMasterData masterData, out IKnxMasterDataProvider provider)
    {
        var baseDir = Path.GetDirectoryName(typeof(DptFactoryTests).Assembly.Location) ?? "";
        var knxMasterFilePath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "SRF.Knx.Config", "Resources", "knx_master.xml"));
        filename = knxMasterFilePath;

        if (!File.Exists(knxMasterFilePath))
        {
            masterData = null!;
            provider = null!;
            return false;
        }

        masterData = KnxMasterDataLoader.LoadFromFile(knxMasterFilePath);
        provider = new KnxMasterDataProviderStub(masterData);
        return true;
    }

    private sealed class KnxMasterDataProviderStub(KnxMasterData masterData) : IKnxMasterDataProvider
    {
        public KnxMasterData GetMasterData() => masterData;

        public bool TryGetDptMaster(DataPointTypeId dptId, out DatapointType? dpt, out DatapointSubtype? dptSubtype)
        {
            throw new NotImplementedException();
        }
    }
}
