#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using LogFSM;
using LogFSM_LogX2019;
using LogFSMConsole;
using System.Xml;
using System.Xml.Linq;
using StataLib;
using Ionic.Zip;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using NPOI.SS.Formula.Functions;
using OpenXesNet.model;
#endregion

namespace LogDataTransformer_PISA22BQ_Module_V01
{
    public class LogDataTransformer_PISA22BQ_Module_V01
    {
        public static void ProcessLogFilesOnly(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {
            try
            { 
                EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.Time;
                if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                    sort = EventDataListExtension.ESortType.None;
                if (ParsedCommandLineArguments.Flags.Contains("ORDER_WITHIN_ELEMENTS"))
                    sort = EventDataListExtension.ESortType.ElementAndTime;

                bool _relativeTimesAreSeconds = false;
                if (ParsedCommandLineArguments.Flags.Contains("RELATIVETIMESARESECONDS"))
                    _relativeTimesAreSeconds = true;

                bool _personIdentifierIsNumber = false;
                if (ParsedCommandLineArguments.Flags.Contains("NUMERICPERSONIDENTIFIER"))
                    _personIdentifierIsNumber = true;

                string _personIdentifier = "PersonIdentifier";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("personidentifier"))
                    _personIdentifier = ParsedCommandLineArguments.ParameterDictionary["personidentifier"];

                string _language = "ENG";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("language"))
                    _language = ParsedCommandLineArguments.ParameterDictionary["language"];
                 
                DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                XmlSerializer logSerializer = new XmlSerializer(typeof(log_pisa_bq_2022));
                 
                logXContainer _ret = new logXContainer() { PersonIdentifierIsNumber = _personIdentifierIsNumber, PersonIdentifierName = _personIdentifier, RelativeTimesAreSeconds = _relativeTimesAreSeconds };

                _ret.LoadCodebookDictionary(ParsedCommandLineArguments.Transform_Dictionary);

                if (ParsedCommandLineArguments.Transform_ConcordanceTable.Trim() != "")
                {
                    if (File.Exists(ParsedCommandLineArguments.Transform_ConcordanceTable))
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.Write("Read Concordance Table... ");

                        _ret.ReadConcordanceTable(ParsedCommandLineArguments.Transform_ConcordanceTable, ParsedCommandLineArguments.Verbose);

                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Found " + _ret.CondordanceTable.Count + " lines.");
                    }
                }


                #region Search Source Files
                 
                List<string> _listOfZIPArchivesWithZipFiles = new List<string>();

                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    // Input is file

                    if (File.Exists(inFolder))
                    {
                        if (inFolder.ToLower().EndsWith(".zip"))
                            _listOfZIPArchivesWithZipFiles.Add(inFolder);
                    }
                    else
                    {
                        if (!Directory.Exists(inFolder))
                        {
                            if (ParsedCommandLineArguments.Verbose)
                                Console.WriteLine("Warning: Directory not exists: '" + inFolder + "'.");

                            continue;
                        }  
                        var _tmpZIPFileList = Directory.GetFiles(inFolder, "*.zip", SearchOption.AllDirectories);

                        foreach (string s in _tmpZIPFileList)
                            _listOfZIPArchivesWithZipFiles.Add(s);
                    }

                }

                #endregion
                 
                int numberOfPersons = 0;

