#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Ionic.Zip;
using LogDataTransformer_IB_REACT_8_12__8_13;
using LogFSM_LogX2019;
using LogFSMConsole;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using StataLib;
# endregion

namespace LogDataTransformer_IRTlibPlayer_V01
{
    public class LogDataTransformer_ALEA_Module_V01
    { 
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

                 

                string _mask = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_mask))
                    _mask = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_mask];

                bool _override = false;
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_overwrite))
                    bool.TryParse(ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_overwrite], out _override);
  
                // Iterate over all input folders

                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    if (!Directory.Exists(inFolder))
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Warning: Directory not exists: '" + inFolder + "'.");

                        continue;
                    }

                    string[] listOfSubFolder = Directory.GetDirectories(inFolder, ParsedCommandLineArguments.Mask, SearchOption.TopDirectoryOnly);
                    foreach (string dfolder in listOfSubFolder)
                    {
                        RunsJSON _runsJSON = null;
                        SessionJSON[] _sessionsJSON = null;
                        ScoringsJSONScoring[] _scoringsJSON = null;

                        if (File.Exists(Path.Combine(dfolder, "run.json")))
                        {
                            string jsonString = File.ReadAllText(Path.Combine(dfolder, "run.json"));
                            _runsJSON = System.Text.Json.JsonSerializer.Deserialize<RunsJSON>(jsonString);

                        }
                        if (File.Exists(Path.Combine(dfolder, "sessions.json")))
                        {
                            string jsonString = File.ReadAllText(Path.Combine(dfolder, "sessions.json"));
                            _sessionsJSON = System.Text.Json.JsonSerializer.Deserialize<SessionJSON[]>(jsonString);
                        }
                        if (File.Exists(Path.Combine(dfolder, "scorings.json")))
                        {
                            string jsonString = File.ReadAllText(Path.Combine(dfolder, "scorings.json"));
                            _scoringsJSON = System.Text.Json.JsonSerializer.Deserialize<ScoringsJSONScoring[]>(jsonString);
                        }


                        if (_sessionsJSON != null && _runsJSON != null)
                        {
                            if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                            {
                                if (ParsedCommandLineArguments.Verbose)
                                    Console.WriteLine("Info: Max number of cases reached.");
                                break;
                            }

                            if (ParsedCommandLineArguments.Verbose)
                                Console.WriteLine("Info: Read Folder '" + dfolder + "' ");


                            foreach (var _session in _sessionsJSON) 
                            { 
                                foreach (var _trace in _session.TaceLogs)
                                { 
                                  if (_trace.Content != "")
                                    { 
                                        ItemBuilder_React_Runtime_trace[] logFragmentList = JsonConvert.DeserializeObject<ItemBuilder_React_Runtime_trace[]>(_trace.Content, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore, MaxDepth = 512 });
                                        foreach (var logFragment in  logFragmentList)
                                        {
                                            var _line = JsonConvert.SerializeObject(logFragment);
                                            List<LogDataTransformer_IB_REACT_8_12__8_13.Log_IB_8_12__8_13> _log = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseLogElements(_line, "Alea_V01", _checkEventAttrbibutes, _session.RunId + "_" + _session.Login);

                                            foreach (var _l in _log)
                                            {
                                                if (_l.EventName == "")
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
                                        

                                    }

                                }

                            }

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

    public class RunsJSON
    {
        [JsonPropertyName("RunId")]
        public string RunId { get; set; }

        [JsonPropertyName("StudyId")]
        public string StudyId { get; set; }

        [JsonPropertyName("GroupId")]
        public string GroupId { get; set; }

        [JsonPropertyName("Scheduled")]
        public DateTime Scheduled { get; set; }

        [JsonPropertyName("Expires")]
        public DateTime Expires { get; set; }

        [JsonPropertyName("ClassLevel")]
        public List<int> ClassLevel { get; set; }

        [JsonPropertyName("Students")]
        public List<RunsJSONStudent> Students { get; set; }

        [JsonPropertyName("UserId")]
        public string UserId { get; set; }

        [JsonPropertyName("TimeStamp")]
        public DateTime TimeStamp { get; set; }
    }

    public class RunsJSONStudent
    {
        [JsonPropertyName("StudentId")]
        public string StudentId { get; set; }

        [JsonPropertyName("Login")]
        public string Login { get; set; }

        [JsonPropertyName("SozioDemoFlag")]
        public bool SozioDemoFlag { get; set; }
    }

    public class SessionJSON
    {
        [JsonProperty("RunId")]
        public string RunId { get; set; }

        [JsonProperty("Login")]
        public string Login { get; set; }

        [JsonProperty("TaceLogs")]
        public List<SessionsJSONTraceLog> TaceLogs { get; set; } = new List<SessionsJSONTraceLog>();

        [JsonProperty("Snapshots")]
        public List<SessionsJSONSnapshot> Snapshots { get; set; } = new List<SessionsJSONSnapshot>();

        [JsonProperty("ScoringResults")]
        public List<SessionsJSONScoringResult> ScoringResults { get; set; } = new List<SessionsJSONScoringResult>();
    }

    public class SessionsJSONTraceLog
    {
        [JsonProperty("CreationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty("IsAssessmentComplete")]
        public bool IsAssessmentComplete { get; set; }

        [JsonProperty("IsAssessmentTransmissionComplete")]
        public bool IsAssessmentTransmissionComplete { get; set; }

        [JsonProperty("Content")]
        public string Content { get; set; }
    }

    public class SessionsJSONSnapshot
    {
        [JsonProperty("CreationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty("Content")]
        public string Content { get; set; }
    }

    public class SessionsJSONScoringResult
    {
        [JsonProperty("RuntimeScoringResult")]
        public string RuntimeScoringResult { get; set; }

        [JsonProperty("CreationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty("ControllerVersion")]
        public string ControllerVersion { get; set; }

        [JsonProperty("Task")]
        public string Task { get; set; }

        [JsonProperty("Item")]
        public string Item { get; set; }

        [JsonProperty("Scope")]
        public string Scope { get; set; }
    }

    public class ScoringsJSON
    {
        public string RunId { get; set; }
        public string Login { get; set; }
        public ScoringsJSONScoredSubTask ScoredSubTask { get; set; }
        public string UserId { get; set; }
        public DateTime TimeStamp { get; set; }
    }
     
    public class ScoringsJSONScoredSubTask
    {
        public string CodeBookEntryId { get; set; }
        public int SubTaskNumber { get; set; }
        public bool IsRawResult { get; set; }
        public bool IsUnknown { get; set; }
        public bool ContainsPersonalOrOffensiveContent { get; set; }
        public string Comment { get; set; }
        public ScoringsJSONScoring Scoring { get; set; }
    }

    // Class for the Scoring object nested inside ScoredSubTask
    public class ScoringsJSONScoring
    {
        public string HitCondition { get; set; }
        public string ResultText { get; set; }
    }
}
