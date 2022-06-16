using System;
using OpenXesNet.id;
using OpenXesNet.extension;
using OpenXesNet.util;
using System.Collections.Generic;
using OpenXesNet.logging;

namespace OpenXesNet.model
{
    /// <inheritdoc/>
    public class XEvent : IXEvent
    {
        /// <summary>
        /// The Id of the event.
        /// </summary>
        readonly XID id;

        public XID Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Map of attributes for this event.
        /// </summary>
        IXAttributeMap attributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XEvent"/> class.
        /// </summary>
        public XEvent() : this(XIDFactory.Instance.CreateId(), new XAttributeMap())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XEvent"/> class with
        /// a giver id.
        /// </summary>
        /// <param name="id">The id for this event.</param>
        public XEvent(XID id) : this(id, new XAttributeMap())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XEvent"/> class with a 
        /// defined set of attributes.
        /// </summary>
        /// <param name="attributes">Attribute map for this event.</param>
        public XEvent(IXAttributeMap attributes) : this(XIDFactory.Instance.CreateId(), attributes)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XEvent"/> class 
        /// with a given id and a specific set of attributes.
        /// </summary>
        /// <param name="id">The id for this event.</param>
        /// <param name="attributes">Attribute map for this event.</param>
        public XEvent(XID id, IXAttributeMap attributes)
        {
            this.id = id;
            this.attributes = attributes;
        }

        public IXAttributeMap GetAttributes()
        {
            return this.attributes;
        }

        public void SetAttributes(IXAttributeMap attributes)
        {
            this.attributes = attributes;
        }

        public bool HasAttributes()
        {
            return (this.attributes.Count > 0);
        }

        public HashSet<XExtension> Extensions
        {
            get
            {
                return XAttributeUtils.ExtractExtensions(this.attributes);
            }
        }

        /// <summary>
        /// Clones this event, i.e. creates a deep copy, but with a new ID, so equals 
        /// does not hold between this and the clone
        /// </summary>
        /// <returns>The clone.</returns>
        public object Clone()
        {
            XEvent clone;
            try
            {
                clone = (XEvent)MemberwiseClone();
            }
            catch (NotSupportedException e)
            {
                XLogging.Error("Cannot clone this object XEvent");
                throw e;
            }
            clone.attributes = ((XAttributeMap)this.attributes.Clone());
            return clone;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:OpenXesNet.model.XEvent"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:OpenXesNet.model.XEvent"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:OpenXesNet.model.XEvent"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(Object obj)
        {
            if (obj is XEvent)
            {
                return ((XEvent)obj).id.Equals(this.id);
            }
            return false;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:OpenXesNet.model.XEvent"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
#pragma warning disable RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
            return this.id.GetHashCode();
#pragma warning restore RECS0025 // Campo no de solo lectura al que se hace referencia en 'GetHashCode()'
        }

        public void Accept(XVisitor visitor, XTrace trace)
        {
            visitor.VisitEventPre(this, trace);

            foreach (XAttribute attribute in this.attributes.Values)
            {
                attribute.Accept(visitor, this);
            }
            visitor.VisitEventPost(this, trace);
        }
    }
}