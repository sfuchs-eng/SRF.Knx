namespace SRF.Knx.Config.OpenHab.Generate
{
    public interface IConfigGenerator
    {
        void WriteConfig(TextWriter to);
    }
}