#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LogFSM_LogX2019;
using LogFSMConsole;
using StataLib;
#endregion

namespace LogDataTransformer_NEPS_V01
{
    public class LogDataTransformer_NEPS_Module_V01
    {
        public static void ProcessLogFilesOnly(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {
            try
            {
                /*
                bool _personIdentifierIsNumber = false;
                if (ParsedCommandLineArguments.Flags.Contains("NUMERICPERSONIDENTIFIER"))
                    _personIdentifierIsNumber = true;
                */

                string _personIdentifier = "lfd";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("personidentifier"))
                    _personIdentifier = ParsedCommandLineArguments.ParameterDictionary["personidentifier"];

                string _language = "ENG";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("language"))
                    _language = ParsedCommandLineArguments.ParameterDictionary["language"];

                List<string> _listOfFiles = new List<string>();
                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    if (!Directory.Exists(inFolder))
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Warning: Directory not exists: '" + inFolder + "'.");

                        continue;
                    }

                    var _tmpFileList = Directory.GetFiles(inFolder, "*_log.dta", SearchOption.AllDirectories);
                    foreach (string s in _tmpFileList)
                    {
                        if (!s.Contains("tbatools"))
                            _listOfFiles.Add(s);

                        if (_listOfFiles.Count > ParsedCommandLineArguments.MaxNumberOfCases && ParsedCommandLineArguments.MaxNumberOfCases != -1)
                            break;
                    }
                    if (_listOfFiles.Count > ParsedCommandLineArguments.MaxNumberOfCases && ParsedCommandLineArguments.MaxNumberOfCases != -1)
                        break;
                }
                 
                logXContainer _ret = CreateGenericLogContainer(_listOfFiles, _personIdentifier, true, ParsedCommandLineArguments.ExcludedElements);
                _ret.LoadCodebookDictionary(ParsedCommandLineArguments.Transform_Dictionary);

                // TODO: Check!
                //_ret.UpdateRelativeTimes();
                _ret.CreateLookup();

                if (ParsedCommandLineArguments.Transform_OutputStata.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create ZIP archive with Stata file(s).");

                    _ret.ExportStata(ParsedCommandLineArguments.Transform_OutputStata, _language);
                }

                if (ParsedCommandLineArguments.Transform_OutputXLSX.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create XLSX file.");

                    _ret.ExportXLSX(ParsedCommandLineArguments);

                }

                if (ParsedCommandLineArguments.Transform_OutputZCSV.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create ZIP archive with CSV file(s).");

                    _ret.ExportCSV(ParsedCommandLineArguments);
                }

                if (ParsedCommandLineArguments.Transform_Codebook.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create Codebook File.");

                    _ret.CreateCodebook(ParsedCommandLineArguments.Transform_Codebook, _language);
                }

