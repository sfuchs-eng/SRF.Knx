using Microsoft.Extensions.Logging;

namespace SRF.Knx.Core.DPT;

public class DptFactory : IDptFactory
{
    public DptFactory(
        Master.KnxMasterData masterData,
        ILogger<DptFactory> logger)
    {
        this.masterData = masterData;
        this.logger = logger;
    }

    private readonly Master.KnxMasterData masterData;
    private readonly ILogger<DptFactory> logger;

    public DptBase Get(int main, int sub)
    {
        // Get master data for main and sub
        var dptMainMaster = masterData.MasterData?.DatapointTypes?.DatapointType.FirstOrDefault(d => d.Number == main);
        if ( dptMainMaster == null )
        {
            logger.LogWarning("DPT main number {Main} not found in master data", main);
            throw new ArgumentOutOfRangeException(nameof(main), $"DPT main number {main} not found in master data");
        }
        var dptSub = dptMainMaster?.DatapointSubtypes?.DatapointSubtype.FirstOrDefault(s => s.Number == sub);
        if (dptSub == null)
        {
            logger.LogWarning("DPT subtype number {Sub} not found for main {Main} in master data", sub, main);
            throw new ArgumentOutOfRangeException(nameof(sub), $"DPT subtype number {sub} not found for main {main} in master data");
        }

        // 
        ///dptSub.PDT
        throw new NotImplementedException();
    }
}
