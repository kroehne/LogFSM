using System;
using OpenXesNet.extension;

namespace OpenXesNet.model
{
    public class XAttributeBoolean : XAttribute, IXAttributeBoolean
    {
        /// <summary>
        /// The value of the attribute
        /// </summary>
        bool value;

        public bool Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeBoolean"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">Value of the attribute.</param>
        public XAttributeBoolean(string key, bool value) : this(key, value, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeBoolean"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">Value of the attribute.</param>
        /// <param name="extension">The extension of the attribute.</param>
        public XAttributeBoolean(string key, bool value, XExtension extension) : base(key, extension)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return ((this.value) ? "true" : "false");
        }

        public override object Clone()
        {
            return base.Clone();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            if (obj is XAttributeBoolean)
            {
                XAttributeBoolean other = (XAttributeBoolean)obj;
                return ((base.Equals(other)) && (this.value == other.value));
            }

            return false;
        }

#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
        public override int GetHashCode() => (new Object[] { this.Key, this.value }).GetHashCode();
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'

        public int CompareTo(XAttribute other)
        {
            if (!(other is XAttributeBoolean))
            {
                throw new InvalidCastException();
            }
            int result = base.CompareTo(other);
            if (result != 0)
            {
                return result;
            }
            return this.value.CompareTo(((XAttributeBoolean)other).value);
        }
    }
}
