using System;
namespace OpenXesNet.logging
{
    public interface IXLoggingListener
    {
        void Log(string message, XLogging.Importance importance);
    }
}
