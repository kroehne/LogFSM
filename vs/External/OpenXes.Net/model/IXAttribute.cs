using System;
using System.Reflection;
using OpenXesNet.extension;

namespace OpenXesNet.model
{
    ///<summary>
    /// <para>This interface defines attributes used for describing
    /// meta-information about event log hierarchy elements. Attributes have a
    ///  name (i.e., a key), which is string-based. 
    /// The value of an attribute is strongly typed, and can be
    /// accessed and modified via sub-interface methods specified
    /// by type.</para>
    /// <para>Attributes may further be defined by an extension,
    /// which makes it possible to assign semantic meaning to
    /// them within a specific domain.</para>
    /// </summary>
    public interface IXAttribute<T> : IXAttributable, ICloneable, IComparable
    {
        /// <summary>
        /// Retrieves the key, i.e. unique identifier, of this attribute.
        /// </summary>
        /// <value>The key of this attribute, as a string</value>
        string Key { get; }

        /// <summary>
        /// Retrieves the extension defining this attribute.
        /// </summary>
        /// <value>
        /// The extension of this attribute. May return <code>null</code>, 
        /// if there is no extension defining this attribute.
        /// </value>
        XExtension Extension { get; }

        T Value { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:OpenXesNet.model.IXAttribute"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:OpenXesNet.model.IXAttribute"/>.</returns>
        string ToString();

        /// <summary>
        /// Allow the specified <c>visitor</c> to inspect the current attributable.
        /// </summary>
        /// <returns>The accept.</returns>
        /// <param name="visitor">Parameter visitor.</param>
        /// <param name="parent">A reference to the parent attributable containing this. 
        /// May be <c>null</c></param>
        void Accept(XVisitor visitor, IXAttributable parent);
    }
}
