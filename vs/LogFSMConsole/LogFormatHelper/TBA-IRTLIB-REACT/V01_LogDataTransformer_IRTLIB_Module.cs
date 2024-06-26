﻿#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ionic.Zip;
using LogFSM_LogX2019;
using LogFSMConsole;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Operators;
using StataLib;
# endregion

namespace LogDataTransformer_IRTlibPlayer_V01
{
    public class LogDataTransformer_IRTLIB_Module_V01
    {

        public static List<json_IRTLib_V01__TokenLis> RetrieveListOfSessesions(string web, string username, string password, string key)
        {
           
            WebClient _client = new WebClient();
            if (username.Trim() != "" || password.Trim() != "")
            {
                string credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username + ":" + password));
                _client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
            }

            if (key.Trim() != "")
            {
                if (web.Contains("?"))
                    web = web + "&apiKey=" + key;
                else
                    web = web + "?apiKey=" + key;
            }

            string reply = _client.DownloadString(web);
            var _ret = JsonConvert.DeserializeObject<List<json_IRTLib_V01__TokenLis>>(reply);

            Console.WriteLine(" - Found " + _ret.Count + " sessions at '" + web);

            return _ret; 
        }

        public static void UpdateFiles(List<json_IRTLib_V01__TokenLis> _sessions, string web, string username, string password, string key, string folder, string mask, bool overwrite)
        {
            foreach (var _s in _sessions)
            {
                bool _selected = true;

                if (mask != "" && mask != "*.*")
                    _selected = CommandLineArguments.FitsMask(_s.sessionId, mask);

                if (File.Exists(folder + _s.sessionId + ".zip") && !overwrite)
                    _selected = false;

                if (_selected) 
                {
                    if (!Directory.Exists(folder))
                    {
                        Console.WriteLine(" Error - Specified directory '" + folder + "' not found");
                    }
                    else
                    {
                        string _adress = web + _s.sessionId + "/result";
                        try
                        {
                            Console.Write(" - Session '" + _s.sessionId + "' --> ");
                            WebClient _client = new WebClient();
                            if (username.Trim() != "" || password.Trim() != "")
                            {
                                string credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username + ":" + password));
                                _client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                            }


                            if (key.Trim() != "")
                            {
                                if (web.Contains("?"))
                                    _adress = _adress + "&apiKey=" + key;
                                else
                                    _adress = _adress + "?apiKey=" + key;
                            }

                            _client.DownloadFile(_adress, folder + "//" + _s.sessionId + ".zip");

                            FileInfo _fi = new FileInfo(folder + "//" + _s.sessionId + ".zip");
                             
                            Console.WriteLine(_fi.Length);

                        }
                        catch (System.Net.WebException _fnf)
                        {
                            Console.WriteLine(" Error - " + _fnf.Status + Environment.NewLine + _fnf.ToString());
                        }
                        catch (Exception _ex)
                        {
                            Console.WriteLine(" Unknown Error - " + _ex.ToString());
                        }
                    }
                }
            }
        }
  
        public static void ProcessLogFilesOnly(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        { 
            try
            {
                bool _personIdentifierIsNumber = false;
                if (ParsedCommandLineArguments.Flags.Contains("NUMERICPERSONIDENTIFIER"))
                    _personIdentifierIsNumber = true;

                string _personIdentifierColumnName = "PersonIdentifier";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("personidentifier"))
                    _personIdentifierColumnName = ParsedCommandLineArguments.ParameterDictionary["personidentifier"];

                bool _checkEventAttrbibutes = false;
                if (ParsedCommandLineArguments.Flags.Contains("CHECKEVENTATTRIBUTES"))
                    _checkEventAttrbibutes = true;

                bool _allowFileRename = false;  
                if (ParsedCommandLineArguments.Flags.Contains("ALLOWFILERENAME"))
                    _allowFileRename = true;

                double _utcoffset = 0;
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("utcoffset"))
                    _utcoffset = double.Parse(ParsedCommandLineArguments.ParameterDictionary["utcoffset"]);
                 
                // Create logXContainer 

                logXContainer _ret = new LogFSM_LogX2019.logXContainer()
                {
                    PersonIdentifierName = _personIdentifierColumnName,
                    PersonIdentifierIsNumber = _personIdentifierIsNumber
                };
                 
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

                // Update from Server if requested

                string _web = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_JOB_TRANSFORM_web))
                    _web = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_JOB_TRANSFORM_web];

                string _username = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_JOB_TRANSFORM_user ))
                    _username = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_JOB_TRANSFORM_user];

                string _password = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_JOB_TRANSFORM_password))
                    _password = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_JOB_TRANSFORM_password];

                string _key = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_JOB_TRANSFORM_key))
                    _key = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_JOB_TRANSFORM_key];

                string _mask = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_mask))
                    _mask = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_mask];

                bool _override = false;
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_overwrite))
                    bool.TryParse(ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_overwrite], out _override);

                if (_web.Trim() != "")
                {
                    var _sessions = RetrieveListOfSessesions(_web, _username, _password, _key);
                    UpdateFiles(_sessions, _web, _username, _password, _key, ParsedCommandLineArguments.Transform_InputFolders[0], _mask, _override);
                }

                if (ParsedCommandLineArguments.Transform_OutputStata.Trim() == "" && ParsedCommandLineArguments.Transform_OutputXLSX.Trim() == "" &&
                    ParsedCommandLineArguments.Transform_OutputZCSV.Trim() == "" && ParsedCommandLineArguments.Transform_OutputSPSS.Trim() == "")
                {
                    return; 
                }

                // Iterate over all input folders

                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    if (!Directory.Exists(inFolder))
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Warning: Directory not exists: '" + inFolder + "'.");

                        continue;
                    }

                    string[] listOfZipFiles = Directory.GetFiles(inFolder, ParsedCommandLineArguments.Mask, SearchOption.AllDirectories);

                    foreach (string zfilename in listOfZipFiles)
                    {
                         
                        if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                        {
                            if (ParsedCommandLineArguments.Verbose)
                                Console.WriteLine("Info: Max number of cases reached.");
                            break;
                        }

                        if (ParsedCommandLineArguments.Verbose)
                            Console.Write("Info: Read Zip File  '" + zfilename + "' ");

                        try
                        { 
                            using (ZipFile zip = ZipFile.Read(zfilename))
                            {
                                foreach (var entry in zip)
                                {
                                    string _sessionFileName = Path.GetFileNameWithoutExtension(zip.Name);
                                    if (_sessionFileName.Contains("_"))
                                        _sessionFileName = _sessionFileName.Substring(0, _sessionFileName.IndexOf("_"));

                                    using (MemoryStream zipStream = new MemoryStream())
                                    {
                                        entry.ExtractWithPassword(zipStream, "");
                                        zipStream.Position = 0;
                                        try
                                        {
                                            if (entry.FileName.StartsWith("browser.log"))
                                            {
                                                // TODO: Process 
                                            }
                                            else if (entry.FileName.StartsWith("server.log"))
                                            {
                                                // TODO: Process 
                                            }
                                            else if (entry.FileName.StartsWith("Results.csv"))
                                            {
                                                // TODO: Process 
                                            }
                                            else if (entry.FileName.StartsWith("Results.sav"))
                                            {
                                                // TODO: Process 
                                            }
                                            else if (entry.FileName.StartsWith("Results.dta"))
                                            {
                                                // TODO: Process 
                                            }
                                            else if (entry.FileName.StartsWith("Session.json"))
                                            {
                                                // TODO: Process 
                                            }
                                            else
                                            { 
                                                StreamReader sr = new StreamReader(zipStream);
                                                string line;
                                                int linecounter = 0;
                                                while ((line = sr.ReadLine()) != null)
                                                {
                                                    if (ParsedCommandLineArguments.Transform_LogVersion == "default")
                                                    {
                                                        if (entry.FileName.StartsWith("Trace.json"))
                                                        {
                                                            string _json = line;

                                                            if (_json.EndsWith(","))
                                                                _json = _json.Substring(0, _json.Length - 1);

                                                            try
                                                            {
                                                                string _personIdentifier = "";
                                                                if (_allowFileRename)
                                                                    _personIdentifier = _sessionFileName;

                                                                List<LogDataTransformer_IB_REACT_8_12__8_13.Log_IB_8_12__8_13> _log = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseLogElements(_json, "IRTlibPlayer_V01", _checkEventAttrbibutes, _personIdentifier);

                                                                // TODO: Add flag to extract full name (project.task) vs. short name (project)

                                                                foreach (var _l in _log)
                                                                {
                                                                    if (_l.Element.Trim() == "")
                                                                        _l.Element = "(Platform)";

                                                                    //TODO: FLAG: _l.PersonIdentifier = _l.SessionId

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
                                                            catch (Exception _ex)
                                                            {

                                                                Console.WriteLine("Error processing file '" + entry.FileName + "' ('" + zfilename + "'): " + _ex.Message);
                                                            }
                                                        }
                                                        else if (entry.FileName.StartsWith("Keyboard.json"))
                                                        {
                                                            string _json = line;
                                                            if (_json.EndsWith(","))
                                                                _json = _json.Substring(0, _json.Length - 1);

                                                            List<KeyStroke> _keyStrokes = JsonConvert.DeserializeObject<List<KeyStroke>>(_json);

                                                            // TODO: Process 
                                                        }
                                                        else if (entry.FileName.StartsWith("Mouse.json"))
                                                        {
                                                            string _json = line;
                                                            if (_json.EndsWith(","))
                                                                _json = _json.Substring(0, _json.Length - 1);

                                                            var _data = JsonConvert.DeserializeObject<MouseEvent[]>(_json);

                                                            // TODO: Process 
                                                        }
                                                        else if (entry.FileName.StartsWith("ItemScore.json"))
                                                        {
                                                            string _json = line;
                                                            if (_json.EndsWith(","))
                                                                _json = _json.Substring(0, _json.Length - 1);

                                                            try
                                                            {
                                                                LogDataTransformer_IRTlibPlayer_V01.json_IRTlib_V01__ItemScore _itemScoreEvent =
                                                                    JsonConvert.DeserializeObject<LogDataTransformer_IRTlibPlayer_V01.json_IRTlib_V01__ItemScore>(_json);

                                                                string task = _itemScoreEvent.Context.Task;
                                                                string item = _itemScoreEvent.Context.Item;
                                                                 
                                                                string personIdentifier = _itemScoreEvent.SessionId;
                                                                if (_allowFileRename)
                                                                    personIdentifier = _sessionFileName;

                                                                LogDataTransformer_IB_REACT_8_12__8_13.itemScore _itemScore =
                                                                    LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseItemScore(_itemScoreEvent.ItemScore, task, item, personIdentifier);

                                                                logxGenericResultElement g = new logxGenericResultElement() { PersonIdentifier = personIdentifier };

                                                                g.Results.Add(item + "." + task + "." + "hitsAccumulated", _itemScore.hitsAccumulated);
                                                                g.Results.Add(item + "." + task + "." + "hitsCount", _itemScore.hitsCount);
                                                                g.Results.Add(item + "." + task + "." + "missesAccumulated", _itemScore.missesAccumulated);
                                                                g.Results.Add(item + "." + task + "." + "missesCount", _itemScore.missesCount);
                                                                g.Results.Add(item + "." + task + "." + "classMaxWeighed", _itemScore.classMaxWeighed);
                                                                g.Results.Add(item + "." + task + "." + "classMaxName", _itemScore.classMaxName);
                                                                g.Results.Add(item + "." + task + "." + "totalResult", _itemScore.totalResult);
                                                                g.Results.Add(item + "." + task + "." + "nbUserInteractions", _itemScore.nbUserInteractions);
                                                                g.Results.Add(item + "." + task + "." + "nbUserInteractionsTotal", _itemScore.nbUserInteractionsTotal);
                                                                g.Results.Add(item + "." + task + "." + "firstReactionTimeTotal", _itemScore.firstReactionTimeTotal);
                                                                g.Results.Add(item + "." + task + "." + "taskExecutionTime", _itemScore.taskExecutionTime);
                                                                g.Results.Add(item + "." + task + "." + "taskExecutionTimeTotal", _itemScore.taskExecutionTimeTotal);

                                                                foreach (var hit in _itemScore.Hits)
                                                                {
                                                                    if (!g.Results.ContainsKey(item + "." + task + "." + hit.Value.Name + ".Result"))
                                                                        g.Results.Add(item + "." + task + "." + hit.Value.Name + ".Result", hit.Value.IsTrue);
                                                                    else
                                                                        g.Results[item + "." + task + "." + hit.Value.Name + ".Result"] = hit.Value.IsTrue;

                                                                    if (!g.Results.ContainsKey(item + "." + task + "." + hit.Value.Name + ".Weight"))
                                                                        g.Results.Add(item + "." + task + "." + hit.Value.Name + ".Weight", hit.Value.Weight);
                                                                    else
                                                                        g.Results[item + "." + task + "." + hit.Value.Name + ".Weight"] = hit.Value.Weight;

                                                                    if (!g.Results.ContainsKey(item + "." + task + "." + hit.Value.Name + ".ResultText"))
                                                                        g.Results.Add(item + "." + task + "." + hit.Value.Name + ".ResultText", hit.Value.ResultText);
                                                                    else
                                                                        g.Results[item + "." + task + "." + hit.Value.Name + ".ResultText"] = hit.Value.ResultText;
                                                                }

                                                                foreach (var cls in _itemScore.ClassResults)
                                                                {
                                                                    if (cls.Value != null)
                                                                    {
                                                                        if (!g.Results.ContainsKey(item + "." + task + "." + cls.Value.HitMissClass + ".Result"))
                                                                            g.Results.Add(item + "." + task + "." + cls.Value.HitMissClass + ".Result", cls.Value.IsTrue);
                                                                        else
                                                                            g.Results[item + "." + task + "." + cls.Value.HitMissClass + ".Result"] = cls.Value.IsTrue;

                                                                        if (!g.Results.ContainsKey(item + "." + task + "." + cls.Value.HitMissClass + ".HitMiss"))
                                                                            g.Results.Add(item + "." + task + "." + cls.Value.HitMissClass + ".HitMiss", cls.Value.Name);
                                                                        else
                                                                            g.Results[item + "." + task + "." + cls.Value.HitMissClass + ".HitMiss"] = cls.Value.Name;

                                                                        if (!g.Results.ContainsKey(item + "." + task + "." + cls.Value.HitMissClass + ".Weight"))
                                                                            g.Results.Add(item + "." + task + "." + cls.Value.HitMissClass + ".Weight", cls.Value.Weight);
                                                                        else
                                                                            g.Results[item + "." + task + "." + cls.Value.HitMissClass + ".Weight"] = cls.Value.Weight;

                                                                        if (!g.Results.ContainsKey(item + "." + task + "." + cls.Value.HitMissClass + ".ResultText"))
                                                                            g.Results.Add(item + "." + task + "." + cls.Value.HitMissClass + ".ResultText", cls.Value.ResultText);
                                                                        else
                                                                            g.Results[item + "." + task + "." + cls.Value.HitMissClass + ".ResultText"] = cls.Value.ResultText;

                                                                    }
                                                                }

                                                                _ret.AddResults(g);

                                                            }
                                                            catch (Exception _ex)
                                                            {
                                                                Console.WriteLine("Error processing ItemScore: " + _ex.ToString());
                                                            }

                                                        }
                                                        else if (entry.FileName.StartsWith("Snapshot.json"))
                                                        {
                                                            string _json = line;
                                                            if (_json.EndsWith(","))
                                                                _json = _json.Substring(0, _json.Length - 1);

                                                            // TODO: Read Person Identifier from Data (not from file name)
                                                            string personIdentifier = _sessionFileName;                                                            
                                                            // if (_allowFileRename)
                                                            //    personIdentifier = _sessionFileName;

                                                            logxGenericResultElement g = new logxGenericResultElement() { PersonIdentifier = personIdentifier };

                                                            JsonDocument doc = JsonDocument.Parse(_json);
                                                            foreach (var jsonelement in doc.RootElement.EnumerateObject())
                                                            {
                                                                // Extract variable values from snapshots
                                                                try
                                                                {
                                                                    if (jsonelement.Name == "variables")
                                                                    {
                                                                        foreach (var jsonchild in jsonelement.Value.EnumerateObject())
                                                                        {
                                                                            string element = jsonchild.Name;
                                                                            string[] subelements = element.Split("/", StringSplitOptions.RemoveEmptyEntries);
                                                                            string testname = subelements[0].Replace("test=", "");
                                                                            string itemname = subelements[1].Replace("item=", "");
                                                                            string taskname = subelements[2].Replace("task=", "");

                                                                            foreach (var varelement in jsonchild.Value.EnumerateObject())
                                                                            {
                                                                                string varname = "";
                                                                                string varvalue = "";
                                                                                string vartype = "";

                                                                                foreach (var part in varelement.Value.EnumerateObject())
                                                                                {
                                                                                    if (part.Name == "name")
                                                                                        varname = part.Value.GetString();
                                                                                    else if (part.Name == "value")
                                                                                        varvalue = part.Value.ToString();
                                                                                    else if (part.Name == "type")
                                                                                        vartype = part.Value.ToString();
                                                                                }

                                                                                g.Results.Add(itemname + "." + taskname + ".var." + varname, varvalue);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                catch (Exception _ex)
                                                                {
                                                                    Console.WriteLine(_ex.ToString());
                                                                }
                                                            }

                                                            _ret.AddResults(g);
                                                        }
                                                        else if (entry.FileName.StartsWith("Log.json"))
                                                        {
                                                            string _json = line;
                                                            if (_json.EndsWith(","))
                                                                _json = _json.Substring(0, _json.Length - 1);
                                                            try
                                                            {

                                                                List<LogDataTransformer_IB_REACT_8_12__8_13.Log_IB_8_12__8_13> _log =
                                                                  LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseTraceLogs(_json, _utcoffset);

                                                                foreach (var _l in _log)
                                                                {

                                                                    // TODO: Read Person Identifier from Data (not from file name)
                                                                    _l.PersonIdentifier = _sessionFileName;

                                                                    if (_l.Element.Trim() == "")
                                                                        _l.Element = "(Platform)";

                                                                    //if (_allowFileRename)
                                                                    //    _l.PersonIdentifier = _sessionFileName;
                                                                     
                                                                    var g = new logxGenericLogElement()
                                                                    {
                                                                        Item = _l.Element,
                                                                        EventID = _l.EventID,
                                                                        EventName = _l.EventName,
                                                                        PersonIdentifier = _l.PersonIdentifier,
                                                                        TimeStamp = _l.TimeStamp
                                                                    };

                                                                    g.EventDataXML = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.XmlSerializeToString(_l);

                                                                   _ret.AddEvent(g, ParsedCommandLineArguments.Elements, ParsedCommandLineArguments.Events, ParsedCommandLineArguments.ExcludedElements,ParsedCommandLineArguments.ExcludedEvents);

                                                                    

                                                                }

                                                            }
                                                            catch (Exception _ex)
                                                            {
                                                                Console.WriteLine("Error processing file '" + entry.FileName + "' ('" + zfilename + "'): " + _ex.Message);
                                                            }

                                                        }
                                                        else if (entry.FileName.StartsWith("Monitoring"))
                                                        {
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("Unknown file type (file: '" + entry.FileName + "' in archive: '" + zip.Name + "'");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (ParsedCommandLineArguments.Verbose)
                                                            Console.WriteLine("failed.");

                                                        Console.WriteLine("Version '" + ParsedCommandLineArguments.Transform_LogVersion + "' not supported.");
                                                        return;
                                                    }
                                                    linecounter++;
                                                }
                                                if (ParsedCommandLineArguments.Verbose)
                                                    Console.WriteLine(" ok ('" + linecounter + " lines).");
                                                sr.Close();

                                            }

                                        }
                                        catch (Exception _ex)
                                        {
                                            Console.WriteLine("Error processing file '" + entry.FileName + "': " + _ex.Message);
                                            return;
                                        }
                                    }

                                }
                            }

                        }
                        catch (Exception _ex)
                        {
                            Console.Write("Info: Read Zip Error  '" + zfilename + "' " + _ex.Message);
                        }
                    }

                }

                logXContainer.ExportLogXContainerData(ParsedCommandLineArguments, _ret);

            }
            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.ToString());
            }
        }
    }
}
