using System.Collections.Generic;
using OpenXesNet.info;
using OpenXesNet.classification;
using OpenXesNet.extension;

namespace OpenXesNet.model
{
    /// <summary>
    /// A log is an element of an XES event log structure. Logs are contained in 
    /// archives. Any log is a list of traces.
    /// Logs represent a collection of traces, which are all representing executions 
    /// of the same kind of process.
    /// </summary>
    public interface IXLog : IXElement, IList<IXTrace>
    {
        /// <summary>
        /// This method returns the list of classifiers defined for this log. This
        /// list can be used for reading or writing, i.e., it must be supported to
        /// add further classifiers to this list.        
        /// </summary>
        /// <value>The list of classifiers defined for this log.</value>
        List<IXEventClassifier> Classifiers { get; }

        /// <summary>
        /// This method returns a list of attributes which are global for all traces, 
        /// i.e.every trace in the log is guaranteed to have these attributes.
        /// </summary>
        /// <value>List of ubiquitous trace attributes.</value>
        List<XAttribute> GlobalTraceAttributes { get; }

        /// <summary>
        /// This method returns a list of attributes which are global for all events, 
        /// i.e.every event in the log is guaranteed to have these attributes.
        /// </summary>
        /// <value>List of ubiquitous event attributes.</value>
        List<XAttribute> GlobalEventAttributes { get; }

        /// <summary>
        /// Runs the given visitor in the current log instance.
        /// </summary>
        /// <returns>The accept.</returns>
        /// <param name="visitor">The visitor to run.</param>
        bool Accept(XVisitor visitor);

        /// <summary>
        /// Returns the cached info for the given classifier, null if not available.
        /// </summary>
        /// <returns>The info.</returns>
        /// <param name="eventClassifier">The given classifier.</param>
        IXLogInfo GetInfo(IXEventClassifier eventClassifier);

        /// <summary>
        /// Adds the given info for the given classifier to the info cache
        /// </summary>
        /// <param name="eventClassifier">The given event classifier.</param>
        /// <param name="logInfo">The given info.</param>
        void SetInfo(IXEventClassifier eventClassifier, IXLogInfo logInfo);
    }
}
