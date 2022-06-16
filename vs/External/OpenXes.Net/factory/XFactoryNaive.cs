using System;
using OpenXesNet.model;
using OpenXesNet.extension;
using OpenXesNet.id;

namespace OpenXesNet.factory
{
    /// <summary>
    /// This factory will create the naive implementations of all model hierarchy 
    /// elements, i.e., no buffering or further optimizations will be employed.
    /// </summary>
    public class XFactoryNaive : IXFactory
    {

        public string Author
        {
            get { return "Alvaro Martinez Romero"; }
        }

        public string Description
        {
            get { return "Creates naive implementations for all available model hierarchy elements, i.e., no optimizations will be employed."; }
        }

        public string Name
        {
            get { return "Standard / naive"; }
        }

        public Uri Uri
        {
            get { return new Uri("http://www.xes-standard.org/"); }
        }

        public string Vendor
        {
            get { return "xes-standard.org"; }
        }

        public XLog CreateLog()
        {
            return new XLog(new XAttributeMapLazy<XAttributeMap>());
        }

        public XLog CreateLog(IXAttributeMap attributes)
        {
            return new XLog(attributes);
        }

        public XTrace CreateTrace()
        {
            return new XTrace(new XAttributeMapLazy<XAttributeMap>());
        }

        public XTrace CreateTrace(IXAttributeMap attributes)
        {
            return new XTrace(attributes);
        }

        public XEvent CreateEvent()
        {
            return new XEvent();
        }

        public XEvent CreateEvent(IXAttributeMap attributes)
        {
            return new XEvent(attributes);
        }

        public XEvent CreateEvent(XID id, IXAttributeMap attributes)
        {
            return new XEvent(id, attributes);
        }

        public XAttributeMap CreateAttributeMap()
        {
            return new XAttributeMap();
        }

        public XAttributeBoolean CreateAttributeBoolean(string key, bool value, XExtension extension)
        {
            return new XAttributeBoolean(key, value, extension);
        }

        public XAttributeContinuous CreateAttributeContinuous(string key, double value, XExtension extension)
        {
            return new XAttributeContinuous(key, value, extension);
        }

        public XAttributeDiscrete CreateAttributeDiscrete(string key, long value, XExtension extension)
        {
            return new XAttributeDiscrete(key, value, extension);
        }

        public XAttributeLiteral CreateAttributeLiteral(string key, string value, XExtension extension)
        {
            return new XAttributeLiteral(key, value, extension);
        }

        public XAttributeTimestamp CreateAttributeTimestamp(string key, DateTime value, XExtension extension)
        {
            return new XAttributeTimestamp(key, value, extension);
        }

        public XAttributeTimestamp CreateAttributeTimestamp(string key, long millis, XExtension extension)
        {
            return new XAttributeTimestamp(key, millis, extension);
        }

        public XAttributeID CreateAttributeID(string key, XID value, XExtension extension)
        {
            return new XAttributeID(key, value, extension);
        }

        public XAttributeList CreateAttributeList(string key, XExtension extension)
        {
            return new XAttributeList(key, extension);
        }

        public XAttributeContainer CreateAttributeContainer(string key, XExtension extension)
        {
            return new XAttributeContainer(key, extension);
        }
    }
}
