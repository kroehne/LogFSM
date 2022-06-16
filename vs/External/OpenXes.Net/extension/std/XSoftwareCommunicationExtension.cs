using System;
using OpenXesNet.model;
using OpenXesNet.factory;
using OpenXesNet.info;

namespace OpenXesNet.extension.std
{
    public class XSoftwareCommunicationExtension : XExtension
    {
        public static readonly Uri EXTENSION_URI = new UriBuilder("http://www.xes-standard.org/swcomm.xesext").Uri;
        public static readonly string PREFIX = "swcomm";
        public static readonly string KEY_LOCAL_HOST = "localHost";
        public static readonly string KEY_LOCAL_PORT = "localPort";
        public static readonly string KEY_REMOTE_HOST = "remoteHost";
        public static readonly string KEY_REMOTE_PORT = "remotePort";
        public static IXAttributeLiteral ATTR_LOCAL_HOST;
        public static IXAttributeDiscrete ATTR_LOCAL_PORT;
        public static IXAttributeLiteral ATTR_REMOTE_HOST;
        public static IXAttributeDiscrete ATTR_REMOTE_PORT;
        static XSoftwareCommunicationExtension singleton = new XSoftwareCommunicationExtension();

        public static XSoftwareCommunicationExtension Instance
        {
            get { return singleton; }
        }

        XSoftwareCommunicationExtension() : base("Software Communication", "swcomm", EXTENSION_URI)
        {

            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;

            ATTR_LOCAL_HOST = factory.CreateAttributeLiteral(KEY_LOCAL_HOST, "__INVALID__", this);
            ATTR_REMOTE_HOST = factory.CreateAttributeLiteral(KEY_REMOTE_HOST, "__INVALID__", this);
            ATTR_LOCAL_PORT = factory.CreateAttributeDiscrete(KEY_LOCAL_PORT, -1L, this);
            ATTR_REMOTE_PORT = factory.CreateAttributeDiscrete(KEY_REMOTE_PORT, -1L, this);

            this.eventAttributes.Add(KEY_LOCAL_HOST, (XAttribute)ATTR_LOCAL_HOST.Clone());
            this.eventAttributes.Add(KEY_LOCAL_PORT, (XAttribute)ATTR_LOCAL_PORT.Clone());
            this.eventAttributes.Add(KEY_REMOTE_HOST, (XAttribute)ATTR_REMOTE_HOST.Clone());
            this.eventAttributes.Add(KEY_REMOTE_PORT, (XAttribute)ATTR_REMOTE_PORT.Clone());


            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_LOCAL_HOST), "Local endpoint - host name");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_LOCAL_PORT), "Local endpoint - port");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_REMOTE_HOST), "Remote endpoint - host name");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_REMOTE_PORT), "Remote endpoint - port");
        }


        public string ExtractLocalHost(XEvent evt)
        {
            return Extract(evt, KEY_LOCAL_HOST, null);
        }

        public XAttributeLiteral AssignLocalHost(XEvent evt, string localHost)
        {
            return Assign(evt, (XAttributeLiteral)ATTR_LOCAL_HOST, localHost);
        }

        public long ExtractLocalPort(XEvent evt)
        {
            return Extract(evt, KEY_LOCAL_PORT, -1L);
        }

        public XAttributeDiscrete assignLocalPort(XEvent evt, long localPort)
        {
            return Assign(evt, (XAttributeDiscrete)ATTR_LOCAL_PORT, localPort);
        }

        public String ExtractRemoteHost(XEvent evt)
        {
            return Extract(evt, KEY_REMOTE_HOST, null);
        }

        public XAttributeLiteral AssignRemoteHost(XEvent evt, string remoteHost)
        {
            return Assign(evt, (XAttributeLiteral)ATTR_REMOTE_HOST, remoteHost);
        }

        public long ExtractRemotePort(XEvent evt)
        {
            return Extract(evt, KEY_REMOTE_PORT, -1L);
        }

        public XAttributeDiscrete AssignRemotePort(XEvent evt, long remotePort)
        {
            return Assign(evt, (XAttributeDiscrete)ATTR_REMOTE_PORT, remotePort);
        }

        long Extract(IXAttributable element, string definedAttribute, long defaultValue)
        {
            XAttribute attribute = element.GetAttributes()[definedAttribute];

            if (attribute == null)
            {
                return defaultValue;
            }
            return ((XAttributeDiscrete)attribute).Value;
        }

        XAttributeDiscrete Assign(IXAttributable element, XAttributeDiscrete definedAttribute, long value)
        {
            XAttributeDiscrete attr = (XAttributeDiscrete)definedAttribute.Clone();

            attr.Value = value;
            element.GetAttributes().Add(definedAttribute.Key, attr);
            return attr;
        }

        string Extract(IXAttributable element, string definedAttribute, string defaultValue)
        {
            XAttribute attribute = element.GetAttributes()[definedAttribute];

            if (attribute == null)
            {
                return defaultValue;
            }
            return ((XAttributeLiteral)attribute).Value;
        }

        XAttributeLiteral Assign(IXAttributable element, XAttributeLiteral definedAttribute, string value)
        {
            if (value != null)
            {
                XAttributeLiteral attr = (XAttributeLiteral)definedAttribute.Clone();

                attr.Value = value;
                element.GetAttributes().Add(definedAttribute.Key, attr);
                return attr;
            }
            return null;
        }
    }
}
