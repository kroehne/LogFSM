using System;
using OpenXesNet.factory;
using OpenXesNet.model;
using OpenXesNet.info;
using OpenXesNet.id;

namespace OpenXesNet.extension.std
{
    public class XMicroExtension : XExtension
    {
        public static readonly Uri EXTENSION_URI = new UriBuilder("http://www.xes-standard.org/micro.xesext").Uri;
        public static readonly String PREFIX = "micro";
        public static readonly String KEY_LENGTH = "length";
        public static readonly String KEY_LEVEL = "level";
        public static readonly String KEY_PID = "parentId";
        public static IXAttributeDiscrete ATTR_LENGTH;
        public static IXAttributeDiscrete ATTR_LEVEL;
        public static IXAttributeID ATTR_PID;
        static XMicroExtension singleton = new XMicroExtension();

        public static XMicroExtension Instance
        {
            get { return singleton; }
        }


        XMicroExtension() : base("Micro", "micro", EXTENSION_URI)
        {
            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;

            ATTR_LENGTH = factory.CreateAttributeDiscrete(KEY_LENGTH, -1L, this);
            ATTR_PID = factory.CreateAttributeID(KEY_PID, new XID(), this);
            ATTR_LEVEL = factory.CreateAttributeDiscrete(KEY_LEVEL, -1L, this);

            this.eventAttributes.Add(KEY_PID, (XAttribute)ATTR_PID.Clone());
            this.eventAttributes.Add(KEY_LEVEL, (XAttribute)ATTR_LEVEL.Clone());
            this.eventAttributes.Add(KEY_LENGTH, (XAttribute)ATTR_LENGTH.Clone());

            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_PID), "Id of parent event of this event");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_LEVEL), "Micro level of this event");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_LENGTH), "Number of child events for this event");
        }

        public long ExtractLength(XEvent evt)
        {
            return Extract(evt, KEY_LENGTH, -1L);
        }

        public void AssignLength(XEvent evt, long length)
        {
            XAttributeDiscrete attr = (XAttributeDiscrete)ATTR_LENGTH.Clone();
            attr.Value = length;
            evt.GetAttributes().Add(QualifiedName(KEY_LENGTH), attr);
        }

        public void RemoveLength(IXAttributable evt)
        {
            Remove(evt, KEY_LENGTH);
        }

        public long ExtractLevel(XEvent evt)
        {
            return Extract(evt, KEY_LEVEL, -1L);
        }

        public void AssignLevel(XEvent evt, long level)
        {
            XAttributeDiscrete attr = (XAttributeDiscrete)ATTR_LEVEL.Clone();
            attr.Value = level;
            evt.GetAttributes().Add(QualifiedName(KEY_LEVEL), attr);
        }

        public void RemoveLevel(IXAttributable evt)
        {
            Remove(evt, KEY_LEVEL);
        }

        public XID ExtractParentId(XEvent evt)
        {
            return Extract(evt, KEY_PID, null);
        }

        public void AssignParentId(XEvent evt, XID parentId)
        {
            Assign(evt, KEY_PID, parentId);
        }

        public void RemoveParentId(IXAttributable evt)
        {
            Remove(evt, KEY_PID);
        }

        XID Extract(IXAttributable element, string definedAttribute, XID defaultValue)
        {
            XAttribute attribute = element.GetAttributes()[QualifiedName(definedAttribute)];
            if (attribute == null)
            {
                return defaultValue;
            }
            return ((XAttributeID)attribute).Value;
        }

        void Assign(IXAttributable element, string definedAttribute, XID value)
        {
            XAttributeID attr = (XAttributeID)ATTR_PID.Clone();
            attr.Value = value;
            element.GetAttributes().Add(QualifiedName(definedAttribute), attr);
        }

        long Extract(IXAttributable element, string definedAttribute, long defaultValue)
        {
            XAttribute attribute = element.GetAttributes()[QualifiedName(definedAttribute)];
            if (attribute == null)
            {
                return defaultValue;
            }
            return ((XAttributeDiscrete)attribute).Value;
        }

        void Remove(IXAttributable element, string definedAttribute)
        {
            element.GetAttributes().Remove(QualifiedName(definedAttribute));
        }
    }
}
