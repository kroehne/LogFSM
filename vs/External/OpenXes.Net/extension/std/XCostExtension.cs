using System;
using OpenXesNet.model;
using OpenXesNet.info;
using OpenXesNet.factory;
using System.Collections.Generic;
using OpenXesNet.extension.std.cost;

namespace OpenXesNet.extension.std
{
    public class XCostExtension : XExtension
    {
        public static readonly Uri EXTENSION_URI = new UriBuilder("http://www.xes-standard.org/cost.xesext").Uri;
        public static readonly string KEY_TOTAL = "total";
        public static readonly string KEY_CURRENCY = "currency";
        public static readonly string KEY_AMOUNT = "amount";
        public static readonly string KEY_DRIVER = "driver";
        public static readonly string KEY_TYPE = "type";
        public static IXAttributeContinuous ATTR_TOTAL;
        public static IXAttributeLiteral ATTR_CURRENCY;
        public static IXAttributeContinuous ATTR_AMOUNT;
        public static IXAttributeLiteral ATTR_DRIVER;
        public static IXAttributeLiteral ATTR_TYPE;
        static XCostExtension singleton = new XCostExtension();

        public static XCostExtension Instance
        {
            get { return singleton; }

        }

        XCostExtension() : base("Cost", "cost", EXTENSION_URI)
        {
            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;

            ATTR_TOTAL = factory.CreateAttributeContinuous(KEY_TOTAL, 0.0D, this);
            ATTR_CURRENCY = factory.CreateAttributeLiteral(KEY_CURRENCY, "__INVALID__", this);
            ATTR_AMOUNT = factory.CreateAttributeContinuous(KEY_AMOUNT, 0.0D, this);
            ATTR_DRIVER = factory.CreateAttributeLiteral(KEY_DRIVER, "__INVALID__", this);
            ATTR_TYPE = factory.CreateAttributeLiteral(KEY_TYPE, "__INVALID__", this);

            this.traceAttributes.Add(KEY_TOTAL, (XAttribute)ATTR_TOTAL.Clone());
            this.traceAttributes.Add(KEY_CURRENCY, (XAttribute)ATTR_CURRENCY.Clone());
            this.eventAttributes.Add(KEY_TOTAL, (XAttribute)ATTR_TOTAL.Clone());
            this.eventAttributes.Add(KEY_CURRENCY, (XAttribute)ATTR_CURRENCY.Clone());
            this.eventAttributes.Add(KEY_AMOUNT, (XAttribute)ATTR_AMOUNT.Clone());
            this.eventAttributes.Add(KEY_DRIVER, (XAttribute)ATTR_DRIVER.Clone());
            this.eventAttributes.Add(KEY_TYPE, (XAttribute)ATTR_TYPE.Clone());

            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_TOTAL), "Total Cost");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CURRENCY), "Currency of Cost");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_AMOUNT), "Cost Amount");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_DRIVER), "Cost Driver");
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_TYPE), "Cost Type");
        }

        public double ExtractTotal(XTrace trace)
        {
            return ExtractTotalPrivate(trace);
        }

        public double ExtractTotal(XEvent evt)
        {
            return ExtractTotalPrivate(evt);
        }

        double ExtractTotalPrivate(IXAttributable element)
        {
            XAttribute attribute = element.GetAttributes()[QualifiedName(KEY_TOTAL)];
            if (attribute == null)
            {
                return -1;
            }
            return ((XAttributeContinuous)attribute).Value;
        }

        public void AssignTotal(XTrace trace, double total)
        {
            AssignTotalPrivate(trace, total);
        }

        public void AssignTotal(XEvent evt, double total)
        {
            AssignTotalPrivate(evt, total);
        }

        void AssignTotalPrivate(IXAttributable element, double total)
        {
            if (total > 0.0D)
            {
                XAttributeContinuous attr = (XAttributeContinuous)ATTR_TOTAL.Clone();

                attr.Value = total;
                element.GetAttributes().Add(QualifiedName(KEY_TOTAL), attr);
            }
        }

        public string ExtractCurrency(XTrace trace)
        {
            return ExtractCurrencyPrivate(trace);
        }

        public string ExtractCurrency(XEvent evt)
        {
            return ExtractCurrencyPrivate(evt);
        }

        string ExtractCurrencyPrivate(IXAttributable element)
        {
            XAttribute attribute = element.GetAttributes()[QualifiedName(KEY_CURRENCY)];
            if (attribute == null)
            {
                return null;
            }
            return ((XAttributeLiteral)attribute).Value;
        }

        public void AssignCurrency(XTrace trace, string currency)
        {
            AssignCurrencyPrivate(trace, currency);
        }

        public void AssignCurrency(XEvent evt, string currency)
        {
            AssignCurrencyPrivate(evt, currency);
        }

        void AssignCurrencyPrivate(IXAttributable element, string currency)
        {
            if ((currency != null) && (currency.Trim().Length > 0))
            {
                XAttributeLiteral attr = (XAttributeLiteral)ATTR_CURRENCY.Clone();
                attr.Value = currency;
                element.GetAttributes().Add(QualifiedName(KEY_CURRENCY), attr);
            }
        }

        public double ExtractAmount(XAttributeContinuous attribute)
        {
            XAttributeContinuous attr = (XAttributeContinuous)attribute.GetAttributes()[QualifiedName(KEY_AMOUNT)];
            if (attr == null)
            {
                return -1;
            }
            return attr.Value;
        }

        public IDictionary<string, double> ExtractAmounts(XTrace trace)
        {
            return XCostAmount.Instance.ExtractValues(trace);
        }

        public IDictionary<string, double> ExtractAmounts(XEvent evt)
        {
            return XCostAmount.Instance.ExtractValues(evt);
        }

        public Dictionary<List<string>, double> ExtractNestedAmounts(XTrace trace)
        {
            return XCostAmount.Instance.ExtractNestedValues(trace);
        }

        public Dictionary<List<string>, double> ExtractNestedAmounts(XEvent evt)
        {
            return XCostAmount.Instance.ExtractNestedValues(evt);
        }

        public void AssignAmount(XAttribute attribute, double amount)
        {
            if (amount > 0.0D)
            {
                XAttributeContinuous attr = (XAttributeContinuous)ATTR_AMOUNT.Clone();

                attr.Value = amount;
                attribute.GetAttributes().Add(QualifiedName(KEY_AMOUNT), attr);
            }
        }

        public void AssignAmounts(XTrace trace, Dictionary<string, double> amounts)
        {
            XCostAmount.Instance.AssignValues(trace, amounts);
        }

        public void AssignAmounts(XEvent evt, Dictionary<string, double> amounts)
        {
            XCostAmount.Instance.AssignValues(evt, amounts);
        }

        public void AssignNestedAmounts(XTrace trace, Dictionary<List<string>, double> amounts)
        {
            XCostAmount.Instance.AssignNestedValues(trace, amounts);
        }

        public void AssignNestedAmounts(XEvent evt, Dictionary<List<string>, double> amounts)
        {
            XCostAmount.Instance.AssignNestedValues(evt, amounts);
        }

        public string ExtractDriver(XAttribute attribute)
        {
            XAttribute attr = attribute.GetAttributes()[QualifiedName(KEY_DRIVER)];
            if (attr == null)
            {
                return null;
            }
            return ((XAttributeLiteral)attr).Value;
        }

        public IDictionary<string, string> ExtractDrivers(XTrace trace)
        {
            return XCostDriver.Instance.ExtractValues(trace);
        }

        public IDictionary<string, string> ExtractDrivers(XEvent evt)
        {
            return XCostDriver.Instance.ExtractValues(evt);
        }

        public Dictionary<List<string>, string> ExtractNestedDrivers(XTrace trace)
        {
            return XCostDriver.Instance.ExtractNestedValues(trace);
        }

        public Dictionary<List<string>, string> extractNestedDrivers(XEvent evt)
        {
            return XCostDriver.Instance.ExtractNestedValues(evt);
        }

        public void AssignDriver(XAttribute attribute, string driver)
        {
            if ((driver != null) && (driver.Trim().Length > 0))
            {
                XAttributeLiteral attr = (XAttributeLiteral)ATTR_DRIVER.Clone();
                attr.Value = driver;
                attribute.GetAttributes().Add(QualifiedName(KEY_DRIVER), attr);
            }
        }

        public void AssignDrivers(XTrace trace, Dictionary<string, string> drivers)
        {
            XCostDriver.Instance.AssignValues(trace, drivers);
        }

        public void AssignDrivers(XEvent evt, Dictionary<string, string> drivers)
        {
            XCostDriver.Instance.AssignValues(evt, drivers);
        }

        public void AssignNestedDrivers(XTrace trace, Dictionary<List<string>, string> drivers)
        {
            XCostDriver.Instance.AssignNestedValues(trace, drivers);
        }

        public void AssignNestedDrivers(XEvent evt, Dictionary<List<string>, string> drivers)
        {
            XCostDriver.Instance.AssignNestedValues(evt, drivers);
        }

        public string ExtractType(XAttribute attribute)
        {
            XAttribute attr = attribute.GetAttributes()[QualifiedName(KEY_TYPE)];
            if (attr == null)
            {
                return null;
            }
            return ((XAttributeLiteral)attr).Value;
        }

        public IDictionary<string, string> ExtractTypes(XTrace trace)
        {
            return XCostType.Instance.ExtractValues(trace);
        }

        public IDictionary<string, string> ExtractTypes(XEvent evt)
        {
            return XCostType.Instance.ExtractValues(evt);
        }

        public Dictionary<List<string>, string> ExtractNestedTypes(XTrace trace)
        {
            return XCostType.Instance.ExtractNestedValues(trace);
        }

        public Dictionary<List<string>, string> ExtractNestedTypes(XEvent evt)
        {
            return XCostType.Instance.ExtractNestedValues(evt);
        }

        public void AssignType(XAttribute attribute, string type)
        {
            if ((type != null) && (type.Trim().Length > 0))
            {
                XAttributeLiteral attr = (XAttributeLiteral)ATTR_TYPE.Clone();
                attr.Value = type;
                attribute.GetAttributes().Add(QualifiedName(KEY_TYPE), attr);
            }
        }

        public void AssignTypes(XTrace trace, Dictionary<List<string>, string> types)
        {
            XCostType.Instance.AssignNestedValues(trace, types);
        }

        public void AssignTypes(XEvent evt, Dictionary<List<string>, string> types)
        {
            XCostType.Instance.AssignNestedValues(evt, types);
        }

        public void AssignNestedTypes(XTrace trace, Dictionary<List<string>, string> types)
        {
            XCostType.Instance.AssignNestedValues(trace, types);
        }

        public void AssignNestedTypes(XEvent evt, Dictionary<List<string>, string> types)
        {
            XCostType.Instance.AssignNestedValues(evt, types);
        }
    }
}
