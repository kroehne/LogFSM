using System;
namespace OpenXesNet.classification
{
    /// <summary>
    /// Implements an event class. An event class is an identity for events, making 
    /// them comparable.If two events are part of the same class, they are considered 
    /// to be equal, i.e.to be referring to the same higher-level concept.
    /// </summary>
    public class XEventClass : IComparable<XEventClass>
    {
        /// <summary>
        /// Unique index of class
        /// </summary>
        protected int index;
        public int Index
        {
            get { return this.index; }
        }
        /// <summary>
        /// Unique identification string of class.
        /// </summary>
        protected string id;
        public string Id
        {
            get { return this.id; }
        }
        /// <summary>
        /// Size of class, i.e. number of represented instances.
        /// </summary>
        protected int size;
        public int Size
        {
            get { return this.size; }
            set { this.size = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.classification.XEventClass"/> class.
        /// </summary>
        /// <param name="id">Unique identification string of the class, i.e. its name.</param>
        /// <param name="index">Unique index of this event class.</param>
        public XEventClass(string id, int index)
        {
            this.id = id;
            this.index = index;
            this.size = 0;
        }

        public int CompareTo(XEventClass other)
        {
            return string.Compare(this.id, other.Id, StringComparison.Ordinal);
        }

        public void IncrementSize()
        {
            this.size++;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is XEventClass)
            {
                return this.id.Equals(((XEventClass)obj).id);
            }
            return false;
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            return this.id.GetHashCode();
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
        }

        public override string ToString()
        {
            return this.id;
        }
    }
}
