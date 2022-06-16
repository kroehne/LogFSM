using System;
using OpenXesNet.classification;
using OpenXesNet.model;
using System.Collections.Generic;

namespace OpenXesNet.info
{
    /// <summary>
    /// This class implements a bare-bones log info summary which can be created on
    /// demand by using applications. The log info summary is based on an event
    /// classifier, which is used to identify event class abstractions.
    /// </summary>
    public class XLogInfo : IXLogInfo
    {
        /// <summary>
        /// Default event classifier. This classifier considers two events as belonging 
        /// to the same class, if they have both the same event name and the same 
        /// lifecycle transition (if available).
        /// </summary>
        public static readonly IXEventClassifier STANDARD_CLASSIFIER =
            new XEventAttributeClassifier("MXML Legacy Classifier", new String[] { "concept:name", "lifecycle:transition" });

        /// <summary>
        /// Standard event classifier. This classifier considers two  events as 
        /// belonging to the same class, if they have the same value for the event name attribute.
        /// </summary>
        public static readonly IXEventClassifier NAME_CLASSIFIER = new XEventNameClassifier();
        /// <summary>
        /// Standard event classifier. This classifier considers two events as belonging 
        /// to the same class, if they have the same value for the resource attribute.
        /// </summary>
        public static readonly IXEventClassifier RESOURCE_CLASSIFIER = new XEventResourceClassifier();
        /// <summary>
        /// Standard event classifier. This classifier considers two events as belonging 
        /// to the same class, if they have the same value for the lifecycle transition attribute.
        /// </summary>
        public static readonly IXEventClassifier LIFECYCLE_TRANSITION_CLASSIFIER = new XEventLifeTransClassifier();

        /// <summary>
        /// The event log which is summarized.
        /// </summary>
        protected XLog log;
        /// <summary>
        /// The total number of events in this log.
        /// </summary>
        protected int numberOfEvents;
        /// <summary>
        /// The number of traces in this log.
        /// </summary>
        protected int numberOfTraces;
        /// <summary>
        /// Maps the event classifiers covered in this log info to their respectively created event classes.
        /// </summary>
        protected Dictionary<IXEventClassifier, XEventClasses> eventClasses;
        /// <summary>
        /// The default event classifier for this log info instance.
        /// </summary>
        protected IXEventClassifier defaultClassifier;
        /// <summary>
        /// Timestamp boundaries for the complete log.
        /// </summary>
        protected IXTimeBounds logBoundaries;
        /// <summary>
        /// Map of timestamp boundaries for each trace, indexed by reference to the respective trace.
        /// </summary>
        protected Dictionary<IXTrace, IXTimeBounds> traceBoundaries;
        /// <summary>
        /// Attribute information registry on the log level.
        /// </summary>
        protected XAttributeInfo logAttributeInfo;
        /// <summary>
        /// Attribute information registry on the trace level.
        /// </summary>
        protected XAttributeInfo traceAttributeInfo;
        /// <summary>
        /// Attribute information registry on the event level.
        /// </summary>
        protected XAttributeInfo eventAttributeInfo;
        /// <summary>
        /// Attribute information registry on the meta level.
        /// </summary>
        protected XAttributeInfo metaAttributeInfo;

        /// <summary>
        /// Creates a new log info summary with the standard event classifier.
        /// </summary>
        /// <returns>The log info for this log.</returns>
        /// <param name="log">The event log to create an info summary for.</param>
        public static XLogInfo Create(XLog log)
        {
            return Create(log, STANDARD_CLASSIFIER);
        }

        /// <summary>
        /// Creates a new log info summary with the custom event classifier.
        /// </summary>
        /// <returns>The event log to create an info summary for.</returns>
        /// <param name="log">The event log to create an info summary for.</param>
        /// <param name="defaultClassifier">The default event classifier to be used.</param>
        public static XLogInfo Create(XLog log, IXEventClassifier defaultClassifier)
        {
            return Create(log, defaultClassifier, null);
        }

        /// <summary>
        /// Creates a new log info summary with a collection of custom  event classifiers.
        /// </summary>
        /// <returns>The event log to create an info summary for.</returns>
        /// <param name="log">The event log to create an info summary for.</param>
        /// <param name="defaultClassifier">The default event classifier to be used.</param>
        /// <param name="classifiers">A collection of additional event classifiers to 
        /// be covered by the created log info instance.</param>
        public static XLogInfo Create(XLog log, IXEventClassifier defaultClassifier,
                List<IXEventClassifier> classifiers)
        {
            return new XLogInfo(log, defaultClassifier, classifiers);
        }

