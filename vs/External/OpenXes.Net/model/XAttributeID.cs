using System;
using OpenXesNet.id;
using OpenXesNet.extension;

namespace OpenXesNet.model
{
    public class XAttributeID : XAttribute, IXAttributeID
    {
        /// <summary>
        /// Value of the attribute
        /// </summary>
        XID value;

        public XID Value
        {
            get { return this.value; }
            set
            {
                this.value = value ?? throw new NullReferenceException("No null value allowed in ID attribute!");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeID"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">Value of the attribute.</param>
        public XAttributeID(string key, XID value) : this(key, value, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeID"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">Value of the attribute.</param>
        /// <param name="extension">The extension of the attribute.</param>
        public XAttributeID(string key, XID value, XExtension extension) : base(key, extension)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public override object Clone()
        {
            XAttributeID clone = (XAttributeID)MemberwiseClone();
            clone.value = ((XID)this.value.Clone());
            return clone;
        }

        public override bool Equals(Object obj)
        {
            if (obj == this)
                return true;
            if (obj is XAttributeID)
            {
                XAttributeID other = (XAttributeID)obj;
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

        public override int CompareTo(object obj)
        {
            if (!(obj is XAttributeID))
            {
                throw new InvalidCastException();
            }
            int result = base.CompareTo(obj);
            if (result != 0)
            {
                return result;
            }
            return this.value.CompareTo(((XAttributeID)obj).value);
        }
    }
}
