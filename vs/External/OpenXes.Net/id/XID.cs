using System;
using OpenXesNet.logging;

namespace OpenXesNet.id
{
    /// <summary>
    /// Implements a unique ID based on UUID.
    /// </summary>
    public class XID : ICloneable, IComparable<XID>
    {
        /// <summary>
        /// Guid implementation of XID identity.
        /// </summary>
        readonly Guid guid;

        /// <summary>
        /// Parses an XID object from its text representation.
        /// </summary>
        /// <returns>The parsed XID.</returns>
        /// <param name="uuid">Text representation of an XID.</param>
        public static XID Parse(string uuid)
        {
            Guid id = Guid.Parse(uuid);
            return new XID(id);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.id.XID"/> class with a reandom value.
        /// </summary>
        public XID()
        {
            this.guid = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.id.XID"/> class with a specified value.
        /// </summary>
        /// <param name="uuid">UUID.</param>
        public XID(Guid uuid)
        {
            this.guid = uuid;
        }

        /// <summary>
        /// Tests XID object for equality
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:OpenXesNet.id.XID"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="T:OpenXesNet.id.XID"/>;
        /// otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is XID)
            {
                XID other = (XID)obj;
                return this.guid.Equals(other.guid);
            }
            return false;
        }

        /// <summary>
        /// Returns the string representation of an XID instance.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:OpenXesNet.id.XID"/>.</returns>
        public override string ToString()
        {
            return this.guid.ToString().ToUpper();
        }

        /// <summary>
        /// Creates a clone of this ID.
        /// </summary>
        /// <returns>The clone.</returns>
        public object Clone()
        {
            XID clone;
            try
            {
                clone = (XID)MemberwiseClone();
            }
            catch (NotSupportedException e)
            {
                XLogging.Log(e.Message, XLogging.Importance.ERROR);
                clone = null;
            }
            return clone;
        }

        public override int GetHashCode()
        {
            return this.guid.GetHashCode();
        }

        public int CompareTo(XID o)
        {
            return this.guid.CompareTo(o.guid);
        }
    }
}
