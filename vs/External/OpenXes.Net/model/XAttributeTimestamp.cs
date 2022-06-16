using System;
using OpenXesNet.extension;
using System.Globalization;
using System.Xml;
using OpenXesNet.logging;

namespace OpenXesNet.model
{
    public class XAttributeTimestamp : XAttribute, IXAttributeTimestamp
    {
        /// <summary>
        /// Value of the attribute.
        /// </summary>
        DateTime value;
        public DateTime Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public long ValueMillis
        {
            get
            {
                return this.value.Ticks / 10000;
            }
            set
            {
                this.value = new DateTime(value, DateTimeKind.Local);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeTimestamp"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">Value of the attribute.</param>
        public XAttributeTimestamp(string key, DateTime value) : this(key, value, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeTimestamp"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="value">Value of the attribute.</param>
        /// <param name="extension">The extension of the attribute.</param>
        public XAttributeTimestamp(string key, DateTime value, XExtension extension) : base(key, extension)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeTimestamp"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="millis">Value of the attribute, in milliseconds.</param>
        public XAttributeTimestamp(string key, long millis) : this(key, millis, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttributeTimestamp"/> class.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="millis">Value of the attribute, in milliseconds.</param>
        /// <param name="extension">The extension of the attribute.</param>
        public XAttributeTimestamp(string key, long millis, XExtension extension) : this(key, new DateTime(millis), extension)
        {

        }

        public override string ToString()
        {
            return this.value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFZ");
        }

        public override object Clone()
        {
            XAttributeTimestamp clone = (XAttributeTimestamp)base.Clone();
            clone.value = new DateTime(clone.value.Ticks);
            return clone;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            if (obj is XAttributeTimestamp)
            {
                XAttributeTimestamp other = (XAttributeTimestamp)obj;
                return ((base.Equals(other)) && (this.value.Equals(other.value)));
            }

            return false;
        }

        public int CompareTo(XAttribute other)
        {
            if (other is XAttributeTimestamp)
            {
                int result = base.CompareTo(other);
                if (result != 0)
                {
                    return result;
                }
                return this.value.CompareTo(((XAttributeTimestamp)other).value);
            }
            throw new InvalidCastException();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Parse the specified datetimeString.
        /// </summary>
        /// <returns>The object containing the value of the specified datetime.</returns>
        /// <param name="datetimeString">Datetime string.</param>
        public static DateTime Parse(string datetimeString)
        {
            string[] formats = { "yyyy-MM-ddTHH:mm:ss.FFFFFZ", "yyyy-MM-ddTHH:mm:ss.FFFFFK",
                "yyyy-MM-ddTHH:mm:ss:FFFFFZ", "yyyy-MM-ddTHH:mm:ss:FFFFFK"};
            DateTime d;

            try
            {
                d = XmlConvert.ToDateTime(datetimeString, formats).ToUniversalTime();
            }
            catch (Exception e)
            {
                XLogging.Log("Could not parse the string '" + datetimeString + "'", XLogging.Importance.WARNING);
                throw e;
            }

            return d;
        }
    }
}
