using System.Collections.Generic;
using OpenXesNet.model;
using OpenXesNet.classification;

namespace OpenXesNet.info
{
    /// <summary>
    /// This interface defines a bare-bones log summary.
    /// </summary>
    public interface IXLogInfo
    {
        /// <summary>
        /// Retrieves the log used for this summary.
        /// </summary>
        /// <returns>The log.</returns>
        IXLog GetLog();

        /// <summary>
        /// Retrieves the total number of events in this log.
        /// </summary>
        /// <returns>The number of events.</returns>
        int NumberOfEvents { get; }

        /// <summary>
        /// Retrieves the number of traces in this log.
        /// </summary>
        /// <returns>The number of traces.</returns>
        int NumberOfTraces { get; }

        /// <summary>
        /// Retrieves the set of event classifiers covered by this log info, i.e., 
        /// for which event classes are registered in this log info instance.
        /// </summary>
        /// <returns>The collection of event classifiers covered by this log info instance</returns>
        HashSet<IXEventClassifier> GetEventClassifiers();

        /// <summary>
        /// Retrieves the event classes for a given classifier.
        /// </summary>
        /// <remarks>
        /// <b>Note:</b>The given event classifier must be covered by this log 
        /// info, i.e., the log info must have been created with this classifier.Otherwise, 
        /// this method will return <code>null</code>. You can  retrieve the collection
        /// of event classifiers covered by this log info instance by calling the method
        /// <code>getEventClassifiers()</code>.
        /// </remarks>
        /// <returns>The requested event classes, or <code>null</code> if the given event 
        /// classifier is not covered by this log info instance.</returns>
        /// <param name="paramXEventClassifier">The classifier for which to retrieve 
        /// the event classes.</param>
        XEventClasses GetEventClasses(IXEventClassifier paramXEventClassifier);

        /// <summary>
        /// Retrieves the event classes of the summarized log, as defined by the 
        /// event classifier used for this summary.
        /// </summary>
        /// <returns>The event classes of the summarized log.</returns>
        XEventClasses GetEventClasses();

        /// <summary>
        /// Retrieves the resource classes of the summarized log.
        /// </summary>
        /// <returns>The resource classes of the summarized log.</returns>
        XEventClasses GetResourceClasses();

        /// <summary>
        /// Retrieves the event name classes of the summarized log.
        /// </summary>
        /// <returns>The event name classes of the summarized log.</returns>
        XEventClasses GetNameClasses();

        /// <summary>
        /// Retrieves the lifecycle transition classes of the summarized log.
        /// </summary>
        /// <returns>The lifecycle transition classes of the summarized log.</returns>
        XEventClasses GetTransitionClasses();

        /// <summary>
        /// Retrieves the global timestamp boundaries of this log.
        /// </summary>
        /// <returns>Timestamp boundaries for the complete log.</returns>
        IXTimeBounds GetLogTimeBoundaries();

        /// <summary>
        /// Retrieves the timestamp boundaries for a specified trace.
        /// </summary>
        /// <returns>Timestamp boundaries for the indicated trace.</returns>
        /// <param name="paramXTrace">Trace to be queried for.</param>
        IXTimeBounds GetTraceTimeBoundaries(IXTrace paramXTrace);

        /// <summary>
        /// Retrieves attribute information about all attributes this log contains 
        /// on the log level.
        /// </summary>
        /// <returns> Attribute information on the log level.</returns>
        IXAttributeInfo GetLogAttributeInfo();

        /// <summary>
        /// Retrieves attribute information about all attributes this log contains on the trace level.
        /// </summary>
        /// <returns>Attribute information on the trace level.</returns>
        IXAttributeInfo GetTraceAttributeInfo();

        /// <summary>
        /// Retrieves attribute information about all attributes this log contains on the event level.
        /// </summary>
        /// <returns>Attribute information on the event level.</returns>
        IXAttributeInfo GetEventAttributeInfo();

        /// <summary>
        /// Retrieves attribute information about all attributes this log contains 
        /// on the meta(i.e., attribute) level.
        /// </summary>
        /// <returns>Attribute information on the meta level.</returns>
        IXAttributeInfo GetMetaAttributeInfo();

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:OpenXesNet.info.IXLogInfo"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:OpenXesNet.info.IXLogInfo"/>.</returns>
        string ToString();
    }
}
