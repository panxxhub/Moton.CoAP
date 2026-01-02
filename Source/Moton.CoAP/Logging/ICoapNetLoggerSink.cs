namespace Moton.CoAP.Logging
{
    public interface ICoapNetLoggerSink
    {
        void ProcessLogMessage(CoapNetLogMessage logMessage);
    }
}
