using System;
using OpenXesNet.extension;

namespace OpenXesNet.model
{
    public class XAttributeDiscrete : XAttribute, IXAttributeDiscrete
    {
        /// <summary>
        /// Thevalue of the attribute
        /// </summary>
        long value;

        public long Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeDiscrete"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">Value of the attribute.</param>
        public XAttributeDiscrete(string key, long value) : this(key, value, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeDiscrete"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">Value of the attribute.</param>
        /// <param name="extension">The extension of the attribute.</param>
        public XAttributeDiscrete(string key, long value, XExtension extension) : base(key, extension)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public override object Clone()
        {
            return base.Clone();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            if (obj is XAttributeDiscrete)
            {
                XAttributeDiscrete other = (XAttributeDiscrete)obj;
                return ((base.Equals(other)) && (this.value == other.value));
            }

            return false;
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            return (new Object[] { this.Key, this.value }).GetHashCode();
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
        }

        public override int CompareTo(object obj)
        {
            if (!(obj is XAttributeDiscrete))
            {
                throw new InvalidCastException();
            }
            int result = base.CompareTo(obj);
            if (result != 0)
            {
                return result;
            }
            return this.value.CompareTo(((XAttributeDiscrete)obj).value);
        }
    }
}
