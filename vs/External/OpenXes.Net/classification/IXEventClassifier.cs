using OpenXesNet.model;

namespace OpenXesNet.classification
{
    /// <summary>
    /// This interface defines a classification of events. It assigns to each event 
    /// instance a class identity, thereby imposing an equality relation on the set of events.
    /// </summary>
    public interface IXEventClassifier
    {
        /// <summary>
        /// Gets or sets the custon name of this classifier.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Checks whether two event instances correspond to the same event class, 
        /// i.e.are equal in that sense.
        /// </summary>
        /// <returns><c>true</c>, if event class was samed, <c>false</c> otherwise.</returns>
        /// <param name="event1">Event1.</param>
        /// <param name="event2">Event2.</param>
        bool SameEventClass(IXEvent event1, IXEvent event2);

        /// <summary>
        /// Retrieves the unique class identity string of a given event
        /// </summary>
        /// <returns>The class identity.</returns>
        /// <param name="evt">Evt.</param>
        string GetClassIdentity(IXEvent evt);

        /// <summary>
        /// Retrieves the set of attribute keys which are used in this event classifier 
        /// (May be used for the construction of events that are not part of an existing event class).
        /// </summary>
        /// <value>A set of attribute keys, which are used for defining this classifier.</value>
        string[] DefiningAttributeKeys { get; }

        /// <summary>
        /// Runs the given visitor for the given log on this classifier.
        /// </summary>
        /// <returns>The accept.</returns>
        /// <param name="visitor">Visitor.</param>
        /// <param name="log">Log.</param>
        void Accept(XVisitor visitor, XLog log);
    }
}
