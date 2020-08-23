namespace LogDataTransformer_PISA_BQ_V01
{
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
    #endregion

    public class LogDataTransformer_PISABQ_Module_V01
    {
        public static void ProcessLogFilesOnly(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {
            try
            {

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
                XmlSerializer logSerializer = new XmlSerializer(typeof(log));

                #region Search Source Files

                List<string> _listOfXMLFiles = new List<string>();
                List<string> _listOfZIPArchivesWithXMLFiles = new List<string>();

                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    // Input is file

                    if (File.Exists(inFolder))
                    {
                        if (inFolder.ToLower().EndsWith(".zip"))
                        {
                            // Single ZIP file 

                            _listOfZIPArchivesWithXMLFiles.Add(inFolder);
                        }
                        else if (inFolder.ToLower().EndsWith(".xml"))
                        {
                            // Single XML file 

                            _listOfXMLFiles.Add(inFolder);
                        }
                    }
                    else
                    {
                        if (!Directory.Exists(inFolder))
                        {
                            if (ParsedCommandLineArguments.Verbose)
                                Console.WriteLine("Warning: Directory not exists: '" + inFolder + "'.");

                            continue;
                        }

                        var _tmpXMLFileList = Directory.GetFiles(inFolder, "*.xml", SearchOption.AllDirectories);

                        foreach (string s in _tmpXMLFileList)
                            _listOfXMLFiles.Add(s);

                        var _tmpZIPFileList = Directory.GetFiles(inFolder, "*.zip", SearchOption.AllDirectories);

                        foreach (string s in _tmpZIPFileList)
                            _listOfZIPArchivesWithXMLFiles.Add(s);
                    }

                }

                #endregion

                logXContainer _ret = new logXContainer()
                {
                    PersonIdentifierIsNumber = _personIdentifierIsNumber,
                    PersonIdentifierName = _personIdentifier
                };

                _ret.LoadCodebookDictionary(ParsedCommandLineArguments.Transform_Dictionary);

                foreach (string xfilename in _listOfXMLFiles)
                {
                    if (xfilename.EndsWith("-log.xml"))
                    { 
                        StreamReader _sr = new StreamReader(xfilename);
                        var _xml = CleanInvalidXmlChars(_sr.ReadToEnd());

                        string _PersonIdentifier = Path.GetFileName(xfilename).Replace("-log.xml", "");

                        processPISA_BQ_single_XML(ParsedCommandLineArguments.Elements, dt1970, _ret,
                            logSerializer, _PersonIdentifier, _xml);

                        _sr.Close();
                    }
                }

                foreach (string zfilename in _listOfZIPArchivesWithXMLFiles)
                {
                    try
                    {
                        using (var outerInputZipFile = ZipFile.Read(zfilename))
                        {
                            foreach (var outerInputZipEntry in outerInputZipFile.Entries)
                            {
                                if (outerInputZipEntry.FileName.EndsWith("-log.xml") && outerInputZipEntry.UncompressedSize != 0)
                                {
                                    #region Single XML file
                                    string _PersonIdentifier = Path.GetFileName(outerInputZipEntry.FileName).Replace("-log.xml", "");

                                    if (ParsedCommandLineArguments.Verbose)
                                        Console.WriteLine(_PersonIdentifier + " -- " + _ret.GetNumberOfPersons);

                                    using (MemoryStream innerZIPEntryMemoryStream = new MemoryStream())
                                    {
                                        outerInputZipEntry.Password = ParsedCommandLineArguments.ZIPPassword;
                                        outerInputZipEntry.Extract(innerZIPEntryMemoryStream);
                                        innerZIPEntryMemoryStream.Position = 0;

                                        var _sr = new StreamReader(innerZIPEntryMemoryStream);
                                        var _xml = CleanInvalidXmlChars(_sr.ReadToEnd());

                                        processPISA_BQ_single_XML(ParsedCommandLineArguments.Elements, dt1970, _ret,
                                            logSerializer, _PersonIdentifier, _xml);

                                        _sr.Close();

                                    }
                                    #endregion
                                }
                                else if (outerInputZipEntry.FileName.EndsWith("Session2.zip") && outerInputZipEntry.UncompressedSize != 0)
                                {
                                    #region ZIP archive with XML files  
                                    string _PersonIdentifier = Path.GetFileName(outerInputZipEntry.FileName).Replace("-Session2.zip", "");

                                    using (MemoryStream outerZIPEntryMemoryStream = new MemoryStream())
                                    {
                                        if (ParsedCommandLineArguments.MaxNumberOfCases > 0 &&
                                            _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                                        {
                                            break;
                                        }

                                        if (ParsedCommandLineArguments.Verbose)
                                            Console.WriteLine(_PersonIdentifier + " -- " + _ret.GetNumberOfPersons);

                                        outerInputZipEntry.Password = ParsedCommandLineArguments.ZIPPassword;
                                        outerInputZipEntry.Extract(outerZIPEntryMemoryStream);
                                        outerZIPEntryMemoryStream.Position = 0;

                                        using (var innerZIP = ZipFile.Read(outerZIPEntryMemoryStream))
                                        {
                                            foreach (var innerZIPEntry in innerZIP.Entries)
                                            {
                                                if (innerZIPEntry.FileName.EndsWith("_Data.zip") && innerZIPEntry.UncompressedSize != 0)
                                                {
                                                    if (ParsedCommandLineArguments.Verbose)
                                                        Console.WriteLine(innerZIPEntry.FileName);

                                                    using (MemoryStream innerZIPEntryMemoryStream = new MemoryStream())
                                                    {
                                                        innerZIPEntry.Password = ParsedCommandLineArguments.ZIPPassword;
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
                                                                        //inner2ZIPEntry.Password = ParsedCommandLineArguments.ZIPPassword;
                                                                        inner2ZIPEntry.Extract(inner2ZIPEntryMemoryStream);
                                                                        inner2ZIPEntryMemoryStream.Position = 0;

                                                                        var _sr = new StreamReader(inner2ZIPEntryMemoryStream);
                                                                        var _xml = CleanInvalidXmlChars(_sr.ReadToEnd());

                                                                        processPISA_BQ_single_XML(ParsedCommandLineArguments.Elements, dt1970, _ret,
                                                                                                   logSerializer, _PersonIdentifier, _xml);
                                                                    }
                                                                }
                                                            }
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion
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

                #region Export Universal Log Format
                 
                _ret.UpdateRelativeTimes();
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

                if (ParsedCommandLineArguments.Transform_ConcordanceTable.Trim() != "")
                {
                    if (!File.Exists(ParsedCommandLineArguments.Transform_ConcordanceTable))
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Create Concordance Table.");

                        _ret.CreateConcordanceTable(ParsedCommandLineArguments.Transform_ConcordanceTable);
                    }
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

                #endregion
            }

            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.Message.ToString());
            }
        }

        private static void processPISA_BQ_single_XML(string[] Element, DateTime dt1970, logXContainer _ret, XmlSerializer logSerializer, string _PersonIdentifier, string _xml)
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
                         
                        _EventValues.Add("RelativeTimeFrame", (_AbsoluteTime - _ElementStart).TotalMilliseconds.ToString());
                        _EventValues.Add("RelativeTimePrevious", (_AbsoluteTime - _PreviousEvent).TotalMilliseconds.ToString());

                        if (Element.Length == 0 || Element.Contains<string>(_Element))
                        {
                            var doc = new XDocument(new XElement(_LogEventName));
                            var root = doc.Root;
                            foreach (string val in _EventValues.Keys)
                                root.Add(new XAttribute(val, _EventValues[val]));

                            logxGenericLogElement _parament = new logxGenericLogElement()
                            {
                                PersonIdentifier = _PersonIdentifier,
                                Item = _Element,
                                EventID = _EventID,
                                EventName = _LogEventName,
                                TimeStamp = _AbsoluteTime,
                                EventDataXML = doc.ToString()
                            };

                            _ret.AddEvent(_parament);
                        }

                        _EventID += 1;
                        _EventVisitCounter += 1;
                        _PreviousEvent = _AbsoluteTime;
                    }

                }

                // check for suspicious times
                /* TODO
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
                */
            }

            _tr.Close();
        }

        private static string CleanInvalidXmlChars(string text)
        {
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(text, re, "");
        }

    }
}
