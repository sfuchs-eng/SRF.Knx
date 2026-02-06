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
        //TODO: implement this using master data
        throw new NotImplementedException();
    }
}
