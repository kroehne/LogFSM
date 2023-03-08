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
using NPOI.Util;
using System.Collections;
using NPOI.POIFS.Crypt;
#endregion

namespace LogDataTransformer_PISA12CA_Module_V01
{
    public class LogDataTransformer_PISA12CA_Module_V01
    {
        public static void ProcessLogFilesOnly(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {
            try
            {
                if (!ParsedCommandLineArguments.RelativeTime)
                {
                    throw new Exception("Only relative times available for this format.");
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

                                            string _cnt = "";
                                            string _nc = "";
                                            string _schoolid = "";
                                            string _StIDStd = "";
                                            double _relativeTime = double.NaN;

                                            string _EventName = "";
                                            string _Element = Path.GetFileName(outerInputZipEntry.FileName).Replace("CBA_","").Replace("_logs12_SPSS.sav", "");

                                            Dictionary<string, string> _EventValues = new Dictionary<string, string>();

                                            foreach (var variable in spssDataset.Variables)
                                            {

                                                if (variable.Name == "cnt")
                                                {
                                                    _cnt = GetValueFromSPSS(record, variable);
                                                }
                                                else if (variable.Name == "nc")
                                                {
                                                    _nc = GetValueFromSPSS(record, variable);
                                                }
                                                else if (variable.Name == "schoolid")
                                                {
                                                    _schoolid = GetValueFromSPSS(record, variable);
                                                }
                                                else if (variable.Name == "StIDStd")
                                                {
                                                    _StIDStd = GetValueFromSPSS(record, variable);
                                                }
                                                else if (variable.Name == "event")
                                                {
                                                    _EventName = GetValueFromSPSS(record, variable);
                                                }
                                                else if (variable.Name == "time")
                                                {
                                                    _relativeTime  =  double.Parse(GetValueFromSPSS(record, variable));
                                                }
                                                else if (variable.Name == "event_number")
                                                {
                                                    _EventValues.Add("event_number", GetValueFromSPSS(record, variable));
                                                }
                                                else if (variable.Name == "event_value")
                                                {
                                                    string _value = GetValueFromSPSS(record, variable);
                                                    if (_value != "NULL")
                                                        _EventValues.Add("event_value", _value);
                                                }
                                                else if (variable.Name == "event_detail")
                                                {
                                                    string _value = GetValueFromSPSS(record, variable);
                                                    if (_value != "NULL")
                                                        _EventValues.Add("event_detail", _value);
                                                } 
                                            }

                                            // parse "event_detail" for "cp007q01" - "cp007q03" (see Notes on log file data - releaseditems.pdf)
                                            if ((_Element == "cp007q01" || _Element == "cp007q02" || _Element == "cp007q03") && _EventName == "ACER_EVENT")
                                            {
                                                char[] _indicators = _EventValues["event_value"].Replace("'", "").ToCharArray();
                                                if (_indicators.Length == 23)
                                                {
                                                    string[] _valueNames = new string[] { "Diamondnowhere", "DiamondSilver", "EmeraldLincoln", "EmeraldUnity", "LeeMandela",
                                                    "LincolnSato", "MandelaEinstein", "MarketLee", "MarketPark", "NobelLee", "nowhereEinstein", "nowhereEmerald", "nowhereSakharov",
                                                    "nowhereUnity","ParkMandela", "Parknowhere", "SakharovMarket", "SakharovNobel", "Satonowhere", "SilverMarket","Silvernowhere",
                                                    "UnityPark","UnitySato"};
                                                    for (int i = 0; i < _valueNames.Length; i++)
                                                        _EventValues.Add(_valueNames[i], _indicators[i].ToString());

                                                    _EventValues.Remove("event_value");
                                                }
                                            }
                                            else if (_Element.StartsWith("cr") && _EventName == "click")
                                            {
                                                string _details = _EventValues["event_detail"];
                                                int _qm = _details.IndexOf("?");
                                                if (_qm != -1)
                                                {
                                                    string _url = _details.Substring(0, _qm);
                                                    string _query = _details.Substring(_qm+1, _details.Length - _qm-1);                                                    
                                                    var reg = new Regex("(?:[?&]|^)([^&]+)=([^&]*)");
                                                    var matches = reg.Matches(_query);
                                                    foreach (System.Text.RegularExpressions.Match match in matches)
                                                        _EventValues.Add(match.Groups[1].Value, Uri.UnescapeDataString(match.Groups[2].Value));

                                                    _EventValues.Remove("event_detail");

                                                }
                                               

                                            }


                                            string _PersonIdentifier = _cnt  + "-" + _nc + "-" + _schoolid + "-" + _StIDStd;
                                              
                                            var doc = new XDocument(new XElement(_EventName));
                                            var root = doc.Root;
                                            foreach (string val in _EventValues.Keys)
                                                root.Add(new System.Xml.Linq.XAttribute(val, _EventValues[val]));

                                            int _EventID = _ret.GetMaxID(_PersonIdentifier);

                                            logxGenericLogElement _newElement = new logxGenericLogElement()
                                            {
                                                PersonIdentifier = _PersonIdentifier,
                                                Item = _Element,
                                                EventID = _EventID,
                                                EventName = _EventName,
                                                RelativeTime = _relativeTime,
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
