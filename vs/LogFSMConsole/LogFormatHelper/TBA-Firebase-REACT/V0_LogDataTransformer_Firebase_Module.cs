#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using LogFSM_LogX2019;
using LogFSMConsole;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Text;
# endregion

namespace LogDataTransformer_Firebase_V01
{

    public class LogDataTransformer_Firebase_Module_V01
    {
        public static List<TokenTestTuple> RetrieveListOfSessesions(string web, string username, string password, string key)
        {

            string _tokenListJson = "";
            using (var wb = new WebClient())
            {
                var _tokenListQuery = new NameValueCollection();
                _tokenListQuery["query"] = "{ getCodes { code, testId } } ";

                var _tokenListResponse = wb.UploadValues(web, "POST", _tokenListQuery);
                _tokenListJson = Encoding.UTF8.GetString(_tokenListResponse);
            }

            List<TokenTestTuple> _allAvailableTokens = new List<TokenTestTuple>();

            var _jsonObjectAvailableTokens = JsonDocument.Parse(_tokenListJson);
            var _rootElementAvailableTokens = _jsonObjectAvailableTokens.RootElement.GetProperty("data").GetProperty("getCodes").EnumerateArray();

            foreach (var _node in _rootElementAvailableTokens)
            {
                _allAvailableTokens.Add(new TokenTestTuple()
                {
                    code = _node.GetProperty("code").ToString(),
                    testId = _node.GetProperty("testId").ToString()
                });
            }

            return _allAvailableTokens;
        }

