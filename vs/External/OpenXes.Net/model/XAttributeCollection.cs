using System;
using System.Text;
using System.Collections.Generic;
using OpenXesNet.extension;

namespace OpenXesNet.model
{
    public abstract class XAttributeCollection : XAttributeLiteral, IXAttributeCollection
    {
        protected List<XAttribute> collection;

        public new List<XAttribute> Value
        {
            get { return this.collection; }
            set { this.collection = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeCollection"/> class.
        /// </summary>
        /// <param name="key">The key of this attribute.</param>
        protected XAttributeCollection(string key) : base(key, "", null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeCollection"/> class.
        /// </summary>
        /// <param name="key">The key of this attribute.</param>
        /// <param name="extension">The extension of the attribute.</param>
        protected XAttributeCollection(string key, XExtension extension) : base(key, "", extension)
        {
        }

        public void AddToCollection(XAttribute attribute)
        {
            if (this.collection != null)
                this.collection.Add(attribute);
        }

        public void RemoveFromCollection(XAttribute attribute)
        {
            if (this.collection != null)
                this.collection.Remove(attribute);
        }

        public List<XAttribute> GetCollection()
        {
            return this.collection ?? new List<XAttribute>(this.GetAttributes().Values);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            String sep = "[";
            foreach (XAttribute attribute in this.GetCollection())
            {
                buf.Append(sep);
                sep = ",";
                buf.Append(attribute.Key);
                buf.Append(":");
                buf.Append(attribute.ToString());
            }
            if (buf.Length == 0)
            {
                buf.Append("[");
            }
            buf.Append("]");
            return buf.ToString();
        }
    }
}
