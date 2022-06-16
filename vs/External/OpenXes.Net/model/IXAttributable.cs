using System;
using System.Collections.Generic;
using OpenXesNet.extension;

namespace OpenXesNet.model
{
    /// <summary>
    /// This interface is implemented by all elements of the log hierarchy, which can 
    /// be equipped with attributes.
    /// </summary>
    public interface IXAttributable
    {
        /// <summary>
        /// Retrieves the attributes set for this element.
        /// </summary>
        /// <returns>A map of attributes.</returns>
        IXAttributeMap GetAttributes();

        /// <summary>
        /// Sets the map of attributes for this element.
        /// </summary>
        /// <param name="attributes">A map of attributes.</param>
        void SetAttributes(IXAttributeMap attributes);

        /// <summary>
        /// Checks for the existence of attributes. This method can be a more 
        /// efficient way of checking for the existance of attributes than using
        /// <see cref="IXAttributable.GetAttributes()"/> in certain situations.
        /// </summary>
        /// <returns><c>true</c>, if this element has any attributes, <c>false</c> otherwise.</returns>
        bool HasAttributes();

        /// <summary>
        /// Retrieves the extensions used by this element, i.e. the extensions used
        /// by all attributes of this element, and the element itself.
        /// </summary>
        /// <returns>The A set of unique extensions.</returns>
        HashSet<XExtension> Extensions { get; }
    }
}