        public static void UpdateFiles(List<TokenTestTuple> _sessions, string web, string username, string password, string key, string folder, string mask, bool overwrite)
        {
            foreach (var _token in _sessions)
            {
                if (overwrite || !File.Exists(folder + _token.code + ".zip"))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                        {

                            #region State Data

                            try
                            {
                                using (var wb = new WebClient())
                                {
                                    var _stateDataQuery = new NameValueCollection();
                                    _stateDataQuery["query"] = "query {getJsonByCodes(codes: [\"" + _token.code + "\"], where: { type: STATE}){ code, data{ json, item, timestamp} }}";

                                    var _stateDataResponse = wb.UploadValues(web, "POST", _stateDataQuery);
                                    var _stateDataResponseString = Encoding.UTF8.GetString(_stateDataResponse);

                                    var snapshotFile = archive.CreateEntry("Snapshot.json");
                                    using (var entryStream = snapshotFile.Open())
                                    using (var streamWriter = new StreamWriter(entryStream))
                                    {
                                        var _jsonObjectSnapshotData = JsonDocument.Parse(_stateDataResponseString);
                                        var _rootElementForSnapshotData = _jsonObjectSnapshotData.RootElement.GetProperty("data").GetProperty("getJsonByCodes").EnumerateArray();

                                        foreach (var _snapshotElement in _rootElementForSnapshotData)
                                        {
                                            var _dataSnapshotPerCode = _snapshotElement.GetProperty("data").EnumerateArray();
                                            foreach (var _snapshotNode in _dataSnapshotPerCode)
                                            {
                                                var _snapshot = new JsonItemTimestampeTriple()
                                                {
                                                    item = _snapshotNode.GetProperty("item").ToString(),
                                                    task = "Task01",   // TODO: Add task name to firebase?!
                                                    json = _snapshotNode.GetProperty("json").ToString(),
                                                    timtestamp = DateTime.Parse(_snapshotNode.GetProperty("timestamp").ToString()),
                                                    personidentifier = _token.code,
                                                };
                                                streamWriter.WriteLine(System.Text.Json.JsonSerializer.Serialize(_snapshot) + ",");
                                            }
                                        }

                                    }
                                }

                            }
                            catch (Exception _ex)
                            {
                                Console.WriteLine("Exception: " + _ex.ToString());
                            }

                            #endregion

                            #region ItemScore Data

                            try
                            {
                                Dictionary<string, JsonItemTimestampeTriple> lastItemScorePerItem = new Dictionary<string, JsonItemTimestampeTriple>();

                                using (var wb = new WebClient())
                                {
                                    var _scoreDataQuery = new NameValueCollection();
                                    _scoreDataQuery["query"] = "query {getJsonByCodes(codes: [\"" + _token.code + "\"], where: { type: SCORE}){ code, data{ json, item, timestamp} }}";

                                    var _scoreDataResponse = wb.UploadValues(web, "POST", _scoreDataQuery);
                                    var _scoreDataResponseString = Encoding.UTF8.GetString(_scoreDataResponse);

                                    var itemscoreFileAll = archive.CreateEntry("ItemScore_All.json");
                                    using (var entryStream = itemscoreFileAll.Open())
                                    using (var streamWriter = new StreamWriter(entryStream))
                                    {
                                        var _jsonObjectItemScoreData = JsonDocument.Parse(_scoreDataResponseString);
                                        var _rootElementForItemScoretData = _jsonObjectItemScoreData.RootElement.GetProperty("data").GetProperty("getJsonByCodes").EnumerateArray();

                                        foreach (var _itemScoreElement in _rootElementForItemScoretData)
                                        {
                                            var _itemScorePerCode = _itemScoreElement.GetProperty("data").EnumerateArray();
                                            foreach (var _itemScoreNode in _itemScorePerCode)
                                            {
                                                var _itemScore = new JsonItemTimestampeTriple()
                                                {
                                                    item = _itemScoreNode.GetProperty("item").ToString(),
                                                    task = "Task01", // TODO: Add task name to firebase?!
                                                    json = _itemScoreNode.GetProperty("json").ToString(),
                                                    timtestamp = DateTime.Parse(_itemScoreNode.GetProperty("timestamp").ToString()),
                                                    personidentifier = _token.code,
                                                };

                                                if (!lastItemScorePerItem.ContainsKey(_itemScore.item + "." + _itemScore.task))
                                                {
                                                    lastItemScorePerItem.Add(_itemScore.item + "." + _itemScore.task, _itemScore);
                                                }
                                                else
                                                {
                                                    if (lastItemScorePerItem[_itemScore.item + "." + _itemScore.task].timtestamp > _itemScore.timtestamp)
                                                        lastItemScorePerItem[_itemScore.item + "." + _itemScore.task] = _itemScore;
                                                }

                                                streamWriter.WriteLine(System.Text.Json.JsonSerializer.Serialize(_itemScore) + ",");
                                            }
                                        }

                                    }

                                    var itemscoreFile = archive.CreateEntry("ItemScore.json");
                                    using (var entryStream = itemscoreFile.Open())
                                    using (var streamWriter = new StreamWriter(entryStream))
                                    {
                                        foreach (var _key in lastItemScorePerItem.Keys)
                                        {
                                            streamWriter.WriteLine(System.Text.Json.JsonSerializer.Serialize(lastItemScorePerItem[_key]) + ",");
                                        }
                                    }
                                }

                            }
                            catch (Exception _ex)
                            {
                                Console.WriteLine("Exception: " + _ex.ToString());
                            }

                            #endregion

                            #region Trace Data

                            try
                            {
                                List<TraceDatePoint> traceDataList = new List<TraceDatePoint>();

                                string _logDataJson = "";
                                using (var wb = new WebClient())
                                {
                                    var _logDataQuery = new NameValueCollection();
                                    _logDataQuery["query"] = "query {getJsonByCodes(codes: [\"" + _token.code + "\"], where: { type: LOG}){ code, data{ json, item, timestamp} }}";

                                    var _logDataResponse = wb.UploadValues(web, "POST", _logDataQuery);
                                    _logDataJson = Encoding.UTF8.GetString(_logDataResponse);
                                }

                                var _jsonObjectLogData = JsonDocument.Parse(_logDataJson);
                                var _rootElementForLogData = _jsonObjectLogData.RootElement.GetProperty("data").GetProperty("getJsonByCodes").EnumerateArray();

                                var itemscoreFile = archive.CreateEntry("Trace.json");
                                using (var entryStream = itemscoreFile.Open())
                                using (var streamWriter = new StreamWriter(entryStream))
                                {
                                    foreach (var _dataPerCode in _rootElementForLogData)
                                    {
                                        string code = _dataPerCode.GetProperty("code").ToString();
                                        var _dataEventsPerCode = _dataPerCode.GetProperty("data").EnumerateArray();
                                        foreach (var _eventNode in _dataEventsPerCode)
                                        {
                                            var _tracedata = new TraceDatePoint()
                                            {
                                                code = _token.code,
                                                item = _eventNode.GetProperty("item").ToString(),
                                                task = "Task01",    // TODO: Add task name to firebase?!
                                                trace = _eventNode.GetProperty("json").ToString(),
                                                test = _token.testId,
                                                timtestamp = _eventNode.GetProperty("timestamp").ToString()
                                            };
                                            streamWriter.WriteLine(System.Text.Json.JsonSerializer.Serialize(_tracedata) + ",");
                                        }

                                    }
                                }


                            }
                            catch (Exception _ex)
                            {
                                Console.WriteLine("Exception: " + _ex.ToString());
                            }

                            #endregion

                        }

                        using (var fileStream = new FileStream(folder + _token.code + ".zip", FileMode.Create))
                        {
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            memoryStream.CopyTo(fileStream);
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

                        _ret.ReadConcordanceTable(ParsedCommandLineArguments.Transform_ConcordanceTable, ParsedCommandLineArguments.Verbose);
                    }
                }

                // Update from Server if requested

                string _web = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_JOB_TRANSFORM_web))
                    _web = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_JOB_TRANSFORM_web];

                string _username = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_JOB_TRANSFORM_user))
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
                            using (Ionic.Zip.ZipFile zip = Ionic.Zip.ZipFile.Read(zfilename))
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

                                                            List<LogDataTransformer_IB_REACT_8_12__8_13.Log_IB_8_12__8_13> _log = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseLogElements(_json, "Firebase_V01", _checkEventAttrbibutes, _sessionFileName);

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
                                                    else if (entry.FileName.StartsWith("ItemScore.json"))
                                                    {
                                                        string _json = line;
                                                        if (_json.EndsWith(","))
                                                            _json = _json.Substring(0, _json.Length - 1);

                                                        try
                                                        {
                                                            JsonItemTimestampeTriple _itemScoreEvent =
                                                                JsonConvert.DeserializeObject<JsonItemTimestampeTriple>(_json);

                                                            string task = _itemScoreEvent.task;
                                                            string item = _itemScoreEvent.item;
                                                            string personIdentifier = _itemScoreEvent.personidentifier;

                                                            var _itemScorePackage = JsonConvert.DeserializeObject<LogDataTransformer_IB_REACT_8_12__8_13.ItemBuilder_React_Runtime_itemscore_package>(_itemScoreEvent.json);

                                                            if (_itemScorePackage.result != null)
                                                            {
                                                                LogDataTransformer_IB_REACT_8_12__8_13.itemScore _itemScore =
                                                                   LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseItemScore(_itemScorePackage.result.ToString(), task, item, personIdentifier);

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

                                                        string personIdentifier = _sessionFileName;
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
                                                            catch { }
                                                        }

                                                        _ret.AddResults(g);
                                                    }
                                                    else if (entry.FileName.StartsWith("ItemScore_All.json"))
                                                    {
                                                        // Ignore this file
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


    public class TraceDatePoint
    {
        public string trace { get; set; }
        public string item { get; set; }
        public string task { get; set; }
        public string code { get; set; }
        public string test { get; set; }
        public string timtestamp { get; set; }
    }

    public class JsonItemTimestampeTriple
    {
        public string item { get; set; }
        public string task { get; set; }
        public DateTime timtestamp { get; set; }
        public string json { get; set; }
        public string personidentifier { get; set; }
    }
    public class TokenTestTuple
    {
        public string code { get; set; }
        public string testId { get; set; }
    }
}
