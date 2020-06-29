namespace LogDataTransformer_IRTlibPlayer_V01
{
 
    #region usings
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Ionic.Zip;
    using LogFSM_LogX2019;
    using LogFSMConsole;
    using Newtonsoft.Json;
    using StataLib;
    #endregion

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
            return _ret; 
        }

        public static void UpdateFiles(List<json_IRTLib_V01__TokenLis> _sessions, string web, string username, string password, string key, string folder)
        {
            foreach (var _s in _sessions)
            {
                if (_s.lastUpdate != DateTime.MinValue || true) 
                {
                    // TODO: Necessary to download all files?

                    if (File.Exists(folder + _s.sessionId + ".zip"))
                    {
                        // TODO: Check DateTimes in ZIP
                    }
                     
                    string _adress = web + _s.sessionId + "/result";
                    try
                    {
                        Console.Write(" - Session '" + _s.sessionId + "' --> ");
                        WebClient _client = new WebClient();
                        if (username.Trim () != "" || password.Trim() != "")
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
                        Console.WriteLine(" Error - " + _fnf.Status + "");
                    }
                    catch (Exception _ex)
                    {
                        Console.WriteLine(" Unknown Error - " + _ex.ToString());
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

                string _language = "ENG";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("language"))
                    _language = ParsedCommandLineArguments.ParameterDictionary["language"];

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
                            Console.WriteLine("Read Concordance Table.");

                        _ret.ReadConcordanceTable(ParsedCommandLineArguments.Transform_ConcordanceTable);
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

                if (_web.Trim() != "")
                {
                    var _sessions = RetrieveListOfSessesions(_web, _username, _password, _key);
                    UpdateFiles(_sessions, _web, _username, _password, _key, ParsedCommandLineArguments.Transform_InputFolders[0]);
                }
  
                // Iterate over all input filters

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
                          
                        using (ZipFile zip = ZipFile.Read(zfilename))
                        {
                            foreach (var entry in zip)
                            {
                                string _sessionFileName = Path.GetFileNameWithoutExtension(zip.Name);

                                using (MemoryStream zipStream = new MemoryStream())
                                {
                                    entry.ExtractWithPassword(zipStream, "");
                                    zipStream.Position = 0;
                                    try
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

                                                        List<LogDataTransformer_IB_REACT_8_12__8_13.Log_IB_8_12__8_13> _log =
                                                            LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseLogElements(_json, "IRTlibPlayer_V01");

                                                        // TODO: Add flag to extract full name (project.task) vs. short name (project)

                                                        foreach (var _l in _log)
                                                        {
                                                            var g = new logxGenericLogElement()
                                                            {
                                                                Item = _l.Element,
                                                                EventID = _l.EventID,
                                                                EventName = _l.EventName,
                                                                PersonIdentifier = _l.PersonIdentifier,
                                                                TimeStamp = _l.TimeStamp
                                                            };

                                                            g.EventDataXML = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.XmlSerializeToString(_l);
                                                            _ret.AddEvent(g);
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

                                                    var  _data = JsonConvert.DeserializeObject<MouseEvent[]>(_json);
                                                    
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

                                                        LogDataTransformer_IB_REACT_8_12__8_13.ItemScore_IB_8_12__8_13 _itemScore = 
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
                                                         
                                                    } catch (Exception _ex)
                                                    {
                                                        Console.WriteLine("Error processing ItemScore: " + _ex.ToString());
                                                    }
                                                     
                                                }
                                                else if (entry.FileName.StartsWith("Snapshot.json"))
                                                {
                                                    // TODO: Process 
                                                }
                                                else if (entry.FileName.StartsWith("Log.json"))
                                                {                                              
                                                    string _json = line;
                                                    if (_json.EndsWith(","))
                                                        _json = _json.Substring(0, _json.Length - 1);
                                                    try
                                                    {
                                                         
                                                        List<LogDataTransformer_IB_REACT_8_12__8_13.Log_IB_8_12__8_13> _log =
                                                          LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseTraceLogs(_json);
                                                         
                                                        foreach (var _l in _log)
                                                        {
                                                            _l.PersonIdentifier = _sessionFileName;
                                                            var g = new logxGenericLogElement()
                                                            {
                                                                Item = _l.Element,
                                                                EventID = _l.EventID,
                                                                EventName = _l.EventName,
                                                                PersonIdentifier = _l.PersonIdentifier,
                                                                TimeStamp = _l.TimeStamp
                                                            };

                                                            g.EventDataXML = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.XmlSerializeToString(_l);
                                                            _ret.AddEvent(g);
                                                        }

                                                    }
                                                    catch (Exception _ex)
                                                    {
                                                        Console.WriteLine("Error processing file '" + entry.FileName + "' ('" + zfilename + "'): " + _ex.Message);
                                                    }

                                                }
                                                else
                                                {
                                                    Console.WriteLine("Unknown file type: " + entry.FileName);
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
                                    catch (Exception _ex)
                                    {
                                        Console.WriteLine("Error processing file '" + entry.FileName + "': " + _ex.Message);
                                        return;
                                    }
                                }

                            }
                        }

                         
                    }

                }

                if (ParsedCommandLineArguments.Transform_OutputStata.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create ZIP archive with Stata file(s).");

                    _ret.ExportStata(ParsedCommandLineArguments.Transform_OutputStata, _language);
                }

                if (ParsedCommandLineArguments.Transform_OutputSPSS.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create ZIP archive with SPSS file(s).");

                    _ret.ExportSPSS(ParsedCommandLineArguments.Transform_OutputSPSS, _language);
                }
                 
                if (ParsedCommandLineArguments.Transform_OutputXLSX.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create XLSX file.");

                    _ret.ExportXLSX(ParsedCommandLineArguments.Transform_OutputXLSX);

                }

                if (ParsedCommandLineArguments.Transform_OutputZCSV.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create ZIP archive with CSV file(s).");

                    _ret.ExportCSV(ParsedCommandLineArguments.Transform_OutputZCSV);
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
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.ToString());
            }
        }
    }
}
