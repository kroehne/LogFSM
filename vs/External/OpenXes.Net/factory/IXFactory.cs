using System;
using OpenXesNet.model;
using OpenXesNet.extension;
using OpenXesNet.id;

namespace OpenXesNet.factory
{
    /// <summary>
    /// Factory interface, providing factory methods for creating all element 
    /// classes of the XES model type hierarchy.
    /// </summary>
    public interface IXFactory
    {
        /// <summary>
        /// Returns the name of the specific factory implementation.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Returns the author name of the specific factory implementation.
        /// </summary>
        /// <value>The author.</value>
        string Author { get; }

        /// <summary>
        /// Returns the vendor of the specific factory implementation.
        /// </summary>
        /// <value>The vendor.</value>
        string Vendor { get; }

        /// <summary>
        /// Returns a description of the specific factory implementation.
        /// </summary>
        /// <value>The description.</value>
        string Description { get; }

        /// <summary>
        /// Returns an URI, pointing to more information about the specific factory implementation.
        /// </summary>
        /// <value>The URI.</value>
        Uri Uri { get; }

        /// <summary>
        /// Creates a new XES log instance (Factory method).
        /// </summary>
        /// <returns>A new log instance.</returns>
        XLog CreateLog();

        /// <summary>
        /// Creates a new XES log instance (Factory method).
        /// </summary>
        /// <returns>A new log instance.</returns>
        /// <param name="attributes">The attributes of the log.</param>
        XLog CreateLog(IXAttributeMap attributes);

        /// <summary>
        /// Creates a new XES trace instance (Factory method).
        /// </summary>
        /// <returns>A new trace instance.</returns>
        XTrace CreateTrace();

        /// <summary>
        /// Creates a new XES trace instance (Factory method).
        /// </summary>
        /// <returns>A new trace instance.</returns>
        /// <param name="attributes">The attributes of the trace.</param>
        XTrace CreateTrace(IXAttributeMap attributes);

        /// <summary>
        /// Creates a new XES event instance (Factory method).
        /// </summary>
        /// <returns>A new event instance.</returns>
        XEvent CreateEvent();

        /// <summary>
        /// Creates a new XES event instance (Factory method).
        /// </summary>
        /// <returns>A new event instance.</returns>
        /// <param name="attributes">The attributes of the event.</param>
        XEvent CreateEvent(IXAttributeMap attributes);

        /// <summary>
        /// Creates a new XES event instance (Factory method).
        /// </summary>
        /// <returns>A new event instance.</returns>
        /// <param name="id">the id of this new event. Only to be used in case of deserializing!</param>
        /// <param name="attributes">The attributes of the event.</param>
        XEvent CreateEvent(XID id, IXAttributeMap attributes);

        /// <summary>
        /// Creates a new XES attribute with boolean type (Factory method).
        /// </summary>
        /// <returns>A new XES attribute map instance.</returns>
        XAttributeMap CreateAttributeMap();

        /// <summary>
        /// Creates a new XES attribute with boolean type (Factory method).
        /// </summary>
        /// <returns>The attribute boolean.</returns>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <param name="extension">The extension defining the attribute (set to 
        /// <code>null</code>, if the attribute is not associated to an extension)</param>
        XAttributeBoolean CreateAttributeBoolean(string key, bool value, XExtension extension = null);

        /// <summary>
        /// Creates a new XES attribute with continuous type (Factory method).
        /// </summary>
        /// <returns>The attribute continuous.</returns>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <param name="extension">The extension defining the attribute (set to 
        /// <code>null</code>, if the attribute is not associated to an extension)</param>
        XAttributeContinuous CreateAttributeContinuous(string key, double value, XExtension extension = null);

        /// <summary>
        /// Creates a new XES attribute with discrete type (Factory method).
        /// </summary>
        /// <returns>The attribute discrete.</returns>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <param name="extension">The extension defining the attribute (set to 
        /// <code>null</code>, if the attribute is not associated to an extension)</param>
        XAttributeDiscrete CreateAttributeDiscrete(string key, long value, XExtension extension = null);

        /// <summary>
        /// Creates a new XES attribute with literal type (Factory method).
        /// </summary>
        /// <returns>The attribute literal.</returns>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <param name="extension">The extension defining the attribute (set to 
        /// <code>null</code>, if the attribute is not associated to an extension)</param>
        XAttributeLiteral CreateAttributeLiteral(string key, string value, XExtension extension = null);

        /// <summary>
        /// Creates a new XES attribute with timestamp type (Factory method).
        /// </summary>
        /// <returns>The attribute timestamp.</returns>
        /// <param name="key">The value of the attribute.</param>
        /// <param name="value">Value.</param>
        /// <param name="extension">The extension defining the attribute (set to 
        /// <code>null</code>, if the attribute is not associated to an extension)</param>
        XAttributeTimestamp CreateAttributeTimestamp(string key, DateTime value, XExtension extension = null);

        /// <summary>
        /// Creates a new XES attribute with timestamp type (Factory method).
        /// </summary>
        /// <returns>The attribute timestamp.</returns>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">The value of the attribute, in milliseconds since 01/01/19700:00 GMT.</param>
        /// <param name="extension">The extension defining the attribute (set to 
        /// <code>null</code>, if the attribute is not associated to an extension)</param>
        XAttributeTimestamp CreateAttributeTimestamp(string key, long value, XExtension extension = null);

        /// <summary>
        /// Creates a new XES attribute with id type (Factory method).
        /// </summary>
        /// <returns>The attribute identifier.</returns>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">Value.</param>
        /// <param name="extension">The extension defining the attribute (set to 
        /// <code>null</code>, if the attribute is not associated to an extension)</param>
        XAttributeID CreateAttributeID(string key, XID value, XExtension extension = null);

        /// <summary>
        /// Creates the attribute list.
        /// </summary>
        /// <returns>The attribute list.</returns>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="extension">The extension defining the attribute (set to 
        /// <code>null</code>, if the attribute is not associated to an extension)</param>
        XAttributeList CreateAttributeList(string key, XExtension extension = null);

        /// <summary>
        /// Creates the attribute container.
        /// </summary>
        /// <returns>The attribute container.</returns>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="extension">The extension defining the attribute (set to 
        /// <code>null</code>, if the attribute is not associated to an extension)</param>
        XAttributeContainer CreateAttributeContainer(string key, XExtension extension = null);
    }
}
