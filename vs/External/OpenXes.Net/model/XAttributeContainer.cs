using System;
using OpenXesNet.extension;

namespace OpenXesNet.model
{
    public class XAttributeContainer : XAttributeCollection, IXAttributeContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeContainer"/> class.
        /// </summary>
        /// <param name="key">Key of the attribute.</param>
        public XAttributeContainer(string key) : base(key, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeContainer"/> class.
        /// </summary>
        /// <param name="key">Key of the attribute.</param>
        /// <param name="extension">The extension for this attribute.</param>
        public XAttributeContainer(string key, XExtension extension) : base(key, extension)
        {
            this.collection = null;
        }
    }
}
