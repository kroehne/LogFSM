using System;
using System.Collections.Generic;
using OpenXesNet.model;
using System.Linq;
namespace OpenXesNet.extension
{
    public class XExtension
    {
        protected string name;
        public string Name
        {
            get { return this.name; }
        }
        protected string prefix;
        public string Prefix
        {
            get { return this.prefix; }
        }
        protected Uri uri;
        public Uri Uri
        {
            get { return this.uri; }
        }
        protected Dictionary<string, XAttribute> allAttributes;

        protected Dictionary<string, XAttribute> logAttributes;
        public Dictionary<string, XAttribute> LogAttributes
        {
            get { return this.logAttributes; }
        }

        protected Dictionary<string, XAttribute> traceAttributes;
        public Dictionary<string, XAttribute> TraceAttributes
        {
            get { return this.traceAttributes; }
        }

        protected Dictionary<string, XAttribute> eventAttributes;
        public Dictionary<string, XAttribute> EventAttributes
        {
            get { return this.eventAttributes; }
        }

        protected Dictionary<string, XAttribute> metaAttributes;
        public Dictionary<string, XAttribute> MetaAttributes
        {
            get { return this.metaAttributes; }
        }

        public XExtension(string name, string prefix, Uri uri)
        {
            this.name = name;
            this.uri = uri;
            this.prefix = prefix;
            this.allAttributes = null;
            this.logAttributes = new Dictionary<string, XAttribute>();
            this.traceAttributes = new Dictionary<string, XAttribute>();
            this.eventAttributes = new Dictionary<string, XAttribute>();
            this.metaAttributes = new Dictionary<string, XAttribute>();
        }

        public Dictionary<string, XAttribute> GetDefinedAttributes()
        {
            this.allAttributes = new Dictionary<string, XAttribute>();
            this.allAttributes.Concat(this.logAttributes).Concat(this.traceAttributes).Concat(this.eventAttributes).Concat(this.metaAttributes);

            return this.allAttributes;
        }

        public bool Equal(object ext)
        {
            if (ext is XExtension)
            {
                return this.uri.Equals(((XExtension)ext).Uri);
            }
            return false;
        }

        public int HashCode()
        {
            return this.uri.GetHashCode();
        }

        public override string ToString()
        {
            return this.name;
        }

        public void Accept(XVisitor visitor, XLog log)
        {
            visitor.VisitExtensionPre(this, log);
            visitor.VisitExtensionPost(this, log);
        }

        protected string QualifiedName(string key)
        {
            return String.Join(":", this.prefix, key);
        }
    }
}
