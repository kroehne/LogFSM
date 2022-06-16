using System;
using OpenXesNet.model;

namespace OpenXesNet.extension.std.cost
{
    public class XCostType : XAbstractNestedAttributeSupport<string>
    {
        static XCostType singleton = new XCostType();

        public static XCostType Instance
        {
            get { return singleton; }
        }

        public override string ExtractValue(XAttribute attribute)
        {
            return XCostExtension.Instance.ExtractType(attribute);
        }

        public override void AssignValue(XAttribute attribute, string value)
        {
            XCostExtension.Instance.AssignType(attribute, value);
        }
    }
}
