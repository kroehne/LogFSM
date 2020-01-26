namespace LogFSM
{
    #region usings
    using LogFSM;
    using LogFSMShared;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    #endregion
     
    public static class EventDataListExtension
    {
        public enum ESortType
        {
            None,
            Time,
            ElementAndTime
        }

        // TODO-TIME
        public static void ComputeTimedifferencePrevious(this IEnumerable<EventData> list, ESortType Sort)
        {
            if (Sort == ESortType.ElementAndTime)
                list = list.OrderBy(o => o.Element).ThenBy(o => o.RelativeTime).ToList();
            else if (Sort == ESortType.Time)
                list = list.OrderBy(o => o.RelativeTime);

            DateTime _lastTimeStamp = DateTime.MinValue;
            foreach (var e in list)
            {
                if (_lastTimeStamp == DateTime.MinValue)
                    e.TimeDifferencePrevious = new TimeSpan(0);
                else
                    e.TimeDifferencePrevious = e.TimeStamp - _lastTimeStamp;

                _lastTimeStamp = e.TimeStamp;
            }
        }

        public static void ComputeTimedifferencePreviousWithRelativeTimes(this IEnumerable<EventData> list, ESortType Sort)
        {
            if (Sort == ESortType.ElementAndTime)
                list = list.OrderBy(o => o.Element).ThenBy(o => o.RelativeTime).ToList();
            else if (Sort == ESortType.Time)
                list = list.OrderBy(o => o.RelativeTime);

            TimeSpan _lastTimeStamp = TimeSpan.MinValue;
            foreach (var e in list)
            {
                if (_lastTimeStamp == TimeSpan.MinValue)
                    e.TimeDifferencePrevious = new TimeSpan(0);
                else
                    e.TimeDifferencePrevious = e.RelativeTime - _lastTimeStamp;

                _lastTimeStamp = e.RelativeTime;
            }
        }

    }

}
