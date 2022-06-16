using OpenXesNet.extension;
using OpenXesNet.classification;

namespace OpenXesNet.model
{
    public abstract class XVisitor
    {
        /// <summary>
        /// Checks whether the visitor may run.
        /// </summary>
        /// <returns><c>true</c> if the visitor meets the preconditions.</returns>
        public abstract bool Precondition();

        /// <summary>
        /// Initializes the visitor.
        /// </summary>
        /// <returns>The init.</returns>
        /// <param name="log">The log to visit.</param>
        public abstract void Init(XLog log);

        /// <summary>
        /// First call made when visiting a log.
        /// </summary>
        /// <param name="log">The log to visit.</param>
        public abstract void VisitLogPre(XLog log);

        /// <summary>
        /// Last call made when visiting a log.
        /// </summary>
        /// <param name="log">The log to visit.</param>
        public abstract void VisitLogPost(XLog log);

        /// <summary>
        /// First call made when visiting an extension.
        /// </summary>
        /// <param name="ext">The extension to visit.</param>
        /// <param name="log">The log containing the extension.</param>
        public abstract void VisitExtensionPre(XExtension ext, XLog log);

        /// <summary>
        /// Last call made when visiting an extension.
        /// </summary>
        /// <param name="ext">The extension to visit.</param>
        /// <param name="log">The log containing the extension.</param>
        public abstract void VisitExtensionPost(XExtension ext, XLog log);

        /// <summary>
        /// First call made when visiting a classifier.
        /// </summary>
        /// <param name="classifier">The classifier to visit.</param>
        /// <param name="log">The log containing the classifier.</param>
        public abstract void VisitClassifierPre(IXEventClassifier classifier, XLog log);

        /// <summary>
        /// First call made when visiting a classifier.
        /// </summary>
        /// <param name="classifier">The classifier to visit.</param>
        /// <param name="log">The log containing the classifier.</param>
        public abstract void VisitClassifierPost(IXEventClassifier classifier, XLog log);

        /// <summary>
        /// First call made when visiting a trace.
        /// </summary>
        /// <param name="trace">The trace to visit.</param>
        /// <param name="log">The log containing the trace.</param>
        public abstract void VisitTracePre(IXTrace trace, XLog log);

        /// <summary>
        /// Last call made when visiting a trace.
        /// </summary>
        /// <param name="trace">The trace to visit.</param>
        /// <param name="log">The log containing the trace.</param>
        public abstract void VisitTracePost(IXTrace trace, XLog log);

        /// <summary>
        /// First call made when visiting an event.
        /// </summary>
        /// <param name="evt">The event to visit.</param>
        /// <param name="trace">The trace containing the event.</param>
        public abstract void VisitEventPre(IXEvent evt, XTrace trace);

        /// <summary>
        /// First call made when visiting an event.
        /// </summary>
        /// <param name="evt">The event to visit.</param>
        /// <param name="trace">The trace containing the event.</param>
        public abstract void VisitEventPost(IXEvent evt, XTrace trace);

        /// <summary>
        /// First call made when visiting an attribute.
        /// </summary>
        /// <param name="attr">The attribute to visit..</param>
        /// <param name="parent">The element (log, trace, event or attribute) containing
        /// the attribute.</param>
        public abstract void VisitAttributePre(XAttribute attr, IXAttributable parent);

        /// <summary>
        /// First call made when visiting an attribute.
        /// </summary>
        /// <param name="attr">The attribute to visit..</param>
        /// <param name="parent">The element (log, trace, event or attribute) containing
        /// the attribute.</param>
        public abstract void VisitAttributePost(XAttribute attr, IXAttributable parent);
    }
}
