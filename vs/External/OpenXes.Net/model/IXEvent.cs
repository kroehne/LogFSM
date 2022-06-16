using OpenXesNet.id;

namespace OpenXesNet.model
{
    /// <summary>
    /// <para>An event is an element of an XES event log structure. Events are sequentially
    /// contained in traces.</para>
    /// <para>Events refer to something that has happened during the execution of a 
    /// process, e.g.the execution of an activity.</para>
    /// </summary>
    public interface IXEvent : IXElement
    {
        /// <summary>
        /// Returns the id of the event
        /// </summary>
        /// <value>The identifier.</value>
        XID Id { get; }

        /// <summary>
        /// Runs the given visitor for the given trace on this event.
        /// </summary>
        /// <returns>The accept.</returns>
        /// <param name="visitor">The visitor to run.</param>
        /// <param name="trace">The current trace.</param>
        void Accept(XVisitor visitor, XTrace trace);
    }
}
