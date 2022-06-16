using System;
using OpenXesNet.model;

namespace OpenXesNet.info
{
    /// <summary>
    /// XT IME bounds.
    /// </summary>
    public interface IXTimeBounds
    {
        /// <summary>
        /// Returns the earliest timestamp of these boundaries (left bound).
        /// </summary>
        /// <returns>The start date.</returns>
        DateTime? GetStartDate();

        /// <summary>
        /// Returns the latest timestamp of these boundaries (right bound).
        /// </summary>
        /// <returns>The end date.</returns>
        DateTime? GetEndDate();

        /// <summary>
        /// Checks, whether the given date is within these boundaries.
        /// </summary>
        /// <returns>Whether the specified date is within these boundaries.</returns>
        /// <param name="paramDate">Date to be checked.</param>
        bool IsWithin(DateTime paramDate);

        /// <summary>
        /// Returns a string representation of these boundaries.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:OpenXesNet.info.IXTimeBounds"/>.</returns>
        string ToString();

        /// <summary>
        /// Registers the given event, i.e. if it has a timestamp, the timestamp 
        /// boundaries will be potentially adjusted to accomodate for inclusion of this event.
        /// </summary>
        /// <param name="evt">Event to be registered.</param>
        void Register(IXEvent evt);

        /// <summary>
        /// Registers the given date. The timestamp boundaries will be  potentially 
        /// adjusted to accomodate for inclusion of this date.
        /// </summary>
        /// <param name="date">Date to be registered.</param>
        void Register(DateTime? date);

        /// <summary>
        /// Registers the given timestamp boundaries. These timestamp boundaries 
        /// will be potentially adjusted to  accomodate for inclusion of the given boundaries.
        /// </summary>
        /// <param name="boundary">Timestamp boundaries to be registered.</param>
        void Register(IXTimeBounds boundary);
    }
}
