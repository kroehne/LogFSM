using System;
using System.Collections.Generic;
using OpenXesNet.logging;
using OpenXesNet.extension;
using OpenXesNet.util;

namespace OpenXesNet.model
{
    public abstract class XAttribute<T> : XAttribute, IXAttribute<T>
    {
        protected XAttribute(string key) : base(key, null)
        {
        }

        protected XAttribute(string key, XExtension extension) : base(key, extension)
        {
        }

        public abstract T Value { get; set; }
    }
    public abstract class XAttribute
    {
        /// <summary>
        /// The attribute key
        /// </summary>
        readonly string key;

        /// <summary>
        /// The attribute extension
        /// </summary>
        readonly XExtension extension;

        public string Key
        {
            get { return this.key; }
        }

        public XExtension Extension
        {
            get { return this.extension; }
        }

        /// <summary>
        /// Map of meta-attributes, i.e. attributes of this attribute.
        /// </summary>
        IXAttributeMap attributes;

        /// <summary>
        /// Initializes an empty instance of the <see cref="T:OpenXesNet.model.XAttribute"/> class.
        /// </summary>
        /// <param name="key">The key, i.e. unique name identifier, of this attribute.</param>
        protected XAttribute(string key) : this(key, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XAttribute"/> class.
        /// </summary>
        /// <param name="key">The key, i.e. unique name identifier, of this attribute.</param>
        /// <param name="extension">The extension used for defining this attribute.</param>
        protected XAttribute(string key, XExtension extension)
        {
            this.key = key;
            this.extension = extension;
        }

        public IXAttributeMap GetAttributes()
        {
            if (this.attributes == null)
            {
                this.attributes = new XAttributeMapLazy<XAttributeMap>();
            }

            return this.attributes;

        }

        public void SetAttributes(IXAttributeMap attributes) => this.attributes = attributes;

        public bool HasAttributes()
        {
            return ((this.attributes != null) && (this.attributes.Count > 0));
        }

        public HashSet<XExtension> Extensions
        {
            get
            {
                if (this.attributes != null)
                {
                    return XAttributeUtils.ExtractExtensions(this.GetAttributes());
                }
                return new HashSet<XExtension>();
            }
        }

        public virtual object Clone()
        {
            XAttribute clone = null;
            try
            {
                clone = (XAttribute)this.MemberwiseClone();

            }
            catch (NotSupportedException e)
            {
                XLogging.Log(e.Message, XLogging.Importance.ERROR);
                return null;
            }
            if (this.attributes != null)
            {
                clone.attributes = ((IXAttributeMap)GetAttributes().Clone());
            }
            return clone;
        }

        public override bool Equals(object obj)
        {
            if (obj is XAttribute)
            {
                XAttribute other = (XAttribute)obj;
                return ((XAttribute)obj).Key.Equals(this.key);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.key.GetHashCode();
        }

        public virtual int CompareTo(object obj)
        {
            return string.Compare(this.key, ((XAttribute)obj).Key, StringComparison.Ordinal);
        }

        public void Accept(XVisitor visitor, IXAttributable parent)
        {
            visitor.VisitAttributePre(this, parent);
            if (this is IXAttributeCollection)
            {
                foreach (XAttribute attribute in ((IXAttributeCollection)this).GetCollection())
                {
                    attribute.Accept(visitor, (IXAttributable)this);
                }

            }
            else if (this.attributes != null)
            {
                foreach (XAttribute attribute in GetAttributes().Values)
                {
                    attribute.Accept(visitor, (IXAttributable)this);
                }

            }
            visitor.VisitAttributePost(this, parent);
        }
    }
}
