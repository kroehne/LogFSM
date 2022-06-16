using System;
using OpenXesNet.model;
using System.Collections.Generic;
using OpenXesNet.factory;

namespace OpenXesNet.extension.std
{
    public abstract class XAbstractNestedAttributeSupport<T>
    {
        public abstract T ExtractValue(XAttribute attribute);

        public abstract void AssignValue(XAttribute attribute, T value);

        public IDictionary<String, T> ExtractValues(IXAttributable element)
        {
            Dictionary<String, T> values = new Dictionary<string, T>();
            Dictionary<List<string>, T> nestedValues = ExtractNestedValues(element);

            foreach (List<string> keys in nestedValues.Keys)
            {
                if (keys.Count == 1)
                {
                    values.Add(keys[0], nestedValues[keys]);
                }
            }
            return values;
        }

        public Dictionary<List<string>, T> ExtractNestedValues(IXAttributable element)
        {
            Dictionary<List<string>, T> nestedValues = new Dictionary<List<string>, T>();
            foreach (XAttribute attr in element.GetAttributes().Values)
            {
                List<string> keys = new List<string>();
                keys.Add(attr.Key);
                ExtractNestedValuesPrivate(attr, nestedValues, keys);
            }
            return nestedValues;
        }

        void ExtractNestedValuesPrivate(XAttribute element, Dictionary<List<string>, T> nestedValues,
                List<string> keys)
        {
            T value = ExtractValue(element);
#pragma warning disable RECS0017 // Posible comparación de tipo de valor con 'null'
            if (value != null)
#pragma warning restore RECS0017 // Posible comparación de tipo de valor con 'null'
            {
                nestedValues.Add(keys, value);
            }
            foreach (XAttribute attr in element.GetAttributes().Values)
            {
                List<string> newKeys = new List<string>(keys);
                newKeys.Add(element.Key);
                ExtractNestedValuesPrivate(attr, nestedValues, newKeys);
            }
        }

        public void AssignValues(IXAttributable element, Dictionary<String, T> values)
        {
            Dictionary<List<string>, T> nestedValues = new Dictionary<List<string>, T>();
            foreach (string key in values.Keys)
            {
                List<string> keys = new List<string>();
                keys.Add(key);
                nestedValues.Add(keys, values[key]);
            }
            AssignNestedValues(element, nestedValues);
        }

        public void AssignNestedValues(IXAttributable element, Dictionary<List<string>, T> amounts)
        {
            foreach (List<string> keys in amounts.Keys)
            {
                AssignNestedValuesPrivate(element, keys, amounts[keys]);
            }
        }

        void AssignNestedValuesPrivate(IXAttributable element, List<string> keys, T value)
        {
            if (keys.Count == 0)
            {
                if (element is XAttribute)
                {
                    AssignValue((XAttribute)element, value);
                }
            }
            else
            {
                String key = keys[0];
                List<string> keysTail = keys.GetRange(1, keys.Count);
                XAttribute attr;
                if (element.GetAttributes().ContainsKey(key))
                {
                    attr = element.GetAttributes()[key];
                }
                else
                {
                    attr = (XFactoryRegistry.Instance.CurrentDefault).CreateAttributeLiteral(key, "", null);

                    element.GetAttributes().Add(key, attr);
                }

                AssignNestedValuesPrivate((IXAttributable)attr, keysTail, value);
            }
        }
    }
}
