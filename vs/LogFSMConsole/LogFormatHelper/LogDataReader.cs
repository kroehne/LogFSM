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
using Ionic.Zip;
using LogFSMConsole;
using StataLib;
using CsvHelper;
using System.Globalization;
using static LogFSM.EventDataListExtension;
using CsvHelper.Configuration;
#endregion


namespace LogFSM
{
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
            {
                _return = EventDataListExtension.SortByRelativeTime(_return, Sort);
                _return.ComputeTimedifferencePreviousWithRelativeTimes();
            }
            else
            {
                _return = EventDataListExtension.SortByTimeStamp(_return, Sort);
                _return.ComputeTimedifferencePrevious();
            }
                          
            return _return;
        }

        public static List<EventData> ReadLogDataLogFSMJson(string FileName, bool RelativeTime, EventDataListExtension.ESortType Sort)
        {
            string _json = File.ReadAllText(FileName);
            List<EventData> _return = JsonConvert.DeserializeObject<List<EventData>>(_json);

            if (RelativeTime)
            {
                _return = EventDataListExtension.SortByRelativeTime(_return, Sort);
                _return.ComputeTimedifferencePreviousWithRelativeTimes();
            }
            else
            {
                _return = EventDataListExtension.SortByTimeStamp(_return, Sort);
                _return.ComputeTimedifferencePrevious();
            }

            return _return;
        }

        public static List<EventData> ReadLogDataLogFSMJsonString(string JSON, bool RelativeTime, EventDataListExtension.ESortType Sort)
        {
            List<EventData> _return = JsonConvert.DeserializeObject<List<EventData>>(JSON);

            if (RelativeTime)
            {
                _return = EventDataListExtension.SortByRelativeTime(_return, Sort);
                _return.ComputeTimedifferencePreviousWithRelativeTimes();
            }
            else
            {
                _return = EventDataListExtension.SortByTimeStamp(_return, Sort);
                _return.ComputeTimedifferencePrevious();
            }
                 
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
            {
                _return = EventDataListExtension.SortByRelativeTime(_return, Sort);
                _return.ComputeTimedifferencePreviousWithRelativeTimes();
            }
            else
            {
                _return = EventDataListExtension.SortByTimeStamp(_return, Sort);
                _return.ComputeTimedifferencePrevious();
            }

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

        public static List<string> GetListOfPersonIdentifiersFromUniversalLogFormat(string ZipFileName, CommandLineArguments ParsedCommandLineArguments)
        {
            string _columnNamePersonIdentifier = "PersonIdentifier";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columnnamepersonidentifier"))
                _columnNamePersonIdentifier = ParsedCommandLineArguments.ParameterDictionary["columnnamepersonidentifier"];
            
            string _columnDelimiter = ";";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columndelimiter"))
                _columnDelimiter = ParsedCommandLineArguments.ParameterDictionary["columndelimiter"];
             
            List<string> _return = new List<string>();
            using (ZipFile zip = ZipFile.Read(ZipFileName))
            { 
                if (zip.ContainsEntry("Log.dta"))
                {
                    extractPersonIdentifierFromStataFile(_columnNamePersonIdentifier, zip, _return, "Log.dta");
                } 
                else if (zip.ContainsEntry("Log.csv"))
                {
                    extractPersonIdentifierFromCSVFile(_columnNamePersonIdentifier, zip, _return, "Log.csv", _columnDelimiter);
                }
                else if (zip.ContainsEntry("Log.sav"))
                {
                    extractPersonIdentifierFromSPSSFile(_columnNamePersonIdentifier, zip, _return, "Log.sav");
                }
                else 
                {
                    foreach (ZipEntry e in zip.Entries)
                    {
                        if (e.FileName.ToLower().EndsWith(".dta"))
                            extractPersonIdentifierFromStataFile(_columnNamePersonIdentifier, zip, _return, e.FileName);
                        else if (e.FileName.ToLower().EndsWith(".csv"))
                            extractPersonIdentifierFromCSVFile(_columnNamePersonIdentifier, zip, _return, e.FileName, _columnDelimiter);
                        else if (e.FileName.ToLower().EndsWith(".sav"))
                            extractPersonIdentifierFromSPSSFile(_columnNamePersonIdentifier, zip, _return, e.FileName);
                    }
                }
            }
            return _return;
        }

        public static List<EventData> ReadLogUniversalLogFormat(string ZipFileName, string PersonIdentifier, bool RelativeTime, CommandLineArguments ParsedCommandLineArguments, string[] Elements, EventDataListExtension.ESortType Sort)
        { 
            string _columnNamePersonIdentifier = "PersonIdentifier";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columnnamepersonidentifier"))
                _columnNamePersonIdentifier = ParsedCommandLineArguments.ParameterDictionary["columnnamepersonidentifier"];

            string _columnNameEventName = "EventName";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columnnameeventname"))
                _columnNameEventName = ParsedCommandLineArguments.ParameterDictionary["columnnameeventname"];

            string _columnNameElement = "Element";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columnnameelement"))
                _columnNameElement = ParsedCommandLineArguments.ParameterDictionary["columnnameelement"];

            string _columnNameTimeStamp = "TimeStamp";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columnnametimestamp"))
                _columnNameTimeStamp = ParsedCommandLineArguments.ParameterDictionary["columnnametimestamp"];

            string _columnDelimiter = ";";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columndelimiter"))
                _columnDelimiter = ParsedCommandLineArguments.ParameterDictionary["columndelimiter"];

            string _ignoretables = "Log;Results";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("ignoretables"))
                _ignoretables = ParsedCommandLineArguments.ParameterDictionary["ignoretables"];

            List<string> _listOfIgnoreTables = new List<string>();
            foreach ( var t in _ignoretables.Split(";").ToArray<string>())
            {
                _listOfIgnoreTables.Add(t.Trim());
            }

            List<string> _notEventSpecificValues = new List<string>();
            _notEventSpecificValues.Add(_columnNameElement);
            _notEventSpecificValues.Add(_columnNameEventName);
            _notEventSpecificValues.Add(_columnNamePersonIdentifier);
            _notEventSpecificValues.Add(_columnNameTimeStamp);

            if (!ParsedCommandLineArguments.Flags.Contains("INCLUDEREFERENCEVARIABLES"))
            {
                _notEventSpecificValues.Add("Path");
                _notEventSpecificValues.Add("ReferenceCounter");
                _notEventSpecificValues.Add("ParentReferenceCounter");
            }

            DateTime dt1960 = new DateTime(1960, 1, 1, 0, 0, 0, 0);

            List<EventData> _return = new List<EventData>(); ;

            using (ZipFile zip = ZipFile.Read(ZipFileName))
            {
                foreach (ZipEntry e in zip.Entries)
                {
                    string _filenameWithoutExt = Path.GetFileNameWithoutExtension(e.FileName);
                    
                    #region STATA
                    if (e.FileName.ToLower().EndsWith(".dta") && !_listOfIgnoreTables.Contains(_filenameWithoutExt))
                    {
                        using (MemoryStream zipStream = new MemoryStream())
                        {
                            e.Extract(zipStream);
                            zipStream.Position = 0;
                            var str = new StataFileReader(zipStream, true);

                            // cache variables 

                            Dictionary<int, StataVariable> _eventDataKeys = new Dictionary<int, StataVariable>();
                            var vardict = new Dictionary<string, Tuple<int, StataVariable>>();
                            for (int i = 0; i < str.Variables.Count; i++)
                            {
                                vardict.Add(str.Variables[i].Name, new Tuple<int, StataVariable>(i, str.Variables[i]));
                                if (!_notEventSpecificValues.Contains(str.Variables[i].Name))
                                    _eventDataKeys.Add(i, str.Variables[i]);
                            }

                            // cache value labels 

                            Dictionary<string, Dictionary<int, string>> valuedict = new Dictionary<string, Dictionary<int, string>>();
                            foreach (var v in str.ValueLabels)
                            {
                                if (!valuedict.ContainsKey(v.Item1))
                                    valuedict.Add(v.Item1, new Dictionary<int, string>());

                                valuedict[v.Item1].Add(v.Item2, v.Item3);
                            }

                            // cache element list 

                            List<string> _selectedElements = new List<string>();
                            string _elementLabelSetName = vardict[_columnNameElement].Item2.ValueLabelName;
                            if (_elementLabelSetName != "")
                            {
                                var _elementLabelSet = valuedict[_elementLabelSetName];
                                foreach (var v in _elementLabelSet.Keys)
                                {
                                    _selectedElements.Add(v.ToString());
                                }
                            }

                            int _elementColumnIndex = vardict[_columnNameElement].Item1;
                            Dictionary<int, string> _elementValueLabelDict = new Dictionary<int, string>();
                            if (vardict[_columnNameElement].Item2.ValueLabelName != "")
                            {
                                _elementValueLabelDict = valuedict[vardict[_columnNameElement].Item2.ValueLabelName];
                            }

                            int _personIdentifierColumnIndex = vardict[_columnNamePersonIdentifier].Item1;
                            Dictionary<int, string> _personIdentifierValueLabelDict = new Dictionary<int, string>();
                            if (vardict[_columnNamePersonIdentifier].Item2.ValueLabelName != "")
                            {
                                _personIdentifierValueLabelDict = valuedict[vardict[_columnNamePersonIdentifier].Item2.ValueLabelName];
                            }

                            int _eventNameColumnIndex = vardict[_columnNameEventName].Item1;
                            Dictionary<int, string> _eventNameValueLabelDict = new Dictionary<int, string>();
                            if (vardict[_columnNameEventName].Item2.ValueLabelName != "")
                            {
                                _eventNameValueLabelDict = valuedict[vardict[_columnNameEventName].Item2.ValueLabelName];
                            }

                            int _timeStampColumnIndex = vardict[_columnNameTimeStamp].Item1;
                            Dictionary<int, string> _timeStampLabelDict = new Dictionary<int, string>();
                            if (vardict[_columnNameTimeStamp].Item2.ValueLabelName != "")
                            {
                                _timeStampLabelDict = valuedict[vardict[_columnNameTimeStamp].Item2.ValueLabelName];
                            }

                            foreach (var _line in str)
                            {
                                if (_selectedElements.Count == 0 || _selectedElements.Contains(_line[_elementColumnIndex].ToString()))
                                { 
                                    string _eventName = _line[_eventNameColumnIndex].ToString();
                                    if (_eventNameValueLabelDict.Count > 0)
                                        _eventName = _eventNameValueLabelDict[int.Parse(_eventName)];

                                    string _personIdentifier = _line[_personIdentifierColumnIndex].ToString();
                                    if (_personIdentifierValueLabelDict.Count > 0)
                                        _personIdentifier = _personIdentifierValueLabelDict[int.Parse(_personIdentifier)];

                                    string _element = _line[_elementColumnIndex].ToString();
                                    if (_elementValueLabelDict.Count > 0 && _elementValueLabelDict.ContainsKey(int.Parse(_element)))
                                        _element = _elementValueLabelDict[int.Parse(_element)];

                                    DateTime _timeStamp = DateTime.MinValue;
                                    if (RelativeTime)
                                        DateTime.TryParse(_line[_timeStampColumnIndex].ToString(), out _timeStamp);
                                    else
                                    {
                                        try
                                        {
                                            _timeStamp = dt1960.AddMilliseconds(double.Parse(_line[_timeStampColumnIndex].ToString()));
                                        }
                                        catch
                                        {
                                            //TODO VERSION 0.3: Add a more proper check for invalid time stamps
                                        }
                                    }

                                    var _eventValues = new Dictionary<string, string>();
                                    foreach (var v in _eventDataKeys.Keys)
                                    {
                                        string _dictName = vardict[_eventDataKeys[v].Name].Item2.ValueLabelName;
                                        if (valuedict.ContainsKey(_dictName))
                                        {
                                            var _dict = valuedict[_dictName];
                                            string _value = _line[v].ToString();
                                            if (_dict.ContainsKey(int.Parse(_value)))
                                            {
                                                _value = _dict[int.Parse(_value)].ToString();
                                            }

                                            _eventValues.Add(_eventDataKeys[v].Name, _value);
                                        }
                                        else
                                        {
                                            _eventValues.Add(_eventDataKeys[v].Name, _line[v].ToString());
                                        }
                                    }

                                    bool _add = true;
                                    if (_personIdentifier != PersonIdentifier)
                                        _add = false;
                                    else if (ParsedCommandLineArguments.Elements.Length != 0 && !ParsedCommandLineArguments.Elements.Contains<string>(_element))
                                        _add = false;
                                    else if (ParsedCommandLineArguments.Events.Length != 0 && !ParsedCommandLineArguments.Events.Contains<string>(_eventName))
                                        _add = false;
                                    else if (ParsedCommandLineArguments.ExcludedElements.Length != 0 && ParsedCommandLineArguments.ExcludedElements.Contains<string>(_element))
                                        _add = false;
                                    else if (ParsedCommandLineArguments.ExcludedEvents.Length != 0 && ParsedCommandLineArguments.ExcludedEvents.Contains<string>(_eventName))
                                        _add = false;

                                    if (_add)
                                    {
                                        _return.Add(new EventData()
                                        {
                                            Element = _element,
                                            EventName = _eventName,
                                            PersonIdentifier = _personIdentifier,
                                            TimeStamp = _timeStamp,
                                            EventValues = _eventValues
                                        });
                                    }
                                     
                                }

                            }

                        }

                    }
                    #endregion

                    #region CSV
                    if (e.FileName.ToLower().EndsWith(".csv") && !_listOfIgnoreTables.Contains(_filenameWithoutExt))
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.Write("- read: " + e.FileName + " (" + e.UncompressedSize + ")");

                        // TODO: Compute exact limit for memory streams
                       
                        if (e.UncompressedSize > 2000000000)
                        {
                            var fileName = Path.GetTempFileName();
                            if (ParsedCommandLineArguments.Verbose)
                                Console.WriteLine("--> temp file: " + fileName);


                            using (FileStream fileStream = File.OpenWrite(fileName))
                            {
                                e.Extract(fileStream);
                            }
                            using (FileStream fileStream = File.OpenRead(fileName))
                            { 
                                extracCSVStream_ReadLogUniversalLogFormat(PersonIdentifier, RelativeTime, ParsedCommandLineArguments, _columnNamePersonIdentifier, _columnNameEventName, _columnNameElement, _columnNameTimeStamp, _columnDelimiter, _notEventSpecificValues, dt1960, _return, fileStream);
                            }
                            File.Delete(fileName);
                        }
                        else
                        {
                            if (ParsedCommandLineArguments.Verbose)
                                Console.WriteLine("");

                            using (MemoryStream zipStream = new MemoryStream())
                            {
                                e.Extract(zipStream);
                                zipStream.Position = 0;
                                extracCSVStream_ReadLogUniversalLogFormat(PersonIdentifier, RelativeTime, ParsedCommandLineArguments, _columnNamePersonIdentifier, _columnNameEventName, _columnNameElement, _columnNameTimeStamp, _columnDelimiter, _notEventSpecificValues, dt1960, _return, zipStream);
                            }
                        }
                      
                    }
                    #endregion
                     
                    #region SPSS
                    if (e.FileName.ToLower().EndsWith(".sav") && !_listOfIgnoreTables.Contains(_filenameWithoutExt))
                    {
                        using (MemoryStream zipStream = new MemoryStream())
                        {
                            e.Extract(zipStream);
                            zipStream.Position = 0;

                            SpssLib.DataReader.SpssReader spssDataset = new SpssLib.DataReader.SpssReader(zipStream);
                            foreach (var record in spssDataset.Records)
                            { 
                                string _personIdentifier = "";
                                string _eventName = "";
                                string _element = "";
                                DateTime _timeStamp = DateTime.MinValue;
                                var _eventValues = new Dictionary<string, string>();

                                foreach (var variable in spssDataset.Variables)
                                {
                                    if (variable.Name == _columnNamePersonIdentifier)
                                    {
                                        _personIdentifier = record.GetValue(variable).ToString();
                                        if (variable.ValueLabels.Count > 0)
                                        {
                                            double _doubleValue = (double)record.GetValue(variable);
                                            if (variable.ValueLabels.ContainsKey(_doubleValue))
                                                _personIdentifier = variable.ValueLabels[_doubleValue];
                                        } 
                                    }
                                    else if (variable.Name == _columnNameEventName)
                                    {
                                        _eventName = record.GetValue(variable).ToString();
                                        if (variable.ValueLabels.Count > 0)
                                        {
                                            double _doubleValue = (double)record.GetValue(variable);
                                            if (variable.ValueLabels.ContainsKey(_doubleValue))
                                                _eventName = variable.ValueLabels[_doubleValue];
                                        }
                                    }
                                    else if (variable.Name == _columnNameElement)
                                    {
                                        _element = record.GetValue(variable).ToString();
                                        if (variable.ValueLabels.Count > 0)
                                        {
                                            double _doubleValue = (double)record.GetValue(variable);
                                            if (variable.ValueLabels.ContainsKey(_doubleValue))
                                                _element = variable.ValueLabels[_doubleValue];
                                        }
                                    }
                                    else if (variable.Name == _columnNameTimeStamp)
                                    {
                                        string _TimeStamp = record.GetValue(variable).ToString();
                                        if (variable.ValueLabels.Count > 0)
                                        {
                                            double _doubleValue = (double)record.GetValue(variable);
                                            if (variable.ValueLabels.ContainsKey(_doubleValue))
                                                _TimeStamp = variable.ValueLabels[_doubleValue];
                                        }

                                        if (RelativeTime)
                                            DateTime.TryParse(_TimeStamp, out _timeStamp);
                                        else
                                        {
                                            try
                                            {
                                                _timeStamp = dt1960.AddMilliseconds(double.Parse(_TimeStamp));
                                            }
                                            catch
                                            {
                                                //TODO VERSION 0.3: Add a more proper check for invalid time stamps
                                            }
                                        }

                                    } 
                                    else
                                    {
                                        if (!_notEventSpecificValues.Contains(variable.Name))
                                        {
                                            string _key = variable.Name;
                                            string _value = record.GetValue(variable).ToString();
                                            if (variable.ValueLabels.Count > 0)
                                            {
                                                double _doubleValue = (double)record.GetValue(variable);
                                                if (variable.ValueLabels.ContainsKey(_doubleValue))
                                                    _value = variable.ValueLabels[_doubleValue];
                                            }

                                            _eventValues.Add(_key, _value);

                                        }
                                    }


                                }

                                bool _add = true;
                                if (_personIdentifier != PersonIdentifier)
                                    _add = false;
                                else if (ParsedCommandLineArguments.Elements.Length != 0 && !ParsedCommandLineArguments.Elements.Contains<string>(_element))
                                    _add = false;
                                else if (ParsedCommandLineArguments.Events.Length != 0 && !ParsedCommandLineArguments.Events.Contains<string>(_eventName))
                                    _add = false;
                                else if (ParsedCommandLineArguments.ExcludedElements.Length != 0 && ParsedCommandLineArguments.ExcludedElements.Contains<string>(_element))
                                    _add = false;
                                else if (ParsedCommandLineArguments.ExcludedEvents.Length != 0 && ParsedCommandLineArguments.ExcludedEvents.Contains<string>(_eventName))
                                    _add = false;

                                if (_add)
                                { 
                                    _return.Add(new EventData()
                                    {
                                        Element = _element,
                                        EventName = _eventName,
                                        PersonIdentifier = _personIdentifier,
                                        TimeStamp = _timeStamp,
                                        EventValues = _eventValues
                                    });
                                }
                                 
                            } 
                        }
                    }
                    #endregion
                }
            }
             
            if (RelativeTime)
            {
                _return = EventDataListExtension.SortByRelativeTime(_return, Sort);
                _return.ComputeTimedifferencePreviousWithRelativeTimes();
            }
            else
            {
                _return = EventDataListExtension.SortByTimeStamp(_return, Sort);
                _return.ComputeTimedifferencePrevious();
            }

            return _return;
        }

        private static void extracCSVStream_ReadLogUniversalLogFormat(string PersonIdentifier, bool RelativeTime, CommandLineArguments ParsedCommandLineArguments, string _columnNamePersonIdentifier, string _columnNameEventName, string _columnNameElement, string _columnNameTimeStamp, string _columnDelimiter, List<string> _notEventSpecificValues, DateTime dt1960, List<EventData> _return, Stream zipStream)
        {
            int _lineCounter = 0;
            using (var reader = new StreamReader(zipStream))
            {
                var _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = _columnDelimiter,
                    ReadingExceptionOccurred = args =>
                    {
                        Console.WriteLine($"Bad data (Line " + _lineCounter + "): <{x.RawRecord}>");
                        return true;
                    }
                };

                using (var csv = new CsvReader(reader, _csvConfig))
                { 
                    var _data_rows = csv.GetRecords<dynamic>();
                    foreach (IDictionary<string, object> row in _data_rows)
                    {
                        string _personIdentifier = row[_columnNamePersonIdentifier].ToString();
                        string _eventName = row[_columnNameEventName].ToString();
                        string _element = row[_columnNameElement].ToString();
                        DateTime _timeStamp = DateTime.MinValue;

                        if (!RelativeTime)
                            DateTime.TryParse(row[_columnNameTimeStamp].ToString(), out _timeStamp);
                        else
                        {
                            try
                            {
                                _timeStamp = dt1960.AddMilliseconds(double.Parse(row[_columnNameTimeStamp].ToString()));
                            }
                            catch
                            {
                                //TODO VERSION 0.3: Add a more proper check for invalid time stamps
                            }
                        }

                        var _eventValues = new Dictionary<string, string>();
                        foreach (var v in row.Keys)
                        {
                            if (!_notEventSpecificValues.Contains(v))
                                _eventValues.Add(v, row[v].ToString());
                        }


                        bool _add = true;
                        if (_personIdentifier != PersonIdentifier)
                            _add = false;
                        else if (ParsedCommandLineArguments.Elements.Length != 0 && !ParsedCommandLineArguments.Elements.Contains<string>(_element))
                            _add = false;
                        else if (ParsedCommandLineArguments.Events.Length != 0 && !ParsedCommandLineArguments.Events.Contains<string>(_eventName))
                            _add = false;
                        else if (ParsedCommandLineArguments.ExcludedElements.Length != 0 && ParsedCommandLineArguments.ExcludedElements.Contains<string>(_element))
                            _add = false;
                        else if (ParsedCommandLineArguments.ExcludedEvents.Length != 0 && ParsedCommandLineArguments.ExcludedEvents.Contains<string>(_eventName))
                            _add = false;

                        if (_add)
                        {
                            _return.Add(new EventData()
                            {
                                Element = _element,
                                EventName = _eventName,
                                PersonIdentifier = _personIdentifier,
                                TimeStamp = _timeStamp,
                                EventValues = _eventValues
                            });
                        }

                    }

                }
            }
        }

        private static void extractPersonIdentifierFromStataFile(string _columnNamePersonIdentifier, ZipFile zip, List<string> _ret, string TableName)
        {
            ZipEntry e = zip[TableName];
            using (MemoryStream zipStream = new MemoryStream())
            {
                e.Extract(zipStream);
                zipStream.Position = 0;
                var str = new StataFileReader(zipStream, true);

                // Person Identifier as Value Label
                foreach (var v in str.ValueLabels)
                {
                    if (v.Item1 == "l_" + _columnNamePersonIdentifier)
                        if (!_ret.Contains(v.Item3.ToString()))
                            _ret.Add(v.Item3.ToString());
                }

                // Person Identifier as Value
                if (_ret.Count == 0)
                {
                    var vardict = new Dictionary<string, Tuple<int, StataVariable>>();
                    for (int i = 0; i < str.Variables.Count; i++)
                        vardict.Add(str.Variables[i].Name, new Tuple<int, StataVariable>(i, str.Variables[i]));
                    int _personIdentifierColumnIndex = vardict[_columnNamePersonIdentifier].Item1;

                    foreach (var _line in str)
                    {
                        string _personIdentifier = _line[_personIdentifierColumnIndex].ToString();
                        if (!_ret.Contains(_personIdentifier))
                            _ret.Add(_personIdentifier);
                    }
                }
            }
        }
         
        private static void extractPersonIdentifierFromSPSSFile(string _columnNamePersonIdentifier, ZipFile zip, List<string> _ret, string TableName)
        {
            ZipEntry e = zip[TableName];
            using (MemoryStream zipStream = new MemoryStream())
            {
                e.Extract(zipStream);
                zipStream.Position = 0;

                SpssLib.DataReader.SpssReader spssDataset = new SpssLib.DataReader.SpssReader(zipStream);
                foreach (var record in spssDataset.Records)
                {
                    foreach (var variable in spssDataset.Variables)
                    {
                        if (variable.Name == _columnNamePersonIdentifier)
                        {
                            string _personIdentifier = record.GetValue(variable).ToString();
                            if (variable.ValueLabels.Count > 0)
                            {
                                double _doubleValue = (double)record.GetValue(variable);
                                if (variable.ValueLabels.ContainsKey(_doubleValue))
                                    _personIdentifier = variable.ValueLabels[_doubleValue];
                            }

                            if (!_ret.Contains(_personIdentifier))
                                _ret.Add(_personIdentifier);
                        }
                    }
                } 
            }
        }

        private static void extractPersonIdentifierFromCSVFile(string _columnNamePersonIdentifier, ZipFile zip, List<string> _ret, string TableName, string _columnDelimiter)
        { 
            ZipEntry e = zip[TableName];
            using (MemoryStream zipStream = new MemoryStream())
            {
                e.Extract(zipStream);
                zipStream.Position = 0;

                int _lineCounter = 0;
                using (var reader = new StreamReader(zipStream))
                {
                    var _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = _columnDelimiter,
                        ReadingExceptionOccurred = args =>
                        {
                            Console.WriteLine($"Bad data (Line " + _lineCounter + "): <{x.RawRecord}>");
                            return true;
                        }
                    };

                    using (var csv = new CsvReader(reader, _csvConfig))
                    { 
                        var _data_rows = csv.GetRecords<dynamic>();
                        foreach (IDictionary<string, object> row in _data_rows)
                        {
                            if (row.ContainsKey(_columnNamePersonIdentifier))
                            {
                                string _personIdentifier = row[_columnNamePersonIdentifier].ToString();
                                if (!_ret.Contains(_personIdentifier))
                                    _ret.Add(_personIdentifier);
                            }

                        }
                    }
                }
            }
        }
    }
}
