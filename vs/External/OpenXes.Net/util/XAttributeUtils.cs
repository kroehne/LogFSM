using System;
using System.Collections.Generic;
using OpenXesNet.model;
using OpenXesNet.extension;
using OpenXesNet.factory;
using OpenXesNet.id;


namespace OpenXesNet.util
{
    /// <summary>
    /// Utilities for working with attributes.
    /// </summary>
    public static class XAttributeUtils
    {
        /// <summary>
        /// Generic object to work with synchronized methods
        /// </summary>
        static readonly object Lock = new object();

        /// <summary>
        /// For the given attribute, returns its type, i.e., the most high-level, 
        /// typed interface this attribute implements.
        /// </summary>
        /// <returns>High-level type interface of this attribute.</returns>
        /// <param name="attribute">Attribute to analyze.</param>
        public static Type GetType(XAttribute attribute)
        {
            return attribute.GetType();
        }

        /// <summary>
        /// For the given attribute, derives the standardized string describing the 
        /// attributes specific type(used, e.g., for serialization).
        /// </summary>
        /// <returns>String representation of the attribute's specific type.</returns>
        /// <param name="attribute">Attribute to extract type string from.</param>
        public static string GetTypeString(XAttribute attribute)
        {
            if (attribute is XAttributeList)
                return "LIST";
            if (attribute is XAttributeContainer)
                return "CONTAINER";
            if (attribute is XAttributeLiteral)
                return "LITERAL";
            if (attribute is XAttributeBoolean)
                return "BOOLEAN";
            if (attribute is XAttributeContinuous)
                return "CONTINUOUS";
            if (attribute is XAttributeDiscrete)
                return "DISCRETE";
            if (attribute is IXAttributeTimestamp)
                return "TIMESTAMP";
            if (attribute is XAttributeID)
            {
                return "ID";
            }
            throw new InvalidOperationException("Unexpected attribute type!");
        }

        /// <summary>
        /// Derives a prototype for the given attribute. This prototype attribute 
        /// will be equal in all respects, expect for the value of the attribute. 
        /// This value will be set to a default value, depending on the specific type 
        /// of the given attribute.
        /// </summary>
        /// <returns>The derived prototype attribute.</returns>
        /// <param name="instance">Attribute to derive prototype from.</param>
        public static XAttribute DerivePrototype(XAttribute instance)
        {
            XAttribute prototype = (XAttribute)instance.Clone();
            if (!(prototype is XAttributeList))
            {
                if (!(prototype is XAttributeContainer))
                    if (prototype is XAttributeLiteral)
                        ((XAttributeLiteral)prototype).Value = "DEFAULT";
                    else if (prototype is XAttributeBoolean)
                        ((XAttributeBoolean)prototype).Value = true;
                    else if (prototype is XAttributeContinuous)
                        ((XAttributeContinuous)prototype).Value = 0.0D;
                    else if (prototype is XAttributeDiscrete)
                        ((XAttributeDiscrete)prototype).Value = 0L;
                    else if (prototype is IXAttributeTimestamp)
                        ((XAttributeTimestamp)prototype).ValueMillis = 0L;
                    else if (prototype is XAttributeID)
                        ((XAttributeID)prototype).Value = XIDFactory.Instance.CreateId();
                    else
                        throw new InvalidOperationException("Unexpected attribute type!");
            }
            return prototype;
        }

        /// <summary>
        /// Composes the appropriate attribute type from the string-based information 
        /// found, e.g., in XML serializations.
        /// </summary>
        /// <returns>An appropriate attribute.</returns>
        /// <param name="factory">Factory to use for creating the attribute.</param>
        /// <param name="key">Key of the attribute.</param>
        /// <param name="value">Value of the attribute.</param>
        /// <param name="type">Type string of the attribute.</param>
        /// <param name="extension">Extension of the attribute (can be <code>null</code>).</param>
        public static XAttribute ComposeAttribute(IXFactory factory, string key, string value, string type,
                XExtension extension)
        {
            type = type.Trim();
            if (type.Equals("LIST", StringComparison.CurrentCultureIgnoreCase))
            {
                XAttributeList attr = factory.CreateAttributeList(key, extension);
                return attr;
            }
            if (type.Equals("CONTAINER", StringComparison.CurrentCultureIgnoreCase))
            {
                XAttributeContainer attr = factory.CreateAttributeContainer(key, extension);
                return attr;
            }
            if (type.Equals("LITERAL", StringComparison.CurrentCultureIgnoreCase))
            {
                XAttributeLiteral attr = factory.CreateAttributeLiteral(key, value, extension);

                return attr;
            }
            if (type.Equals("BOOLEAN", StringComparison.CurrentCultureIgnoreCase))
            {
                XAttributeBoolean attr = factory.CreateAttributeBoolean(key, bool.Parse(value), extension);

                return attr;
            }
            if (type.Equals("CONTINUOUS", StringComparison.CurrentCultureIgnoreCase))
            {
                XAttributeContinuous attr = factory.CreateAttributeContinuous(key, double.Parse(value), extension);

                return attr;
            }
            if (type.Equals("DISCRETE", StringComparison.CurrentCultureIgnoreCase))
            {
                XAttributeDiscrete attr = factory.CreateAttributeDiscrete(key, long.Parse(value), extension);

                return attr;
            }
            if (type.Equals("TIMESTAMP", StringComparison.CurrentCultureIgnoreCase))
            {
                IXAttributeTimestamp attr;
                try
                {
                    attr = factory.CreateAttributeTimestamp(key, DateTime.Parse(value), extension);
                }
                catch (FormatException)
                {
                    throw new InvalidOperationException("OpenXES: could not parse date-time attribute. Value: " + value);
                }

                return (XAttributeTimestamp)attr;
            }
            if (type.Equals("ID", StringComparison.CurrentCultureIgnoreCase))
            {
                XAttributeID attr = factory.CreateAttributeID(key, XID.Parse(value), extension);
                return attr;
            }
            throw new InvalidOperationException("OpenXES: could not parse attribute type!");
        }

        /// <summary>
        /// Static helper method for extracting all extensions from an attribute map.
        /// </summary>
        /// <returns>The set of extensions in the attribute map.</returns>
        /// <param name="attributeMap"> The attribute map from which to extract extensions.</param>
        public static HashSet<XExtension> ExtractExtensions(IXAttributeMap attributeMap)
        {
            HashSet<XExtension> extensions = new HashSet<XExtension>();
            foreach (XAttribute attribute in attributeMap.Values)
            {
                XExtension extension = attribute.Extension;
                if (extension != null)
                {
                    extensions.Add(extension);
                }
            }
            return extensions;
        }
    }
}
