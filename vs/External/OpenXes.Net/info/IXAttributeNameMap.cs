using System;
using OpenXesNet.model;
namespace OpenXesNet.info
{
    public interface IXAttributeNameMap
    {
        /// <summary>
        /// Returns the name of this mapping.
        /// </summary>
        /// <value>The name of the mapping.</value>
        string MappingName { get; }

        /// <summary>
        /// Returns the name mapped onto the provided attribute by this mapping.
        /// If no mapping for the given attribute is provided by this map, <code>null</code> is returned.
        /// </summary>
        /// <returns>The mapping for the given attribute, or <code>null</code>, 
        /// if no such mapping exists.</returns>
        /// <param name="attribute">Attribute to retrieve mapping for.</param>
        string Get(XAttribute attribute);

        /// <summary>
        /// Returns the name mapped onto the provided attribute key by this mapping.
        /// If no mapping for the given attribute key is provided by this map, 
        /// <code>null</code> is returned.
        /// </summary>
        /// <returns>The mapping for the given attribute key, or <code>null</code>, 
        /// if no such mapping exists.</returns>
        /// <param name="key">Attribute key to retrieve mapping for.</param>
        string Get(string key);
    }
}
