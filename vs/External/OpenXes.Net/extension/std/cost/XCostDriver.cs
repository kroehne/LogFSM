using System;
using OpenXesNet.model;
namespace OpenXesNet.extension.std.cost
{
    public class XCostDriver : XAbstractNestedAttributeSupport<String>
    {
        static XCostDriver singleton = new XCostDriver();

        public static XCostDriver Instance
        {
            get
            { return singleton; }

        }

        public override string ExtractValue(XAttribute attribute)
        {
            return XCostExtension.Instance.ExtractDriver(attribute);
        }

        public override void AssignValue(XAttribute attribute, String value)
        {
            XCostExtension.Instance.AssignDriver(attribute, value);
        }
    }
}
