using System;
using OpenXesNet.model;
using OpenXesNet.factory;
using OpenXesNet.info;

namespace OpenXesNet.extension.std
{
    public class XOrganizationalExtension : XExtension
    {
        public static readonly Uri EXTENSION_URI = new UriBuilder("http://www.xes-standard.org/org.xesext").Uri;
        public static readonly String KEY_RESOURCE = "resource";
        public static readonly String KEY_ROLE = "role";
        public static readonly String KEY_GROUP = "group";
        public static IXAttributeLiteral ATTR_RESOURCE;
        public static IXAttributeLiteral ATTR_ROLE;
        public static IXAttributeLiteral ATTR_GROUP;
        static XOrganizationalExtension singleton = new XOrganizationalExtension();

        public static XOrganizationalExtension Instance
        {
            get { return singleton; }
        }

        XOrganizationalExtension() : base("Organizational", "org", EXTENSION_URI)
        {
            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;
            ATTR_RESOURCE = factory.CreateAttributeLiteral(KEY_RESOURCE, "__INVALID__", this);

            ATTR_ROLE = factory.CreateAttributeLiteral(KEY_ROLE, "__INVALID__", this);

            ATTR_GROUP = factory.CreateAttributeLiteral(KEY_GROUP, "__INVALID__", this);

            this.eventAttributes.Add(KEY_RESOURCE, (XAttribute)ATTR_RESOURCE.Clone());
            this.eventAttributes.Add(KEY_ROLE, (XAttribute)ATTR_ROLE.Clone());
            this.eventAttributes.Add(KEY_GROUP, (XAttribute)ATTR_GROUP.Clone());

            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_RESOURCE), "Resource");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_ROLE), "Role");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_GROUP), "Group");
            XGlobalAttributeNameMap.Instance.RegisterMapping("DE", QualifiedName(KEY_RESOURCE), "Akteur");
            XGlobalAttributeNameMap.Instance.RegisterMapping("DE", QualifiedName(KEY_ROLE), "Rolle");
            XGlobalAttributeNameMap.Instance.RegisterMapping("DE", QualifiedName(KEY_GROUP), "Gruppe");
            XGlobalAttributeNameMap.Instance.RegisterMapping("FR", QualifiedName(KEY_RESOURCE), "Agent");
            XGlobalAttributeNameMap.Instance.RegisterMapping("FR", QualifiedName(KEY_ROLE), "Rôle");
            XGlobalAttributeNameMap.Instance.RegisterMapping("FR", QualifiedName(KEY_GROUP), "Groupe");
            XGlobalAttributeNameMap.Instance.RegisterMapping("ES", QualifiedName(KEY_RESOURCE), "Recurso");
            XGlobalAttributeNameMap.Instance.RegisterMapping("ES", QualifiedName(KEY_ROLE), "Papel");
            XGlobalAttributeNameMap.Instance.RegisterMapping("ES", QualifiedName(KEY_GROUP), "Grupo");
            XGlobalAttributeNameMap.Instance.RegisterMapping("PT", QualifiedName(KEY_RESOURCE), "Recurso");
            XGlobalAttributeNameMap.Instance.RegisterMapping("PT", QualifiedName(KEY_ROLE), "Papel");
            XGlobalAttributeNameMap.Instance.RegisterMapping("PT", QualifiedName(KEY_GROUP), "Grupo");
        }

        public String extractResource(XEvent evt)
        {
            XAttribute attribute = evt.GetAttributes()[QualifiedName(KEY_RESOURCE)];
            if (attribute == null)
            {
                return null;
            }
            return ((XAttributeLiteral)attribute).Value;
        }

        public void assignResource(XEvent evt, String resource)
        {
            if ((resource != null) && (resource.Trim().Length > 0))
            {
                XAttributeLiteral attr = (XAttributeLiteral)ATTR_RESOURCE.Clone();
                attr.Value = resource.Trim();
                evt.GetAttributes().Add(QualifiedName(KEY_RESOURCE), attr);
            }
        }

        public String extractRole(XEvent evt)
        {
            XAttribute attribute = evt.GetAttributes()[QualifiedName(KEY_ROLE)];
            if (attribute == null)
            {
                return null;
            }
            return ((XAttributeLiteral)attribute).Value;
        }

        public void assignRole(XEvent evt, String role)
        {
            if ((role != null) && (role.Trim().Length > 0))
            {
                XAttributeLiteral attr = (XAttributeLiteral)ATTR_ROLE.Clone();
                attr.Value = role.Trim();
                evt.GetAttributes().Add(QualifiedName(KEY_ROLE), attr);
            }
        }

        public String extractGroup(XEvent evt)
        {
            XAttribute attribute = evt.GetAttributes()[QualifiedName(KEY_GROUP)];
            if (attribute == null)
            {
                return null;
            }
            return ((XAttributeLiteral)attribute).Value;
        }

        public void assignGroup(XEvent evt, String group)
        {
            if ((group != null) && (group.Trim().Length > 0))
            {
                XAttributeLiteral attr = (XAttributeLiteral)ATTR_GROUP.Clone();
                attr.Value = group.Trim();
                evt.GetAttributes().Add(QualifiedName(KEY_GROUP), attr);
            }
        }
    }
}
