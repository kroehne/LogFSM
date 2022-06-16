using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenXesNet.model
{
    public class XAttributeMap : Dictionary<string, XAttribute>, IXAttributeMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeMap"/> class.
        /// </summary>
        /// <param name="size">The initial size of the map.</param>
        public XAttributeMap(int size) : base(size)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeMap"/> class.
        /// </summary>
        public XAttributeMap() : base(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeMap"/> class.
        /// </summary>
        /// <param name="template">Copy the contents of this attribute map to the new attrribute map..</param>
        public XAttributeMap(Dictionary<string, XAttribute> template) : base(template.Count)
        {
            this.Union(template);
        }

        public List<XAttribute> AsList()
        {
            return new List<XAttribute>(this.Values);
        }

        /// <summary>
        /// Creates a clone, i.e. deep copy, of this attribute map.
        /// </summary>
        /// <returns>The clone.</returns>
        public object Clone()
        {
            XAttributeMap clone = new XAttributeMap(this.Count);
            foreach (XAttribute value in this.Values)
            {
                clone.Add(value.Key, (XAttribute)value.Clone());
            }
            return clone;
        }
    }
}
