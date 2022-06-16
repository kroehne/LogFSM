using System;
namespace OpenXesNet.logging
{
    public class XStdOutLoggingListener : IXLoggingListener
    {
        public void Log(string message, XLogging.Importance importance)
        {
            string trace = String.Format("[{0}] {1} - {2}", importance.ToString(), DateTime.Now.ToLocalTime().ToString(), message);
            System.Console.WriteLine(trace);
        }
    }
}
