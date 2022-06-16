using System;
using System.Collections.Generic;
using OpenXesNet.extension;
using OpenXesNet.classification;
using OpenXesNet.info;

namespace OpenXesNet.model
{
    public class XLog : List<IXTrace>, IXLog
    {
        /// <summary>
        /// Map of attributes for this log.
        /// </summary>
        IXAttributeMap attributes;

        /// <summary>
        /// The extensions.
        /// </summary>
        HashSet<XExtension> extensions;
        public HashSet<XExtension> Extensions
        {
            get { return this.extensions; }
        }

        /// <summary>
        /// The classifiers.
        /// </summary>
        List<IXEventClassifier> classifiers;
        public List<IXEventClassifier> Classifiers
        {
            get { return this.classifiers; }
        }

        /// <summary>
        /// The global trace attributes.
        /// </summary>
        List<XAttribute> globalTraceAttributes;
        public List<XAttribute> GlobalTraceAttributes
        {
            get { return this.globalTraceAttributes; }
        }

        /// <summary>
        /// The global event attributes.
        /// </summary>
        List<XAttribute> globalEventAttributes;
        public List<XAttribute> GlobalEventAttributes
        {
            get { return this.globalEventAttributes; }
        }

        /// <summary>
        /// The cached classifier.
        /// </summary>
        IXEventClassifier cachedClassifier;

        /// <summary>
        /// Single-item cache. Only the last info is cached. 
        /// Typically, only one classifier will be used for a log.
        /// </summary>
        IXLogInfo cachedInfo;

        /// <summary>
        /// The log version
        /// </summary>
        string version;
        public string Version
        {
            get { return this.version; }
            set { this.version = value; }
        }

        /// <summary>
        /// Log features
        /// </summary>
        string features;
        public string Features
        {
            get { return this.features; }
            set { this.features = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XLog"/> class.
        /// </summary>
        /// <param name="attributes">The attribute map used to store this log's attributes.</param>
        /// <param name="initialCapacity">Initial capacity. If not specified, a default
        /// value of 0 is used.</param>
        public XLog(IXAttributeMap attributes, int initialCapacity = 0) : base(initialCapacity)
        {
            this.attributes = attributes;
            this.extensions = new HashSet<XExtension>();
            this.classifiers = new List<IXEventClassifier>();
            this.globalEventAttributes = new List<XAttribute>();
            this.globalTraceAttributes = new List<XAttribute>();
            this.cachedInfo = null;
            this.cachedClassifier = null;
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
            return this.attributes.Count > 0;
        }

        public object Clone()
        {
            XLog clone = new XLog((IXAttributeMap)this.attributes.Clone(), this.Count)
            {
                extensions = new HashSet<XExtension>(this.extensions),
                classifiers = new List<IXEventClassifier>(this.classifiers),
                globalTraceAttributes = new List<XAttribute>(this.globalTraceAttributes),
                globalEventAttributes = new List<XAttribute>(this.globalEventAttributes),
                cachedClassifier = null,
                cachedInfo = null
            };
            foreach (IXTrace trace in this)
            {
                this.Add((IXTrace)trace.Clone());
            }

            return clone;
        }

        public bool Accept(XVisitor visitor)
        {
            if (!visitor.Precondition())
            {
                return false;
            }
            visitor.Init(this);
            visitor.VisitLogPre(this);
            foreach (XExtension ext in this.extensions)
            {
                ext.Accept(visitor, this);
            }

            foreach (IXEventClassifier classif in this.classifiers)
            {
                classif.Accept(visitor, this);
            }

            foreach (XAttribute attr in this.attributes.Values)
            {
                attr.Accept(visitor, this);
            }

            foreach (IXTrace trace in this)
            {
                trace.Accept(visitor, this);
            }

            visitor.VisitLogPost(this);
            return true;

        }

        public IXLogInfo GetInfo(IXEventClassifier classifier)
        {
            return classifier.Equals(this.cachedClassifier) ? this.cachedInfo : null;
        }

        public void SetInfo(IXEventClassifier classifier, IXLogInfo info)
        {
            this.cachedClassifier = classifier;
            this.cachedInfo = info;
        }
    }
}
