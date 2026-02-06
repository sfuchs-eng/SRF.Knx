
namespace SRF.Knx.Config.OpenHab.Generate;

public interface IThing
{
    IEnumerable<IChannel> Channels { get; }
}
