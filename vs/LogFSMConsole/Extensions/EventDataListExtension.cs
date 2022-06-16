#region usings
using LogFSM;
using LogFSMShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace LogFSM
{    
    public static class EventDataListExtension
    {
        public enum ESortType
        {
            None,
            Time,
            ElementAndTime
        }

        public static List<EventData> ComputeRelativeTimes(List<EventData> List)
        {
            DateTime firstTimeStamp = List.Min(e => e.TimeStamp);
            foreach (var e in List)
            {
                e.RelativeTime = e.TimeStamp - firstTimeStamp;
            }
            return List;
        }


        public static List<EventData> SortByTimeStamp(List<EventData> List, ESortType Sort)
        {
            if (Sort == ESortType.ElementAndTime)
                return List.OrderBy(o => o.Element).ThenBy(o => o.TimeStamp).ToList();
            else if (Sort == ESortType.Time)
                return List.OrderBy(o => o.TimeStamp).ToList();
            else
                return List;
        }

        public static List<EventData> SortByRelativeTime(List<EventData> List, ESortType Sort)
        {
            if (Sort == ESortType.ElementAndTime)
                return List.OrderBy(o => o.Element).ThenBy(o => o.TimeStamp).ToList();
            else if (Sort == ESortType.Time)
                return List.OrderBy(o => o.TimeStamp).ToList();
            else
                return List;
        }

        public static void ComputeTimedifferencePrevious(this IEnumerable<EventData> List)
        {
            DateTime _lastTimeStamp = DateTime.MinValue;
            foreach (var e in List)
            {
                if (_lastTimeStamp == DateTime.MinValue)
                    e.TimeDifferencePrevious = new TimeSpan(0);
                else
                    e.TimeDifferencePrevious = e.TimeStamp - _lastTimeStamp;

                _lastTimeStamp = e.TimeStamp;
            }
        }

        public static void ComputeTimedifferencePreviousWithRelativeTimes(this IEnumerable<EventData> List)
        { 
            TimeSpan _lastTimeStamp = TimeSpan.MinValue;
            foreach (var e in List)
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
