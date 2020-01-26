namespace LogFSM
{
    #region usings
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using LogFSMShared;
    using Newtonsoft.Json;
    using System.Xml.Serialization;
    #endregion

    public static class LogDataReader
    { 
        public static List<EventData> ReadLogDataJsonLite(string FileName, bool RelativeTime, EventDataListExtension.ESortType Sort)
        {
            string _json = File.ReadAllText(FileName);
            var _data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(_json);
            List<EventData> _return = new List<EventData>();
            foreach (var _l in _data)
            {
                EventData e = new EventData();
                foreach (var _k in _l.Keys)
                {
                    if (_k == "PersonIdentifier")
                        e.PersonIdentifier = _l[_k];
                    else if (_k == "TimeStamp")
                        e.TimeStamp = DateTime.Parse(_l[_k]);
                    else if (_k == "EventName")
                        e.EventName = _l[_k];
                    else if (_k == "TimeDifferencePrevious")
                        e.TimeDifferencePrevious = TimeSpan.Parse(_l[_k]);
                    else
                        e.AddEventValue(_k, _l[_k]);
                }

                _return.Add(e);
            }

            if (RelativeTime)
                _return.ComputeTimedifferencePreviousWithRelativeTimes(Sort);
            else
                _return.ComputeTimedifferencePrevious(Sort);

            return _return;
        }

        public static List<EventData> ReadLogDataLogFSMJson(string FileName, bool RelativeTime, EventDataListExtension.ESortType Sort)
        {
            string _json = File.ReadAllText(FileName);
            List<EventData> _return = JsonConvert.DeserializeObject<List<EventData>>(_json);

            if (RelativeTime)
                _return.ComputeTimedifferencePreviousWithRelativeTimes(Sort);
            else
                _return.ComputeTimedifferencePrevious(Sort);

            return _return;
        }

        public static List<EventData> ReadLogDataLogFSMJsonString(string JSON, bool RelativeTime, EventDataListExtension.ESortType Sort)
        {
            List<EventData> _return = JsonConvert.DeserializeObject<List<EventData>>(JSON);

            if (RelativeTime)
                _return.ComputeTimedifferencePreviousWithRelativeTimes(Sort);
            else
                _return.ComputeTimedifferencePrevious(Sort);

            return _return;
        }

        public static List<EventData> ReadLogDataEEFromXMLString(string XML, bool RelativeTime, EventDataListExtension.ESortType Sort)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XML);

            string PersonIdentifier = "Unknown";
            List<EventData> _return = new List<EventData>();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            nsmgr.AddNamespace("cbaloggingmodel", "http://www.softcon.de/cba/cbaloggingmodel");

            foreach (XmlNode row in doc.SelectNodes("//logEntry[@xsi:type='cbaloggingmodel:CBAItemLogEntry']", nsmgr))
            {
                PersonIdentifier = row.Attributes["user"].Value.ToString();
            }

            foreach (XmlNode row in doc.SelectNodes("//logEntry"))
            {
                if (row.ChildNodes.Count == 1)
                {
                    XDocument _xmlElement = XDocument.Parse(row.ChildNodes[0].OuterXml);
                    if (row.ChildNodes[0].Attributes["xsi:type"] != null)
                    {
                        EventData _parament = new EventData() { EventName = row.ChildNodes[0].Attributes["xsi:type"].Value.Replace("cbaloggingmodel:", ""), PersonIdentifier = PersonIdentifier, TimeStamp = DateTime.Parse(row.Attributes["timeStamp"].Value) };
                        _return.Add(_parament);
                        AddEventData(_xmlElement.Root, _parament, _return);
                    }

                }
            }

            if (RelativeTime)
                _return.ComputeTimedifferencePreviousWithRelativeTimes(Sort);
            else
                _return.ComputeTimedifferencePrevious(Sort);

            return _return;
        }

        public static List<EventData> ReadLogDataEE(string FileName, bool RelativeTime, EventDataListExtension.ESortType Sort)
        {
            string _xml = File.ReadAllText(FileName);
            var _return = ReadLogDataEEFromXMLString(_xml, RelativeTime, Sort);
            return _return;
        }

        private static void AddEventData(XElement xmlelement, EventData parent, List<EventData> eventdata)
        {
            foreach (var a in xmlelement.Attributes())
            {
                if (a.Name.Namespace.NamespaceName == "")
                {
                    parent.EventValues.Add(a.Name.ToString(), a.Value);
                }
            }


            foreach (XElement x in xmlelement.Elements())
            {
                EventData _newparent = new EventData() { EventName = parent.EventName + ":" + x.Name.LocalName, PersonIdentifier = parent.PersonIdentifier, TimeStamp = parent.TimeStamp };
                eventdata.Add(_newparent);
                AddEventData(x, _newparent, eventdata);
            }
        }
    }
}
