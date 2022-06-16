using System;
using OpenXesNet.factory;
using OpenXesNet.model;
using OpenXesNet.info;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenXesNet.extension.std
{
    public class XSemanticExtension : XExtension
    {
        public static readonly Uri EXTENSION_URI = new UriBuilder("http://www.xes-standard.org/semantic.xesext").Uri;
        public static readonly String KEY_MODELREFERENCE = "modelReference";
        public static IXAttributeLiteral ATTR_MODELREFERENCE;
        static XSemanticExtension singleton = new XSemanticExtension();

        public static XSemanticExtension Instance
        {
            get { return singleton; }
        }

        XSemanticExtension() : base("Semantic", "semantic", EXTENSION_URI)
        {
            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;
            ATTR_MODELREFERENCE = factory.CreateAttributeLiteral(KEY_MODELREFERENCE, "__INVALID__", this);

            this.logAttributes.Add(KEY_MODELREFERENCE, (XAttribute)ATTR_MODELREFERENCE.Clone());
            this.traceAttributes.Add(KEY_MODELREFERENCE, (XAttribute)ATTR_MODELREFERENCE.Clone());
            this.eventAttributes.Add(KEY_MODELREFERENCE, (XAttribute)ATTR_MODELREFERENCE.Clone());
            this.metaAttributes.Add(KEY_MODELREFERENCE, (XAttribute)ATTR_MODELREFERENCE.Clone());

            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_MODELREFERENCE), "Ontology Model Reference");
            XGlobalAttributeNameMap.Instance.RegisterMapping("DE", QualifiedName(KEY_MODELREFERENCE), "Ontologie-Modellreferenz");
            XGlobalAttributeNameMap.Instance.RegisterMapping("FR", QualifiedName(KEY_MODELREFERENCE),
                    "Référence au Modèle Ontologique");
            XGlobalAttributeNameMap.Instance.RegisterMapping("ES", QualifiedName(KEY_MODELREFERENCE),
                    "Referencia de Modelo Ontológico");
            XGlobalAttributeNameMap.Instance.RegisterMapping("PT", QualifiedName(KEY_MODELREFERENCE),
                    "Referência de Modelo Ontológico");
        }

        public IList<string> ExtractModelReferences(IXAttributable target)
        {
            IList<string> modelReferences = new List<string>();
            XAttributeLiteral modelReferenceAttribute = (XAttributeLiteral)target.GetAttributes()[QualifiedName(KEY_MODELREFERENCE)];

            if (modelReferenceAttribute != null)
            {
                String refString = modelReferenceAttribute.Value.Trim();
                foreach (Match m in Regex.Matches(refString, @"\\s"))
                {
                    modelReferences.Add(m.Value.Trim());
                }
            }
            return modelReferences;
        }

        public IList<Uri> ExtractModelReferenceURIs(IXAttributable target)
        {
            IList<string> refStrings = ExtractModelReferences(target);
            IList<Uri> refURIs = new List<Uri>(refStrings.Count);
            foreach (String refString in refStrings)
            {
                refURIs.Add(new UriBuilder(refString).Uri);
            }
            return refURIs;
        }

        public void AssignModelReferences(IXAttributable target, List<string> modelReferences)
        {
            StringBuilder sb = new StringBuilder();
            foreach (String mRef in modelReferences)
            {
                sb.Append(mRef);
                sb.Append(" ");
            }
            if (sb.ToString().Trim().Length > 0)
            {
                XAttributeLiteral attr = (XAttributeLiteral)ATTR_MODELREFERENCE.Clone();

                attr.Value = sb.ToString().Trim();
                target.GetAttributes().Add(QualifiedName(KEY_MODELREFERENCE), attr);
            }
        }

        public void AssignModelReferenceUris(IXAttributable target, List<Uri> modelReferenceURIs)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Uri mUri in modelReferenceURIs)
            {
                sb.Append(mUri.ToString());
                sb.Append(" ");
            }
            if (sb.ToString().Trim().Length > 0)
            {
                XAttributeLiteral attr = (XAttributeLiteral)ATTR_MODELREFERENCE.Clone();

                attr.Value = sb.ToString().Trim();
                target.GetAttributes().Add(QualifiedName(KEY_MODELREFERENCE), attr);
            }
        }
    }
}
