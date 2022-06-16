using System;
using System.Collections.Generic;
using OpenXesNet.model;
using OpenXesNet.factory;
using OpenXesNet.info;
using OpenXesNet.logging;

namespace OpenXesNet.extension.std
{
    public class XTimeExtension : XExtension
    {
        public static readonly Uri EXTENSION_URI = new UriBuilder("http://www.xes-standard.org/time.xesext").Uri;
        public static readonly String KEY_TIMESTAMP = "timestamp";
        public static IXAttributeTimestamp ATTR_TIMESTAMP;
        static XTimeExtension singleton = new XTimeExtension();

        public static XTimeExtension Instance
        {
            get { return singleton; }
        }

        XTimeExtension() : base("Time", "time", EXTENSION_URI)
        {
            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;
            ATTR_TIMESTAMP = factory.CreateAttributeTimestamp(KEY_TIMESTAMP, 0L, this);

            this.eventAttributes.Add(KEY_TIMESTAMP, (XAttribute)ATTR_TIMESTAMP.Clone());

            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_TIMESTAMP), "Timestamp");
            XGlobalAttributeNameMap.Instance.RegisterMapping("DE", QualifiedName(KEY_TIMESTAMP), "Zeitstempel");
            XGlobalAttributeNameMap.Instance.RegisterMapping("FR", QualifiedName(KEY_TIMESTAMP), "Horodateur");
            XGlobalAttributeNameMap.Instance.RegisterMapping("ES", QualifiedName(KEY_TIMESTAMP), "Timestamp");
            XGlobalAttributeNameMap.Instance.RegisterMapping("PT", QualifiedName(KEY_TIMESTAMP), "Timestamp");
        }

        public DateTime? ExtractTimestamp(XEvent evt)
        {
            try{
                return  ((XAttributeTimestamp)evt.GetAttributes()[QualifiedName(KEY_TIMESTAMP)]).Value;
            }catch(KeyNotFoundException){
                XLogging.Log("Key '" + QualifiedName(KEY_TIMESTAMP) + "' not available", XLogging.Importance.WARNING);
                return null;
            }
        }

        public void AssignTimestamp(XEvent evt, DateTime timestamp)
        {
            AssignTimestamp(evt, timestamp.Ticks);
        }

        public void AssignTimestamp(XEvent evt, long time)
        {
            XAttributeTimestamp attr = (XAttributeTimestamp)ATTR_TIMESTAMP.Clone();
            attr.ValueMillis = time;
            evt.GetAttributes().Add(QualifiedName(KEY_TIMESTAMP), attr);
        }
    }
}
