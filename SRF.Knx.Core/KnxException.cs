namespace SRF.Knx.Core;

public class KnxException : System.Exception
{
    public KnxException() { }
    public KnxException(string message) : base(message) { }
    public KnxException(string message, Exception inner) : base(message, inner) { }
}
