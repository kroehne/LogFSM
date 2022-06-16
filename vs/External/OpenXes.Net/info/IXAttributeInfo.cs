using System.Collections.Generic;
using OpenXesNet.model;
using OpenXesNet.extension;
using System;

namespace OpenXesNet.info
{
    /// <summary>
    /// This interface defines an attribute information registry.
    /// Instances of this interface can be used to store aggregate 
    /// information about the classes of attributes contained in 
    /// a specific attributable type.
    /// </summary>
    public interface IXAttributeInfo
    {
        /// <summary>
        /// Provides access to prototypes of all registered attributes.
        /// </summary>
        /// <returns>The attributes.</returns>
        List<XAttribute> GetAttributes();

        /// <summary>
        /// Provides access to prototypes of all registered attributes' keys.
        /// </summary>
        /// <returns>The attribute keys.</returns>
        List<string> GetAttributeKeys();

        /// <summary>
        /// Returns the total frequency, i.e. number of occurrences, for the requested attribute.
        /// </summary>
        /// <returns>Total frequency of that attribute as registered.</returns>
        /// <param name="key">Key of an attribute.</param>
        int GetFrequency(string key);

        /// <summary>
        /// Returns the total frequency, i.e. number of occurrences, for the requested attribute.
        /// </summary>
        /// <returns>Total frequency of that attribute as registered.</returns>
        /// <param name="attribute">An attribute.</param>
        int GetFrequency(XAttribute attribute);

        /// <summary>
        /// Returns the relative frequency, i.e. between 0 and 1, for the requested attribute.
        /// </summary>
        /// <returns>Relative frequency of that attribute as registered.</returns>
        /// <param name="key">Key of an attribute.</param>
        double GetRelativeFrequency(string key);

        /// <summary>
        /// Returns the relative frequency, i.e. between 0 and 1, for the requested attribute.
        /// </summary>
        /// <returns>Relative frequency of that attribute as registered.</returns>
        /// <param name="attribute">An attribute.</param>
        double GetRelativeFrequency(XAttribute attribute);

        /// <summary>
        /// For a given type, returns prototypes of all registered attributes with that type.
        /// </summary>
        /// <returns>A collection of attribute prototypes registered for that type.</returns>
        /// <param name="type">Requested attribute type (type-specific attribute interface class)</param>
        HashSet<XAttribute> GetAttributesForType(Type type);

        /// <summary>
        /// For a given type, returns the keys of all registered attributes with that type.
        /// </summary>
        /// <returns>A collection of attribute keys registered for that type.</returns>
        /// <param name="type">Requested attribute type (type-specific attribute interface class).</param>
        HashSet<string> GetKeysForType(Type type);

        /// <summary>
        /// For a given extension, returns prototypes of all registered attributes defined by that extension.
        /// </summary>
        /// <returns>A collection of attribute prototypes registered for that extension.</returns>
        /// <param name="extension">Requested attribute extension.</param>
        HashSet<XAttribute> GetAttributesForExtension(XExtension extension);

        /// <summary>
        /// For a given extension, returns the keys of all registered attributes defined by that extension.
        /// </summary>
        /// <returns>A collection of attribute keys registered for that extension.</returns>
        /// <param name="extension">Requested attribute extension.</param>
        HashSet<string> GetKeysForExtension(XExtension extension);

        /// <summary>
        /// Returns prototypes of all registered attributes defined by no extension.
        /// </summary>
        /// <returns>A collection of attribute prototypes registered for no extension.</returns>
        HashSet<XAttribute> GetAttributesWithoutExtension();

        /// <summary>
        /// Returns keys of all registered attributes defined by no extension.
        /// </summary>
        /// <returns>A collection of attribute keys registered for no extension.</returns>
        HashSet<string> GetKeysWithoutExtension();
    }
}
