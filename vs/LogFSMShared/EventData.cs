using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogFSMShared
{

    public class EventData
    {
        public string PersonIdentifier { get; set; }

        public string Element { get; set; }

        public string EventName { get; set; }

        public DateTime TimeStamp { get; set; }

        public TimeSpan RelativeTime { get; set; }

        public TimeSpan TimeDifferencePrevious { get; set; }

        public Dictionary<string, string> EventValues { get; set; }

        public void AddEventValue(string Name, string Value)
        {
            if (!EventValues.ContainsKey(Name))
                EventValues.Add(Name, "");

            EventValues[Name] = Value;
        }

        public string GetEventValue(string Name)
        {
            if (EventValues.ContainsKey(Name))
                return EventValues[Name];
            else if (Name.ToLower() == "eventname")
                return EventName;
            else if (Name.ToLower() == "element")
                return Element;
            else if (Name.ToLower() == "timestamp")
                return TimeStamp.ToString(); // TODO: Define format
            else if (Name.ToLower() == "relativetime")
                return RelativeTime.ToString(); ; // TODO: Define format

            return "";
        }

        public EventData()
        {
            EventValues = new Dictionary<string, string>();
        }

    }
}
