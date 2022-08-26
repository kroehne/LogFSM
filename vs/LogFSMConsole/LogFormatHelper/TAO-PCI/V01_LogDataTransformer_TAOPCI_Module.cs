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
using CsvHelper;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Bcpg.OpenPgp;
using LZStringCSharp;
using NPOI.POIFS.FileSystem;
using LogDataTransformer_IB_REACT_8_12__8_13;
using Newtonsoft.Json;
#endregion

namespace LogDataTransformer_TAOPCI_V01
{
    public class LogDataTransformer_TAOPCI_Module_V01
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

                if (!ParsedCommandLineArguments.RelativeTime)
                {
                    ParsedCommandLineArguments.RelativeTime = true;
                    Console.Write("Note: Changed to relative times. ");
                }

                EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.ElementAndTime;
                if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                    sort = EventDataListExtension.ESortType.None;

              
                #region Search Source Files

                List<string> _listOfCSVFiles = new List<string>();
                
                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    if (File.Exists(inFolder))
                    {
                        if (inFolder.ToLower().EndsWith(".csv"))
                        {
                            _listOfCSVFiles.Add(inFolder);
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

                        var _tmpCSVFileList = Directory.GetFiles(inFolder, "*.csv", SearchOption.AllDirectories);

                        foreach (string s in _tmpCSVFileList)
                            _listOfCSVFiles.Add(s);
                         
                    }

                }

                #endregion

                #region Process Source Files

                logXContainer _ret = new logXContainer() { PersonIdentifierIsNumber = _personIdentifierIsNumber, PersonIdentifierName = _personIdentifier };
                _ret.LoadCodebookDictionary(ParsedCommandLineArguments.Transform_Dictionary);

                int _logcounter = 0;
             
                
                foreach (string txtFile in _listOfCSVFiles)
                {
                    if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Info: Max number of cases reached.");
                        break;
                    }
                      
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Info: Read File  '" + Path.GetFileName(txtFile) + "' ");

                    try
                    { 
                        string _taoColumnNamePersonIdentifier = "Test Taker";

                        using (var reader = new StreamReader(txtFile))
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            var _data_rows = csv.GetRecords<dynamic>();
                             
                            foreach (IDictionary<string, object> row in _data_rows)
                            {
                                if (row.ContainsKey(_taoColumnNamePersonIdentifier))
                                {
                                    _personIdentifier = row[_taoColumnNamePersonIdentifier].ToString();

                                    // TODO: Mask Person Identifiers

                                    foreach (var c in row)
                                    {
                                        if (c.Key.EndsWith("-RESPONSE"))
                                        {
                                            string _itemName = c.Key.Substring(0, c.Key.Length - 9);
                                            string _json = c.Value.ToString();
                                            try
                                            {
                                                _json = LZString.DecompressFromBase64(_json);

                                                try
                                                {

                                                    var _collection = JsonConvert.DeserializeObject<ItemBuilder_React_Runtime_trace_collection>(_json);
                                                    foreach (ItemBuilder_React_Runtime_trace logFragment in _collection.logs)
                                                    {

                                                        logFragment.metaData.userId = _personIdentifier;
                                                        var _line = JsonConvert.SerializeObject(logFragment);

                                                        List<LogDataTransformer_IB_REACT_8_12__8_13.Log_IB_8_12__8_13> _log = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseLogElements(_line, "TAOPCI_V01");

                                                        // TODO: Add flag to extract full name (project.task) vs. short name (project)

                                                        foreach (var _l in _log)
                                                        {
                                                            if (_l.EventName == "")
                                                                _l.Element = "(Platform)";

                                                            var g = new logxGenericLogElement()
                                                            {
                                                                Item = _l.Element,
                                                                EventID = _l.EventID,
                                                                EventName = _l.EventName,
                                                                PersonIdentifier = _l.PersonIdentifier,
                                                                TimeStamp = _l.TimeStamp
                                                            };

                                                            try
                                                            {
                                                                g.EventDataXML = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.XmlSerializeToString(_l);
                                                                _ret.AddEvent(g);
                                                            }
                                                            catch (Exception _innerex)
                                                            {
                                                                Console.WriteLine("Error Processing xml: '" + g.EventDataXML + "' - Details: " + _innerex.Message);
                                                            }
                                                        }

                                                    }


                                                }
                                                catch (Exception _ex)
                                                {
                                                    Console.WriteLine("Unkown error " + _ex.Message);
                                                }
                                            }
                                            catch 
                                            {
                                                Console.WriteLine("No valid base64 encoded LZString found");
                                            }

                                       
                                        }
                                    }
                                } 
                            }
                        } 
                    }
                    catch (Exception _ex)
                    {
                        Console.WriteLine("Error processing file '" + txtFile + "': " + _ex.Message);
                        return;
                    }
                    Console.WriteLine("ok."); 
                }

                #endregion

                logXContainer.ExportLogXContainerData(ParsedCommandLineArguments, _ret);

            }
            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.Message.ToString());
            }
        }
         
    }
}
