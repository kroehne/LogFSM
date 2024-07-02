#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using LogFSM_LogX2019;
using LogFSMConsole;
using CsvHelper;
using LZStringCSharp;
using LogDataTransformer_IB_REACT_8_12__8_13;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace LogDataTransformer_TAOPCI_V01
{
    public class LogDataTransformer_TAOPCI_Module_V01
    {
        public static void ProcessLogFilesOnly(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {
            try
            {
                bool _relativeTimesAreSeconds = false;
                if (ParsedCommandLineArguments.Flags.Contains("RELATIVETIMESARESECONDS"))
                    _relativeTimesAreSeconds = true;

                bool _personIdentifierIsNumber = false;
                if (ParsedCommandLineArguments.Flags.Contains("NUMERICPERSONIDENTIFIER"))
                    _personIdentifierIsNumber = true;

                bool _checkEventAttrbibutes = false;
                if (ParsedCommandLineArguments.Flags.Contains("CHECKEVENTATTRIBUTES"))
                    _checkEventAttrbibutes = true;

                string _personIdentifier = "PersonIdentifier";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("personidentifier"))
                    _personIdentifier = ParsedCommandLineArguments.ParameterDictionary["personidentifier"];
                  
                string _mask = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_mask))
                    _mask = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_mask];

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
                                Console.WriteLine(" - Warning: Directory not exists: '" + inFolder + "'.");

                            continue;
                        }

                        var _tmpCSVFileList = Directory.GetFiles(inFolder, "*.csv", SearchOption.AllDirectories);

                        foreach (string s in _tmpCSVFileList)
                            _listOfCSVFiles.Add(s);
                         
                    }
                }

                #endregion

                #region Process Source Files

                logXContainer _ret = new logXContainer() { PersonIdentifierIsNumber = _personIdentifierIsNumber, PersonIdentifierName = _personIdentifier, RelativeTimesAreSeconds = _relativeTimesAreSeconds };
                _ret.LoadCodebookDictionary(ParsedCommandLineArguments.Transform_Dictionary);
                foreach (string csvFile in _listOfCSVFiles)
                {
                    if (CommandLineArguments.FitsMask(csvFile, _mask)) {

                        if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                        {
                            if (ParsedCommandLineArguments.Verbose)
                                Console.WriteLine(" - Info: Max number of cases reached.");
                            break;
                        }

                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine(" - Info: Read File  '" + Path.GetFileName(csvFile) + "' ");

                        try
                        {
                            string _taoColumnNamePersonIdentifier = "Test Taker";

                            using (var reader = new StreamReader(csvFile))
                            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                            {
                                var _data_rows = csv.GetRecords<dynamic>();
                                int _row = 1;
                                foreach (IDictionary<string, object> row in _data_rows)
                                {
                                    if (row.ContainsKey(_taoColumnNamePersonIdentifier))
                                    {
                                        _personIdentifier = row[_taoColumnNamePersonIdentifier].ToString();

                                        int _col = 0;
                                        foreach (var c in row)
                                        {
                                            if (c.Key.EndsWith("-RESPONSE"))
                                            {
                                                string _itemName = c.Key.Substring(0, c.Key.Length - 9);
                                                string _json = c.Value.ToString();
                                                try
                                                {
                                                    _json = LZString.DecompressFromBase64(_json);

                                                    // results  
                                                     
                                                    dynamic _dynresults = JsonConvert.DeserializeObject<dynamic>(_json);
                                                    var _results = new Dictionary<string, object>();

                                                    if (_dynresults.score != null)
                                                    { 
                                                        foreach (var _hits in _dynresults.score)
                                                            foreach (var _hit in _hits)
                                                                foreach (JProperty k in _hit)
                                                                    _results.Add(_itemName + "_" + k.Name, k.Value);
                                                    }
                                                     
                                                    if (_dynresults.scoreRaw != null)
                                                    {
                                                        foreach (var _group in _dynresults.scoreRaw)
                                                            foreach (JProperty k in _group)
                                                                _results.Add(_itemName + "_" + k.Name, k.Value);
                                                    }

                                                    if (_results.Count>0)
                                                        _ret.AddResults(new logxGenericResultElement() { PersonIdentifier = _personIdentifier, Results = _results });

                                                    // log 
                                                    try
                                                    {
                                                        var _collection = JsonConvert.DeserializeObject<ItemBuilder_React_Runtime_trace_collection>(_json);
                                                        foreach (ItemBuilder_React_Runtime_trace logFragment in _collection.logs)
                                                        {
                                                            logFragment.metaData.userId = _personIdentifier;
                                                            var _line = JsonConvert.SerializeObject(logFragment);
                                                            
                                                            List<LogDataTransformer_IB_REACT_8_12__8_13.Log_IB_8_12__8_13> _log = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseLogElements(_line, "TAOPCI_V01", _checkEventAttrbibutes, "");

                                                            // TODO: Add flag to extract full name (project.task) vs. short name (project)
                                                           
                                                            foreach (var _l in _log)
                                                            {
                                                                if (_l.Element.Trim() == "")
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
                                                                    _ret.AddEvent(g, ParsedCommandLineArguments.Elements, ParsedCommandLineArguments.Events, ParsedCommandLineArguments.ExcludedElements,ParsedCommandLineArguments.ExcludedEvents);
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
                                                    Console.WriteLine(" - Info: No valid base64 encoded LZString found (file: '" + csvFile + "', row: '" + _row + "', column '" + _col + "'.");
                                                }
                                            }
                                            _col++;
                                        }
                                    }
                                    _row++;
                                }
                            }
                        }
                        catch (Exception _ex)
                        {
                            Console.WriteLine("Error processing file '" + csvFile + "': " + _ex.Message);
                            return;
                        }

                    }
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
