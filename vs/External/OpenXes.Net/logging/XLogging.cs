using System;
using System.Collections.Generic;

namespace OpenXesNet.logging
{
    public static class XLogging
    {
        static Dictionary<IXLoggingListener, Importance> listeners = new Dictionary<IXLoggingListener, Importance>();

        public static void AddListener(IXLoggingListener listener, Importance importance =Importance.INFO)
        {
            if(listeners.ContainsKey(listener)){
                listeners[listener] = importance;
            }else {
                listeners.Add(listener,importance);
            }
        }

        public static void RemoveListener(IXLoggingListener listener){
            lock(new object()){
                if(listeners.ContainsKey(listener)){
                    listeners.Remove(listener);
                }
            }
        }

        public static void Trace(String message)
        {
            Log(message, Importance.TRACE);
        }

        public static void Debug(String message)
        {
            Log(message, Importance.DEBUG);
        }

        public static void Info(String message)
        {
            Log(message, Importance.INFO);
        }

        public static void Warn(String message)
        {
            Log(message, Importance.WARNING);
        }

        public static void Error(String message)
        {
            Log(message, Importance.ERROR);
        }

        public static void Log(string message, Importance importance)
        {
            foreach(IXLoggingListener listener in listeners.Keys){
                Importance _importance = listeners[listener];
                if(_importance >= importance){
                    listener.Log(message, listeners[listener]);
                }
            }
        }

        public enum Importance
        {
            TRACE=0, DEBUG, INFO, WARNING, ERROR
        }
    }
}
