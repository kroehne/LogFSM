using System;
namespace OpenXesNet.classification
{
    /// <summary>
    /// Implements an event classifier based on the activity name of events.
    /// </summary>
    public class XEventNameClassifier : XEventAttributeClassifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.classification.XEventNameClassifier"/> class.
        /// </summary>
        public XEventNameClassifier() : base("Event Name", new String[] { "concept:name" })
        {
        }
    }
}
