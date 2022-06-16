using System;
using System.Collections.Generic;
using System.Text;

namespace OpenXesNet.classification
{
    /// <summary>
    /// Composite event classifier, which can hold any number of lower-level 
    /// classifiers, concatenated with boolean AND logic.
    /// This classifier will consider two events as equal, if all of its lower-level
    /// classifiers consider them as equal.
    /// </summary>
    public class XEventAndClassifier : XEventAttributeClassifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.classification.XEventAndClassifier"/> class.
        /// </summary>
        /// <param name="comparators">Any number of lower-level classifiers, which are evaluated
        /// with boolean AND logic. If multiple lower-level classifiers
        /// use the same keys, this key is used only once in this classifier.</param>
        public XEventAndClassifier(IXEventClassifier[] comparators) : base("", new string[] { })
        {
            SortedSet<string> sKeys = new SortedSet<string>();

            Array.Sort(comparators);

            List<string> names = new List<string>();

            foreach (IXEventClassifier comparator in comparators)
            {
                names.Add(comparator.Name);
                sKeys.UnionWith(comparator.DefiningAttributeKeys);
            }

            this.name = "(" + String.Join(" AND ", names) + ")";

            sKeys.CopyTo(this.keys);
        }
    }
}
