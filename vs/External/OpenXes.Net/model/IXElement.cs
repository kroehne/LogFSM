using System;

namespace OpenXesNet.model
{
    /// <summary>
    /// <para>This interface is implemented by all elements of an event log structure. 
    /// It defines that all elements are attributable.
    /// </para>
    /// <para>Further, this interface requires all event log elements to be cloneable.</para>
    /// </summary>
    public interface IXElement : IXAttributable, ICloneable
    {
    }
}
