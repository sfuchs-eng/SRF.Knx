namespace SRF.Knx.Config.Exceptions;

public class KnxAddressFormatException : KnxConfigurationException {
    public KnxAddressFormatException(string message, Exception inner) : base(message, inner) { }
}
