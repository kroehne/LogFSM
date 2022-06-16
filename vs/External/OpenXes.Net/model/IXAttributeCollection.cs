using System;
using System.Collections.Generic;

namespace OpenXesNet.model
{
    public interface IXAttributeCollection : IXAttribute<List<XAttribute>>
    {
        /// <summary>
        /// Adds a new attribute to the current collection.
        /// </summary>
        /// <param name="attribute">The attribute to add.</param>
        void AddToCollection(XAttribute attribute);

        /// <summary>
        /// Returns a list of the attributes contained in the current collection.
        /// </summary>
        /// <returns>The collection.</returns>
        List<XAttribute> GetCollection();
    }
}
