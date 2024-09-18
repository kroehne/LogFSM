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
using NPOI.SS.Formula.Functions;
using System.Text.RegularExpressions;
using NPOI.HSSF.Util;
#endregion

namespace LogDataTransformer_TAOPCI_V02b
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


                Dictionary<string, string> uuidFile = new Dictionary<string, string>();
 
                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    var allJsonFilenames = Directory.EnumerateFiles(inFolder, "*.json", SearchOption.AllDirectories)
                                         .ToList();

                    using (StreamWriter sw = new StreamWriter(Path.Combine(ParsedCommandLineArguments.Transform_OutputFolder, "overview.csv"), true))
                    {
                        sw.WriteLine("file\tLogin\tUUID\tContent\tPath");                  
                    }

                    foreach (var jsonFile in allJsonFilenames)
                    {
                        try
                        {
                            #region Extract 

                            var _json = File.ReadAllText(jsonFile);
                            var _obj = JObject.Parse(_json);

                            Dictionary<string, string> __jsonDataForLogin = new Dictionary<string, string>();

                            string __login = "";

                            var responseNodes = ExtractResponseNodes(_obj);

                            if (_obj.ContainsKey("raw_data"))
                            {
                                foreach (var raw_data_items in _obj["raw_data"])
                                {
                                    if (raw_data_items.GetType() == typeof(JProperty))
                                    {
                                        if (raw_data_items.Path == "raw_data.login")
                                        {
                                            __login = (raw_data_items as JProperty).Value.ToString();
                                        }
                                        else if (raw_data_items.Path == "raw_data.items")
                                        {
                                            foreach (var items in raw_data_items.Children())
                                            {
                                                foreach (var item in items.Children())
                                                {
                                                    foreach (var prop in item.Children())
                                                    {
                                                        foreach (var p in prop.Children())
                                                        {
                                                            if (p.GetType() == typeof(JProperty))
                                                            {
                                                                if ((p as JProperty).Name == "responses")
                                                                {
                                                                    string __value = (p as JProperty).Value["RESPONSE"]["value"].ToString();
                                                                    __jsonDataForLogin.Add(p.Path + ".RESPONSE.value", __value);

                                                                }
                                                                else if ((p as JProperty).Name == "state")
                                                                {

                                                                    if ((p as JProperty).Contains("RESPONSE")){
                                                                        string __state = (p as JProperty).Value["RESPONSE"]["state"].ToString();
                                                                        __jsonDataForLogin.Add(p.Path + ".RESPONSE.state", __state);

                                                                        string __response = (p as JProperty).Value["RESPONSE"]["response"].ToString();
                                                                        __jsonDataForLogin.Add(p.Path + ".RESPONSE.response", __response);
                                                                    }

                                                                   
                                                                }
                                                            }

                                                        }

                                                    }


                                                }

                                            }
                                        }
                                        else if (raw_data_items.Path == "raw_data.rawItems")
                                        {
                                            foreach (var items in raw_data_items.Children())
                                            {
                                                foreach (var prop in items.Children())
                                                {
                                                    foreach (var p in prop.Children())
                                                    {
                                                        if (p.GetType() == typeof(JProperty))
                                                        {
                                                            if ((p as JProperty).Name == "responses")
                                                            {
                                                                foreach (var r in p.Children())
                                                                {
                                                                    foreach (var x in r)
                                                                    {
                                                                        string __value = x["RESPONSE"]["value"].ToString();
                                                                        __jsonDataForLogin.Add(p.Path + ".RESPONSE.value", __value);

                                                                    }
                                                                }
                                                            }
                                                            else if ((p as JProperty).Name == "state")
                                                            {
                                                                foreach (var r in p.Children())
                                                                {
                                                                    foreach (var x in r)
                                                                    {
                                                                        if ((x as JProperty).Name == "RESPONSE")
                                                                        {
                                                                            string __value = ((x as JProperty).Value as JToken)["state"].ToString();
                                                                            __jsonDataForLogin.Add(p.Path + ".RESPONSE.state", __value);

                                                                        }

                                                                    }
                                                                }
                                                            }
                                                            else if ((p as JProperty).Name == "rawResponses")
                                                            {
                                                                foreach (var r in p.Children())
                                                                {
                                                                    foreach (var x in r)
                                                                    {
                                                                        string __value = x["value"].ToString();

                                                                        if (__value.Contains("uuid"))
                                                                        {
                                                                            __jsonDataForLogin.Add(x.Path + ".value", __value);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                            }


                            Console.WriteLine("File: " + jsonFile + " Login: " + __login);

                            foreach (var k in __jsonDataForLogin.Keys)
                            {
                                string _uuid = ExtractUuid(__jsonDataForLogin[k]);

                                if (_uuid == "")
                                {
                                    string _value = __jsonDataForLogin[k];
                                }

                                if (!uuidFile.ContainsKey(_uuid))
                                    uuidFile.Add(_uuid, __login + "|" + jsonFile);

                                string _content = "";

                                if (__jsonDataForLogin[k].Contains("componentsState"))
                                {
                                    _content += " State";
                                }
                                if (__jsonDataForLogin[k].Contains(".hitText"))
                                {
                                    _content += " Scoring";
                                }
                                if (__jsonDataForLogin[k].Contains("entryId"))
                                {
                                    _content += " Traces";
                                }

                                string _line = Path.GetFileNameWithoutExtension(jsonFile) + "\t" + __login + "\t" + _uuid + "\t" + _content + "\t" + k;
                                using (StreamWriter sw = new StreamWriter(Path.Combine(ParsedCommandLineArguments.Transform_OutputFolder, "overview.csv"), true))
                                {
                                    sw.WriteLine(_line);
                                }

                                Console.Write(" - " + k + " | " + _uuid + ": " + _content);
                                Console.WriteLine();


                            }
                            #endregion
                        }
                        catch (Exception _ex)
                        {
                            Console.WriteLine(_ex.ToString());
                        }

                     
                    }



                }

                logXContainer _ret = new logXContainer() { PersonIdentifierIsNumber = _personIdentifierIsNumber, PersonIdentifierName = _personIdentifier, RelativeTimesAreSeconds = _relativeTimesAreSeconds };
                _ret.LoadCodebookDictionary(ParsedCommandLineArguments.Transform_Dictionary);
                logXContainer.ExportLogXContainerData(ParsedCommandLineArguments, _ret);

            }
            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.Message.ToString());
            }
        }

        public static List<JToken> ExtractResponseNodes(JObject obj)
        {
            List<JToken> responseNodes = new List<JToken>();

            // Navigate through the JSON hierarchy
            var rawData = obj["raw_data"];
            if (rawData != null)
            {
                var items = rawData["items"];
                if (items != null)
                {
                    foreach (var item in items.Children())
                    {
                        foreach (var state in item.Children())
                        {
                            if (state != null)
                            {
                                var responses = state["responses"];
                                if (responses != null)
                                {
                                    foreach (var response in responses.Children())
                                    {
                                        foreach (var x in response.Children())
                                        {
                                            responseNodes.Add(x);
                                        }
                                        
                                                                        
                                    }
                                }
                            }
                        } 
                    }
                }
            }

            return responseNodes;
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

        public static string ExtractUuid(string jsonString)
        {
            jsonString = jsonString.Replace("\"\"", "\"").Replace("\\\"", "\"");
            string pattern = @"""uuid"":\s*""([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})""";
            Regex regex = new Regex(pattern); 
            System.Text.RegularExpressions.Match match = regex.Match(jsonString);             
            if (match.Success)
            {
                return match.Groups[1].Value;
            } 
            return "";
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
