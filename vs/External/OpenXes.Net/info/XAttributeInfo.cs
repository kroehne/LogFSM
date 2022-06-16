using System;
using OpenXesNet.model;
using OpenXesNet.extension;
using System.Collections.Generic;
using OpenXesNet.util;
using System.Linq;

namespace OpenXesNet.info
{
    /// <summary>
    /// This class provides aggregate information about attributes within one container 
    /// in the log type hierarchy.For example, it may store information about all 
    /// event attributes in a log.
    /// </summary>
    public class XAttributeInfo : IXAttributeInfo
    {
        /// <summary>
        /// Mapping from attribute keys to attribute prototypes.
        /// </summary>
        readonly Dictionary<string, XAttribute> keyMap;
        /// <summary>
        /// Mapping from attribute types to attribute prototypes.
        /// </summary>
        readonly Dictionary<Type, HashSet<XAttribute>> typeMap;
        /// <summary>
        /// Mapping from attribute extensions to attribute prototypes.
        /// </summary>
        readonly Dictionary<XExtension, HashSet<XAttribute>> extensionMap;
        /// <summary>
        /// Attribute prototypes for non-extension attributes.
        /// </summary>
        readonly HashSet<XAttribute> noExtensionSet;
        /// <summary>
        /// Mapping from attribute keys to absolute frequency.
        /// </summary>
        readonly Dictionary<string, int> frequencies;
        /// <summary>
        /// Total absolute frequency of all registered attributes.
        /// </summary>
        int totalFrequency;

        /// <summary>
        /// Creates a new attribute information registry.
        /// </summary>
        public XAttributeInfo()
        {
            this.keyMap = new Dictionary<string, XAttribute>();
            this.frequencies = new Dictionary<string, int>();
            this.typeMap = new Dictionary<Type, HashSet<XAttribute>>();
            this.extensionMap = new Dictionary<XExtension, HashSet<XAttribute>>();
            this.noExtensionSet = new HashSet<XAttribute>();
            this.totalFrequency = 0;
        }

        public List<XAttribute> GetAttributes()
        {
            return this.keyMap.Values.ToList();
        }

        public List<string> GetAttributeKeys()
        {
            return this.keyMap.Keys.ToList();
        }

        public int GetFrequency(String key)
        {
            return this.frequencies[key];
        }

        public int GetFrequency(XAttribute attribute)
        {
            return GetFrequency(attribute.Key);
        }

        public double GetRelativeFrequency(String key)
        {
            return this.frequencies[key] / this.totalFrequency;
        }

        public double GetRelativeFrequency(XAttribute attribute)
        {
            return GetRelativeFrequency(attribute.Key);
        }

        public HashSet<XAttribute> GetAttributesForType(Type type)
        {
            if (!type.IsSubclassOf(typeof(XAttribute)))
            {
                throw new NotSupportedException(String.Format("Type {0} is not a valid XAttribute", type.Name));
            }
            HashSet<XAttribute> typeSet = this.typeMap[type];
            if (typeSet == null)
            {
                typeSet = new HashSet<XAttribute>();
            }
            return typeSet;
        }

        public HashSet<String> GetKeysForType(Type type)
        {
            HashSet<XAttribute> typeCollection = GetAttributesForType(type);
            HashSet<string> keySet = new HashSet<string>();
            foreach (XAttribute attribute in typeCollection)
            {
                keySet.Add(attribute.Key);
            }
            return keySet;
        }

        public HashSet<XAttribute> GetAttributesForExtension(XExtension extension)
        {
            if (extension == null)
            {
                return GetAttributesWithoutExtension();
            }
            HashSet<XAttribute> extensionSet = this.extensionMap[extension];
            if (extensionSet == null)
            {
                extensionSet = new HashSet<XAttribute>();
            }
            return extensionSet;
        }

        public HashSet<String> GetKeysForExtension(XExtension extension)
        {
            HashSet<XAttribute> extensionCollection = GetAttributesForExtension(extension);
            HashSet<string> keySet = new HashSet<string>();
            foreach (XAttribute attribute in extensionCollection)
            {
                keySet.Add(attribute.Key);
            }
            return keySet;
        }

        public HashSet<XAttribute> GetAttributesWithoutExtension()
        {
            return this.noExtensionSet;
        }

        public HashSet<String> GetKeysWithoutExtension()
        {
            return GetKeysForExtension(null);
        }

        /// <summary>
        /// Registers a concrete attribute with this registry.
        /// </summary>
        /// <param name="attribute">Attribute to be registered.</param>
        public void Register(XAttribute attribute)
        {
            lock (this)
            {
                if (!(this.keyMap.ContainsKey(attribute.Key)))
                {
                    XAttribute prototype = XAttributeUtils.DerivePrototype(attribute);

                    this.keyMap.Add(attribute.Key, prototype);

                    this.frequencies.Add(attribute.Key, 1);

                    HashSet<XAttribute> typeSet;
                    if (this.typeMap.ContainsKey(XAttributeUtils.GetType(prototype)))
                    {
                        typeSet = this.typeMap[XAttributeUtils.GetType(prototype)];
                        this.typeMap.Remove(XAttributeUtils.GetType(prototype));
                    }
                    else
                    {
                        typeSet = typeSet = new HashSet<XAttribute>();
                    }
                    typeSet.Add(prototype);
                    this.typeMap.Add(XAttributeUtils.GetType(prototype), typeSet);

                    if (attribute.Extension == null)
                    {
                        this.noExtensionSet.Add(prototype);
                    }
                    else
                    {
                        HashSet<XAttribute> extensionSet;

                        if (!this.extensionMap.ContainsKey(attribute.Extension))
                        {
                            extensionSet = new HashSet<XAttribute>();

                        }
                        else
                        {
                            extensionSet = this.extensionMap[attribute.Extension];
                            this.extensionMap.Remove(attribute.Extension);
                        }
                        extensionSet.Add(prototype);
                        this.extensionMap.Add(attribute.Extension, extensionSet);
                    }
                }
                else
                {
                    this.frequencies[attribute.Key] += 1;
                }
                this.totalFrequency += 1;
            }
        }
    }
}
