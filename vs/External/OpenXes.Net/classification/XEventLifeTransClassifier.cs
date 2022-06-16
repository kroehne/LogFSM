using System;
namespace OpenXesNet.classification
{
    /// <summary>
    /// Implements an event classifier based on the lifecycle transition attribute of events
    /// </summary>
    public class XEventLifeTransClassifier : XEventAttributeClassifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.classification.XEventLifeTransClassifier"/> class.
        /// </summary>
        public XEventLifeTransClassifier() : base("Lifecycle transition", new String[] { "lifecycle:transition" })
        {
        }
    }
}
