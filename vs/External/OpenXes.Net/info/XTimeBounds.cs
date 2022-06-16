using System;
using OpenXesNet.extension.std;
using OpenXesNet.model;

namespace OpenXesNet.info
{
    /// <summary>
    /// This class implements timestamp boundaries, which can be used to describe 
    /// the temporal extent of a log, or of a contained trace.
    /// </summary>
    public class XTimeBounds : IXTimeBounds
    {
        /// <summary>
        /// The earliest timestamp of these boundaries (left bound).
        /// </summary>
        protected DateTime? first;
        /// <summary>
        /// The latest timestamp of these boundaries (right bound).
        /// </summary>
        protected DateTime? last;

        /// <summary>
        /// Creates new timestamp boundaries.
        /// </summary>
        public XTimeBounds()
        {
            this.first = null;
            this.last = null;
        }

        public DateTime? GetStartDate()
        {
            return this.first;
        }

        public DateTime? GetEndDate()
        {
            return this.last;
        }

        public bool IsWithin(DateTime date)
        {
            if (this.first == null)
                return false;
            if (date.Equals(this.first))
                return true;
            if (date.Equals(this.last))
            {
                return true;
            }
            return ((DateTime.Compare(date, (DateTime)this.first) > 0) && (DateTime.Compare(date, (DateTime)this.last) < 0));
        }

        public void Register(IXEvent evt)
        {
            DateTime? date = XTimeExtension.Instance.ExtractTimestamp((XEvent)evt);
            if (date != null)
            {
                Register((DateTime)date);
            }
        }

        public void Register(DateTime? date)
        {
            if (date != null)
                if (this.first == null)
                {
                    this.first = date;
                    this.last = date;
                }
                else if (DateTime.Compare((DateTime)date, (DateTime)this.first) < 0)

                {
                    this.first = date;
                }
                else if (DateTime.Compare((DateTime)date, (DateTime)this.first) > 0)

                {
                    this.last = date;
                }
        }

        public void Register(IXTimeBounds boundary)
        {
            Register(boundary.GetStartDate());
            Register(boundary.GetEndDate());
        }

        public override String ToString()
        {
            return this.first.ToString() + " -- " + this.last.ToString();
        }
    }
}
