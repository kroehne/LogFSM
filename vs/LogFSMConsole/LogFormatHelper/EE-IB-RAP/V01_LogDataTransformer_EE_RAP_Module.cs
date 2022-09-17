#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Ionic.Zip;
using LogFSM_LogX2019;
using LogFSMConsole;
using StataLib;
#endregion

namespace LogDataTransformer_EE_RAP_V01
{
    public class LogDataTransformer_EE_RAP_Module_V01
    {
        public static void ProcessLogFilesOnly(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {
            try
            {
                bool _personIdentifierIsNumber = false;
                if (ParsedCommandLineArguments.Flags.Contains("NUMERICPERSONIDENTIFIER"))
                    _personIdentifierIsNumber = true;

                bool _relativeTimesAreSeconds = false;
                if (ParsedCommandLineArguments.Flags.Contains("RELATIVETIMESARESECONDS"))
                    _relativeTimesAreSeconds = true;

                string _personIdentifier = "PersonIdentifier";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("personidentifier"))
                    _personIdentifier = ParsedCommandLineArguments.ParameterDictionary["personidentifier"];

                string _language = "ENG";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("language"))
                    _language = ParsedCommandLineArguments.ParameterDictionary["language"];

                List<string> _listOfXMLFiles = new List<string>();
                List<string> _listOfZIPArchivesWithXMLFiles = new List<string>();

                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    if (File.Exists(inFolder))
                    {
                        if (inFolder.ToLower().EndsWith(".zip"))
                        {
                            _listOfZIPArchivesWithXMLFiles.Add(inFolder);
                        }
                        else if (inFolder.ToLower().EndsWith(".xml"))
                        {
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
                 
                logXContainer _ret = new logXContainer() { PersonIdentifierIsNumber = _personIdentifierIsNumber, PersonIdentifierName = _personIdentifier, RelativeTimesAreSeconds = _relativeTimesAreSeconds };
                _ret.LoadCodebookDictionary(ParsedCommandLineArguments.Transform_Dictionary);

                if (ParsedCommandLineArguments.Transform_ConcordanceTable.Trim() != "")
                {
                    if (File.Exists(ParsedCommandLineArguments.Transform_ConcordanceTable))
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Read Concordance Table.");

                        _ret.ReadConcordanceTable(ParsedCommandLineArguments.Transform_ConcordanceTable, ParsedCommandLineArguments.Verbose);
                    }
                }

                foreach (string zfilename in _listOfZIPArchivesWithXMLFiles)
                {
                    using (ZipFile zip = ZipFile.Read(zfilename))
                    { 
                        foreach (var entry in zip)
                        {
                            if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                                break;

                            // TODO: Check FitsMask
                            if (1==1 || CommandLineArguments.FitsMask(entry.FileName, ParsedCommandLineArguments.Mask))
                            {
                                if (ParsedCommandLineArguments.Verbose)
                                    Console.Write("Info: Read File  '" + entry.FileName + "' ");
                                 
                                using (MemoryStream zipStream = new MemoryStream())
                                {
                                    entry.ExtractWithPassword(zipStream, "");
                                    zipStream.Position = 0;
                                    try
                                    {
                                        StreamReader sr = new StreamReader(zipStream);
                                        string _fileContentAsString = sr.ReadToEnd();
                                        if (_fileContentAsString.Trim().Length > 0)
                                            ReadLogDataEEFromXMLString(_fileContentAsString, _ret);
                                    }
                                    catch (Exception _ex)
                                    {
                                        Console.WriteLine("Error processing file '" + entry.FileName + "': " + _ex.Message);
                                        return;
                                    }
                                }

                                Console.WriteLine("ok.");
                            }

                        }

                    }

                    if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Info: Max number of cases reached.");
                        break;
                    }
                }

                foreach (string xfilename in _listOfXMLFiles)
                {  
                    if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Info: Max number of cases reached.");
                        break;
                    }

                    if (1 == 1 || CommandLineArguments.FitsMask(Path.GetFileName(xfilename), ParsedCommandLineArguments.Mask))
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Info: Read File  '" + Path.GetFileName(xfilename) + "' ");

                        try
                        {
                            StreamReader sr = new StreamReader(xfilename);
                            string _fileContentAsString = sr.ReadToEnd();
                            if (_fileContentAsString.Trim().Length > 0)
                                ReadLogDataEEFromXMLString(_fileContentAsString, _ret);
                        }
                        catch (Exception _ex)
                        {
                            Console.WriteLine("Error processing file '" + xfilename + "': " + _ex.Message);
                            return;
                        }
                        Console.WriteLine("ok.");
                    }
                }

                logXContainer.ExportLogXContainerData(ParsedCommandLineArguments, _ret);
            }
            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.Message.ToString());
            }
        }

        // TODO: Fix bug with EventID and ParentEventID

        private static void  ReadLogDataEEFromXMLString(string XML, logXContainer _ret)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XML);

            string PersonIdentifier = "Unknown";
            string Element = "";

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            nsmgr.AddNamespace("cbaloggingmodel", "http://www.softcon.de/cba/cbaloggingmodel");

            foreach (XmlNode row in doc.SelectNodes("//logEntry[@xsi:type='cbaloggingmodel:CBAItemLogEntry']", nsmgr))
            {
                PersonIdentifier = row.Attributes["user"].Value.ToString();
                Element = row.Attributes["name"].Value.Replace("de.softcon.cba.runtime.","").ToString();
            }

            int _logcounter = 0;
            foreach (XmlNode row in doc.SelectNodes("//logEntry"))
            {
                if (row.ChildNodes.Count == 1)
                {
                    XDocument _xmlElement = XDocument.Parse(row.ChildNodes[0].OuterXml);
                    if (row.ChildNodes[0].Attributes["xsi:type"] != null)
                    {
                        logxGenericLogElement _parament = new logxGenericLogElement()
                        {
                            EventName = row.ChildNodes[0].Attributes["xsi:type"].Value.Replace("cbaloggingmodel:", ""),
                            PersonIdentifier = PersonIdentifier,
                            TimeStamp = DateTime.Parse(row.Attributes["timeStamp"].Value),
                            Item = Element,
                            EventID = _logcounter
                        };
                        _logcounter++;

                        AddEventData(_xmlElement.Root, _parament, _ret);
                        _ret.AddEvent(_parament);
                    }

                } 
            }
             
        }

        private static void AddEventData(XElement xmlelement, logxGenericLogElement parent, logXContainer _ret)
        {
            var doc = new XDocument(new XElement(parent.EventName));
            var root = doc.Root;
             
            foreach (var a in xmlelement.Attributes())
            {
                if (a.Name.Namespace.NamespaceName == "")
                {
                    root.Add(new XAttribute(a.Name.ToString(), a.Value)); 
                }
            }
            parent.EventDataXML = doc.ToString();

            int _logcounter = 0;
            foreach (XElement x in xmlelement.Elements())
            {
                logxGenericLogElement _newparent = new logxGenericLogElement()
                {
                    Item = parent.Item,
                    EventName = parent.EventName + "." + x.Name.LocalName,
                    PersonIdentifier = parent.PersonIdentifier,
                    TimeStamp = parent.TimeStamp,
                    EventID = _logcounter, 
                };
                AddEventData(x, _newparent, _ret); 
                _ret.AddEvent(_newparent);
                _logcounter++;
            }
        }
    }
}
