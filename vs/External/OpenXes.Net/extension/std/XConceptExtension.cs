using System;
using System.Collections.Generic;
using OpenXesNet.extension;
using OpenXesNet.model;
using OpenXesNet.factory;
using OpenXesNet.info;
using OpenXesNet.logging;

namespace OpenXesNet.extension.std
{
    public class XConceptExtension : XExtension
    {
        public static readonly Uri EXTENSION_URI = new UriBuilder("http://www.xes-standard.org/concept.xesext").Uri;
        public static readonly String KEY_NAME = "name";
        public static readonly String KEY_INSTANCE = "instance";
        public static IXAttributeLiteral ATTR_NAME;
        public static IXAttributeLiteral ATTR_INSTANCE;
        static XConceptExtension singleton = new XConceptExtension();

        public static XConceptExtension Instance
        {
            get
            {
                return singleton;
            }
        }

        XConceptExtension() : base("Concept", "concept", EXTENSION_URI)
        {
            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;

            ATTR_NAME = factory.CreateAttributeLiteral(KEY_NAME, "__INVALID__", this);
            ATTR_INSTANCE = factory.CreateAttributeLiteral(KEY_INSTANCE, "__INVALID__", this);

            this.logAttributes.Add(KEY_NAME, (XAttribute)ATTR_NAME.Clone());
            this.traceAttributes.Add(KEY_NAME, (XAttribute)ATTR_NAME.Clone());
            this.eventAttributes.Add(KEY_NAME, (XAttribute)ATTR_NAME.Clone());
            this.eventAttributes.Add(KEY_INSTANCE, (XAttribute)ATTR_INSTANCE.Clone());

            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_NAME), "Name");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_INSTANCE), "Instance");
            XGlobalAttributeNameMap.Instance.RegisterMapping("DE", QualifiedName(KEY_NAME), "Name");
            XGlobalAttributeNameMap.Instance.RegisterMapping("DE", QualifiedName(KEY_INSTANCE), "Instanz");
            XGlobalAttributeNameMap.Instance.RegisterMapping("FR", QualifiedName(KEY_NAME), "Appellation");
            XGlobalAttributeNameMap.Instance.RegisterMapping("FR", QualifiedName(KEY_INSTANCE), "Entité");
            XGlobalAttributeNameMap.Instance.RegisterMapping("ES", QualifiedName(KEY_NAME), "Nombre");
            XGlobalAttributeNameMap.Instance.RegisterMapping("ES", QualifiedName(KEY_INSTANCE), "Instancia");
            XGlobalAttributeNameMap.Instance.RegisterMapping("PT", QualifiedName(KEY_NAME), "Nome");
            XGlobalAttributeNameMap.Instance.RegisterMapping("PT", QualifiedName(KEY_INSTANCE), "Instância");
        }

        public string ExtractName(IXElement element)
        {
            try
            {
                return ((XAttributeLiteral)element.GetAttributes()[QualifiedName(KEY_NAME)]).Value;
            }
            catch (KeyNotFoundException)
            {
                XLogging.Log("Key '" + KEY_NAME + "' not available", XLogging.Importance.WARNING);
                return null;
            }
        }

        public void AssignName(IXElement element, string name)
        {
            if ((name != null) && (name.Trim().Length > 0))
            {
                XAttributeLiteral attr = (XAttributeLiteral)ATTR_NAME.Clone();
                attr.Value = name;
                element.GetAttributes().Add(QualifiedName(KEY_NAME), attr);
            }
        }

        public string ExtractInstance(XEvent evt)
        {
            try
            {
                return ((XAttributeLiteral)evt.GetAttributes()[QualifiedName(KEY_INSTANCE)]).Value;
            }
            catch (KeyNotFoundException)
            {
                XLogging.Log("Key '" + QualifiedName(KEY_INSTANCE) + "' not available", XLogging.Importance.WARNING);
                return null;
            }
        }

        public void AssignInstance(XEvent evt, string instance)
        {
            if ((instance != null) && (instance.Trim().Length > 0))
            {
                XAttributeLiteral attr = (XAttributeLiteral)ATTR_INSTANCE.Clone();
                attr.Value =instance;
                evt.GetAttributes().Add(QualifiedName(KEY_INSTANCE), attr);
            }
        }
    }
}