                if (_ret.ExportErrors.Count > 0)
                {
                    Console.WriteLine(_ret.ExportErrors.Count + " error(s) creating output files.");
                    if (ParsedCommandLineArguments.Verbose)
                    {
                        for (int i = 0; i < _ret.ExportErrors.Count; i++)
                        {
                            Console.WriteLine(_ret.ExportErrors[i]);
                        }
                    }
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.Message.ToString());
            }
        }

        public static logXContainer CreateGenericLogContainer(List<string> StataFiles, string PersonIdentifierName, bool PersonIdentifierIsNumber, string[] ExcludedElements)
        {

            logXContainer _ret = new logXContainer() { PersonIdentifierName = PersonIdentifierName, PersonIdentifierIsNumber = PersonIdentifierIsNumber };
            try
            {
                int _progressCounter = 0;

                foreach (string StataLogInFileName in StataFiles)
                {
                    Console.WriteLine(StataLogInFileName + " - " + _progressCounter + " / " + StataFiles.Count());
                    StataFileReader _stataLogFileReader = new StataFileReader(StataLogInFileName, true);

                    // cache stataVariables and stataValueLabels for performance reasons

                    var _stataVariables = _stataLogFileReader.Variables;
                    var _stataValueLabes = _stataLogFileReader.ValueLabels;

                    int _personIdentifierIndex = GetVariableIndex(_ret.PersonIdentifierName, _stataVariables);
                    int _absolutTimeIndex = GetVariableIndex("AbsoluteTime", _stataVariables);
                    int _lineCounterIndex = GetVariableIndex("LineCounter", _stataVariables);
                    int _logTypeIndex = GetVariableIndex("LogType", _stataVariables);
                    int _logCounterIndex = GetVariableIndex("LogCounter", _stataVariables);
                    int _relativeTimeIndex = GetVariableIndex("RelativeTime", _stataVariables);
                    int _absoluteTimeIndex = GetVariableIndex("AbsoluteTime", _stataVariables);
                    int _elementIndex = GetVariableIndex("Element", _stataVariables);
                    int _varNameIndex = GetVariableIndex("VariableName", _stataVariables);
                    int _varValueIndex = GetVariableIndex("VariableValue", _stataVariables);
                    int _varValueLabelIndex = GetVariableIndex("ValueLabel", _stataVariables);
                    int _xmlIndex = GetVariableIndex("XML", _stataVariables);

                    // find first RealTime for each TestTaker

                    Dictionary<string, DateTime> _firstLoginForEachPerson = new Dictionary<string, DateTime>();
                    foreach (var _line in _stataLogFileReader)
                    {
                        string _personIdentifier = GetStringValue(_personIdentifierIndex, _line, _stataVariables, _stataValueLabes);
                        DateTime _absolutetime = GetDateTimeValue(_absolutTimeIndex, _line, _stataVariables, _stataValueLabes);

                        if (!_firstLoginForEachPerson.ContainsKey(_personIdentifier))
                            _firstLoginForEachPerson.Add(_personIdentifier, DateTime.MaxValue);

                        if (_absolutetime.Year != 1)
                        {
                            if (_firstLoginForEachPerson[_personIdentifier] > _absolutetime)
                                _firstLoginForEachPerson[_personIdentifier] = _absolutetime;
                        }
                    }

                    // extract log data to genereric log element

                    foreach (var _line in _stataLogFileReader)
                    {
                        string _personIdentifier = GetStringValue(_personIdentifierIndex, _line, _stataVariables, _stataValueLabes);
                        int _linecounter = GetIntValue(_lineCounterIndex, _line, _stataVariables);
                        string _logType = GetStringValue(_logTypeIndex, _line, _stataVariables, _stataValueLabes);
                        int _logcounter = GetIntValue(_logCounterIndex, _line, _stataVariables);
                        long _relativetime = GetLogValue(_relativeTimeIndex, _line, _stataVariables);

                        DateTime _absolutetime = GetDateTimeValue(_absolutTimeIndex, _line, _stataVariables, _stataValueLabes);
                        if (_absolutetime.Year == 1)
                            _absolutetime = _firstLoginForEachPerson[_personIdentifier].AddMilliseconds(-1 * _relativetime);

                        string _element = GetStringValue(_elementIndex, _line, _stataVariables, _stataValueLabes);
                        string _varname = GetStringValue(_varNameIndex, _line, _stataVariables, _stataValueLabes);
                        string _value = GetStringValue(_varValueIndex, _line, _stataVariables, _stataValueLabes);
                        string _valueLabel = GetStringValue(_varValueLabelIndex, _line, _stataVariables, _stataValueLabes);
                        string _xml = GetStringValue(_xmlIndex, _line, _stataVariables, _stataValueLabes);

                        #region process log events

                        if (!ExcludedElements.Contains<string>(_element))
                        {
                            if (_logType == "TTLogVariableChanged")
                            {
                                _ret.AddEvent(new logxGenericLogElement()
                                {
                                    EventID = _logcounter,
                                    Item = _element,
                                    EventName = typeof(TBAToolsVariableChanged).Name,
                                    EventDataXML = TBAToolsLogReader.XmlSerializeToString(new TBAToolsVariableChanged() { Sender = _element, Variable = _varname, Value = _value, ValueLabel = _valueLabel }),
                                    PersonIdentifier = _personIdentifier,
                                    TimeStamp = _absolutetime,
                                    RelativeTime = _relativetime
                                });
                            }
                            else if (_logType == "TTLogRealTime")
                            {
                                _ret.AddEvent(new logxGenericLogElement()
                                {
                                    EventID = _logcounter,
                                    Item = _element,
                                    EventName = typeof(TBAToolsRealTime).Name,
                                    EventDataXML = TBAToolsLogReader.XmlSerializeToString(new TBAToolsRealTime() { RealTime = _absolutetime }),
                                    PersonIdentifier = _personIdentifier,
                                    TimeStamp = _absolutetime,
                                    RelativeTime = _relativetime
                                });
                            }
                            else if (_logType == "TTLogLoading")
                            {
                                _ret.AddEvent(new logxGenericLogElement()
                                {
                                    EventID = _logcounter,
                                    Item = _element,
                                    EventName = typeof(TBAToolsLoading).Name,
                                    EventDataXML = TBAToolsLogReader.XmlSerializeToString(new TBAToolsLoading() { Sender = _element }),
                                    PersonIdentifier = _personIdentifier,
                                    TimeStamp = _absolutetime,
                                    RelativeTime = _relativetime
                                });
                            }
                            else if (_logType == "TTLogLoaded")
                            {
                                _ret.AddEvent(new logxGenericLogElement()
                                {
                                    EventID = _logcounter,
                                    Item = _element,
                                    EventName = typeof(TBAToolsLoaded).Name,
                                    EventDataXML = TBAToolsLogReader.XmlSerializeToString(new TBAToolsLoaded() { Sender = _element }),
                                    PersonIdentifier = _personIdentifier,
                                    TimeStamp = _absolutetime,
                                    RelativeTime = _relativetime
                                });
                            }
                            else if (_logType == "TTLogUnloaded")
                            {
                                _ret.AddEvent(new logxGenericLogElement()
                                {
                                    EventID = _logcounter,
                                    Item = _element,
                                    EventName = typeof(TBAToolsUnloaded).Name,
                                    EventDataXML = TBAToolsLogReader.XmlSerializeToString(new TBAToolsUnloaded() { Sender = _element }),
                                    PersonIdentifier = _personIdentifier,
                                    TimeStamp = _absolutetime,
                                    RelativeTime = _relativetime
                                });
                            }
                            else if (_logType == "TTLogUnloading")
                            {
                                _ret.AddEvent(new logxGenericLogElement()
                                {
                                    EventID = _logcounter,
                                    Item = _element,
                                    EventName = typeof(TBAToolsUnloading).Name,
                                    EventDataXML = TBAToolsLogReader.XmlSerializeToString(new TBAToolsUnloading() { Sender = _element }),
                                    PersonIdentifier = _personIdentifier,
                                    TimeStamp = _absolutetime,
                                    RelativeTime = _relativetime
                                });
                            }
                            else if (_logType == "TTLogIBStopTask")
                            {
                                _ret.AddEvent(new logxGenericLogElement()
                                {
                                    EventID = _logcounter,
                                    Item = _element,
                                    EventName = typeof(TBAToolsIBStopTask).Name,
                                    EventDataXML = TBAToolsLogReader.XmlSerializeToString(new TBAToolsIBStopTask() { Sender = _element }),
                                    PersonIdentifier = _personIdentifier,
                                    TimeStamp = _absolutetime,
                                    RelativeTime = _relativetime
                                });
                            }
                            else
                            {
                                _ret.AddEvent(new logxGenericLogElement()
                                {
                                    EventID = _logcounter,
                                    EventName = _logType,
                                    Item = _element,
                                    PersonIdentifier = _personIdentifier,
                                    EventDataXML = _xml,
                                    TimeStamp = _absolutetime,
                                    RelativeTime = _relativetime
                                });
                            }
                        }
                        #endregion

                    }
                    _stataLogFileReader.Close();
                    _progressCounter++;
                }


            }
            catch (Exception _ex)
            {
                Console.WriteLine(_ex.ToString());
                throw new Exception();
            }
            return _ret;
        }

        public static int GetVariableIndex(string Variable, List<StataVariable> StataVariables)
        {
            return StataVariables.FindIndex(var => var.Name == Variable);
        }


        public static string GetStringValue(string Variable, object[] Line, List<StataVariable> StataVariables, List<Tuple<string, Int32, string>> StataValueLabels)
        {
            int _index = GetVariableIndex(Variable, StataVariables);
            if (_index != -1)
                return GetStringValue(_index, Line, StataVariables, StataValueLabels);
            else
                return "";
        }

        public static string GetStringValue(int Index, object[] Line, List<StataVariable> StataVariables, List<Tuple<string, Int32, string>> StataValueLabels)
        {
            if (Index == -1 || Index > StataVariables.Count)
            {
                throw new Exception("Can't getStringValue ");
            }
            if (StataVariables[Index].ValueLabelName != "")
            {

                int _intValue = -1;
                int.TryParse(Line[Index].ToString(), out _intValue);
                if (_intValue != -1)
                {
                    var _value = StataValueLabels.Where(r => r.Item1 == StataVariables[Index].ValueLabelName & r.Item2 == _intValue).Select(t => t.Item3).FirstOrDefault();
                    if (_value != null)
                        return _value;
                    else
                        return _intValue.ToString();
                }
            }
            return Line[Index].ToString().ToString();
        }

        public static DateTime GetDateTimeValue(string Variable, object[] Line, List<StataVariable> StataVariables, List<Tuple<string, Int32, string>> StataValueLabels)
        {
            return GetDateTimeValue(GetVariableIndex(Variable, StataVariables), Line, StataVariables, StataValueLabels);
        }

        public static DateTime GetDateTimeValue(int Index, object[] Line, List<StataVariable> StataVariables, List<Tuple<string, Int32, string>> StataValueLabels)
        {
            return DateTime.Parse(GetStringValue(Index, Line, StataVariables, StataValueLabels));
        }

        public static int GetIntValue(string Variable, object[] Line, List<StataVariable> StataVariables)
        {
            return GetIntValue(GetVariableIndex(Variable, StataVariables), Line, StataVariables);
        }

        public static int GetIntValue(int Index, object[] Line, List<StataVariable> StataVariables)
        {
            int _intValue = -1;
            if (int.TryParse(Line[Index].ToString(), out _intValue))
                return _intValue;
            else
                throw new Exception("Variable not int!");
        }

        public static long GetLogValue(string Variable, object[] Line, List<StataVariable> StataVariables)
        {
            return GetIntValue(GetVariableIndex(Variable, StataVariables), Line, StataVariables);
        }

        public static long GetLogValue(int Index, object[] Line, List<StataVariable> StataVariables)
        {
            long _longValue = -1;
            if (long.TryParse(Line[Index].ToString(), out _longValue))
                return _longValue;
            else
                throw new Exception("Variable not int!");
        }


        public static double GetDoubleValue(string Variable, object[] Line, List<StataVariable> StataVariables)
        {
            return GetDoubleValue(GetVariableIndex(Variable, StataVariables), Line, StataVariables);
        }

        public static double GetDoubleValue(int Index, object[] Line, List<StataVariable> StataVariables)
        {
            double _doublValue = -1;
            if (double.TryParse(Line[Index].ToString(), out _doublValue))
                return _doublValue;
            else
                throw new Exception("Variable not double!");
        }
    }
}
