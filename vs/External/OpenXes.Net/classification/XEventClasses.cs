using System;
using System.Collections.Generic;
using System.Linq;
using OpenXesNet.model;

namespace OpenXesNet.classification
{
    /// <summary>
    /// <para>A set of event classes. For any log, this class can be used to impose a
    /// classification of events.Two events which belong to the same event class can
    /// be considered equal, i.e.to refer to the same higher-level concept they
    /// represent(e.g., an activity).</para>
    /// <para>Event classes are imposed on a log by a specific classifier.This class can
    /// be configured with such a classifier, which is then used to derive the actual
    /// event classes from a log, by determining the identity of the contained
    /// events.</para>
    /// </summary>
    public class XEventClasses
    {
        static readonly object Lock = new object();
        /// <summary>
        /// The classifier used for creating the set of event classes.
        /// </summary>
        protected IXEventClassifier classifier;
        /// <summary>
        /// Map holding the event classes, indexed by their unique identifier string
        /// </summary>
        protected Dictionary<string, XEventClass> classMap;

        /// <summary>
        /// Creates a new set of event classes, factory method.
        /// </summary>
        /// <returns>A set of event classes, as an instance of this class.</returns>
        /// <param name="classifier">The classifier to be used for event comparison.</param>
        /// <param name="log">The log, on which event classes should be imposed.</param>
        public static XEventClasses DeriveEventClasses(IXEventClassifier classifier, XLog log)
        {
            lock (Lock)
            {
                XEventClasses nClasses = new XEventClasses(classifier);
                nClasses.Register(log);
                nClasses.HarmonizeIndexes();
                return nClasses;
            }
        }

        /// <summary>
        /// Creates a new instance, i.e. an empty set of event classes.
        /// </summary>
        /// <param name="classifier">The classifier used for event comparison.</param>
        public XEventClasses(IXEventClassifier classifier)
        {
            this.classifier = classifier;
            this.classMap = new Dictionary<string, XEventClass>();
        }

        /// <summary>
        /// Returns the classifier used for determining event classes.
        /// </summary>
        /// <returns>A classifier used in this set of classes.</returns>
        public IXEventClassifier Classifier
        {
            get { return this.classifier; }
        }

        /// <summary>
        /// Returns the collection of event classes contained in this instance
        /// </summary>
        /// <value>A collection of event classes.</value>
        public List<XEventClass> Classes
        {
            get
            {
                List<XEventClass> items = new List<XEventClass>(this.classMap.Values);
                return items;
            }
        }

        /// <summary>
        /// Returns the size of this set of event classes.
        /// </summary>
        /// <value>The number of event classes contained in this set.</value>
        public int Count
        {
            get { return this.classMap.Count; }
        }

        /// <summary>
        /// For any given event, returns the corresponding event class as determined by this set.
        /// </summary>
        /// <returns>The event class of this event, as found in this set of event 
        /// classes. If no matching event class is found, this method may return 
        /// <code>null</code>.</returns>
        /// <param name="evt">The event of which the event class should be determined.</param>
        public XEventClass GetClassOf(IXEvent evt)
        {
            return (this.classMap[this.classifier.GetClassIdentity(evt)]);
        }

        /// <summary>
        /// Returns a given event class by its identity, i.e. its unique identifier string.
        /// </summary>
        /// <returns>The requested event class. If no matching event class is found, 
        /// this method may return <code>null</code>.</returns>
        /// <param name="classIdentity">Identifier string of the requested event class.</param>
        public XEventClass GetByIdentity(string classIdentity)
        {
            return this.classMap[classIdentity];
        }

        /// <summary>
        /// Returns a given event class by its unique index.
        /// </summary>
        /// <returns>The requested event class. If no matching event class is found, 
        /// this method may return <code>null</code>.</returns>
        /// <param name="index">Unique index of the requested event class.</param>
        public XEventClass GetByIndex(int index)
        {
            foreach (XEventClass eventClass in this.classMap.Values)
            {
                if (eventClass.Index == index)
                {
                    return eventClass;
                }
            }
            return null;
        }

        /// <summary>
        /// Registers a log with this set of event classes. This will result in all 
        /// events of this log being analyzed, and potentially new event classes 
        /// being added to this set of event classes.Event classes will be 
        /// incremented in size, as new members of these classes are found among the 
        /// events in the log.
        /// </summary>
        /// <param name="log"> The log to be analyzed</param>
        public void Register(IXLog log)
        {
            foreach (IXTrace trace in log)
            {
                Register(trace);
            }
        }

        /// <summary>
        /// Registers a trace with this set of event classes. This will result in all 
        /// events of this trace being analyzed, and potentially new event classes 
        /// being added to this set of event classes.Event classes will be 
        /// incremented in size, as new members of these classes are found among the 
        /// events in the trace.
        /// </summary>
        /// <param name="trace">The trace to be analyzed.</param>
        public void Register(IXTrace trace)
        {
            foreach (IXEvent evt in trace)
            {

                Register(evt);
            }
        }

        /// <summary>
        /// Registers an event with this set of event classes. This will potentially 
        /// add a new event class to this set of event classes.An event class will 
        /// be incremented in size, if the given event is found to be a member of it.
        /// </summary>
        /// <param name="evt">The event to be analyzed.</param>
        public void Register(IXEvent evt)
        {
            Register(this.classifier.GetClassIdentity(evt));
        }

        /// <summary>
        /// Registers an event class with this set of event classes. This will potentially 
        /// add a new event class to this set of event classes.An event class will 
        /// be incremented in size, if the given event is found to be a member of it.
        /// </summary>
        /// <param name="classId"> The event class id to be analyzed.</param>
        public void Register(string classId)
        {
            lock (Lock)
            {
                XEventClass eventClass;
                if (classId != null && this.classMap.ContainsKey(classId))
                {
                    eventClass = this.classMap[classId];
                    eventClass.IncrementSize();
                }
                else
                {
                    eventClass = new XEventClass(classId, this.classMap.Count);
                    this.classMap.Add(classId, eventClass);
                }
            }
        }

        /// <summary>
        /// This method harmonizeds the indices of all contained event classes. 
        /// Indices are re-assigned according to the natural order of class 
        /// identities, i.e., the alphabetical order of class identity strings.This
        /// method should be called after the composition or derivation of event
        /// classes is complete, e.g., after scanning a log for generating the log
        /// info. Using parties should not have to worry about event class
        /// harmonization, and can thus safely ignore this method.
        /// </summary>
        public void HarmonizeIndexes()
        {
            lock (Lock)
            {
                List<XEventClass> classList = this.classMap.Values.ToList();
                classList.Sort();

                this.classMap.Clear();
                for (int i = 0; i < classList.Count(); ++i)
                {
                    XEventClass original = classList[i];
                    XEventClass harmonized = new XEventClass(original.Id, i);
                    harmonized.Size = original.Size;
                    this.classMap.Add(harmonized.Id, harmonized);
                }
            }
        }

        public override bool Equals(Object obj)
        {
            if (obj is XEventClasses)
            {
                return ((XEventClasses)obj).Classifier.Equals(this.classifier);
            }
            return false;
        }

        public override string ToString() => "Event classes defined by " + this.classifier.Name;

        public override int GetHashCode() => base.GetHashCode();
    }
}
