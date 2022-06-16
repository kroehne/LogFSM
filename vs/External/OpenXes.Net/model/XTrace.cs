using System;
using System.Collections.Generic;
using OpenXesNet.extension;
using OpenXesNet.util;

namespace OpenXesNet.model
{
    public class XTrace : List<IXEvent>, IXTrace
    {
        /// <summary>
        /// Map of attributes for this trace.
        /// </summary>
        IXAttributeMap attributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OpenXesNet.model.XTrace"/> class with
        /// a specific capacity.
        /// </summary>
        /// <param name="attributes">Attribute map used to store this trace's attributes.</param>
        /// <param name="initialCapacity">Initial capacity. If not specified, 0 is used by default</param>
        public XTrace(IXAttributeMap attributes, int initialCapacity = 0) : base(initialCapacity)
        {
            this.attributes = attributes;
        }

        public void Accept(XVisitor visitor, XLog log)
        {
            visitor.VisitTracePre(this, log);

            foreach (XAttribute attribute in this.attributes.Values)
            {
                attribute.Accept(visitor, this);
            }

            foreach (IXEvent evt in this)
            {

                evt.Accept(visitor, this);
            }

            visitor.VisitTracePost(this, log);
        }

        /// <summary>
        /// Creates a clone, i.e. deep copy, of this trace.
        /// </summary>
        /// <returns>The clone.</returns>
        public object Clone()
        {
            XTrace clone = new XTrace((IXAttributeMap)this.attributes.Clone(), Count);
            foreach (IXEvent evt in this)
            {
                clone.Add((IXEvent)evt.Clone());
            }
            return clone;
        }

        public IXAttributeMap GetAttributes()
        {
            return this.attributes;
        }

        public HashSet<XExtension> Extensions
        {
            get
            {
                return XAttributeUtils.ExtractExtensions(this.attributes);
            }
        }

        public bool HasAttributes()
        {
            return this.attributes.Count > 0;
        }

        public int InsertOrdered(IXEvent evt)
        {
            lock (this)
            {
                if (this.Count == 0)
                {
                    Add(evt);
                    return 0;
                }
                XAttribute insTsAttr = evt.GetAttributes()["time:timestamp"];

                if (insTsAttr == null)
                {

                    Add(evt);
                    return (Count - 1);
                }
                DateTime insTs = ((IXAttributeTimestamp)insTsAttr).Value;
                for (int i = Count - 1; i >= 0; --i)
                {
                    XAttribute refTsAttr = (this[i]).GetAttributes()["time:timestamp"];

                    if (refTsAttr == null)
                    {

                        Add(evt);
                        return (Count - 1);
                    }
                    DateTime refTs = ((IXAttributeTimestamp)refTsAttr).Value;
                    if (insTs.CompareTo(refTs) < 0)
                        continue;

                    Insert(i + 1, evt);
                    return (i + 1);
                }


                Insert(0, evt);
                return 0;
            }
        }

        public void SetAttributes(IXAttributeMap attributes)
        {
            this.attributes = attributes;
        }
    }
}
