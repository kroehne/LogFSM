using System;
using OpenXesNet.model;
using OpenXesNet.extension.std;

namespace OpenXesNet.extension.std.cost
{
    public class XCostAmount : XAbstractNestedAttributeSupport<double>
    {
        static XCostAmount singleton = new XCostAmount();

        public static XCostAmount Instance
        {
            get { return singleton; }

        }

        public override double ExtractValue(XAttribute attribute)
        {
            return XCostExtension.Instance.ExtractAmount((XAttributeContinuous)attribute);
        }

        public override void AssignValue(XAttribute attribute, double value)
        {
            XCostExtension.Instance.AssignAmount(attribute, value);
        }
    }
}
