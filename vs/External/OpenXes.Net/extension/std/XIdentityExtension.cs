using System;
using System.Collections.Generic;
using OpenXesNet.model;
using OpenXesNet.factory;
using OpenXesNet.id;
using OpenXesNet.info;
using OpenXesNet.logging;

namespace OpenXesNet.extension.std
{
    public class XIdentityExtension : XExtension
    {
        public static readonly Uri EXTENSION_URI = new UriBuilder("http://www.xes-standard.org/identity.xesext").Uri;
        public static readonly String KEY_ID = "id";
        public static IXAttributeID ATTR_ID;
        static XIdentityExtension singleton = new XIdentityExtension();

        public static XIdentityExtension Instance
        {
            get { return singleton; }
        }

        XIdentityExtension() : base("Identity", "identity", EXTENSION_URI)
        {
            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;
            ATTR_ID = factory.CreateAttributeID(KEY_ID, XIDFactory.Instance.CreateId(), this);

            this.logAttributes.Add(KEY_ID, (XAttribute)ATTR_ID.Clone());
            this.traceAttributes.Add(KEY_ID, (XAttribute)ATTR_ID.Clone());
            this.eventAttributes.Add(KEY_ID, (XAttribute)ATTR_ID.Clone());
            this.metaAttributes.Add(KEY_ID, (XAttribute)ATTR_ID.Clone());

            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_ID), "Identity");
            XGlobalAttributeNameMap.Instance.RegisterMapping("DE", QualifiedName(KEY_ID), "Identität");
            XGlobalAttributeNameMap.Instance.RegisterMapping("FR", QualifiedName(KEY_ID), "Identité");
            XGlobalAttributeNameMap.Instance.RegisterMapping("ES", QualifiedName(KEY_ID), "Identidad");
            XGlobalAttributeNameMap.Instance.RegisterMapping("PT", QualifiedName(KEY_ID), "Identidade");
        }

        public XID ExtractID(IXAttributable element)
        {
            try
            {
                return ((XAttributeID)element.GetAttributes()[QualifiedName(KEY_ID)]).Value;
            }
            catch (KeyNotFoundException)
            {
                XLogging.Log("Key '" + QualifiedName(KEY_ID) + "' not available", XLogging.Importance.WARNING);
                return null;
            }
        }

        public void AssignID(IXAttributable element, XID id)
        {
            if (id != null)
            {
                XAttributeID attr = (XAttributeID)ATTR_ID.Clone();
                attr.Value = id;
                element.GetAttributes().Add(QualifiedName(KEY_ID), attr);
            }
        }
    }
}
