using System;
namespace OpenXesNet.classification
{
    /// <summary>
    /// Implements an event classifier based on the resource name attribute of events.
    /// </summary>
    public class XEventResourceClassifier : XEventAttributeClassifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.classification.XEventResourceClassifier"/> class.
        /// </summary>
        public XEventResourceClassifier() : base("Resource", new String[] { "org:resource" })
        {
        }
    }
}
