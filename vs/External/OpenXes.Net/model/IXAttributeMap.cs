using System;
using System.Collections.Generic;
namespace OpenXesNet.model
{
    /// <summary>
    /// <para>
    /// An attribute map is used to hold a set of attributes, indexed by their 
    /// key strings, for event log hierarchy elements.
    /// </para>
    /// <para>
    /// It is required to be cloneable, so that it can be replicated efficiently and reliably.
    /// </para>
    /// </summary>
    public interface IXAttributeMap : IDictionary<string, XAttribute>, ICloneable
    {
        /// <summary>
        /// Retrieves the attributes set for this element.
        /// </summary>
        /// <returns>A list of attributes.</returns>
        List<XAttribute> AsList();
    }
}
