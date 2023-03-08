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
using System.Text.Json.Nodes;
using System.Text.Json;
using SpssLib.SpssDataset;
#endregion

namespace LogDataTransformer_TIMSSeT19_V01
{
    public class LogDataTransformer_TIMSSeT19_Module_V01
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

                string _personIdentifierName = "PersonIdentifier";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("personidentifier"))
                    _personIdentifierName = ParsedCommandLineArguments.ParameterDictionary["personidentifier"];

                string _language = "ENG";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("language"))
                    _language = ParsedCommandLineArguments.ParameterDictionary["language"];


                DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                XmlSerializer logSerializer = new XmlSerializer(typeof(log_pisa_bq_2015_to_2018));

                #region Search Source Files

                List<string> _listOfZIPArchivesWithSPSSFiles = new List<string>();

                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    // Input is file

                    if (File.Exists(inFolder))
                    {
                        if (inFolder.ToLower().EndsWith(".zip"))
                        {
                            // Single ZIP file 

                            _listOfZIPArchivesWithSPSSFiles.Add(inFolder);
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
                            _listOfZIPArchivesWithSPSSFiles.Add(s);
                    }

                }

                #endregion

                logXContainer _ret = new logXContainer() { PersonIdentifierIsNumber = _personIdentifierIsNumber, PersonIdentifierName = _personIdentifierName, RelativeTimesAreSeconds = _relativeTimesAreSeconds };

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
                 
                foreach (string zfilename in _listOfZIPArchivesWithSPSSFiles)
                {
                    try
                    {
                        using (var outerInputZipFile = ZipFile.Read(zfilename))
                        {
                             
                            foreach (var outerInputZipEntry in outerInputZipFile.Entries)
                            {
                                if (outerInputZipEntry.FileName.EndsWith(".sav") && outerInputZipEntry.UncompressedSize != 0)
                                {
                                    using (MemoryStream outerZIPEntryMemoryStream = new MemoryStream())
                                    { 
                                        if (ParsedCommandLineArguments.Verbose)
                                        {
                                            Console.WriteLine("Read '" + outerInputZipEntry.FileName + "'");
                                        }

                                        outerInputZipEntry.Password = ParsedCommandLineArguments.ZIPPassword;
                                        outerInputZipEntry.Extract(outerZIPEntryMemoryStream);
                                        outerZIPEntryMemoryStream.Position = 0;

                                        SpssLib.DataReader.SpssReader spssDataset = new SpssLib.DataReader.SpssReader(outerZIPEntryMemoryStream);
                                        foreach (var record in spssDataset.Records)
                                        {

                                            string _PersonIdentifier = "";
                                            string _EventName = "";
                                            string _ItemName = ""; 
                                            Dictionary<string, string> _EventValues = new Dictionary<string, string>();

                                            foreach (var variable in spssDataset.Variables)
                                            {
                                                
                                                if (variable.Name == "idstud")
                                                {
                                                    _PersonIdentifier = GetValueFromSPSS(record, variable);
                                                }
                                                else if (variable.Name == "eventname")
                                                {
                                                    _EventName = GetValueFromSPSS(record, variable).Replace(":", "");
                                                }
                                                else if (variable.Name == "information")
                                                {
                                                    if (record.GetValue(variable) != null)
                                                    {
                                                        string _valueString = GetValueFromSPSS(record, variable);
                                                        if (_valueString.StartsWith("{"))
                                                        {
                                                            using (JsonDocument document = JsonDocument.Parse(_valueString))
                                                            {
                                                                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                                                                {
                                                                    if (property.Value.ValueKind.ToString() == "Object")
                                                                    {
                                                                        foreach (JsonProperty innerProperty in property.Value.EnumerateObject())
                                                                            _EventValues.Add("information_" + property.Name + "_" + innerProperty.Name, innerProperty.Value.ToString());
                                                                    }
                                                                    else
                                                                    {
                                                                        _EventValues.Add("information_" + property.Name, property.Value.ToString());
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            _EventValues.Add("information", _valueString);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (record.GetValue(variable) != null)
                                                    {

                                                        _EventValues.Add(variable.Name, GetValueFromSPSS(record, variable));
                                                    }
                                                }
                                            }

                                            DateTime _TimeStamp = DateTime.MinValue;
                                            if (_EventValues.ContainsKey("timeunixsec"))
                                            {
                                                _TimeStamp = dt1970.AddSeconds(long.Parse(_EventValues["timeunixsec"]));
                                            } else if (_EventValues.ContainsKey("TimeUnixSec"))
                                            {
                                                _TimeStamp = dt1970.AddSeconds(long.Parse(_EventValues["TimeUnixSec"]));
                                            }

                                            if (_EventValues.ContainsKey("timemilisec"))
                                            {
                                                _TimeStamp = _TimeStamp.AddMilliseconds(long.Parse(_EventValues["timemilisec"]));
                                            }
                                            else if (_EventValues.ContainsKey("TimeMilisec"))
                                            {
                                                _TimeStamp = _TimeStamp.AddMilliseconds(long.Parse(_EventValues["TimeMilisec"]));
                                            }

                                            _ItemName = "";
                                            if (_EventValues.ContainsKey("BlockName"))
                                            {
                                                _ItemName = _EventValues["BlockName"].Replace("[", "").Replace("]", "");
                                            }

                                            if (_ItemName == "")
                                                _ItemName = "empty";

                                            if (_EventName == "")
                                                _EventName = "empty";


                                            var doc = new XDocument(new XElement(_EventName));
                                            var root = doc.Root;
                                            foreach (string val in _EventValues.Keys)
                                                root.Add(new System.Xml.Linq.XAttribute(val, _EventValues[val]));

                                            int _EventID = _ret.GetMaxID(_PersonIdentifier);

                                            logxGenericLogElement _newElement = new logxGenericLogElement()
                                            {
                                                PersonIdentifier = _PersonIdentifier,                                                
                                                Item = _ItemName,
                                                EventID = _EventID,
                                                EventName = _EventName,
                                                TimeStamp = _TimeStamp,
                                                EventDataXML = doc.ToString()
                                            };

                                            _ret.AddEvent(_newElement);
 
                                        }

                                    }
                                } 
                            }
                        }
                    }
                    catch (Exception _ex)
                    {
                        _ret.ExportErrors.Add("Error reading file '" + zfilename + "': " + _ex.Message + "/" + _ex.ToString());
                    }
                }

                logXContainer.ExportLogXContainerData(ParsedCommandLineArguments, _ret);
            }

            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.Message.ToString());
            }
        }
 

        public static string GetValueFromSPSS(Record record, Variable variable)
        {
            if (record.GetValue(variable) != null)
            {
                return record.GetValue(variable).ToString().Replace("\0", string.Empty);

            }
            return "";
        }

    }

}