        /// <summary>
        /// Creates a new log summary.
        /// </summary>
        /// <param name="log">The log to create a summary of.</param>
        /// <param name="defaultClassifier">The event classifier to be used.</param>
        /// <param name="classifiers">Classifiers.</param>
        /// <param name="classifiers">A collection of additional event classifiers to 
        /// be covered by the created log info instance.</param>
        public XLogInfo(XLog log, IXEventClassifier defaultClassifier, List<IXEventClassifier> classifiers)
        {
            this.log = log;
            this.defaultClassifier = defaultClassifier;
            if (classifiers == null)
            {
                classifiers = new List<IXEventClassifier>(0);
            }
            this.eventClasses = new Dictionary<IXEventClassifier, XEventClasses>(classifiers.Count + 4);
            foreach (IXEventClassifier classifier in classifiers)
            {
                this.eventClasses.Add(classifier, new XEventClasses(classifier));
            }
            this.eventClasses.Add(this.defaultClassifier, new XEventClasses(this.defaultClassifier));
            this.eventClasses.Add(NAME_CLASSIFIER, new XEventClasses(NAME_CLASSIFIER));
            this.eventClasses.Add(RESOURCE_CLASSIFIER, new XEventClasses(RESOURCE_CLASSIFIER));
            this.eventClasses.Add(LIFECYCLE_TRANSITION_CLASSIFIER, new XEventClasses(LIFECYCLE_TRANSITION_CLASSIFIER));
            this.numberOfEvents = 0;
            this.numberOfTraces = 0;
            this.logBoundaries = new XTimeBounds();
            this.traceBoundaries = new Dictionary<IXTrace, IXTimeBounds>();
            this.logAttributeInfo = new XAttributeInfo();
            this.traceAttributeInfo = new XAttributeInfo();
            this.eventAttributeInfo = new XAttributeInfo();
            this.metaAttributeInfo = new XAttributeInfo();
            Setup();
        }

        /// <summary>
        /// Creates the internal data structures of this summary on setup from the log.
        /// </summary>
        protected void Setup()
        {
            lock (this)
            {
                RegisterAttributes(this.logAttributeInfo, this.log);
                foreach (XTrace trace in this.log)
                {
                    this.numberOfTraces += 1;
                    RegisterAttributes(this.traceAttributeInfo, trace);
                    XTimeBounds traceBounds = new XTimeBounds();
                    foreach (XEvent evt in trace)
                    {
                        RegisterAttributes(this.eventAttributeInfo, evt);
                        foreach (XEventClasses classes in this.eventClasses.Values)
                        {
                            classes.Register(evt);
                        }
                        traceBounds.Register(evt);
                        this.numberOfEvents += 1;
                    }
                    this.traceBoundaries.Add(trace, traceBounds);
                    this.logBoundaries.Register(traceBounds);
                }

                foreach (XEventClasses classes in this.eventClasses.Values)
                {
                    classes.HarmonizeIndexes();
                }
            }
        }

        /// <summary>
        /// Registers all attributes of a given attributable, i.e. model type hierarchy 
        /// element, in the given attribute info registry.
        /// </summary>
        /// <param name="attributeInfo">Attribute info registry to use for registration.</param>
        /// <param name="attributable">Attributable whose attributes to register.</param>
        protected void RegisterAttributes(XAttributeInfo attributeInfo, IXAttributable attributable)
        {
            if (attributable.HasAttributes())
                foreach (XAttribute attribute in attributable.GetAttributes().Values)
                {
                    attributeInfo.Register(attribute);
                    RegisterAttributes(this.metaAttributeInfo, (IXAttributable)attribute);
                }
        }

        public IXLog GetLog()
        {
            return this.log;
        }

        public int NumberOfEvents
        {
            get { return this.numberOfEvents; }
        }

        public int NumberOfTraces
        {
            get { return this.numberOfTraces; }
        }

        public XEventClasses GetEventClasses(IXEventClassifier classifier)
        {
            return this.eventClasses[classifier];
        }

        public HashSet<IXEventClassifier> GetEventClassifiers()
        {
            return new HashSet<IXEventClassifier>(this.eventClasses.Keys);
        }

        /// <summary>
        /// Gets the XEventClasses using the default classifier.
        /// </summary>
        /// <returns>The name classes.</returns>
        public XEventClasses GetEventClasses()
        {
            return GetEventClasses(this.defaultClassifier);
        }

        /// <summary>
        /// Gets the XEventClasses using a default Resource classifier.
        /// </summary>
        /// <returns>The name classes.</returns>
        public XEventClasses GetResourceClasses()
        {
            return GetEventClasses(RESOURCE_CLASSIFIER);
        }
        /// <summary>
        /// Gets the XEventClasses using a default Name classifier.
        /// </summary>
        /// <returns>The name classes.</returns>
        public XEventClasses GetNameClasses()
        {
            return GetEventClasses(NAME_CLASSIFIER);
        }

        /// <summary>
        /// Gets the XEventClasses using a default Transition classifier.
        /// </summary>
        /// <returns>The name classes.</returns>
        public XEventClasses GetTransitionClasses()
        {
            return GetEventClasses(LIFECYCLE_TRANSITION_CLASSIFIER);
        }

        public IXTimeBounds GetLogTimeBoundaries()
        {
            return this.logBoundaries;
        }

        public IXTimeBounds GetTraceTimeBoundaries(IXTrace trace)
        {
            return ((XTimeBounds)this.traceBoundaries[trace]);
        }

        public IXAttributeInfo GetLogAttributeInfo()
        {
            return this.logAttributeInfo;
        }

        public IXAttributeInfo GetTraceAttributeInfo()
        {
            return this.traceAttributeInfo;
        }

        public IXAttributeInfo GetEventAttributeInfo()
        {
            return this.eventAttributeInfo;
        }

        public IXAttributeInfo GetMetaAttributeInfo()
        {
            return this.metaAttributeInfo;
        }
    }
}
