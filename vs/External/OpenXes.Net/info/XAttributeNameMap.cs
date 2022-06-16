using System;
using System.Collections.Generic;
using OpenXesNet.logging;
using System.Text;
using OpenXesNet.model;

namespace OpenXesNet.info
{
    public class XAttributeNameMap : Dictionary<string, string>, IXAttributeNameMap
    {
        /// <summary>
        /// ame of the mapping.
        /// </summary>
        readonly string name;

        /// <summary>
        /// Creates a new attribute name mapping instance.
        /// </summary>
        /// <param name="name">Name of the mapping.</param>
        public XAttributeNameMap(string name)
        {
            this.name = name;
        }

        public string MappingName
        {
            get { return this.name; }
        }

        public string Get(XAttribute attribute)
        {
            return Get(attribute.Key);
        }

        public string Get(string key)
        {
            try
            {
                return (this[key]);
            }
            catch (KeyNotFoundException)
            {
                XLogging.Log(String.Format("The key '{0}' is nor registered", key), XLogging.Importance.WARNING);
                return null;
            }
        }

        /// <summary>
        /// Registers a mapping for a given attribute.
        /// </summary>
        /// <param name="attribute">Attribute for which to register a mapping.</param>
        /// <param name="alias">Alias string to map the attribute to.</param>
        public void RegisterMapping(XAttribute attribute, string alias)
        {
            this.RegisterMapping(attribute.Key, alias);
        }

        /// <summary>
        /// Registers a mapping for a given attribute.
        /// </summary>
        /// <param name="attributeKey">Attribute key for which to register a mapping.</param>
        /// <param name="alias">Alias string to map the attribute to.</param>
        public void RegisterMapping(string attributeKey, string alias)
        {
            lock (this)
            {
                try
                {
                    this.Add(attributeKey, alias);
                }
                catch (ArgumentException)
                {
                    XLogging.Log(String.Format("Mapping '{0}->{1}'already exists. Skipping register process", attributeKey, alias), XLogging.Importance.WARNING);
                }
            }
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Attribute name map: ");
            sb.Append(this.name);
            foreach (string key in this.Keys)
            {
                sb.Append("\n");
                sb.Append(key);
                sb.Append(" -> ");
                sb.Append(this[key]);
            }
            return sb.ToString();
        }
    }
}
