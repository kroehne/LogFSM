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
using System.Linq;
using NPOI.HPSF;
using Stateless.Graph;
#endregion

namespace LogDataTransformer_TAOPCI_V02
{
    public class LogDataTransformer_TAOPCI_Module_V02
    {
        public static void ProcessLogFilesOnly(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {
            try
            {
                bool _extractScoringFromSnapshots = false;
                if (ParsedCommandLineArguments.Flags.Contains("EXTRACTSCORINGFROMSNAPSHOTS"))
                    _extractScoringFromSnapshots = true;

                bool _extractVariablesFromSnapshots = false;
                if (ParsedCommandLineArguments.Flags.Contains("EXTRACTVARIABLESFROMSNAPSHOT"))
                    _extractVariablesFromSnapshots = true;
                

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

                #region List all ID's and Items

                List<string> _listOfIDs = new List<string>();
                List<string> _listOfItems = new List<string>();
                  
                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {

                    var jsonFilenames = Directory.EnumerateFiles(inFolder, "*.json", SearchOption.AllDirectories)
                                         .Select(filePath => Path.GetFileName(filePath))
                                         .ToList();
                    
                    try
                    {
                        var _tmpIDS = jsonFilenames
                            .Select(filename => filename.Split(new string[] { "--" }, StringSplitOptions.None)[0])
                            .Distinct()
                            .ToList();
                        _listOfIDs = _listOfIDs.Union(_tmpIDS).ToList();

                    }
                    catch (Exception ex) { }
                    try
                    {
                        var _tmpItems = jsonFilenames
                            .Select(filename => filename.Split(new string[] { "--" }, StringSplitOptions.None)[1].Replace(".json","").ReplaceCharacters(new List<char> {(char)61477,'=' }, '_'))                            
                            .Distinct()
                            .ToList();
                        _listOfItems = _listOfItems.Union(_tmpItems).ToList();

                    }
                    catch (Exception ex) { }
                }
                 
                #endregion

                #region Process Files

                logXContainer _ret = new logXContainer() { PersonIdentifierIsNumber = _personIdentifierIsNumber, PersonIdentifierName = _personIdentifier, RelativeTimesAreSeconds = _relativeTimesAreSeconds };
                _ret.LoadCodebookDictionary(ParsedCommandLineArguments.Transform_Dictionary);

                #region Snapshot Data

                if (_extractScoringFromSnapshots || _extractVariablesFromSnapshots)
                {
                    List<string> _listOfResulSnapshotFiles = new List<string>();
                    foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                    {
                        if (Directory.Exists(Path.Combine(inFolder, "TasksState")))
                        {
                            var jsonFilenames = Directory.EnumerateFiles(Path.Combine(inFolder, "TasksState"), "*.json", SearchOption.AllDirectories).ToList();
                            _listOfResulSnapshotFiles = _listOfResulSnapshotFiles.Union(jsonFilenames).ToList();
                        }
                    }

                    foreach (string jsonFile in _listOfResulSnapshotFiles)
                    {
                        try
                        {
                            var _json = File.ReadAllText(jsonFile);
                            var _person = Path.GetFileNameWithoutExtension(jsonFile).Split(new string[] { "--" }, StringSplitOptions.None)[0];
                            var _item = Path.GetFileNameWithoutExtension(jsonFile).Split(new string[] { "--" }, StringSplitOptions.None)[1].Replace(".json", "").ReplaceCharacters(new List<char> { (char)61477, '=' }, '_');
                          

                            if (_extractScoringFromSnapshots)
                            {
                                var _results = new Dictionary<string, object>();

                                Dictionary<string, string> _hitInfoDic = new Dictionary<string, string>();
                                var _jObj = FindTaskResults(JObject.Parse(_json));
                                if (_jObj != null)
                                {
                                    var _taskresults = _jObj.FirstOrDefault().FirstOrDefault();
                                    var _listOfHitInfos = _taskresults.ToList();
                                    foreach (var _hitInfo in _listOfHitInfos)
                                    {
                                        if (_hitInfo.Type == JTokenType.Property)
                                        {
                                            var prop = (JProperty)_hitInfo;
                                            string key = prop.Name;
                                            string value = prop.Value.ToString();
                                            _hitInfoDic.Add(key, value);
                                        }
                                    }

                                    // extract list of classes and
                                    // create dictionary with active/not active per hit

                                    Dictionary<string, bool> _activeHits = new Dictionary<string, bool>();
                                    Dictionary<string, string> _resultTexts = new Dictionary<string, string>();
                                    List<string> _classes = new List<string>();

                                    foreach (var i in _hitInfoDic.Keys)
                                    {
                                        if (i.StartsWith("hit."))
                                        {
                                            _activeHits.Add(i.Replace("hit.", ""), bool.Parse(_hitInfoDic[i]));
                                        }

                                        if (i.StartsWith("hitClass."))
                                        {
                                            if (!_classes.Contains(_hitInfoDic[i]))
                                                _classes.Add(_hitInfoDic[i]);
                                        }
                                        if (i.StartsWith("hitText."))
                                        {
                                            _resultTexts.Add(i.Replace("hitText.", ""), _hitInfoDic[i]);

                                        }

                                    }

                                    // create dictionary with class per hit and
                                    // count number of hits per classes  

                                    Dictionary<string, string> _hitClass = new Dictionary<string, string>();
                                    Dictionary<string, int> _numberOfActivHitsPerClass = new Dictionary<string, int>();

                                    foreach (var i in _activeHits.Keys)
                                    {
                                        _hitClass.Add(i, _hitInfoDic["hitClass." + i]);
                                        if (_activeHits[i])
                                        {
                                            if (!_numberOfActivHitsPerClass.ContainsKey(_hitInfoDic["hitClass." + i]))
                                            {
                                                _numberOfActivHitsPerClass.Add(_hitInfoDic["hitClass." + i], 1);
                                            }
                                            else
                                            {
                                                _numberOfActivHitsPerClass[_hitInfoDic["hitClass." + i]] += 1;
                                            }
                                        }
                                    }
                                    

                                    // create a dictionary with first active hit for each class

                                    Dictionary<string, string> _firstActiveHitPerClass = new Dictionary<string, string>();
                                    foreach (var i in _classes)
                                    {
                                        _firstActiveHitPerClass.Add(i, _hitInfoDic["classFirstActiveHit." + i]);
                                    }
                                 
                                    // deactive hits, if the hits are not the first active hit

                                    foreach (var k in _activeHits.Keys)
                                    {
                                        bool isActive = _activeHits[k];
                                        string belongsToClass = _hitClass[k];
                                        bool isMultipleActive = _numberOfActivHitsPerClass[belongsToClass] > 1;
                                        bool istFirstHit = _firstActiveHitPerClass[belongsToClass] == k;
                                        if (isMultipleActive && isActive && !istFirstHit)
                                            _activeHits[k] = false;
                                    }

                                    foreach (var k in _activeHits.Keys)
                                    {
                                        if (_activeHits[k])
                                        {
                                            _results.Add(_item + "_" + _hitClass[k] + ".hit", k);
                                            _results.Add(_item + "_" + _hitClass[k] + ".hitText", _resultTexts[k]);
                                        }

                                    }

                                    if (_results.Count > 0)
                                        _ret.AddResults(new logxGenericResultElement() { PersonIdentifier = _person, Results = _results });

                                }
                            }
                            if (_extractVariablesFromSnapshots)
                            {
                                var _results = new Dictionary<string, object>();

                                var _jObj = FindVariables(JObject.Parse(_json));
                                if (_jObj != null)
                                {
                                    var _variablelist = _jObj.FirstOrDefault().FirstOrDefault();
                                    var _listOfVariableValues = _variablelist.ToList();
                                    foreach (var _variable in _listOfVariableValues)
                                    {
                                        if (_variable.Type == JTokenType.Property)
                                        {
                                            var prop = (JProperty)_variable;
                                            string key = prop.Name;
                                            var obj = JObject.Parse(prop.Value.ToString());                                            
                                            string value = obj["value"].ToString();
                                            _results.Add(_item + "_" + key, value);

                                        }
                                    }

                                    if (_results.Count > 0)
                                        _ret.AddResults(new logxGenericResultElement() { PersonIdentifier = _person, Results = _results });
                                }
                            }
                        }
                        catch (Exception _ex)
                        {
                            Console.WriteLine("Unkown error " + _ex.Message);
                        }
                    }
                }


                #endregion

                #region Result Data

                if (!_extractScoringFromSnapshots)
                {
                    List<string> _listOfResulJSONFiles = new List<string>();
                    foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                    {
                        if (Directory.Exists(Path.Combine(inFolder, "ScoringResult")))
                        {
                            var jsonFilenames = Directory.EnumerateFiles(Path.Combine(inFolder, "ScoringResult"), "*.json", SearchOption.AllDirectories).ToList();
                            _listOfResulJSONFiles = _listOfResulJSONFiles.Union(jsonFilenames).ToList();
                        }
                    }

                    foreach (string jsonFile in _listOfResulJSONFiles)
                    {
                        try
                        {
                            var _json = File.ReadAllText(jsonFile);
                            var _person = Path.GetFileNameWithoutExtension(jsonFile).Split(new string[] { "--" }, StringSplitOptions.None)[0];
                            var _item = Path.GetFileNameWithoutExtension(jsonFile).Split(new string[] { "--" }, StringSplitOptions.None)[1].Replace(".json", "").ReplaceCharacters(new List<char> { (char)61477, '=' }, '_');

                            var _allresults = JsonConvert.DeserializeObject<TAOPCI_V2_ResultHelper_Root>(_json);
                            var _lastResult = _allresults.Results.OrderByDescending(element => element.Timestamp).FirstOrDefault();

                            var _results = new Dictionary<string, object>();

                            foreach (var _hit in _lastResult.Hits.Keys)
                                _results.Add(_item + "_" + _hit, _lastResult.Hits[_hit]);

                            if (_results.Count > 0)
                                _ret.AddResults(new logxGenericResultElement() { PersonIdentifier = _person, Results = _results });

                        }
                        catch (Exception _ex)
                        {
                            Console.WriteLine("Unkown error " + _ex.Message);
                        }
                    }
                }

                #endregion

                #region Log Data

                List<string> _listOfTracLogJSONFiles = new List<string>();
                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    if (Directory.Exists(Path.Combine(inFolder, "TraceLog")))
                    {
                        var jsonFilenames = Directory.EnumerateFiles(Path.Combine(inFolder, "TraceLog"), "*.json", SearchOption.AllDirectories).ToList();
                        _listOfTracLogJSONFiles = _listOfTracLogJSONFiles.Union(jsonFilenames).ToList();
                    }
                }

                foreach (string jsonFile in _listOfTracLogJSONFiles)
                {                    
                    try
                    {
                        var _json = File.ReadAllText(jsonFile);
                        var _person = Path.GetFileNameWithoutExtension(jsonFile).Split(new string[] { "--" }, StringSplitOptions.None)[0];
                        var _item = Path.GetFileNameWithoutExtension(jsonFile).Split(new string[] { "--" }, StringSplitOptions.None)[1].Replace(".json", "").ReplaceCharacters(new List<char> { (char)61477, '=' }, '_');

                        var _collection = JsonConvert.DeserializeObject<ItemBuilder_React_Runtime_trace_collectionpciV02>(_json);
                        foreach (ItemBuilder_React_Runtime_trace_element logFragment in _collection.logEntriesList)
                        {                                               
                            var _line = JsonConvert.SerializeObject(logFragment);

                            List<LogDataTransformer_IB_REACT_8_12__8_13.Log_IB_8_12__8_13> _log = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseLogElements(_line, "TAOPCI_V02", _checkEventAttrbibutes, _person);

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

                #endregion
         
                #endregion

                logXContainer.ExportLogXContainerData(ParsedCommandLineArguments, _ret);

            }
            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.Message.ToString());
            }
        }

        public static JToken FindTaskResults(JToken token)
        { 
            if (token.Type == JTokenType.Object)
            {
                var jsonObject = (JObject)token;
                if (jsonObject.ContainsKey("taskResults"))
                {
                    return jsonObject["taskResults"];
                }
                 
                foreach (var property in jsonObject.Properties())
                {
                    var result = FindTaskResults(property.Value);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            { 
                foreach (var item in (JArray)token)
                {
                    var result = FindTaskResults(item);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
             
            return null;
        }

        public static JToken FindVariables(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                var jsonObject = (JObject)token;
                if (jsonObject.ContainsKey("variables"))
                {
                    return jsonObject["variables"];
                }

                foreach (var property in jsonObject.Properties())
                {
                    var result = FindVariables(property.Value);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var item in (JArray)token)
                {
                    var result = FindVariables(item);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
   
    public class TAOPCI_V2_ResultHelper_Root
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        public string Uuid { get; set; }

        public string ItemUrl { get; set; }

        [JsonProperty("results")]
        public List<TAOPCI_V2_ResultHelper_Element> Results { get; set; }

        [JsonProperty("__v")]
        public int V { get; set; }
    }

    public class TAOPCI_V2_ResultHelper_Element
    {
        [JsonProperty("result")]
        public Dictionary<string, string> Hits { get; set; }

        public DateTime Timestamp { get; set; }
    }
 
    public static class StringExtensions
    {
        // Extension method to replace a list of characters in a string
        public static string ReplaceCharacters(this string input, IEnumerable<char> charsToReplace, char replacement)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // Convert the input string to an array of characters
            char[] chars = input.ToCharArray();

            // Replace specified characters with the replacement character
            for (int i = 0; i < chars.Length; i++)
            {
                if (charsToReplace.Contains(chars[i]))
                {
                    chars[i] = replacement;
                }
            }

            // Return the modified string
            return new string(chars);
        }
    }

}