                foreach (string zfilename in _listOfZIPArchivesWithZipFiles)
                {
                    try
                    { 
                        using (var outerInputZipFile = ZipFile.Read(zfilename))
                        {
                            var totalFiles = outerInputZipFile.Entries.Count(x => x.FileName.EndsWith("Session2.zip"));

                            foreach (var outerInputZipEntry in outerInputZipFile.Entries)
                            {
                                if (outerInputZipEntry.FileName.EndsWith("Session2.zip") && outerInputZipEntry.UncompressedSize != 0)
                                {
                                    string _PersonIdentifier = Path.GetFileName(outerInputZipEntry.FileName).Replace("-Session2.zip", "");
                                    using (MemoryStream outerZIPEntryMemoryStream = new MemoryStream())
                                    {

                                        outerInputZipEntry.Password = ParsedCommandLineArguments.ZIPPassword;
                                        outerInputZipEntry.Extract(outerZIPEntryMemoryStream);
                                        outerZIPEntryMemoryStream.Position = 0;
                                        
                                        using (var innerZIP = ZipFile.Read(outerZIPEntryMemoryStream))
                                        {
                                            foreach (var innerZIPEntry in innerZIP.Entries)
                                            { 
                                                if (innerZIPEntry.FileName.EndsWith("-log.xml") && outerInputZipEntry.UncompressedSize != 0)
                                                {

                                                    if (ParsedCommandLineArguments.Verbose)
                                                    {
                                                        int percent = (int)Math.Round((double)numberOfPersons / (double)totalFiles * 100, 0);
                                                        Console.WriteLine(percent + "% processed, " + numberOfPersons + " persons found.");
                                                    }

                                                    string _checkPersonIdentifier = Path.GetFileName(outerInputZipEntry.FileName).Replace("-Session2.zip", "");
                                                    if (_checkPersonIdentifier != _PersonIdentifier)
                                                    {
                                                        throw new Exception("Person identifier miss-match.");
                                                    }

                                                    using (MemoryStream innerZIPEntryMemoryStream = new MemoryStream())
                                                    {
                                                        innerZIPEntry.Password = ParsedCommandLineArguments.ZIPPassword;
                                                        innerZIPEntry.Extract(innerZIPEntryMemoryStream);
                                                        innerZIPEntryMemoryStream.Position = 0;

                                                        var _sr = new StreamReader(innerZIPEntryMemoryStream);
                                                        var _xml = CleanInvalidXmlChars(_sr.ReadToEnd());

                                                        processPISA_BQ_single_XML(ParsedCommandLineArguments.Elements, dt1970, _ret,
                                                            logSerializer, _PersonIdentifier, _xml);

                                                        _sr.Close();

                                                    }

                                                    if (ParsedCommandLineArguments.MaxNumberOfCases >= 1 && numberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                                                        break;

                                                    numberOfPersons += 1;
                                                }
                                            }
                                        }
                                    }
                                } 
                            }
                        }
                    }
                    catch (Exception _ex)
                    {
                        _ret.ExportErrors.Add("Error reading file '" + zfilename + "': " + _ex.Message);
                    }
                }

                if (ParsedCommandLineArguments.MaxNumberOfCases > 0 &&
                   _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Info: Max number of cases reached.");
                }

                logXContainer.ExportLogXContainerData(ParsedCommandLineArguments, _ret);
            }

            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.Message.ToString());
            }
        }

        private static void processPISA_BQ_single_XML(string[] Element, DateTime dt1970, logXContainer _ret, XmlSerializer logSerializer, string _PersonIdentifier, string _xml)
        {  
            var _tr = new StringReader(_xml);            
            log_pisa_bq_2022 _log = (log_pisa_bq_2022)logSerializer.Deserialize(_tr);           
            int _numberOFEventsForXML = 0;

            if (_log.ItemGroup != null)
            { 
                // update epoch information
                foreach (var i in _log.ItemGroup)
                {
                    if (!i.epochSpecified && i.userEvents.Length > 0)
                        i.epoch = i.userEvents[0].epoch;
                }
                
                List<log_pisa_bq_2022_itemGroup> _sortedItemGroupList = _log.ItemGroup.OrderBy(o => o.epoch).ToList();
                 
                DateTime _MinAbsoluteTime = DateTime.MaxValue;
                DateTime _PreviousEvent = DateTime.MaxValue;
                if (_log.User != _PersonIdentifier)
                {
                    throw new Exception("Person identifier miss-match.");
                } 
                int _EventID = _ret.GetMaxID(_PersonIdentifier);
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
                            if (_event.Items[i] != "")
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
                        }
                        

                        // TODO: Generalize!
                        /*
                        if (_EventValues.ContainsKey("Context") && _EventValues.ContainsKey("Value"))
                        {
                            if ( _EventValues["Context"] == "ST114Q01TA01" && _EventValues["Value"] != "null")
                            {
                                _EventValues["Value"] = MaskString(_EventValues["Value"]);
                            }
                        }
                          */

                        _EventValues.Add("RelativeTimeFrame", (_AbsoluteTime - _ElementStart).TotalMilliseconds.ToString());
                        _EventValues.Add("RelativeTimePrevious", (_AbsoluteTime - _PreviousEvent).TotalMilliseconds.ToString());

                        if (Element.Length == 0 || Element.Contains<string>(_Element))
                        {
                            var doc = new XDocument(new XElement(_LogEventName));
                            var root = doc.Root;
                            foreach (string val in _EventValues.Keys)
                                root.Add(new System.Xml.Linq.XAttribute(val, _EventValues[val]));

                            logxGenericLogElement _newElement = new logxGenericLogElement()
                            {
                                PersonIdentifier = _PersonIdentifier,
                                Item = _Element,
                                EventID = _EventID,
                                EventName = _LogEventName,
                                TimeStamp = _AbsoluteTime,
                                EventDataXML = doc.ToString()
                            };

                            _ret.AddEvent(_newElement);
                        }

                        _EventID += 1;
                        _EventVisitCounter += 1;
                        _PreviousEvent = _AbsoluteTime;

                        _numberOFEventsForXML += 1;
                    }

                } 
            }
            
           
            _tr.Close();
        }
 
        private static string CleanInvalidXmlChars(string text)
        {
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(text, re, "");
        }

        private static string MaskString(string text)
        {
            return string.Concat(Enumerable.Repeat("x", text.Length));
        }

    }
}
