using System;
using OpenXesNet.model;
using OpenXesNet.factory;
using OpenXesNet.info;
using System.Linq;

namespace OpenXesNet.extension.std
{
    public class XLifecycleExtension : XExtension
    {
        public static readonly Uri EXTENSION_URI = new UriBuilder("http://www.xes-standard.org/lifecycle.xesext").Uri;
        public static readonly String KEY_MODEL = "model";
        public static readonly String KEY_TRANSITION = "transition";
        public static readonly String VALUE_MODEL_STANDARD = "standard";
        public static IXAttributeLiteral ATTR_MODEL;
        public static IXAttributeLiteral ATTR_TRANSITION;
        static XLifecycleExtension singleton = new XLifecycleExtension();

        public static XLifecycleExtension Instance
        {
            get { return singleton; }
        }

        Object readResolve()
        {
            return singleton;
        }

        XLifecycleExtension() : base("Lifecycle", "lifecycle", EXTENSION_URI)
        {
            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;
            ATTR_MODEL = factory.CreateAttributeLiteral(KEY_MODEL, "standard", this);
            ATTR_TRANSITION = factory.CreateAttributeLiteral(KEY_TRANSITION, StandardModel.COMPLETE.ToString(),
                    this);

            this.logAttributes.Add(KEY_MODEL, (XAttributeLiteral)ATTR_MODEL.Clone());
            this.eventAttributes.Add(KEY_TRANSITION, (XAttributeLiteral)ATTR_TRANSITION.Clone());

            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_MODEL), "Lifecycle Model");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_TRANSITION), "Lifecycle Transition");
            XGlobalAttributeNameMap.Instance.RegisterMapping("DE", QualifiedName(KEY_MODEL), "Lebenszyklus-Model");
            XGlobalAttributeNameMap.Instance.RegisterMapping("DE", QualifiedName(KEY_TRANSITION), "Lebenszyklus-Transition");
            XGlobalAttributeNameMap.Instance.RegisterMapping("FR", QualifiedName(KEY_MODEL), "Modèle du Cycle Vital");
            XGlobalAttributeNameMap.Instance.RegisterMapping("FR", QualifiedName(KEY_TRANSITION), "Transition en Cycle Vital");
            XGlobalAttributeNameMap.Instance.RegisterMapping("ES", QualifiedName(KEY_MODEL), "Modelo de Ciclo de Vida");
            XGlobalAttributeNameMap.Instance.RegisterMapping("ES", QualifiedName(KEY_TRANSITION), "Transición en Ciclo de Vida");
            XGlobalAttributeNameMap.Instance.RegisterMapping("PT", QualifiedName(KEY_MODEL), "Modelo do Ciclo de Vida");
            XGlobalAttributeNameMap.Instance.RegisterMapping("PT", QualifiedName(KEY_TRANSITION), "Transição do Ciclo de Vida");
        }

        public string ExtractModel(XLog log)
        {
            XAttribute attribute = log.GetAttributes()[QualifiedName(KEY_MODEL)];
            if (attribute == null)
            {
                return null;
            }
            return ((XAttributeLiteral)attribute).Value;
        }

        public void AssignModel(XLog log, string model)
        {
            if ((model != null) && (model.Trim().Length > 0))
            {
                XAttributeLiteral modelAttr = (XAttributeLiteral)ATTR_MODEL.Clone();

                modelAttr.Value = model.Trim();
                log.GetAttributes().Add(QualifiedName(KEY_MODEL), modelAttr);
            }
        }

        public bool UsesStandardModel(XLog log)
        {
            string model = ExtractModel(log);
            if (model == null)
            {
                return false;
            }
            return (model.Trim().Equals("standard"));
        }

        public string ExtractTransition(XEvent evt)
        {
            XAttribute attribute = evt.GetAttributes()[QualifiedName(KEY_TRANSITION)];
            if (attribute == null)
            {
                return null;
            }
            return ((XAttributeLiteral)attribute).Value;
        }

        public StandardModel ExtractStandardTransition(XEvent evt)
        {
            String transition = ExtractTransition(evt);
            if (transition != null)
            {
                return Decode(transition);
            }
            return StandardModel.UNKNOWN;
        }

        public void AssignTransition(XEvent evt, string transition)
        {
            if ((transition != null) && (transition.Trim().Length > 0))
            {
                XAttributeLiteral transAttr = (XAttributeLiteral)ATTR_TRANSITION.Clone();

                transAttr.Value = transition.Trim();
                evt.GetAttributes().Add(QualifiedName(KEY_TRANSITION), transAttr);
            }
        }

        public void AssignStandardTransition(XEvent evt, StandardModel transition)
        {
            AssignTransition(evt, transition.ToString());
        }

        public enum StandardModel
        {
            SCHEDULE, ASSIGN, WITHDRAW, REASSIGN, START, SUSPEND,
            RESUME, PI_ABORT, ATE_ABORT, COMPLETE, AUTOSKIP, MANUALSKIP, UNKNOWN
        }



        public static StandardModel Decode(string encoding)
        {
            encoding = encoding.Trim().ToLower();
            foreach (String transition in Enum.GetNames(typeof(StandardModel)))
            {
                if (transition.Equals(encoding))
                {
                    return (StandardModel)Enum.Parse(typeof(StandardModel), transition);
                }
            }
            return StandardModel.UNKNOWN;
        }
    }
}
