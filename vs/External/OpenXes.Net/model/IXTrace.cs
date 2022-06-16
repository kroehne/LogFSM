using System;
using System.Collections.Generic;

namespace OpenXesNet.model
{
    /// <summary>
    /// <para>A trace is an element of an XES event log structure. Traces are contained in logs.
    /// Any trace is a list of events.</para>
    /// <para>Traces describe sequences of events, as they have occurred 
    /// during one execution of a process, in their given order.</para>
    /// </summary>
    public interface IXTrace : IXElement, IList<IXEvent>
    {
        /// <summary>
        /// Insert the event in an ordered manner, if timestamp information is available in this trace.
        /// </summary>
        /// <returns>Index of the inserted event.</returns>
        /// <param name="evt">The event to be inserted..</param>
        int InsertOrdered(IXEvent evt);

        /// <summary>
        /// Runs the given visitor for the given log on this trace.
        /// </summary>
        /// <returns>The accept.</returns>
        /// <param name="visitor">Visitor.</param>
        /// <param name="log">Log.</param>
        void Accept(XVisitor visitor, XLog log);
    }
}
