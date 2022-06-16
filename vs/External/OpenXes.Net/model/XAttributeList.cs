using System;
using System.Collections.Generic;

using OpenXesNet.extension;

namespace OpenXesNet.model
{
    public class XAttributeList : XAttributeCollection, IXAttributeList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeList"/> class.
        /// </summary>
        /// <param name="key">The attribute key.</param>
        public XAttributeList(string key) : base(key, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeList"/> class.
        /// </summary>
        /// <param name="key">The attribute key.</param>
        /// <param name="extension">The attribute extension.</param>
        public XAttributeList(string key, XExtension extension) : base(key, extension)
        {
            this.collection = new List<XAttribute>();
        }

        public override object Clone()
        {
            XAttributeList clone = (XAttributeList)base.Clone();
            XAttribute[] attrs = { };
            this.collection.CopyTo(attrs);
            clone.collection = new List<XAttribute>(attrs);

            return clone;
        }
    }
}
