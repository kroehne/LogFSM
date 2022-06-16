using System;
using OpenXesNet.model;
using System.Collections.Generic;
using System.Globalization;

namespace OpenXesNet.classification
{
    /// <summary>
    /// Event classifier which considers two events as equal, if, for a set of given 
    /// (configurable) attributes, they have the same values.
    /// </summary>
    public class XEventAttributeClassifier : IXEventClassifier, IComparable<XEventAttributeClassifier>
    {
        /// <summary>
        /// Used to connect multiple attributes
        /// </summary>
        static readonly string CONCATENATION_SYMBOL = "+";
        /// <summary>
        /// Keys of the attributes used for event comparison.
        /// </summary>
        protected string[] keys;
        /// <summary>
        /// Name of the classifier.
        /// </summary>
        protected string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.classification.XEventAttributeClassifier"/> class.
        /// </summary>
        /// <param name="name">Name of the classifier.</param>
        /// <param name="keys">Attribute to be used for event classification.</param>
        public XEventAttributeClassifier(string name, string[] keys)
        {
            this.name = name;
            this.keys = keys;
            Array.Sort(this.keys);
        }

        public void Accept(XVisitor visitor, XLog log)
        {
            visitor.VisitClassifierPre(this, log);
            visitor.VisitClassifierPost(this, log);
        }

        public int CompareTo(XEventAttributeClassifier other)
        {
            if (!(other.name.Equals(this.name)))
            {
                return string.Compare(this.name, other.name, StringComparison.Ordinal);
            }
            if (this.keys.Length != other.keys.Length)
            {
                return (this.keys.Length - other.keys.Length);
            }
            for (int i = 0; i < this.keys.Length; ++i)
            {
                if (!(this.keys[i].Equals(other.keys[i])))
                {
                    return string.Compare(this.keys[i], other.keys[i], StringComparison.CurrentCulture);
                }
            }

            return 0;
        }

        public string GetClassIdentity(IXEvent evt)
        {
            List<string> values = new List<string>();
            foreach (string key in keys)
            {
                values.Add(evt.GetAttributes()[key].ToString());
            }
            return String.Join(CONCATENATION_SYMBOL, values);
        }

        public string[] DefiningAttributeKeys
        {
            get { return this.keys; }
        }

        public List<string> DefiningAttributeKeysAsList
        {
            get { return new List<string>(this.keys); }
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public bool SameEventClass(IXEvent event1, IXEvent event2)
        {
            return this.GetClassIdentity(event1).Equals(this.GetClassIdentity(event2));
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            result = prime * result + this.keys.GetHashCode();
            result = prime * result + ((this.name == null) ? 0 : this.name.GetHashCode());
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            return result;
        }

        public override string ToString() => this.name;

        public override bool Equals(object obj)
        {
            if (obj is XEventAttributeClassifier)
            {
                return (CompareTo((XEventAttributeClassifier)obj) == 0);
            }
            return false;
        }
    }
}
