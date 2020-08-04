namespace LogFSMConsole
{
    #region usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Ionic.Zip;
    using LogFSMShared;
    using StataLib;
    using System.IO;
    using Newtonsoft.Json;
    using LogFSM;
    using System.Text.RegularExpressions;
    using System.Xml.Serialization;
    using System.Xml;
    using Ionic.Zlib; 
    using System.Globalization;
    using LogDataTransformer_PIAAC_R1_V01;
    using CsvHelper;
    #endregion

    public class LogDataPreparer
    {

        #region Flat Log File 

        public static void ReadLogDataFlatV01(string ZipFileName, string OutFileName, string[] Element, bool Verbose, int MaxNumberOfStudents, 
            CommandLineArguments ParsedCommandLineArguments)
        {
            Console.WriteLine("Module: Read log data from flat file prepared by LogFSM (format = 'dataflatv01a').");

            string _columnDelimiter = ",";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columndelimiter"))
                _columnDelimiter = ParsedCommandLineArguments.ParameterDictionary["columndelimiter"];

            if (!CheckReadLogDataFlatV01(ZipFileName, _columnDelimiter))
            {
                Console.WriteLine("Data not in the expected flat file format. Check format and consider to provide an alternative value for the attribute 'datafiletype'.");
                return;
            }
            
            string _personIdentifierColumnName = "PersonIdentifier";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("personidentifiercolumnname"))
                _personIdentifierColumnName = ParsedCommandLineArguments.ParameterDictionary["personidentifiercolumnname"];

            string _elementColumnName = "Element";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("elementcolumnname"))
                _elementColumnName = ParsedCommandLineArguments.ParameterDictionary["elementcolumnname"];

            string _eventNameColumnName = "EventName";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("eventnamecolumnname"))
                _eventNameColumnName = ParsedCommandLineArguments.ParameterDictionary["eventnamecolumnname"];

            string _timeRelativeTimeColumnName = "RelativeTime";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("relativetimecolumnname"))
                _timeRelativeTimeColumnName = ParsedCommandLineArguments.ParameterDictionary["relativetimecolumnname"];

            string _relativeTimeFormatString = "SECONDS";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("relativetimeformatstring"))
                _relativeTimeFormatString = ParsedCommandLineArguments.ParameterDictionary["relativetimeformatstring"];

            string _timeStampColumnName = "TimeStamp";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("timestampcolumnname"))
                _timeStampColumnName = ParsedCommandLineArguments.ParameterDictionary["timestampcolumnname"];

            string _timeStampFormatString = "dd.MM.yyyy HH:mm.ss:ffff";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("timestampformatstring"))
                _timeStampFormatString = ParsedCommandLineArguments.ParameterDictionary["timestampformatstring"];

            string _missingValueString = "NA";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("missingvaluestring"))
                _missingValueString = ParsedCommandLineArguments.ParameterDictionary["missingvaluestring"];

            EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.Time;
            if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                sort = EventDataListExtension.ESortType.None;
            if (ParsedCommandLineArguments.Flags.Contains("ORDER_WITHIN_ELEMENTS"))
                sort = EventDataListExtension.ESortType.ElementAndTime;

            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            List<EventData> _inMemoryTempDataEvents = new List<EventData>();

            int _numberOfStudents = 0;
            string _personIdentifier = "";

            using (ZipFile outputZipFile = new ZipFile())
            {
                using (Stream fileStream = File.OpenRead(ZipFileName),
                          zippedStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(zippedStream))
                    {
                        int _lineCounter = 0;
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            csv.Configuration.Delimiter = _columnDelimiter;
                            csv.Configuration.BadDataFound = x =>
                            {
                                Console.WriteLine($"Bad data (Line " + _lineCounter + "): <{x.RawRecord}>");
                            };

                            // prepare reader

                            var _data_rows = csv.GetRecords<dynamic>();

                            // process line by line

                            foreach (IDictionary<string, object> row in _data_rows)
                            {

                                // check required columns 

                                if (ParsedCommandLineArguments.RelativeTime)
                                {
                                    if (!row.Keys.Contains(_personIdentifierColumnName) ||
                                   !row.Keys.Contains(_elementColumnName) ||
                                   !row.Keys.Contains(_eventNameColumnName) ||
                                   !row.Keys.Contains(_timeRelativeTimeColumnName))
                                    {
                                        Console.WriteLine("Required columns (" + _personIdentifierColumnName + ", " + _elementColumnName + ", " + _eventNameColumnName + ", " + _timeRelativeTimeColumnName + ") not found (Line " + _lineCounter + ")");
                                        continue;
                                    };
                                }
                                else
                                {
                                    if (!row.Keys.Contains(_personIdentifierColumnName) ||
                                    !row.Keys.Contains(_elementColumnName) ||
                                    !row.Keys.Contains(_eventNameColumnName) ||
                                    !row.Keys.Contains(_timeStampColumnName))
                                    { 
                                        Console.WriteLine("Required columns (" + _personIdentifierColumnName + ": " + row.Keys.Contains(_personIdentifierColumnName).ToString() + ", " +
                                                                                 _elementColumnName + ": " + row.Keys.Contains(_elementColumnName).ToString() +  ", " + 
                                                                                 _eventNameColumnName + ": " + row.Keys.Contains(_eventNameColumnName).ToString() + ", " +
                                                                                 _timeStampColumnName + ": " + row.Keys.Contains(_timeStampColumnName).ToString() + ") not found (Line " + _lineCounter + ")");
                                        return;
                                    };
                                }

                                if (row[_personIdentifierColumnName].ToString() != _personIdentifier && _inMemoryTempDataEvents.Count > 0)
                                {
                                    _numberOfStudents += 1;
                                    if (ParsedCommandLineArguments.RelativeTime)
                                        _inMemoryTempDataEvents.ComputeTimedifferencePreviousWithRelativeTimes(sort);
                                    else
                                        _inMemoryTempDataEvents.ComputeTimedifferencePrevious(sort);

                                    outputZipFile.AddEntry(_personIdentifier + ".json", JsonConvert.SerializeObject(_inMemoryTempDataEvents, Newtonsoft.Json.Formatting.Indented));
                                    _inMemoryTempDataEvents = new List<EventData>();

                                    if (Verbose)
                                        Console.WriteLine(_personIdentifier + " -- " + _numberOfStudents);
                                }

                                _personIdentifier = row[_personIdentifierColumnName].ToString();

                                // Pre-process event values

                                EventData e = new EventData()
                                {
                                    PersonIdentifier = _personIdentifier,
                                    EventName = row[_eventNameColumnName].ToString(),
                                    Element = row[_elementColumnName].ToString()
                                };

                                // Try to parse datetime string 

                                DateTime _timeStamp = DateTime.MinValue;
                                TimeSpan _relativeTime = TimeSpan.MinValue;
                                if (ParsedCommandLineArguments.RelativeTime)
                                {

                                    if (!ParseTime(row[_timeRelativeTimeColumnName].ToString(), _relativeTimeFormatString, out _relativeTime))
                                        Console.WriteLine("Date time format error for relative time (value '" + row[_timeStampColumnName].ToString() + "' does not match the format string '" + _timeStampFormatString + "' provided as argument for 'timestampformatstring')");

                                    e.RelativeTime = _relativeTime;
                                }
                                else
                                {
                                    if (!DateTime.TryParseExact(row[_timeStampColumnName].ToString(), _timeStampFormatString, provider, DateTimeStyles.None, out _timeStamp))
                                        Console.WriteLine("Date time format error for time stamp (value '" + row[_timeStampColumnName].ToString() + "' does not match the format string '" + _timeStampFormatString + "' provided as argument for  'timestampformatstring')");

                                    e.TimeStamp = _timeStamp;

                                }

                                // Add additional attributes

                                foreach (string _key in row.Keys)
                                {
                                    if (_key != _personIdentifier && _key != _eventNameColumnName && _key != _elementColumnName && _key != _timeStampColumnName)
                                    {
                                        string _value = row[_key].ToString();
                                        string _column = _key;
                                        if (_column == "")
                                            _column = "Var";

                                        if (_value != _missingValueString)
                                            e.AddEventValue(_column, _value);
                                    }
                                }

                                _lineCounter += 1;
                                _inMemoryTempDataEvents.Add(e);
                            }
                        }
                    }
                }

                if (_inMemoryTempDataEvents.Count > 0)
                {
                    _numberOfStudents += 1;
                    if (ParsedCommandLineArguments.RelativeTime)
                        _inMemoryTempDataEvents.ComputeTimedifferencePreviousWithRelativeTimes(sort);
                    else
                        _inMemoryTempDataEvents.ComputeTimedifferencePrevious(sort);

                    outputZipFile.AddEntry(_personIdentifier + ".json", JsonConvert.SerializeObject(_inMemoryTempDataEvents, Newtonsoft.Json.Formatting.Indented));

                    if (Verbose)
                        Console.WriteLine(_personIdentifier + " -- " + _numberOfStudents);
                }

                outputZipFile.Save(OutFileName);
            }
        }

        /// <summary>
        /// CheckReadLogDataFlatV01: Try to read the provided ZipFileName as GZIP file containing a CSV file
        /// </summary>
        /// <param name="ZipFileName">ZIP file name with data expected in format 'dataflatv01a'</param>
        /// <param name="ColumnDelimiter">Column delimiter</param>
        /// <returns></returns>
        private static bool CheckReadLogDataFlatV01(string ZipFileName, string ColumnDelimiter)
        {
            try
            {
                using (Stream fileStream = File.OpenRead(ZipFileName))
                {
                    using (var zippedStream = new GZipStream(fileStream, CompressionMode.Decompress))
                    {
                        using (var reader = new StreamReader(zippedStream))
                        {
                            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                            {
                                csv.Configuration.Delimiter = ColumnDelimiter;
                                var _data_rows = csv.GetRecords<dynamic>();
                                return _data_rows.Count() > 0;
                            }
                        }
                    }
                }
            }
            catch (ZlibException _ex)
            {
                Console.WriteLine("Error reading zip archive: " + _ex.ToString());
                return false;
            }
            catch (Exception _ex)
                {
                Console.WriteLine("Unknown error: " + _ex.ToString());
                return false;
            }
        }

        private static bool ParseTime(string RelativeTimeString, string RelativeTimeFormatString, out TimeSpan RelativeTime)
        {
            // TODO: Document Format String "SECONDS", "MILLISECONDS", "MINUTES" ...
            RelativeTime = TimeSpan.MinValue;
            if (RelativeTimeFormatString == "SECONDS")
            {
                double _seconds = double.MaxValue;
                if (!double.TryParse(RelativeTimeString, NumberStyles.Any, CultureInfo.InvariantCulture, out _seconds))
                    return false;            
                RelativeTime = TimeSpan.FromSeconds(_seconds);
            }
            else if (RelativeTimeFormatString == "MILLISECONDS")
            {
                double _milliseconds = double.MaxValue;
                if (!double.TryParse(RelativeTimeString, NumberStyles.Any, CultureInfo.InvariantCulture, out _milliseconds))
                    return false;
                RelativeTime = TimeSpan.FromMilliseconds(_milliseconds);
            }
            else if (RelativeTimeFormatString == "MINUTES")
            {
                double _minutes = double.MaxValue;
                if (!double.TryParse(RelativeTimeString, NumberStyles.Any, CultureInfo.InvariantCulture, out _minutes))
                    return false;
                RelativeTime = TimeSpan.FromMinutes(_minutes);
            }
            else
            {
                if (!TimeSpan.TryParseExact(RelativeTimeString, RelativeTimeFormatString, CultureInfo.InvariantCulture,  out RelativeTime))
                    return false;
            }
            return true;
        }

        #endregion

        #region Universal Log Format
        public static void ReadLogDataGenericV01(string ZipFileName, string OutFileName, string[] Elements, bool Verbose,
            CommandLineArguments ParsedCommandLineArguments)
        {
            Console.WriteLine("Module: Read log data in the universal log format V01.");
       
            bool _absolute = true;
            if (ParsedCommandLineArguments.RelativeTime)
                _absolute = false;

            string _columnNamePersonIdentifier = "caseid";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columnnamepersonidentifier"))
                _columnNamePersonIdentifier = ParsedCommandLineArguments.ParameterDictionary["columnnamepersonidentifier"];

            string _columnNameEventName = "EventName";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columnnameeventname"))
                _columnNameEventName = ParsedCommandLineArguments.ParameterDictionary["columnnameeventname"];

            string _columnNameElement= "Element";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columnnameelement"))
                _columnNameElement = ParsedCommandLineArguments.ParameterDictionary["columnnameelement"];

            string _columnNameTimeStamp= "TimeStamp";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columnnametimestamp"))
                _columnNameTimeStamp = ParsedCommandLineArguments.ParameterDictionary["columnnametimestamp"];

            EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.Time;
            if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                sort = EventDataListExtension.ESortType.None;
            if (ParsedCommandLineArguments.Flags.Contains("ORDER_WITHIN_ELEMENTS"))
                sort = EventDataListExtension.ESortType.ElementAndTime;


            if (!CheckReadLogDataGenericV01(ZipFileName, _columnNamePersonIdentifier, _columnNameEventName, _columnNameElement, _columnNameTimeStamp, Verbose, true))
            {
                Console.WriteLine("Data not in the expected generic log format. Check format and consider to provide an alternative value for the attribute 'datafiletype'.");
                return;
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

            Dictionary<string, List<EventData>> _inMemoryTempData = new Dictionary<string, List<EventData>>();

            using (ZipFile zip = ZipFile.Read(ZipFileName))
            {
                foreach (ZipEntry e in zip.Entries)
                { 
                    if (e.FileName != "Log.dta")
                    {
                        if (Verbose)
                            Console.Write("Read file '" + e.FileName + "' ");

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
                            if (_elementLabelSetName!= "")
                            {
                                var _elementLabelSet = valuedict[_elementLabelSetName];
                                foreach (var v in _elementLabelSet.Keys)
                                {
                                    _selectedElements.Add(v.ToString());
                                }
                            }
                            
                            int _elementColumnIndex = vardict[_columnNameElement].Item1;
                            Dictionary<int, string> _elementValueLabelDict = new Dictionary<int, string>();
                            if (vardict[_columnNameElement].Item2.ValueLabelName!= "")
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
                                if (_selectedElements.Count == 0 ||_selectedElements.Contains(_line[_elementColumnIndex].ToString()))
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
                                    if (!_absolute)
                                    {
                                        DateTime.TryParse(_line[_timeStampColumnIndex].ToString(), out _timeStamp);
                                    }
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

                                    if (!_inMemoryTempData.ContainsKey(_personIdentifier))
                                        _inMemoryTempData.Add(_personIdentifier, new List<EventData>());

                                    if (Elements.Length == 0 || Elements.Contains<string>(_element))
                                    {
                                        _inMemoryTempData[_personIdentifier].Add(new EventData()
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

                        if (Verbose)
                            Console.WriteLine("ok.");
                    }

                }
            }


            using (ZipFile outzip = new ZipFile())
            {
                List<string> _persons = _inMemoryTempData.Keys.ToList<string>();

                foreach (string _personIdentifier in _persons)
                {
                    // sort events by time stamp

                    var _l = _inMemoryTempData[_personIdentifier];

                    if (ParsedCommandLineArguments.RelativeTime)
                    {
                        _l.ComputeTimedifferencePreviousWithRelativeTimes(sort);
                        _l = _l.OrderBy(o => o.RelativeTime).ToList();
                    }
                    else
                    {
                        _l.ComputeTimedifferencePrevious(sort);
                        _l = _l.OrderBy(o => o.TimeStamp).ToList();
                    }
                        
                    string json = JsonConvert.SerializeObject(_l, Newtonsoft.Json.Formatting.Indented);
                    outzip.AddEntry(_personIdentifier + ".json", json);

                }
                outzip.Save(OutFileName);
            }


        }


        private static bool CheckReadLogDataGenericV01(string ZipFileName, string ColumnNamePersonIdentifier, string ColumnNameEventName, string ColumnNameElement, string ColumnNameTimeStamp, bool Verbose, bool CheckFirstFileOnly)
        {
            // Check a) the first [CheckFirstFileOnly == TRUE] or b) all [CheckFirstFileOnly == false] Stata files if the required column names are present 

            using (ZipFile zip = ZipFile.Read(ZipFileName))
            {
                foreach (ZipEntry e in zip.Entries)
                {
                    if (e.FileName != "Log.dta")
                    {
                        if (Verbose)
                            Console.Write("Check file '" + e.FileName + "' ");

                        using (MemoryStream zipStream = new MemoryStream())
                        {
                            e.Extract(zipStream);
                            zipStream.Position = 0;
                            var str = new StataFileReader(zipStream, false);
                            bool valid = true;

                            if (str.Variables.FindIndex(f => f.Name == ColumnNameEventName) == -1)
                            {
                                Console.WriteLine("Column '" + ColumnNameEventName + "' not found in file '" + e.FileName + "'. Provide the name of the column that contains the event name using the argument 'ColumnNameEventName'.");
                                valid = false;
                            }
                            else if (str.Variables.FindIndex(f => f.Name == ColumnNameElement) == -1)
                            {
                                Console.WriteLine("Column '" + ColumnNameElement + "' not found in file '" + e.FileName + "'. Provide the name of the column that contains the name of the element (i.e., item, unit, ...) using the argument 'ColumnNameElement'.");
                                valid = false;
                            }
                            else if (str.Variables.FindIndex(f => f.Name == ColumnNamePersonIdentifier) == -1)
                            {
                                Console.WriteLine("Column '" + ColumnNamePersonIdentifier + "' not found in file '" + e.FileName + "'. Provide the name of the column that contains the person identifier using the argument 'ColumnNamePersonIdentifier'.");
                                valid = false;
                            }
                            else if (str.Variables.FindIndex(f => f.Name == ColumnNameTimeStamp) == -1)
                            {
                                Console.WriteLine("Column '" + ColumnNameTimeStamp + "' not found in file '" + e.FileName + "'. Provide the name of the column that contains the time stamp using the argument 'ColumnNameTimeStamp'.");
                                valid = false;
                            }

                            if (!valid)
                            {
                                if (Verbose)
                                {
                                    Console.WriteLine("failed. Found the following column names in file '" + e.FileName + "':");
                                    foreach (var v in str.Variables)
                                    {
                                        Console.WriteLine("- '" + v.Name + "': " + v.Description);
                                    }
                                }

                                return false;
                            } 
                            else
                            {
                                if (Verbose)
                                    Console.WriteLine("ok.");

                                if (CheckFirstFileOnly)
                                    return true;
                            }

                        }
                    }
                }
            }
            
            return true;
        }
        #endregion

        #region PISA CA

        public static void ReadPISA_from_DME_ZIP__CA_from_session_files(string ZipFileName, string OutFileName, string Password, string[] Element, string[] ExcludeEvents, int max, bool addEventAttributeCounters, string Cycle, bool Verbose, CommandLineArguments ParsedCommandLineArguments)
        {

            if (ParsedCommandLineArguments.RelativeTime)
            {
                throw new Exception("Generic format with relative times is not implemented yet.");
            }

            EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.Time;
            if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                sort = EventDataListExtension.ESortType.None;
            if (ParsedCommandLineArguments.Flags.Contains("ORDER_WITHIN_ELEMENTS"))
                sort = EventDataListExtension.ESortType.ElementAndTime;


            List<EventData> _inMemoryTempDataEvents = new List<EventData>();
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            int numberOfStudents = 0;

            Dictionary<Tuple<string, string, string>, Dictionary<string, int>> _eventAttributeCounter = new Dictionary<Tuple<string, string, string>, Dictionary<string, int>>();
            //               ^ element, event ^property           ^ value ^ counter

            string[] _ignoreAttributesForCounter = new string[] { "eventCounter", "time", "timeStamp", "event_name", "itemDuration", "TimeTotal", "TimeFAction", "NbrAction", "fteData", "rule", "position" };

            using (ZipFile outputZipFile = new ZipFile())
            {
                using (var outerInputZipFile = ZipFile.Read(ZipFileName))
                {
                    foreach (var outerInputZipEntry in outerInputZipFile.Entries)
                    {
                        if (outerInputZipEntry.FileName.EndsWith("Session1.zip") && outerInputZipEntry.UncompressedSize != 0)
                        {
                            string _PersonIdentifier = Path.GetFileName(outerInputZipEntry.FileName).Replace("-Session1.zip", "");
                            using (MemoryStream outerZIPEntryMemoryStream = new MemoryStream())
                            {
                                if (_inMemoryTempDataEvents.Count > 0)
                                {
                                    if (ParsedCommandLineArguments.RelativeTime)
                                        _inMemoryTempDataEvents.ComputeTimedifferencePreviousWithRelativeTimes(sort);
                                    else
                                        _inMemoryTempDataEvents.ComputeTimedifferencePrevious(sort);
                                    
                                    outputZipFile.AddEntry(_PersonIdentifier + ".json", JsonConvert.SerializeObject(_inMemoryTempDataEvents, Newtonsoft.Json.Formatting.Indented));
                                }

                                if (Verbose)
                                    Console.WriteLine(_PersonIdentifier + " -- " + numberOfStudents);

                                outerInputZipEntry.Password = Password;
                                outerInputZipEntry.Extract(outerZIPEntryMemoryStream);
                                outerZIPEntryMemoryStream.Position = 0;
                                numberOfStudents += 1;

                                _inMemoryTempDataEvents = new List<EventData>();

                                using (var innerZIP = ZipFile.Read(outerZIPEntryMemoryStream))
                                {
                                    foreach (var innerZIPEntry in innerZIP.Entries)
                                    {
                                        if (innerZIPEntry.FileName.StartsWith("trace/") && innerZIPEntry.FileName.EndsWith(".xml") && innerZIPEntry.UncompressedSize != 0)
                                        {
                                            using (MemoryStream innerZIPEntryMemoryStream = new MemoryStream())
                                            {
                                                innerZIPEntry.Password = Password;
                                                innerZIPEntry.Extract(innerZIPEntryMemoryStream);
                                                innerZIPEntryMemoryStream.Position = 0;

                                                var _sr = new StreamReader(innerZIPEntryMemoryStream);
                                                var _xml = CleanInvalidXmlChars(_sr.ReadToEnd());

                                                var _xmlReaderSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, IgnoreWhitespace = true, IgnoreComments = true };
                                                var xmlReader = XmlReader.Create(new StringReader(_xml), _xmlReaderSettings);
                                                {
                                                    xmlReader.MoveToContent();

                                                    Dictionary<string, string> _EventValues = new Dictionary<string, string>();
                                                    string _currentElement = "";
                                                    string _currentValue = "";
                                                    while (xmlReader.Read())
                                                    {
                                                        if (xmlReader.IsStartElement() && xmlReader.Name == "event")
                                                        {
                                                            if (_EventValues.Keys.Count > 0)
                                                            {
                                                                string _currentElementName = _EventValues["unitId"];
                                                                string _currentEventName = _EventValues["event_name"];
                                                                bool _include = true;
                                                                if (Element.Length > 0)
                                                                {
                                                                    if (!Element.Contains<string>(_currentElementName))
                                                                        _include = false;
                                                                }

                                                                if (ExcludeEvents.Length > 0)
                                                                {
                                                                    if (ExcludeEvents.Contains<string>(_currentEventName))
                                                                        _include = false;
                                                                }

                                                                if (_include)
                                                                {
                                                                    _inMemoryTempDataEvents.Add(new EventData()
                                                                    {
                                                                        EventName = _currentEventName,
                                                                        PersonIdentifier = _PersonIdentifier,
                                                                        TimeStamp = dt1970.AddMilliseconds(double.Parse(_EventValues["time"])),
                                                                        TimeDifferencePrevious = TimeSpan.MinValue,
                                                                        EventValues = _EventValues
                                                                    });

                                                                    if (addEventAttributeCounters)
                                                                    {
                                                                        foreach (string _e in _EventValues.Keys)
                                                                        {
                                                                            Tuple<string, string, string> _key = new Tuple<string, string, string>(_currentElementName, _currentEventName, _e);

                                                                            if (!_eventAttributeCounter.ContainsKey(_key))
                                                                                _eventAttributeCounter.Add(_key, new Dictionary<string, int>());

                                                                            if (!_ignoreAttributesForCounter.Contains<string>(_e))
                                                                            {
                                                                                if (!_eventAttributeCounter[_key].ContainsKey(_EventValues[_e]))
                                                                                    _eventAttributeCounter[_key].Add(_EventValues[_e], 0);
                                                                                _eventAttributeCounter[_key][_EventValues[_e]] += 1;
                                                                            }
                                                                        }

                                                                    }
                                                                }
                                                            }
                                                            _EventValues = new Dictionary<string, string>();
                                                        }
                                                        else if (xmlReader.IsStartElement())
                                                        {
                                                            _currentElement = xmlReader.Name;
                                                        }
                                                        else if (xmlReader.NodeType == XmlNodeType.EndElement)
                                                        {
                                                            if (_currentElement != "")
                                                            {
                                                                _EventValues.Add(_currentElement, _currentValue);
                                                                _currentValue = "";
                                                                _currentElement = "";
                                                            }
                                                        }
                                                        else if (xmlReader.Value != "")
                                                        {
                                                            _currentValue = xmlReader.Value;
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }


                        if (max >= 1 && numberOfStudents >= max)
                            break;
                    }
                }

                string _eventSummary = "Unit" + "\t" + "EventName" + "\t" + "Property" + "\t" + "Value" + "\t" + "Count" + Environment.NewLine;
                foreach (var _key1 in _eventAttributeCounter.Keys)
                {
                    if (_eventAttributeCounter[_key1].Keys.Count == 0)
                    {
                        _eventSummary += (_key1.Item1 + "\t" + _key1.Item2 + "\t" + _key1.Item3 + "\t" + "NA" + "\t" + "NA") + Environment.NewLine;
                    }
                    else
                    {
                        foreach (var _key2 in _eventAttributeCounter[_key1].Keys)
                        {
                            _eventSummary += (_key1.Item1 + "\t" + _key1.Item2 + "\t" + _key1.Item3 + "\t" + _key2 + "\t" + _eventAttributeCounter[_key1][_key2]) + Environment.NewLine;
                        }
                    }
                }
                outputZipFile.AddEntry("EventSummary.dat", _eventSummary);

                outputZipFile.Save(OutFileName);
            }

        }

        #endregion

        #region PIAAC R1

        public static void ReadPIAAC_r1_from_preprocessed_TextFile_from_LDA(string ZipFile, string OutFileName, string Password, string[] Element, bool Verbose, int MaxNumberOfStudents, CommandLineArguments ParsedCommandLineArguments)
        {
            Console.Write("Module: Read PIAAC R1 from text files preprocessed by the 'PIAAC Log Data Analyzer'.");

            if (!ParsedCommandLineArguments.RelativeTime)
            {
                ParsedCommandLineArguments.RelativeTime = true;
                Console.Write("Note: Changed to relative times. ");
            }

            EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.ElementAndTime;
            if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                sort = EventDataListExtension.ESortType.None;

            Dictionary<string, string> _lookup = LogDataTransformer_PIAACR1_Module_V01.GetPIAACR1LookupDictionary();
 
            List<EventData> _inMemoryTempDataEvents = new List<EventData>();
            int numberOfStudents = 0;
            string _PersonIdentifier = "";

            using (ZipFile outputZipFile = new ZipFile())
            {
                 
                using (var outerInputZipFile = Ionic.Zip.ZipFile.Read(ZipFile))
                {
                    foreach (var outerInputZipEntry in outerInputZipFile.Entries)
                    {
                        if (outerInputZipEntry.FileName.EndsWith(".txt", StringComparison.InvariantCulture) && outerInputZipEntry.UncompressedSize != 0)
                        {
                            using (MemoryStream outerZIPEntryMemoryStream = new MemoryStream())
                            {
                                outerInputZipEntry.Password = Password;
                                outerInputZipEntry.Extract(outerZIPEntryMemoryStream);
                                outerZIPEntryMemoryStream.Position = 0;

                                
                                using (var reader = new StreamReader(outerZIPEntryMemoryStream, Encoding.UTF8))
                                {
                                    int _lineCounter = 0;
                                    string line = "";
                                    while ((line = reader.ReadLine()) != null)
                                    { 
                                        string[] _cols = line.Split('\t');
                                        if (_cols.Length > 1 && _lineCounter > 0)
                                        {
                                            if (_cols[0] + "_" + _cols[1] != _PersonIdentifier && _inMemoryTempDataEvents.Count > 0)
                                            {
                                                numberOfStudents += 1;
                                                if (ParsedCommandLineArguments.RelativeTime)
                                                    _inMemoryTempDataEvents.ComputeTimedifferencePreviousWithRelativeTimes(sort);
                                                else
                                                    _inMemoryTempDataEvents.ComputeTimedifferencePrevious(sort);

                                                outputZipFile.AddEntry(_PersonIdentifier + ".json", JsonConvert.SerializeObject(_inMemoryTempDataEvents, Newtonsoft.Json.Formatting.Indented));
                                                _inMemoryTempDataEvents = new List<EventData>();
                                                 
                                                if (Verbose)
                                                    Console.WriteLine(_PersonIdentifier + " -- " + numberOfStudents);

                                            }

                                            _PersonIdentifier = _cols[0] + "_" + _cols[1];

                                            string _rawBooklet = _cols[2];
                                            string _rawSequenceID = _cols[3];
                                            string _rawEventName = _cols[4];
                                            string _rawEventType = _cols[5]; 
                                            string _relativeTimeString = _cols[6];

                                            TimeSpan _relativeTime = TimeSpan.FromMilliseconds(double.Parse(_cols[6], CultureInfo.InvariantCulture));

                                            string _rawEventDescription = _cols[7];
                                            
                                            string _itemID = _rawBooklet + "_" + _rawSequenceID;

                                            if (!ParsedCommandLineArguments.Flags.Contains("USE_BOOKLLET_SEQUENCE_ID"))
                                            { 
                                                if (_lookup.ContainsKey(_itemID))
                                                    _itemID = _lookup[_itemID];
                                                
                                            }

                                            bool _isFiltered = Element.Length == 0;
                                            if (Element.Contains(_itemID))
                                                _isFiltered = true;

                                            if (_isFiltered)
                                            {
                                                if (_rawEventDescription.StartsWith("<cbaloggingmodel", StringComparison.InvariantCulture))
                                                {
                                                    // IB xml events

                                                    _inMemoryTempDataEvents.AddRange(piaac_R1_extract_IB_XML(_rawEventDescription, _PersonIdentifier, _rawSequenceID, _rawBooklet, _rawEventType, _rawEventName, _relativeTime, ParsedCommandLineArguments.Flags));
                                                }
                                                else if (_rawEventDescription.StartsWith("<cbascoringresultmm", StringComparison.InvariantCulture))
                                                {
                                                    // IB item score

                                                    if (_rawEventType == "ItemScore")
                                                    {
                                                        if (ParsedCommandLineArguments.Flags.Contains("INCLUDE_ALL_ITEMSCORES") ||  !ParsedCommandLineArguments.Flags.Contains("INCLUDE_NO_ITEMSCORES"))  
                                                            _inMemoryTempDataEvents.AddRange(piaac_R1_extract_IB_XML(_rawEventDescription, _PersonIdentifier, _rawSequenceID, _rawBooklet, _rawEventType, _rawEventName, _relativeTime, ParsedCommandLineArguments.Flags));
                                                    }
                                                    else if (_rawEventType == "itemScoreResult")
                                                    {
                                                        // by defaul ignore second item score 
                                                        if (ParsedCommandLineArguments.Flags.Contains("INCLUDE_ALL_ITEMSCORES"))  
                                                            _inMemoryTempDataEvents.AddRange(piaac_R1_extract_IB_XML(_rawEventDescription, _PersonIdentifier, _rawSequenceID, _rawBooklet, _rawEventType, _rawEventName, _relativeTime, ParsedCommandLineArguments.Flags));
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("The value '" + _rawEventName + "' was not expected in line " + _lineCounter + ".");
                                                    }
                                                }
                                                else if (_rawEventDescription.StartsWith("http://localhost:8080", StringComparison.InvariantCulture))
                                                {
                                                    // ignore snapshots
                                                }
                                                else
                                                {
                                                    // Pre-process event values

                                                    EventData e = new EventData()
                                                    {
                                                        PersonIdentifier = _PersonIdentifier,
                                                        EventName = _rawEventType,
                                                        Element = _rawSequenceID,
                                                        RelativeTime = _relativeTime
                                                    };
                                                    
                                                    _rawEventDescription = _rawEventDescription.Replace("|$*", "|*$");
                                                    string[] _kvps = _rawEventDescription.Split("|*$");
                                                    foreach (string _kvp in _kvps)
                                                    {
                                                        string[] _kv = _kvp.Split('=');
                                                        if (_kv.Length == 2)
                                                        {
                                                            e.AddEventValue(_kv[0], _kv[1]);
                                                        }
                                                        else if (_kv.Length == 1)
                                                        {
                                                            e.AddEventValue("value", _kv[0]);
                                                        }
                                                        else
                                                        {
                                                            string _remainingvalue = _kv[1];
                                                            for (int i=2; i< _kv.Length; i++)
                                                            {
                                                                _remainingvalue = _remainingvalue + "=" + _kv[i];
                                                            }
                                                            e.AddEventValue(_kv[0], _remainingvalue);
                                                        } 
                                                    }

                                                    if (!ParsedCommandLineArguments.Flags.Contains("HIDE_BOOKLET_INFORMATION"))
                                                        e.AddEventValue("PIAACBookelt", _rawBooklet);

                                                    if (!ParsedCommandLineArguments.Flags.Contains("HIDE_EVENT_INFORMATION"))
                                                    {
                                                        e.AddEventValue("PIAACSequenceID", _rawSequenceID);
                                                        e.AddEventValue("PIAACEventType", _rawEventType);
                                                        e.AddEventValue("PIAACEventName", _rawEventName);
                                                    }

                                                    if (!ParsedCommandLineArguments.Flags.Contains("HIDE_TIME_INFORMATION"))
                                                        e.AddEventValue("RelativeTimeString", _relativeTimeString);
                                                    
                                                   
                                                    _inMemoryTempDataEvents.Add(e);
                                                }   
                                            } 
                                        }
                                        _lineCounter++;
                                         
                                        if (MaxNumberOfStudents >= 1 && numberOfStudents >= MaxNumberOfStudents)
                                            break;
                                    }
                                }

                                if (_inMemoryTempDataEvents.Count > 0)
                                {
                                    numberOfStudents += 1;
                                    if (ParsedCommandLineArguments.RelativeTime)
                                        _inMemoryTempDataEvents.ComputeTimedifferencePreviousWithRelativeTimes(sort);
                                    else
                                        _inMemoryTempDataEvents.ComputeTimedifferencePrevious(sort);

                                    outputZipFile.AddEntry(_PersonIdentifier + ".json", JsonConvert.SerializeObject(_inMemoryTempDataEvents, Newtonsoft.Json.Formatting.Indented));

                                }
                            }
                        }
                    }
                }
                 
                outputZipFile.Save(OutFileName);
            }
        }

        public static List<EventData> piaac_R1_extract_IB_XML(string XML, string PersonIdentifier, string Element, string Booklet, string  EventType, string EventName, TimeSpan RelativeTime, List<string> Flags)
        {
            List<EventData> _return = new List<EventData>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XML);

            piaac_R1_extract_IB_XML_process_child(doc, _return, "", 0,  PersonIdentifier, Element, Booklet, EventType, EventName, RelativeTime, Flags);

            return _return;
        }
 
        private static void piaac_R1_extract_IB_XML_process_child(XmlNode doc, List<EventData> List, string Path, int Level, string PersonIdentifier, string Element, string Booklet, string EventType, string EventName, TimeSpan RelativeTime, List<string> Flags)
        { 
            string _name = "";
            foreach (XmlNode n in doc.ChildNodes)
            {
                _name = n.LocalName;
                if (Path.Trim() != "")
                    _name = Path + "." + _name;

                XmlAttributeCollection atributos = n.Attributes;
                EventData e = new EventData()
                {
                    PersonIdentifier = PersonIdentifier,
                    RelativeTime = RelativeTime, 
                    EventName = _name,
                    Element = Element
                };

                foreach (XmlAttribute at in atributos)
                {
                    if (at.LocalName == "cbaloggingmodel" || at.LocalName == "cbascoringresultmm" || at.LocalName == "snapshot" || at.LocalName == "xmi" || at.LocalName == "xsi")
                    {
                        // ignore attributes by default

                        if (Flags.Contains("INCLUDE_XML_ATTRIBUTES"))
                            e.AddEventValue(at.LocalName, at.Value);
                    }
                    else
                    {
                        e.AddEventValue(at.LocalName, at.Value);
                    }

                }
                if (!Flags.Contains("HIDE_BOOKLET_INFORMATION"))
                    e.AddEventValue("PIAACBookelt", Booklet);

                if (!Flags.Contains("HIDE_EVENT_INFORMATION"))
                {
                    e.AddEventValue("PIAACEventType", EventType);
                    e.AddEventValue("PIAACEventName", EventName);
                }

                if (!Flags.Contains("HIDE_XML_NESTING_LEVEL"))
                    e.AddEventValue("XMLNextingLevel", Level.ToString());

                List.Add(e);
            }
             
            foreach (XmlNode n in doc.ChildNodes)
            {
                piaac_R1_extract_IB_XML_process_child(n, List, _name, Level+1, PersonIdentifier, Element, Booklet, EventType, EventName, RelativeTime, Flags);
            }
        }

        #endregion

        #region PISA BQ

        public static void ReadPISA_from_DME_ZIP__BQ_from_session_files(string ZipFileName, string OutFileName, string Password, string[] Element, int max, string Cycle, bool Verbose, CommandLineArguments ParsedCommandLineArguments)
        {

            if (ParsedCommandLineArguments.RelativeTime)
            {
                throw new Exception("Generic format with relative times is not implemented yet.");
            }

            EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.Time;
            if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                sort = EventDataListExtension.ESortType.None;
            if (ParsedCommandLineArguments.Flags.Contains("ORDER_WITHIN_ELEMENTS"))
                sort = EventDataListExtension.ESortType.ElementAndTime;

            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            List<EventData> _inMemoryTempDataEvents = new List<EventData>();
            XmlSerializer logSerializer = new XmlSerializer(typeof(log));
            int numberOfStudents = 0;
            string _PreviousPersonIdentifier = "";
            string _PersonIdentifier = "";

            using (ZipFile outputZipFile = new ZipFile())
            {
                using (var outerInputZipFile = ZipFile.Read(ZipFileName))
                {
                    foreach (var outerInputZipEntry in outerInputZipFile.Entries)
                    {
                        if (outerInputZipEntry.FileName.EndsWith("Session2.zip") && outerInputZipEntry.UncompressedSize != 0)
                        {
                            _PreviousPersonIdentifier = _PersonIdentifier;
                            _PersonIdentifier = Path.GetFileName(outerInputZipEntry.FileName).Replace("-Session2.zip", "");

                            using (MemoryStream outerZIPEntryMemoryStream = new MemoryStream())
                            {
                                Console.WriteLine(_PersonIdentifier + " -- " + numberOfStudents);
                                outerInputZipEntry.Password = Password;
                                outerInputZipEntry.Extract(outerZIPEntryMemoryStream);
                                outerZIPEntryMemoryStream.Position = 0;
                                numberOfStudents += 1;

                                if (_inMemoryTempDataEvents.Count > 0 && _PreviousPersonIdentifier != "")
                                {
                                    if (ParsedCommandLineArguments.RelativeTime)
                                        _inMemoryTempDataEvents.ComputeTimedifferencePreviousWithRelativeTimes(sort);
                                    else
                                        _inMemoryTempDataEvents.ComputeTimedifferencePrevious(sort);
                                    outputZipFile.AddEntry(_PreviousPersonIdentifier + ".json", JsonConvert.SerializeObject(_inMemoryTempDataEvents, Newtonsoft.Json.Formatting.Indented));
                                }

                                _inMemoryTempDataEvents = new List<EventData>();

                                using (var innerZIP = ZipFile.Read(outerZIPEntryMemoryStream))
                                {
                                    foreach (var innerZIPEntry in innerZIP.Entries)
                                    {
                                        if (innerZIPEntry.FileName.EndsWith("_Data.zip") && innerZIPEntry.UncompressedSize != 0)
                                        {
                                            Console.WriteLine(innerZIPEntry.FileName);

                                            using (MemoryStream innerZIPEntryMemoryStream = new MemoryStream())
                                            {
                                                innerZIPEntry.Password = Password;
                                                innerZIPEntry.Extract(innerZIPEntryMemoryStream);
                                                innerZIPEntryMemoryStream.Position = 0;

                                                using (var inner2Zip = ZipFile.Read(innerZIPEntryMemoryStream))
                                                {
                                                    foreach (var inner2ZIPEntry in inner2Zip.Entries)
                                                    {
                                                        if (inner2ZIPEntry.FileName.EndsWith("-log.xml") && inner2ZIPEntry.UncompressedSize != 0)
                                                        {
                                                            using (MemoryStream inner2ZIPEntryMemoryStream = new MemoryStream())
                                                            {
                                                                inner2ZIPEntry.Password = Password;
                                                                inner2ZIPEntry.Extract(inner2ZIPEntryMemoryStream);
                                                                inner2ZIPEntryMemoryStream.Position = 0;

                                                                var _sr = new StreamReader(inner2ZIPEntryMemoryStream);
                                                                var _xml = CleanInvalidXmlChars(_sr.ReadToEnd());

                                                                processPISA_BQ_single_XML(Element, dt1970, _inMemoryTempDataEvents, logSerializer, _PersonIdentifier, _xml);
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }


                        if (max >= 1 && numberOfStudents >= max)
                            break;
                    }
                }

                outputZipFile.Save(OutFileName);
            }
        }

        public static void ReadPISA_from_DME_ZIP__BQ_from_zip_with_XML_files(string ZipFileName, string InnerZipFileName, string OutFileName, string Password, string[] Element, int max, string Cycle, bool Verbose, CommandLineArguments ParsedCommandLineArguments)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            List<EventData> _inMemoryTempDataEvents = new List<EventData>();
            XmlSerializer logSerializer = new XmlSerializer(typeof(log));
            int numberOfPersonss = 0;

            EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.Time;
            if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                sort = EventDataListExtension.ESortType.None;
            if (ParsedCommandLineArguments.Flags.Contains("ORDER_WITHIN_ELEMENTS"))
                sort = EventDataListExtension.ESortType.ElementAndTime;

            using (ZipFile outputZipFile = new ZipFile())
            {
                using (var outerInputZipFile = ZipFile.Read(ZipFileName))
                {
                    foreach (var outerInputZipEntry in outerInputZipFile.Entries)
                    {
                        if (outerInputZipEntry.FileName.EndsWith(InnerZipFileName) && outerInputZipEntry.UncompressedSize != 0)
                        {
                            using (MemoryStream outerZIPEntryMemoryStream = new MemoryStream())
                            {
                                outerInputZipEntry.Password = Password;
                                outerInputZipEntry.Extract(outerZIPEntryMemoryStream);
                                outerZIPEntryMemoryStream.Position = 0;

                                using (var innerZIP = ZipFile.Read(outerZIPEntryMemoryStream))
                                {
                                    foreach (var innerZIPEntry in innerZIP.Entries)
                                    {
                                        if (innerZIPEntry.FileName.EndsWith("-log.xml") && innerZIPEntry.UncompressedSize != 0)
                                        {
                                            numberOfPersonss += 1;

                                            string _PersonIdentifier = Path.GetFileName(innerZIPEntry.FileName).Replace("-log.xml", "");
                                            Console.WriteLine(_PersonIdentifier + " -- " + numberOfPersonss);

                                            if (_inMemoryTempDataEvents.Count > 0)
                                            {
                                                if (ParsedCommandLineArguments.RelativeTime)
                                                    _inMemoryTempDataEvents.ComputeTimedifferencePreviousWithRelativeTimes(sort);
                                                else
                                                    _inMemoryTempDataEvents.ComputeTimedifferencePrevious(sort);
                                                outputZipFile.AddEntry(_PersonIdentifier + ".json", JsonConvert.SerializeObject(_inMemoryTempDataEvents, Newtonsoft.Json.Formatting.Indented));
                                            }

                                            _inMemoryTempDataEvents = new List<EventData>();

                                            using (MemoryStream innerZIPEntryMemoryStream = new MemoryStream())
                                            {
                                                innerZIPEntry.Password = Password;
                                                innerZIPEntry.Extract(innerZIPEntryMemoryStream);
                                                innerZIPEntryMemoryStream.Position = 0;

                                                var _sr = new StreamReader(innerZIPEntryMemoryStream);
                                                var _xml = CleanInvalidXmlChars(_sr.ReadToEnd());

                                                processPISA_BQ_single_XML(Element, dt1970, _inMemoryTempDataEvents, logSerializer, _PersonIdentifier, _xml);

                                            }
                                        }

                                        if (max >= 1 && numberOfPersonss >= max)
                                            break;
                                    }
                                }
                            }
                        }



                    }
                }

                outputZipFile.Save(OutFileName);
            }
        }

        public static void ReadPISA_from_ZIP__BQ_with_XML_files(string ZipFileName, string OutFileName, string Password, string[] Element, int max, string Cycle, bool Verbose, CommandLineArguments ParsedCommandLineArguments)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            List<EventData> _inMemoryTempDataEvents = new List<EventData>();
            XmlSerializer logSerializer = new XmlSerializer(typeof(log));
            int numberOfPersonss = 0;

            EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.Time;
            if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                sort = EventDataListExtension.ESortType.None;
            if (ParsedCommandLineArguments.Flags.Contains("ORDER_WITHIN_ELEMENTS"))
                sort = EventDataListExtension.ESortType.ElementAndTime;

            using (ZipFile outputZipFile = new ZipFile())
            {
                using (var outerInputZipFile = ZipFile.Read(ZipFileName))
                {
                    foreach (var outerInputZipEntry in outerInputZipFile.Entries)
                    {
                        if (outerInputZipEntry.FileName.EndsWith("-log.xml") && outerInputZipEntry.UncompressedSize != 0)
                        {
                            numberOfPersonss += 1;

                            string _PersonIdentifier = Path.GetFileName(outerInputZipEntry.FileName).Replace("-log.xml", "");
                            Console.WriteLine(_PersonIdentifier + " -- " + numberOfPersonss);

                            if (_inMemoryTempDataEvents.Count > 0)
                            {
                                if (ParsedCommandLineArguments.RelativeTime)
                                    _inMemoryTempDataEvents.ComputeTimedifferencePreviousWithRelativeTimes(sort);
                                else
                                    _inMemoryTempDataEvents.ComputeTimedifferencePrevious(sort);
                                outputZipFile.AddEntry(_PersonIdentifier + ".json", JsonConvert.SerializeObject(_inMemoryTempDataEvents, Newtonsoft.Json.Formatting.Indented));
                            }

                            _inMemoryTempDataEvents = new List<EventData>();

                            using (MemoryStream innerZIPEntryMemoryStream = new MemoryStream())
                            {
                                outerInputZipEntry.Password = Password;
                                outerInputZipEntry.Extract(innerZIPEntryMemoryStream);
                                innerZIPEntryMemoryStream.Position = 0;

                                var _sr = new StreamReader(innerZIPEntryMemoryStream);
                                var _xml = CleanInvalidXmlChars(_sr.ReadToEnd());

                                processPISA_BQ_single_XML(Element, dt1970, _inMemoryTempDataEvents, logSerializer, _PersonIdentifier, _xml);

                            }

                        }

                        if (max >= 1 && numberOfPersonss >= max)
                            break;
                    }
                }

                outputZipFile.Save(OutFileName);
            }
        }

        private static void processPISA_BQ_single_XML(string[] Element, DateTime dt1970, List<EventData> _inMemoryTempDataEvents, XmlSerializer logSerializer, string _PersonIdentifier, string _xml)
        {
            var _tr = new StringReader(_xml);
            log _log = (log)logSerializer.Deserialize(_tr);

            if (_log.itemGroup != null)
            {
                // update epoch information
                foreach (var i in _log.itemGroup)
                {
                    if (!i.epochSpecified && i.userEvents.Length > 0)
                        i.epoch = i.userEvents[0].epoch;
                }

                List<logItemGroup> _sortedItemGroupList = _log.itemGroup.OrderBy(o => o.epoch).ToList();

                DateTime _MinAbsoluteTime = DateTime.MaxValue;
                DateTime _PreviousEvent = DateTime.MaxValue;
                if (_log.User != _PersonIdentifier)
                {
                    throw new Exception("Person identifier missmatch.");
                }

                int _EventID = 0;
                int _EventVisitCounter = 0;

                Dictionary<string, int> _elementVisitCounterDict = new Dictionary<string, int>();
                string _currentElement = "";

                foreach (var p in _sortedItemGroupList)
                {
                    string _Element = p.code;

                    if (_currentElement != _Element)
                    {
                        _EventVisitCounter = 0;
                        _currentElement = _Element;
                        if (!_elementVisitCounterDict.ContainsKey(_Element))
                            _elementVisitCounterDict.Add(_Element, 0);
                        else
                            _elementVisitCounterDict[_Element] += 1;
                    }

                    DateTime _ElementStart = dt1970.AddMilliseconds(p.epoch);
                    if (_PreviousEvent == DateTime.MaxValue)
                        _PreviousEvent = _ElementStart;

                    foreach (var _event in p.userEvents)
                    {
                        string _LogEventName = _event.type;

                        DateTime _AbsoluteTime = dt1970.AddMilliseconds(_event.epoch);
                        if (_AbsoluteTime < _MinAbsoluteTime)
                            _MinAbsoluteTime = _AbsoluteTime;

                        Dictionary<string, string> _EventValues = new Dictionary<string, string>();
                        for (int i = 0; i < _event.ItemsElementName.Length; i++)
                        {
                            if (_event.ItemsElementName[i].ToString() == "context")
                                _EventValues.Add("Context", _event.Items[i]);
                            else if (_event.ItemsElementName[i].ToString() == "value")
                                _EventValues.Add("Value", _event.Items[i]);
                            else if (_event.ItemsElementName[i].ToString() == "id")
                                _EventValues.Add("Id", _event.Items[i]);
                            else
                            {
                                throw new Exception("Element name not expected.");
                            }
                        }

                        _EventValues.Add("Element", _Element);
                        _EventValues.Add("RelativeTimeFrame", (_AbsoluteTime - _ElementStart).TotalMilliseconds.ToString());
                        _EventValues.Add("RelativeTimePrevious", (_AbsoluteTime - _PreviousEvent).TotalMilliseconds.ToString());

                        if (Element.Length == 0 || Element.Contains<string>(_Element))
                        {
                            _inMemoryTempDataEvents.Add(
                                new EventData()
                                {
                                    EventName = _LogEventName,
                                    PersonIdentifier = _PersonIdentifier,
                                    TimeStamp = _AbsoluteTime, 
                                    TimeDifferencePrevious = (_AbsoluteTime - _PreviousEvent),
                                    EventValues = _EventValues
                                });
                        }

                        _EventID += 1;
                        _EventVisitCounter += 1;
                        _PreviousEvent = _AbsoluteTime;
                    }

                }

                // check for suspicious times

                _currentElement = "";
                List<string> framesWithSuspiciousData = new List<string>();
                foreach (var v in _inMemoryTempDataEvents)
                {
                    if (_currentElement != v.EventName)
                        _currentElement = v.EventName;

                    if (v.TimeDifferencePrevious.TotalMinutes > 30)
                        v.AddEventValue("Flag", "TimeToLong"); 

                    if (v.TimeDifferencePrevious.TotalMilliseconds < 0)
                        v.AddEventValue("Flag", "TimeNegative");
                }
            }
        }

        #endregion
         
        private static string CleanInvalidXmlChars(string text)
        {
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(text, re, "");
        }
          
    }
}
