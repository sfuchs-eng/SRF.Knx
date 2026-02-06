using SRF.Knx.Config.OpenHab.BaseConfig;

namespace SRF.Knx.Config.OpenHab.Generate;

public interface IBridge : IConfigGenerator
{
    public BridgeConfig Config { get; }
    IEnumerable<IThing> Things { get; }
}
