namespace SRF.Knx.Config.Exceptions
{
    public class KnxConfigurationException : ApplicationException
    {
        public KnxConfigurationException() : base()
        {
        }

        public KnxConfigurationException(string msg) : base(msg)
        {
        }

        public KnxConfigurationException(string msg, Exception inner) : base(msg, inner)
        {
        }
    }
}
