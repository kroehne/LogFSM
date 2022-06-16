using System;
using OpenXesNet.extension;

namespace OpenXesNet.model
{
    public class XAttributeLiteral : XAttribute, IXAttributeLiteral
    {
        /// <summary>
        /// Value of the attribute
        /// </summary>
        string value;

        public string Value
        {
            get { return this.value; }
            set { this.value = value ?? throw new NullReferenceException("No null value allowed in literal attribute!"); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeLiteral"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        public XAttributeLiteral(string key, string value) : this(key, value, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeLiteral"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <param name="extension">The extension of the attribute.</param>
        public XAttributeLiteral(string key, string value, XExtension extension) : base(key, extension)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return this.value;
        }

        public override object Clone()
        {
            XAttributeLiteral clone = (XAttributeLiteral)base.Clone();
            clone.value = string.Copy(this.value);
            return clone;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            if (obj is XAttributeLiteral)
            {
                XAttributeLiteral other = (XAttributeLiteral)obj;
                return ((base.Equals(other)) && (this.value.Equals(other.value)));
            }

            return false;
        }

        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            return (new Object[] { this.Key, this.value }).GetHashCode();
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
        }

        public int CompareTo(XAttribute other)
        {
            if (!(other is XAttributeLiteral))
            {
                throw new InvalidCastException();
            }
            int result = base.CompareTo(other);
            if (result != 0)
            {
                return result;
            }
            return string.Compare(this.value, ((XAttributeLiteral)other).value, StringComparison.Ordinal);
        }
    }
}
