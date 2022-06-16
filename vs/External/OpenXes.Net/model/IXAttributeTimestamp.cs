using System;
namespace OpenXesNet.model
{
    /// <summary>
    /// Attribute with DateTime type value
    /// </summary>
    public interface IXAttributeTimestamp : IXAttribute<DateTime>
    {
        /// <summary>
        /// Gets or sets the timestamp value of this attribute in milliseconds.
        /// </summary>
        /// <value>The value millis.</value>
        long ValueMillis { get; set; }
    }
}
