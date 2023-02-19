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
using LogFSMShared;
using Newtonsoft.Json;
using OpenXesNet.model;
using static NPOI.POIFS.Crypt.CryptoFunctions;
using HtmlAgilityPack;
#endregion

namespace LogDataTransformer_PISA_CA_V01
{
    public class LogDataTransformer_PISACA_Module_V01
    {
        public static void ProcessLogFilesOnly(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {
            try
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
                XmlSerializer logSerializer = new XmlSerializer(typeof(log));

                #region Search Source Files
 
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
                            _listOfZIPArchivesWithXMLFiles.Add(s);
                    }

                }

                #endregion

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


                int numberOfPersons = 0;
                foreach (string zfilename in _listOfZIPArchivesWithXMLFiles)
                {
                    try
                    {
                        using (var outerInputZipFile = ZipFile.Read(zfilename))
                        {
                            var totalFiles = outerInputZipFile.Entries.Count(x => x.FileName.EndsWith("Session1.zip"));
                            var currentFile = 0;
                            int percent = 0;

                            foreach (var outerInputZipEntry in outerInputZipFile.Entries)
                            {
                                if (outerInputZipEntry.FileName.EndsWith("Session1.zip") && outerInputZipEntry.UncompressedSize != 0)
                                {
                                    string _PersonIdentifier = Path.GetFileName(outerInputZipEntry.FileName).Replace("-Session1.zip", "");
                                    using (MemoryStream outerZIPEntryMemoryStream = new MemoryStream())
                                    {


                                        if (ParsedCommandLineArguments.Verbose)
                                        {
                                            if ((int)Math.Round((double)numberOfPersons / (double)totalFiles * 100, 0) > percent + 9)
                                            {
                                                percent = (int)Math.Round((double)numberOfPersons / (double)totalFiles * 100, 0);
                                                Console.WriteLine(percent + "% processed, " + numberOfPersons + " persons found.");
                                            }
                                        }

                                        outerInputZipEntry.Password = ParsedCommandLineArguments.ZIPPassword;  
                                        outerInputZipEntry.Extract(outerZIPEntryMemoryStream);
                                        outerZIPEntryMemoryStream.Position = 0;
                                        numberOfPersons += 1;
                                         
                                        using (var innerZIP = ZipFile.Read(outerZIPEntryMemoryStream))
                                        {
                                            foreach (var innerZIPEntry in innerZIP.Entries)
                                            {
                                                if (innerZIPEntry.FileName.StartsWith("trace/") && innerZIPEntry.FileName.EndsWith(".xml") && innerZIPEntry.UncompressedSize != 0)
                                                {
                                                    using (MemoryStream innerZIPEntryMemoryStream = new MemoryStream())
                                                    {
                                                        innerZIPEntry.Password = ParsedCommandLineArguments.ZIPPassword;
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
                                                                        string _currentItemName = _EventValues["itemId"];

                                                                        bool _include = true;
                                                                        if (ParsedCommandLineArguments.Elements.Length > 0)
                                                                        {
                                                                            if (!ParsedCommandLineArguments.Elements.Contains<string>(_currentElementName))
                                                                                _include = false;
                                                                        }

                                                                        if (ParsedCommandLineArguments.ExcludedElements.Length > 0)
                                                                        {
                                                                            if (ParsedCommandLineArguments.ExcludedElements.Contains<string>(_currentEventName))
                                                                                _include = false;
                                                                        }

                                                                        if (_include)
                                                                        {
                                                                            var doc = new XDocument(new XElement(_currentEventName));
                                                                            var root = doc.Root;
                                                                            foreach (string val in _EventValues.Keys)
                                                                                root.Add(new System.Xml.Linq.XAttribute(val, _EventValues[val]));

                                                                            int _EventID = _ret.GetMaxID(_PersonIdentifier);

                                                                            logxGenericLogElement _newElement = new logxGenericLogElement()
                                                                            {
                                                                                PersonIdentifier = _PersonIdentifier,
                                                                                Item = _currentItemName,
                                                                                EventID = _EventID,
                                                                                EventName = _currentEventName,
                                                                                TimeStamp = dt1970.AddMilliseconds(double.Parse(_EventValues["time"])),
                                                                                EventDataXML = doc.ToString()
                                                                            };

                                                                            _ret.AddEvent(_newElement);
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
                                                                    if (_currentElement == "")
                                                                    {
                                                                        throw new Exception("Value not expected here.");
                                                                    }
                                                                    else if (_currentElement.StartsWith("fteData"))
                                                                    {
                                                                        HtmlDocument hap = new HtmlDocument();
                                                                        hap.LoadHtml(xmlReader.Value);

                                                                        //<div class="mathTextArea"  ...
                                                                        HtmlNodeCollection nodes_mathTextArea = hap.DocumentNode.SelectNodes("//div[@class='mathTextArea']");
                                                                        if (nodes_mathTextArea != null)
                                                                            for (int i = 0; i < nodes_mathTextArea.Count; i++)
                                                                                _EventValues.Add(_currentElement + "_mathTextArea_" + i, nodes_mathTextArea[i].InnerText);

                                                                        //<div class="responseTextArea" 
                                                                        HtmlNodeCollection nodes_responseTextArea = hap.DocumentNode.SelectNodes("//div[@class='responseTextArea']");
                                                                        if (nodes_responseTextArea != null)
                                                                            for (int i = 0; i < nodes_responseTextArea.Count; i++)
                                                                                _EventValues.Add(_currentElement + "_responseTextArea_" + i, nodes_responseTextArea[i].InnerText);

                                                                        _currentValue = xmlReader.Value;

                                                                        // TODO: Make sure to find everything from fte-cdata-blocks

                                                                        // Consider masking free text responses
                                                                    }
                                                                    else
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
                                }

                                 
                                if (ParsedCommandLineArguments.MaxNumberOfCases >= 1 && numberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                                    break;
                                 
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

        private static string CleanInvalidXmlChars(string text)
        {
            string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(text, re, "");
        }
    }

}
