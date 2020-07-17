namespace LogFSMConsole
{

    #region usings

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Stateless;
    using Stateless.Graph;
    using LogFSMShared;
    using LogFSM;
    using System.Diagnostics;
    using System.Data;
    using Ionic.Zip;
    using Newtonsoft.Json;
    using System.Threading.Tasks;
    using System.Globalization;

    #endregion

    public class Program
    {

        public static void Main(string[] args)
        {

#if DEBUG
            args = new string[1];
            args[0] = @"";
 #endif

            Stopwatch _watch = new Stopwatch();
            _watch.Start();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
             
            string[] _assemblyFullName = Assembly.GetExecutingAssembly().FullName.Split(',');
            CommandLineArguments _parsedCommandLineArguments = new CommandLineArguments(args, _assemblyFullName[0], _assemblyFullName[1].Split('=')[1]);

            if (_parsedCommandLineArguments.Job == "info")
            {
                Console.WriteLine("OK;" + _parsedCommandLineArguments.Version);
                return;
            }

#if DEBUG

            _parsedCommandLineArguments.RuntimePath = @"C:\work\github\LogFSM\vs\bin\dist\win-x64\";
            _parsedCommandLineArguments.IsDebug = true;

#endif

            _parsedCommandLineArguments.PrintWelcomeMessage();
             
            #region Check Arguments 

            if (!_parsedCommandLineArguments.CheckCommandLineArguments())
            { 
                _watch.Stop();
                Console.WriteLine("Application stopped.");
                Console.WriteLine("Time elapsed: {0}", _watch.Elapsed);
                return;
            }

            #endregion

            #region Prepare Default Files 

            if (!_parsedCommandLineArguments.PrepareDefaulfFiles())
            {
                _watch.Stop();
                Console.WriteLine("Application stopped.");
                Console.WriteLine("Time elapsed: {0}", _watch.Elapsed);
                return;
            }

            #endregion

            #region Run Job

            if (_parsedCommandLineArguments.Job == CommandLineArguments._CMDA_JOB_fsm_default)
            {
                runJobFSM(_watch, _parsedCommandLineArguments);
            }
            else if (_parsedCommandLineArguments.Job == CommandLineArguments._CMDA_JOB_prepare)
            {
                runJobPrepare(_watch, _parsedCommandLineArguments);
            }
            else if (_parsedCommandLineArguments.Job == CommandLineArguments._CMDA_JOB_transform)
            {
                LogDataTransformer.RunJobTransform(_watch, _parsedCommandLineArguments);
            }

            #endregion

            _watch.Stop();
            Console.WriteLine("Time elapsed: {0}", _watch.Elapsed);

        }
         
        private static void runJobFSM(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {

            #region Generate FSM

            Assembly _compiledFSMAssembly = null;
            IFSMSyntaxReader _syntax = null;


            if (ParsedCommandLineArguments.FSMFileTypeIsCSharp)
            {
                // TODO Add feature to compile existing fsm source code

                // FSMCompiler _compiler = new FSMCompiler(ParsedCommandLineArguments.FSMFileName, "LogFSMTests", "LogFSMTests");
                // _compiledFSMAssembly = _compiler.GetAssembly();

            }
            else if (ParsedCommandLineArguments.FSMFileTypeIsCustom01)
            {

                var _fsmSyntax = File.ReadAllText(ParsedCommandLineArguments.FSMFileName);
                 _syntax = new FSMSimpleSyntax(_fsmSyntax);

                try
                {
                    string _lastFSMJson = JsonConvert.SerializeObject(_syntax, Newtonsoft.Json.Formatting.Indented);
                    File.AppendAllText(Path.Combine(ParsedCommandLineArguments.OutputPath, "logfsmlastfsmjson.txt"), _lastFSMJson);
                }
                catch (Exception _ex)
                {
                    File.AppendAllText(Path.Combine(ParsedCommandLineArguments.OutputPath, "logfsmlasterror.txt"), "Error: " + _ex.ToString());
                    Watch.Stop();
                    Console.WriteLine("Error writing to file '" + Path.Combine(ParsedCommandLineArguments.OutputPath, "logfsmlastfsmjson.txt") + "'. See 'logfsmlasterror' for details.");
                    Console.WriteLine("Application stopped.");
                    Console.WriteLine("Time elapsed: {0}", Watch.Elapsed);
                    return;
                }
   
                #region Analytically check FSM

                if (!ParsedCommandLineArguments.CheckFSMCustom01(_syntax))
                {
                    Watch.Stop();
                    Console.WriteLine("Error in the fsm definition. See 'logfsmlasterror' for details.");
                    Console.WriteLine("Application stopped.");
                    Console.WriteLine("Time elapsed: {0}", Watch.Elapsed);
                    return;
                }

                #endregion

                FSMFactory _factory = new FSMFactory(Path.Combine(ParsedCommandLineArguments.OutputPath, "logfsmlastsource.cs"), 
                    "LogFSMTests", "LogFSMTests", _syntax.NumberOfMachines, ParsedCommandLineArguments);

                _factory.DefineStates(_syntax.States, _syntax.Start, _syntax.End);
                _factory.DefineTriggers(_syntax.Triggers);
                _factory.CreateTriggerDefinition(_syntax.Triggers);
                _factory.CreateRules(_syntax.Transitions, _syntax.Ignores, _syntax.Operators);

                _compiledFSMAssembly = _factory.GetAssembly();
         
                if (_compiledFSMAssembly == null || _factory.HasErrors)
                {
                    Console.WriteLine("Error compiling FSM.");
                    Watch.Stop();
                    Console.WriteLine("Application stopped.");
                    Console.WriteLine("Time elapsed: {0}", Watch.Elapsed);
                    return;
                }

            }

            #endregion

            #region Try to ceate instance of the compiled fsm
            try
            {
                ILogFSM _myGeneratedFSM = (ILogFSM)_compiledFSMAssembly.CreateInstance("LogFSMTests" + "." + "LogFSMTests");
                if (_myGeneratedFSM == null)
                    throw new Exception("Class is null.");

            }
            catch (Exception _ex)
            {
                File.AppendAllText(Path.Combine(ParsedCommandLineArguments.OutputPath, "logfsmlasterror.txt"), "Error: " + _ex.ToString());
                Console.WriteLine("Error instantiating the compiled fsm. See 'logfsmlasterror' for details.");
                Watch.Stop();
                Console.WriteLine("Application stopped.");
                Console.WriteLine("Time elapsed: {0}", Watch.Elapsed);
                return;
            }
            #endregion

            #region Run FSM

            Console.WriteLine("Preparation complete.");
            Console.WriteLine("Time elapsed: {0}", Watch.Elapsed);

            ParsedCommandLineArguments.currentNumberOfPersons = 0;
            List<string> _temporaryResultFiles = new List<string>();
            List<EventData> _logData = new List<EventData>();

            EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.Time;

            if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                sort = EventDataListExtension.ESortType.None;
            else if (ParsedCommandLineArguments.Flags.Contains("ORDER_WITHIN_ELEMENTS"))
                sort = EventDataListExtension.ESortType.ElementAndTime;

            if (ParsedCommandLineArguments.DataFileTypeIsEE || ParsedCommandLineArguments.DataFileTypeIsLogFSMJson || ParsedCommandLineArguments.DataFileTypeIsJsonLite)
            {
                ILogFSM _myGeneratedFSM = (ILogFSM)_compiledFSMAssembly.CreateInstance("LogFSMTests" + "." + "LogFSMTests");

                if (ParsedCommandLineArguments.DataFileTypeIsEE)
                {
                    _logData = LogDataReader.ReadLogDataEE(ParsedCommandLineArguments.DataFileName, ParsedCommandLineArguments.RelativeTime, sort);
                }
                else if (ParsedCommandLineArguments.DataFileTypeIsLogFSMJson)
                {
                    _logData = LogDataReader.ReadLogDataLogFSMJson(ParsedCommandLineArguments.DataFileName, ParsedCommandLineArguments.RelativeTime, sort);
                }
                else if (ParsedCommandLineArguments.DataFileTypeIsJsonLite)
                {
                    _logData = LogDataReader.ReadLogDataJsonLite(ParsedCommandLineArguments.DataFileName, ParsedCommandLineArguments.RelativeTime, sort);
                }
                _temporaryResultFiles.AddRange(ProcessLogData(ParsedCommandLineArguments, _myGeneratedFSM, _logData));
            }
            else if (ParsedCommandLineArguments.DataFileTypeIsEEZip || ParsedCommandLineArguments.DataFileTypeIsLogFSMJsonZip || ParsedCommandLineArguments.DataFileTypeIsJsonLiteZip)
            {
                using (ZipFile zip = ZipFile.Read(ParsedCommandLineArguments.ZIPFileName))
                {
                    if (ParsedCommandLineArguments.DataFileName != "")
                    {
                        ILogFSM _myGeneratedFSM = (ILogFSM)_compiledFSMAssembly.CreateInstance("LogFSMTests" + "." + "LogFSMTests");
                        ZipEntry entry = zip[ParsedCommandLineArguments.DataFileName];
                        ExtractAndProcess(ParsedCommandLineArguments, entry, _logData, _temporaryResultFiles, _myGeneratedFSM, sort);
                    }
                    else if (ParsedCommandLineArguments.DataFileFilter != "")
                    {

                        /*
                        Parallel.ForEach(zip.Entries, (entry, state) =>
                        {
                            if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && ParsedCommandLineArguments.currentNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                                state.Break();
                            
                            if (CommandLineArguments.FitsMask(entry.FileName, ParsedCommandLineArguments.DataFileFilter))
                            {
                                ILogFSM _myGeneratedFSM = (ILogFSM)_compiledFSMAssembly.CreateInstance("LogFSMTests" + "." + "LogFSMTests");
                                ExtractAndProcess(ParsedCommandLineArguments, entry, _logData, _temporaryResultFiles, _myGeneratedFSM);
                            }
                        });
                         */
                         
                        foreach (var entry in zip)
                        {
                            if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && ParsedCommandLineArguments.currentNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                                break;
                            if (CommandLineArguments.FitsMask(entry.FileName, ParsedCommandLineArguments.DataFileFilter))
                            {
                                ILogFSM _myGeneratedFSM = (ILogFSM)_compiledFSMAssembly.CreateInstance("LogFSMTests" + "." + "LogFSMTests");
                                ExtractAndProcess(ParsedCommandLineArguments, entry, _logData, _temporaryResultFiles, _myGeneratedFSM, sort);
                            }

                        }
                       
                    }
                    else
                    {
                        foreach (var entry in zip)
                        {
                            if (ParsedCommandLineArguments.MaxNumberOfCases >= 1 && ParsedCommandLineArguments.currentNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                                break;
                             
                            ILogFSM _myGeneratedFSM = (ILogFSM)_compiledFSMAssembly.CreateInstance("LogFSMTests" + "." + "LogFSMTests");
                            ExtractAndProcess(ParsedCommandLineArguments, entry, _logData, _temporaryResultFiles, _myGeneratedFSM, sort);
                        }
                    }
                }
            }

            #endregion

            Console.WriteLine("FSM complete.");
            Console.WriteLine("Time elapsed: {0}", Watch.Elapsed);
            Console.WriteLine("Save data.");

            #region Combine Results
            if (_temporaryResultFiles.Count == 1)
            {
                 
                #region . Single PersonIdentifier / Result File
                try
                {
                    File.Copy(_temporaryResultFiles[0], ParsedCommandLineArguments.OutFileName, true);
                    File.Delete(_temporaryResultFiles[0]);
                }
                catch (Exception _ex)
                {
                    File.AppendAllText(Path.Combine(ParsedCommandLineArguments.OutputPath, "logfsmlasterror.txt"), "Error: " + _ex.ToString());
                    Console.WriteLine("Error moving temp file. See 'logfsmlasterror' for details.");
                    Watch.Stop();
                    Console.WriteLine("Application stopped.");
                    Console.WriteLine("Time elapsed: {0}", Watch.Elapsed);
                    return;
                }
                #endregion
            }
            else
            {
                #region . Multiple PersonIdentifiers / Result Files
                try
                {

                    Console.Write("Combine " + _temporaryResultFiles.Count + " results: ");
                    LogFSMResultJSON _combinedResults = new LogFSMResultJSON();
                    int _progress = 0;

                    Dictionary<Tuple<int, string, string>, int> _transitionFrequencyTotalsTableCounter = new Dictionary<Tuple<int, string, string>, int>();
                    //               ^ Machine
                    //                       ^ From
                    //                               ^To      ^Frequency

   

                    for (int i = 0; i < _temporaryResultFiles.Count; i += 1)
                    {
                        string _json = File.ReadAllText(_temporaryResultFiles[i]);
                        LogFSMResultJSON _singleResults = JsonConvert.DeserializeObject<LogFSMResultJSON>(_json);

                        foreach (var key in _singleResults.Tables.Keys)
                        {
                            if (!_combinedResults.Tables.ContainsKey(key))
                                _combinedResults.Tables.Add(key, new DataTable());

                            _combinedResults.Tables[key].Merge(_singleResults.Tables[key], false, MissingSchemaAction.Add);
                            
                            if (!ParsedCommandLineArguments.Flags.Contains("SKIPMERGINGTRANSITIONFREQUENCY")) //TODO: Document Flag
                            {
                                #region Combine TransitionFrequencyTable --> TransitionFrequencyTotalTable
                                if (key.StartsWith("TransitionFrequencyTable_", StringComparison.InvariantCulture))
                                {
                                    int _machine = int.Parse(key.Replace("TransitionFrequencyTable_", ""));
                                    var _tab = _singleResults.Tables[key];
                                    foreach (DataRow row in _tab.Rows)
                                    {
                                        Tuple<int, string, string> _transitionFrequencyTotalsTableCounterKey = new Tuple<int, string, string>(_machine, row["From"].ToString(), row["To"].ToString());
                                        int _value = int.Parse(row["Frequency"].ToString());
                                        if (!_transitionFrequencyTotalsTableCounter.ContainsKey(_transitionFrequencyTotalsTableCounterKey))
                                            _transitionFrequencyTotalsTableCounter.Add(_transitionFrequencyTotalsTableCounterKey, _value);
                                        else
                                            _transitionFrequencyTotalsTableCounter[_transitionFrequencyTotalsTableCounterKey] += _value;
                                    }
                                }
                            }
                            #endregion


                        }
                        if ((int)Math.Round((double)(100 * i) / _temporaryResultFiles.Count) > _progress)
                        {
                            _progress = (int)Math.Round((double)(100 * i) / _temporaryResultFiles.Count);
                            Console.Write("|");
                        }
                      
                    }

                    if (!ParsedCommandLineArguments.Flags.Contains("SKIPMERGINGTRANSITIONFREQUENCY")) //TODO: Document Flag
                    {
                        #region Create "TransitionFrequencyTotalsTable"

                        DataTable _transitionFrequencyTotalsTable = new DataTable("TransitionFrequencyTotalsTable");

                        DataColumn _machineTransitionFrequencyTotalsTable = new DataColumn();
                        _machineTransitionFrequencyTotalsTable.DataType = System.Type.GetType("System.String");
                        _machineTransitionFrequencyTotalsTable.ColumnName = "Machine";
                        _machineTransitionFrequencyTotalsTable.Caption = "Machine";
                        _machineTransitionFrequencyTotalsTable.ReadOnly = false;
                        _machineTransitionFrequencyTotalsTable.Unique = false;
                        _transitionFrequencyTotalsTable.Columns.Add(_machineTransitionFrequencyTotalsTable);

                        DataColumn _stateNameFrom = new DataColumn();
                        _stateNameFrom.DataType = System.Type.GetType("System.String");
                        _stateNameFrom.ColumnName = "From";
                        _stateNameFrom.Caption = "From";
                        _stateNameFrom.ReadOnly = false;
                        _stateNameFrom.Unique = false;
                        _transitionFrequencyTotalsTable.Columns.Add(_stateNameFrom);

                        DataColumn _stateNameTo = new DataColumn();
                        _stateNameTo.DataType = System.Type.GetType("System.String");
                        _stateNameTo.ColumnName = "To";
                        _stateNameTo.Caption = "To";
                        _stateNameTo.ReadOnly = false;
                        _stateNameTo.Unique = false;
                        _transitionFrequencyTotalsTable.Columns.Add(_stateNameTo);

                        DataColumn _frequency = new DataColumn();
                        _frequency.DataType = System.Type.GetType("System.String");
                        _frequency.ColumnName = "Frequency";
                        _frequency.Caption = "Frequency";
                        _frequency.ReadOnly = false;
                        _frequency.Unique = false;
                        _transitionFrequencyTotalsTable.Columns.Add(_frequency);

                        foreach (var _key in _transitionFrequencyTotalsTableCounter.Keys)
                        {
                            DataRow _row = _transitionFrequencyTotalsTable.NewRow();
                            _row["Machine"] = _key.Item1;
                            _row["From"] = _key.Item2;
                            _row["To"] = _key.Item3;
                            _row["Frequency"] = _transitionFrequencyTotalsTableCounter[_key].ToString();
                            _transitionFrequencyTotalsTable.Rows.Add(_row);
                        }

                        _combinedResults.Tables.Add("TransitionFrequencyTotalsTable", _transitionFrequencyTotalsTable);
                        #endregion
 
                        #region "Create TransitionFrequencyPlot"

                        Dictionary<Tuple<int, string>, int> _totalOutGoing = new Dictionary<Tuple<int, string>, int>();
                        foreach (var _key in _transitionFrequencyTotalsTableCounter.Keys)
                        {
                            int _Machine = _key.Item1; 
                            string _From = _key.Item2;
                            string _To = _key.Item3;

                            Tuple<int, string> _counterKey = new Tuple<int, string>(_Machine, _From);
                            if (!_totalOutGoing.ContainsKey(_counterKey))
                                _totalOutGoing.Add(_counterKey, 0);
                            _totalOutGoing[_counterKey] += _transitionFrequencyTotalsTableCounter[_key];

                        }
                         
                        for (int i=0; i< _syntax.NumberOfMachines; i++)
                        {
                            string _tmp = " digraph CFA {" + Environment.NewLine + 
                                "node[fontname = Helvetica shape = ellipse penwidth=2 fontsize=15];" + Environment.NewLine +
                                "edge[fontname = Helvetica color = black arrowtail = none fontsize = 14]; " + Environment.NewLine;
                            foreach (var _key in _transitionFrequencyTotalsTableCounter.Keys)
                            {
                                int _Machine = _key.Item1;
                                if (i != _Machine-1)
                                    continue;
                                
                                string _From = _key.Item2;
                                string _To = _key.Item3;
                                Tuple<int, string> _counterKey = new Tuple<int, string>(_Machine, _From);

                                double _penWidth = Math.Round( 0.5 + ((double)_transitionFrequencyTotalsTableCounter[_key] / (double)_totalOutGoing[_counterKey]) * 5.5,2);

                                //
                                //_tmp += _From + " -> " + _To + "[label='" + _transitionFrequencyTotalsTableCounter[_key] + " / " + _totalOutGoing[_counterKey] + " (" + String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Math.Round((double)_transitionFrequencyTotalsTableCounter[_key] / (double)_totalOutGoing[_counterKey], 2)) + ")' " + 
                                // "penwidth=" + String.Format(CultureInfo.InvariantCulture, "{0:0.00}",_penWidth) + "]" + Environment.NewLine


                                _tmp += _From + " -> " + _To + " [label=< <i>" + String.Format(CultureInfo.InvariantCulture, "{0:0.00}", Math.Round((double)_transitionFrequencyTotalsTableCounter[_key] / (double)_totalOutGoing[_counterKey], 2)) + "</i>> " +
                                    "penwidth=" + String.Format(CultureInfo.InvariantCulture, "{0:0.00}", _penWidth) + "]" + Environment.NewLine;
                            }
                            _tmp += "}" + Environment.NewLine;

                            string _filePath = Path.Combine(ParsedCommandLineArguments.OutputPath, "logfsmtransitionfrequencygraph_" + i + ".txt");
                            File.WriteAllText(_filePath, _tmp);
                        }
                       


                        #endregion
                    }
                     

                    string _newfile = ParsedCommandLineArguments.OutFileName;
                    using (StreamWriter file = File.CreateText(_newfile))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, _combinedResults);
                    }

                    /*string json = JsonConvert.SerializeObject(_combinedResults, Newtonsoft.Json.Formatting.Indented);
                    string _newfile = ParsedCommandLineArguments.OutFileName;
                    File.WriteAllText(_newfile, json);
                    */

                }
                catch (Exception _ex)
                {
                    File.AppendAllText(Path.Combine(ParsedCommandLineArguments.OutputPath, "logfsmlasterror.txt"), "Error: " + _ex.ToString());
                    Console.WriteLine("Error moving temp file. See 'logfsmlasterror' for details.");
                    Watch.Stop();
                    Console.WriteLine("Application stopped.");
                    Console.WriteLine("Time elapsed: {0}", Watch.Elapsed);
                    return;
                }
                #endregion
            }

            #endregion


            #region Post Process UML Charts
            try
            {
                // TODO: Document 'DOTLABELFONTSIZE'
                string _labelFontSize = "10";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("DOTLABELFONTSIZE"))
                    _labelFontSize = ParsedCommandLineArguments.ParameterDictionary["DOTLABELFONTSIZE"];
                 
                if (_syntax != null && !ParsedCommandLineArguments.Flags.Contains("SKIP_POST_PROCESSING_UML_CHART"))
                {

                    // Create readable label

                    // TODO: Modify that labels are only changed conditional on the transition (i.e., start state, beginning of the DOT syntax line)

                    Dictionary<string, string> _umlEventNames = new Dictionary<string, string>();
                    foreach (var t in _syntax.Triggers)
                    {
                        // use only values of the condition string
                        string _label = string.Join("/", t.Condition.Values);
                          
                        foreach (var o in _syntax.Operators.Values)
                        {
                            if (o.TriggerName == t.GetTriggerName && o.OperatorString != "")
                            {
                                _label += " !";
                                break;
                            }
                        }

                        if (!_umlEventNames.ContainsKey(_label))
                            _umlEventNames.Add(t.GetTriggerName, _label);
                    }



                    for (int i = 0; i < _syntax.NumberOfMachines; i++)
                    {
                        string _filePath = Path.Combine(ParsedCommandLineArguments.OutputPath, "logfsmmachine_" + i + ".txt");
                        if (File.Exists(_filePath))
                        {
                            string text = File.ReadAllText(_filePath);
                            foreach (var k in _umlEventNames.Keys)
                            {
                                // Replace labels and add font size
                                text = text.Replace("label=\"" +  k + "\"", "label=\"" + _umlEventNames[k] + "\" fontsize=8");
                            }

                            // Mark start state 

                            text = text.Replace("node [shape=Mrecord]", "node [shape=oval fontsize=" + _labelFontSize + "]");

                            // Remove state machine identifier string

                            text = text.Replace("_logfsm_id_"+i,"");

                            if (_syntax.Start.ContainsKey(i+1))
                                text = text.Replace(_syntax.Start[i+1] + " [label=\"" + _syntax.Start[i+1] + "\"]", _syntax.Start[i+1] + " [peripheries=2 label=\"" + _syntax.Start[i+1] + "\"]");

                            // Mark end states

                            if (_syntax.End.ContainsKey(i + 1))
                            {
                                foreach (string s in _syntax.End[i + 1])
                                {
                                    // from 'Endstate [label="Endstate"];'  to 'Endstate [style='filled' fillcolor='gray'];'
                                    text = text.Replace(s + " [label=\"" + s + "\"]", s + " [style='filled' fillcolor='gray' label=\"" + s + "\"]");
                                }
                            }
                                
                            File.WriteAllText(_filePath, text);
                        }
                    }

                }

            }
            catch (Exception _ex)
            {
                File.AppendAllText(Path.Combine(ParsedCommandLineArguments.OutputPath, "logfsmlasterror.txt"), "Error: " + _ex.ToString());
                Console.WriteLine("Error post processing UML state chart. See 'logfsmlasterror' for details.");
                Watch.Stop();
                Console.WriteLine("Application stopped.");
                Console.WriteLine("Time elapsed: {0}", Watch.Elapsed);
                return;
            }

    
            #endregion

        }

        private static void runJobPrepare(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {

            if (ParsedCommandLineArguments.DataFileTypeIsNEPSZipVersion01A)
            {
                LogDataPreparer.ReadLogDataGenericV01(ParsedCommandLineArguments.ZIPFileName, ParsedCommandLineArguments.OutFileName, ParsedCommandLineArguments.Elements, ParsedCommandLineArguments.Verbose, ParsedCommandLineArguments);
            }
            if (ParsedCommandLineArguments.DataFileTypeIsNDataFlatV01A)
            {
                LogDataPreparer.ReadLogDataFlatV01(ParsedCommandLineArguments.ZIPFileName, ParsedCommandLineArguments.OutFileName, ParsedCommandLineArguments.Elements, ParsedCommandLineArguments.Verbose, ParsedCommandLineArguments.MaxNumberOfCases, ParsedCommandLineArguments);
            } 
            else if (ParsedCommandLineArguments.DataFileTypeIsPISABQZipVersion01A)
            {
                LogDataPreparer.ReadPISA_from_ZIP__BQ_with_XML_files(ParsedCommandLineArguments.ZIPFileName, ParsedCommandLineArguments.OutFileName, ParsedCommandLineArguments.ZIPPassword, ParsedCommandLineArguments.Elements, ParsedCommandLineArguments.MaxNumberOfCases, ParsedCommandLineArguments.DataFileTypeDetails, ParsedCommandLineArguments.Verbose, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.DataFileTypeIsPISABQZipVersion01B)
            {
                LogDataPreparer.ReadPISA_from_DME_ZIP__BQ_from_session_files(ParsedCommandLineArguments.ZIPFileName,  ParsedCommandLineArguments.OutFileName, ParsedCommandLineArguments.ZIPPassword, ParsedCommandLineArguments.Elements, ParsedCommandLineArguments.MaxNumberOfCases, ParsedCommandLineArguments.DataFileTypeDetails, ParsedCommandLineArguments.Verbose, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.DataFileTypeIsPISABQZipVersion01C)
            {
                LogDataPreparer.ReadPISA_from_DME_ZIP__BQ_from_zip_with_XML_files(ParsedCommandLineArguments.ZIPFileName, ParsedCommandLineArguments.ZIPFileFilterName, ParsedCommandLineArguments.OutFileName, ParsedCommandLineArguments.ZIPPassword, ParsedCommandLineArguments.Elements, ParsedCommandLineArguments.MaxNumberOfCases, ParsedCommandLineArguments.DataFileTypeDetails, ParsedCommandLineArguments.Verbose, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.DataFileTypeIsPISACAZipVersion01A)
            {                
                LogDataPreparer.ReadPISA_from_DME_ZIP__CA_from_session_files(ParsedCommandLineArguments.ZIPFileName, ParsedCommandLineArguments.OutFileName, ParsedCommandLineArguments.ZIPPassword, ParsedCommandLineArguments.Elements, ParsedCommandLineArguments.ExcludedElements, ParsedCommandLineArguments.MaxNumberOfCases, ParsedCommandLineArguments.AddEventInfo, ParsedCommandLineArguments.DataFileTypeDetails, ParsedCommandLineArguments.Verbose, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.DataFileTypeIsPIAACLdaRawZipVersion01A)
            {
                LogDataPreparer.ReadPIAAC_r1_from_preprocessed_TextFile_from_LDA(ParsedCommandLineArguments.ZIPFileName, ParsedCommandLineArguments.OutFileName, ParsedCommandLineArguments.ZIPPassword, ParsedCommandLineArguments.Elements, ParsedCommandLineArguments.Verbose, ParsedCommandLineArguments.MaxNumberOfCases, ParsedCommandLineArguments);
            }

        }

        private static void replaceNullValuesInDataTable(DataTable dataTable, string NewNullValue)
        {
            List<string> _columnNames = dataTable.Columns
                                    .Cast<DataColumn>()
                                    .Select(x => x.ColumnName)
                                    .ToList();
            foreach (DataRow row in dataTable.Rows)
                foreach (string columnName in _columnNames)
                    row[columnName] = row[columnName] == null ? NewNullValue : row[columnName];
        }

        private static void ExtractAndProcess(CommandLineArguments ParsedCommandLineArguments, ZipEntry entry, List<EventData> _logData, List<string> _resultFiles, ILogFSM _myGeneratedFSM, EventDataListExtension.ESortType Sort)
        {
            
            using (MemoryStream zipStream = new MemoryStream())
            {
                entry.ExtractWithPassword(zipStream, "");
                zipStream.Position = 0;
                try
                {
                    StreamReader sr = new StreamReader(zipStream);
                    string _fileContentAsString = sr.ReadToEnd();
                    if (ParsedCommandLineArguments.DataFileTypeIsEEZip)
                    {
                        _logData = LogDataReader.ReadLogDataEEFromXMLString(_fileContentAsString, ParsedCommandLineArguments.RelativeTime, Sort);
                    }
                    else if (ParsedCommandLineArguments.DataFileTypeIsLogFSMJsonZip)
                    {
                        _logData = LogDataReader.ReadLogDataLogFSMJsonString(_fileContentAsString, ParsedCommandLineArguments.RelativeTime, Sort);
                    }
                    else if (ParsedCommandLineArguments.DataFileTypeIsJsonLiteZip)
                    {
                        _logData = LogDataReader.ReadLogDataLogFSMJsonString(_fileContentAsString, ParsedCommandLineArguments.RelativeTime, Sort);
                    }
                    _resultFiles.AddRange(ProcessLogData(ParsedCommandLineArguments, _myGeneratedFSM, _logData));
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("Error processing file '" + entry.FileName + "': " + _ex.Message);
                    return;
                }
            }
        }

        private static List<string> ProcessLogData(CommandLineArguments ParsedCommandLineArguments, ILogFSM _myGeneratedFSM, List<EventData> _logDataAll)
        {
            List<string> _tmpReturn = new List<string>();
            var _groupdLogData = _logDataAll.GroupBy(x => x.PersonIdentifier);

            string _outputTimeStampFormatString = "dd.MM.yyyy hh:mm:ss.fff";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputtimestampformatstring"))
                _outputTimeStampFormatString = ParsedCommandLineArguments.ParameterDictionary["outputtimestampformatstring"];

            string _outputRelativeTimeFormatString = "hh':'mm':'ss':'fff";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputrelativetimeformatstring"))
                _outputRelativeTimeFormatString = ParsedCommandLineArguments.ParameterDictionary["outputrelativetimeformatstring"];
             
            foreach (var _data in _groupdLogData)
            {
               
                List<EventData> _logData = _data.ToList<EventData>();
                List<string> _PersonIdentifiers = _logData.Select(x => x.PersonIdentifier).Distinct().ToList<string>();
                string _personIdentifier = _PersonIdentifiers[0];
                 
                if (ParsedCommandLineArguments.MaxNumberOfCases > 0)
                {
                    if (ParsedCommandLineArguments.currentNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                        break;
                    else
                        Console.Write(" - PersonIdentifier '" + _personIdentifier + "' (", ParsedCommandLineArguments.currentNumberOfPersons, "/", ParsedCommandLineArguments.MaxNumberOfCases, "): ");
                }
                else
                {
                    Console.Write(" - PersonIdentifier '" + _personIdentifier + "': ");
                }
                    
                 
                _myGeneratedFSM.Process(_logData);

                LogFSMResultJSON _resultJsonObject = new LogFSMResultJSON();

                #region Analyze Column Names

                List<string> _keys = new List<string>();
                foreach (var e in _logData)
                {
                    foreach (var c in e.EventValues.Keys)
                    {
                        if (!_keys.Contains(c))
                            _keys.Add(c);
                    }
                }
                _keys.Sort();
                 
                int _numberOfProccessedMachines = 0;
                foreach (string k in _keys)
                {
                    if (k.StartsWith("StateBefore_") || k.StartsWith("StateAfter_") || k.StartsWith("Result_"))
                    {
                        string[] _parts = k.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        int _machineNumber = _numberOfProccessedMachines;
                        if (int.TryParse(_parts[1], out _machineNumber))
                        {
                            if (_numberOfProccessedMachines < _machineNumber)
                                _numberOfProccessedMachines = _machineNumber;
                        }
                    }
                }

                Console.WriteLine(" (number of machines: '" + (_numberOfProccessedMachines) + "')");
                ParsedCommandLineArguments.currentNumberOfPersons += 1;

                #endregion

                #region AugmentedLogDataTable

                Dictionary<string, int> _columnMapper = new Dictionary<string, int>();
                _columnMapper.Add("PersonIdentifier", _columnMapper.Keys.Count);

                if (!ParsedCommandLineArguments.Flags.Contains("EXCLUDE_ELEMENT"))
                    _columnMapper.Add("Element", _columnMapper.Keys.Count);

                if (ParsedCommandLineArguments.RelativeTime)
                {
                    _columnMapper.Add("RelativeTime", _columnMapper.Keys.Count);
                }
                else
                { 
                    _columnMapper.Add("TimeStamp", _columnMapper.Keys.Count);
                }
                
                _columnMapper.Add("EventName", _columnMapper.Keys.Count);
                for (int i = 1; i <= _numberOfProccessedMachines; i++)
                {
                    _columnMapper.Add("StateBefore_" + i, _columnMapper.Keys.Count);
                    _columnMapper.Add("StateAfter_" + i, _columnMapper.Keys.Count);
                    _columnMapper.Add("Result_" + i, _columnMapper.Keys.Count);
                }

                _columnMapper.Add("TimeDifference", _columnMapper.Keys.Count);

                foreach (var k in _keys)
                {
                    if (!_columnMapper.ContainsKey(k))
                        _columnMapper.Add(k, _columnMapper.Keys.Count);
                }

                DataTable _augmentedLogDataTable = new DataTable("AugmentedLogDataTable");
                foreach (string _c in _columnMapper.Keys)
                {
                    DataColumn _col = new DataColumn();
                    _col.DataType = System.Type.GetType("System.String");
                    _col.ColumnName = _c;
                    _col.Caption = _c;
                    _col.ReadOnly = false;
                    _col.Unique = false;
                    _augmentedLogDataTable.Columns.Add(_col);
                }

                _resultJsonObject.Tables.Add("AugmentedLogDataTable", _augmentedLogDataTable);

                List<string> _fsmVariables = _myGeneratedFSM.GetVarNames;
                string _vartablePersonIdentifier = _personIdentifier;
                Dictionary<string, string> _lastVariableValue = new Dictionary<string, string>();
                Dictionary<string, int> _numberOfVariableChanges = new Dictionary<string, int>();
                foreach (string _var in _fsmVariables)
                {
                    _lastVariableValue.Add(_var, "");
                    _numberOfVariableChanges.Add(_var, 0);
                }
                 
                #endregion

                List<string> _listOfDefinedTriggers = new List<string>();  

                for (int i = 1; i <= _numberOfProccessedMachines; i++)
                {
                    #region AugmentedLogDataTable, StateEventFrequencyTable and SequenceTable
                   
                    // TODO: Remove column 'type'

                    Dictionary<Tuple<string, string, string, string, string>, int> _StateEventFrequencyDictionary = new Dictionary<Tuple<string, string, string, string, string>, int>();
                    //    ^ PersonIDentifier
                    //             ^ State
                    //                   ^ Typ=Event | Trigger
                    //                             ^ Result
                    //                                    ^ EventName | Trigger
                    //                                                        ^ Frequency 

                    Dictionary<Tuple<string, string, string>, int> _stateTransitionFrequency = new Dictionary<Tuple<string, string, string>, int>();
                    //               ^ PersonIdentifier
                    //                        ^ StateFrom
                    //                               ^ StateTo 
                    //                                        ^ Frequency

                    List<Tuple<string, string, TimeSpan>> _completeSequenceOfStates = new List<Tuple<string, string, TimeSpan>>();
                    //         ^ PersonIdentifier
                    //                 ^ State
                    //                          ^ Time on State


                    string _currentState = "";
                    string _currentPersonIdentifier = "";
                    TimeSpan _timeOnCurrentState = new TimeSpan(0);
                    Dictionary<string, List<string>> _VisitedStateNames = new Dictionary<string, List<string>>();

                    foreach (var e in _logData)
                    {
                        _currentPersonIdentifier = e.PersonIdentifier;

                        #region AugmentedLogDataTable for all Machines 
                        if (i == 1)
                        {
                            DataRow _row = _augmentedLogDataTable.NewRow();
                            foreach (var c in _columnMapper.Keys)
                            {
                                string _value = "##NA##";
                                if (c == "PersonIdentifier")
                                    _value = e.PersonIdentifier;
                                else if (c == "Element" && !ParsedCommandLineArguments.Flags.Contains("EXCLUDE_ELEMENT"))
                                    _value = e.Element;
                                else if (c == "TimeStamp") 
                                    _value = e.TimeStamp.ToString(_outputTimeStampFormatString);  
                                else if (c == "RelativeTime")
                                    _value = e.RelativeTime.ToString(_outputRelativeTimeFormatString); 
                                else if (c == "EventName")
                                    _value = e.EventName;
                                else if (c == "TimeDifference")
                                    _value = e.TimeDifferencePrevious.TotalMilliseconds.ToString();
                                else if (e.EventValues.Keys.Contains(c))
                                    _value = e.EventValues[c];
                                _row[c] = _value;

                            }
                            _augmentedLogDataTable.Rows.Add(_row);

                            foreach (string _var in _fsmVariables)
                            {
                                string _value = e.GetEventValue(_var);
                                if (_lastVariableValue[_var] != _value)
                                {
                                    _numberOfVariableChanges[_var] += 1;
                                    _lastVariableValue[_var] = _value;
                                }
                            }
                        }
                        #endregion

                        string _StateBefore = e.EventValues["StateBefore_" + i];
                        string _StateAfter = e.EventValues["StateAfter_" + i];
                        if (!_VisitedStateNames.ContainsKey(_currentPersonIdentifier))
                            _VisitedStateNames.Add(_currentPersonIdentifier, new List<string>());

                        if (!_VisitedStateNames[_currentPersonIdentifier].Contains(_StateBefore))
                            _VisitedStateNames[_currentPersonIdentifier].Add(_StateBefore);
                        if (!_VisitedStateNames[_currentPersonIdentifier].Contains(_StateAfter))
                            _VisitedStateNames[_currentPersonIdentifier].Add(_StateAfter);

                        if (_StateAfter != _StateBefore)
                        {
                            _timeOnCurrentState = _timeOnCurrentState + e.TimeDifferencePrevious;
                            _completeSequenceOfStates.Add(new Tuple<string, string, TimeSpan>(_currentPersonIdentifier, _StateBefore, _timeOnCurrentState));
                            Tuple<string, string, string> _transitionKey = new Tuple<string, string, string>(_currentPersonIdentifier, _StateBefore, _StateAfter);
                            //    ^ PersonIdentifier
                            //            ^ StateFrom
                            //                    ^ StateTo
                            if (!_stateTransitionFrequency.ContainsKey(_transitionKey))
                                _stateTransitionFrequency.Add(_transitionKey, 1);
                            else
                                _stateTransitionFrequency[_transitionKey] += 1;

                            _timeOnCurrentState = new TimeSpan(0);
                            _currentState = _StateAfter;
                        }
                        else
                        {
                            _timeOnCurrentState = _timeOnCurrentState + e.TimeDifferencePrevious;
                        }

                        string _Result = e.EventValues["Result_" + i];
                        Tuple<string, string, string, string, string> _key1 = new Tuple<string, string, string, string, string>(_currentPersonIdentifier, _StateBefore, "Event", e.EventName, _Result);
                        //    ^ PersonIdentifier
                        //             ^ State
                        //                   ^ Typ=Event
                        //                             ^ Result
                        //                                    ^ EventName

                        if (!_StateEventFrequencyDictionary.ContainsKey(_key1))
                            _StateEventFrequencyDictionary.Add(_key1, 1);
                        else
                            _StateEventFrequencyDictionary[_key1] += 1;
                    }
                    _completeSequenceOfStates.Add(new Tuple<string, string, TimeSpan>(_currentPersonIdentifier, _currentState, _timeOnCurrentState));

                    DataTable _sequenceTable = new DataTable("SequenceTable");
                    DataColumn _personIdentifierSequenceTable = new DataColumn();
                    _personIdentifierSequenceTable.DataType = System.Type.GetType("System.String");
                    _personIdentifierSequenceTable.ColumnName = "PersonIdentifier";
                    _personIdentifierSequenceTable.Caption = "PersonIdentifier";
                    _personIdentifierSequenceTable.ReadOnly = false;
                    _personIdentifierSequenceTable.Unique = false;
                    _sequenceTable.Columns.Add(_personIdentifierSequenceTable);

                    DataColumn _stateSequenceTable = new DataColumn();
                    _stateSequenceTable.DataType = System.Type.GetType("System.String");
                    _stateSequenceTable.ColumnName = "State";
                    _stateSequenceTable.Caption = "State";
                    _stateSequenceTable.ReadOnly = false;
                    _stateSequenceTable.Unique = false;
                    _sequenceTable.Columns.Add(_stateSequenceTable);

                    DataColumn _durationSequencTable = new DataColumn();
                    _durationSequencTable.DataType = System.Type.GetType("System.String");
                    _durationSequencTable.ColumnName = "Duration";
                    _durationSequencTable.Caption = "Duration";
                    _durationSequencTable.ReadOnly = false;
                    _durationSequencTable.Unique = false;
                    _sequenceTable.Columns.Add(_durationSequencTable);

                    foreach (var _tuple in _completeSequenceOfStates)
                    {
                        DataRow _row = _sequenceTable.NewRow();
                        _row["PersonIdentifier"] = _tuple.Item1;
                        _row["State"] = _tuple.Item2;
                        _row["Duration"] = ((TimeSpan)_tuple.Item3).TotalMilliseconds.ToString();
                        _sequenceTable.Rows.Add(_row);
                    }

                    #endregion

                    #region StateEventFrequencyTable
                    DataTable _stateEventFrequencyTable = new DataTable("StateEventFrequencyTable");

                    DataColumn _personIdentifierStateEventFrequencyTable = new DataColumn();
                    _personIdentifierStateEventFrequencyTable.DataType = System.Type.GetType("System.String");
                    _personIdentifierStateEventFrequencyTable.ColumnName = "PersonIdentifier";
                    _personIdentifierStateEventFrequencyTable.Caption = "PersonIdentifier";
                    _personIdentifierStateEventFrequencyTable.ReadOnly = false;
                    _personIdentifierStateEventFrequencyTable.Unique = false;
                    _stateEventFrequencyTable.Columns.Add(_personIdentifierStateEventFrequencyTable);

                    DataColumn _stateEventFrequencyTableTyp = new DataColumn();
                    _stateEventFrequencyTableTyp.DataType = System.Type.GetType("System.String");
                    _stateEventFrequencyTableTyp.ColumnName = "Typ";
                    _stateEventFrequencyTableTyp.Caption = "Typ";
                    _stateEventFrequencyTableTyp.ReadOnly = false;
                    _stateEventFrequencyTableTyp.Unique = false;
                    _stateEventFrequencyTable.Columns.Add(_stateEventFrequencyTableTyp);

                    DataColumn _stateEventFrequencyTableState = new DataColumn();
                    _stateEventFrequencyTableState.DataType = System.Type.GetType("System.String");
                    _stateEventFrequencyTableState.ColumnName = "State";
                    _stateEventFrequencyTableState.Caption = "State";
                    _stateEventFrequencyTableState.ReadOnly = false;
                    _stateEventFrequencyTableState.Unique = false;
                    _stateEventFrequencyTable.Columns.Add(_stateEventFrequencyTableState);

                    DataColumn _stateEventFrequencyTableName = new DataColumn();
                    _stateEventFrequencyTableName.DataType = System.Type.GetType("System.String");
                    _stateEventFrequencyTableName.ColumnName = "Name";
                    _stateEventFrequencyTableName.Caption = "Name";
                    _stateEventFrequencyTableName.ReadOnly = false;
                    _stateEventFrequencyTableName.Unique = false;
                    _stateEventFrequencyTable.Columns.Add(_stateEventFrequencyTableName);

                    DataColumn _stateEventFrequencyTableResult = new DataColumn();
                    _stateEventFrequencyTableResult.DataType = System.Type.GetType("System.String");
                    _stateEventFrequencyTableResult.ColumnName = "Result";
                    _stateEventFrequencyTableResult.Caption = "Result";
                    _stateEventFrequencyTableResult.ReadOnly = false;
                    _stateEventFrequencyTableResult.Unique = false;
                    _stateEventFrequencyTable.Columns.Add(_stateEventFrequencyTableResult);

                    DataColumn _stateEventFrequencyTableFrequency = new DataColumn();
                    _stateEventFrequencyTableFrequency.DataType = System.Type.GetType("System.String");
                    _stateEventFrequencyTableFrequency.ColumnName = "Frequency";
                    _stateEventFrequencyTableFrequency.Caption = "Frequency";
                    _stateEventFrequencyTableFrequency.ReadOnly = false;
                    _stateEventFrequencyTableFrequency.Unique = false;
                    _stateEventFrequencyTable.Columns.Add(_stateEventFrequencyTableFrequency);

                    foreach (var _key in _StateEventFrequencyDictionary.Keys)
                    {
                        DataRow _row = _stateEventFrequencyTable.NewRow();
                        _row["PersonIdentifier"] = _key.Item1;
                        _row["State"] = _key.Item2;
                        _row["Typ"] = _key.Item3;
                        _row["Name"] = _key.Item4;
                        _row["Result"] = _key.Item5;
                        _row["Frequency"] = _StateEventFrequencyDictionary[_key];
                        _stateEventFrequencyTable.Rows.Add(_row);
                    }
                    #endregion

                    #region StateSummaryDataTable

                    DataTable _stateSummaryTable = new DataTable("StateSummaryDataTable");

                    DataColumn _personIdentifierStateSummaryDataTable = new DataColumn();
                    _personIdentifierStateSummaryDataTable.DataType = System.Type.GetType("System.String");
                    _personIdentifierStateSummaryDataTable.ColumnName = "PersonIdentifier";
                    _personIdentifierStateSummaryDataTable.Caption = "PersonIdentifier";
                    _personIdentifierStateSummaryDataTable.ReadOnly = false;
                    _personIdentifierStateSummaryDataTable.Unique = false;
                    _stateSummaryTable.Columns.Add(_personIdentifierStateSummaryDataTable);

                    DataColumn _colState = new DataColumn();
                    _colState.DataType = System.Type.GetType("System.String");
                    _colState.ColumnName = "State";
                    _colState.Caption = "State";
                    _colState.ReadOnly = false;
                    _colState.Unique = false;
                    _stateSummaryTable.Columns.Add(_colState);

                    DataColumn _colTotalTime = new DataColumn();
                    _colTotalTime.DataType = System.Type.GetType("System.String");
                    _colTotalTime.ColumnName = "TotalTime";
                    _colTotalTime.Caption = "TotalTime";
                    _colTotalTime.ReadOnly = false;
                    _colTotalTime.Unique = false;
                    _stateSummaryTable.Columns.Add(_colTotalTime);

                    DataColumn _colFrequency = new DataColumn();
                    _colFrequency.DataType = System.Type.GetType("System.String");
                    _colFrequency.ColumnName = "Frequency";
                    _colFrequency.Caption = "Frequency";
                    _colFrequency.ReadOnly = false;
                    _colFrequency.Unique = false;
                    _stateSummaryTable.Columns.Add(_colFrequency);

                    DataColumn _colShortestVisit = new DataColumn();
                    _colShortestVisit.DataType = System.Type.GetType("System.String");
                    _colShortestVisit.ColumnName = "ShortestVisit";
                    _colShortestVisit.Caption = "ShortestVisit";
                    _colShortestVisit.ReadOnly = false;
                    _colShortestVisit.Unique = false;
                    _stateSummaryTable.Columns.Add(_colShortestVisit);

                    DataColumn _colLongestVisit = new DataColumn();
                    _colLongestVisit.DataType = System.Type.GetType("System.String");
                    _colLongestVisit.ColumnName = "LongestVisit";
                    _colLongestVisit.Caption = "LongestVisit";
                    _colLongestVisit.ReadOnly = false;
                    _colLongestVisit.Unique = false;
                    _stateSummaryTable.Columns.Add(_colLongestVisit);

                    DataColumn _colAverageVisit = new DataColumn();
                    _colAverageVisit.DataType = System.Type.GetType("System.String");
                    _colAverageVisit.ColumnName = "AverageVisit";
                    _colAverageVisit.Caption = "AverageVisit";
                    _colAverageVisit.ReadOnly = false;
                    _colAverageVisit.Unique = false;
                    _stateSummaryTable.Columns.Add(_colAverageVisit);

                    foreach (string _person in _VisitedStateNames.Keys)
                    {
                        foreach (string _defaultState in _VisitedStateNames[_person])
                        {
                            TimeSpan _totalTime = new TimeSpan(0);
                            TimeSpan _shortestVisit = new TimeSpan(-1);
                            TimeSpan _longestVisit = new TimeSpan(-1);
                            int _frequencyOfVisit = 0;
                            foreach (var v in _completeSequenceOfStates)
                            {
                                if (v.Item2 == _defaultState)
                                {
                                    TimeSpan _timeOnState = (TimeSpan)v.Item3;
                                    _totalTime += _timeOnState;
                                    _frequencyOfVisit += 1;
                                    if (_shortestVisit.TotalMilliseconds < 0)
                                        _shortestVisit = _timeOnState;
                                    else if (_timeOnState < _shortestVisit)
                                        _shortestVisit = _timeOnState;
                                    if (_timeOnState > _longestVisit)
                                        _longestVisit = _timeOnState;
                                }
                            }

                            DataRow _row = _stateSummaryTable.NewRow();
                            _row["PersonIdentifier"] = _person;
                            _row["State"] = _defaultState;
                            _row["TotalTime"] = _totalTime.TotalMilliseconds.ToString();
                            _row["Frequency"] = _frequencyOfVisit;
                            _row["ShortestVisit"] = _shortestVisit.TotalMilliseconds.ToString();
                            _row["LongestVisit"] = _longestVisit.TotalMilliseconds.ToString();
                            if (_frequencyOfVisit == 0)
                            {
                                _row["AverageVisit"] = "##NA##"; // TODO: Define Parameter for Output NA-Value
                            }
                            else
                            {
                                _row["AverageVisit"] = new TimeSpan(_totalTime.Ticks / _frequencyOfVisit).TotalMilliseconds.ToString();
                            }
                            _stateSummaryTable.Rows.Add(_row);
                        }
                    }

                    #endregion

                    #region TransitionFrequencyTable & TransitionFrequencyTotalsTable 

                    DataTable _transitionFrequencyTable = new DataTable("TransitionFrequencyTable");

                    DataColumn _personIdentifierTransitionFrequencyTable = new DataColumn();
                    _personIdentifierTransitionFrequencyTable.DataType = System.Type.GetType("System.String");
                    _personIdentifierTransitionFrequencyTable.ColumnName = "PersonIdentifier";
                    _personIdentifierTransitionFrequencyTable.Caption = "PersonIdentifier";
                    _personIdentifierTransitionFrequencyTable.ReadOnly = false;
                    _personIdentifierTransitionFrequencyTable.Unique = false;
                    _transitionFrequencyTable.Columns.Add(_personIdentifierTransitionFrequencyTable);

                    DataColumn _stateNameFrom = new DataColumn();
                    _stateNameFrom.DataType = System.Type.GetType("System.String");
                    _stateNameFrom.ColumnName = "From";
                    _stateNameFrom.Caption = "From";
                    _stateNameFrom.ReadOnly = false;
                    _stateNameFrom.Unique = false;
                    _transitionFrequencyTable.Columns.Add(_stateNameFrom);

                    DataColumn _stateNameTo = new DataColumn();
                    _stateNameTo.DataType = System.Type.GetType("System.String");
                    _stateNameTo.ColumnName = "To";
                    _stateNameTo.Caption = "To";
                    _stateNameTo.ReadOnly = false;
                    _stateNameTo.Unique = false;
                    _transitionFrequencyTable.Columns.Add(_stateNameTo);

                    DataColumn _frequency = new DataColumn();
                    _frequency.DataType = System.Type.GetType("System.String");
                    _frequency.ColumnName = "Frequency";
                    _frequency.Caption = "Frequency";
                    _frequency.ReadOnly = false;
                    _frequency.Unique = false;
                    _transitionFrequencyTable.Columns.Add(_frequency);

                    foreach (var _key in _stateTransitionFrequency.Keys)
                    {
                        DataRow _row = _transitionFrequencyTable.NewRow();
                        _row["PersonIdentifier"] = _key.Item1;
                        _row["From"] = _key.Item2;
                        _row["To"] = _key.Item3;
                        _row["Frequency"] = _stateTransitionFrequency[_key].ToString();
                        _transitionFrequencyTable.Rows.Add(_row);
                    }

                    #endregion

                    #region NGramTable

                    Dictionary<string, Tuple<int, TimeSpan, int, string>> _dictionaryOfNGrams = new Dictionary<string, Tuple<int, TimeSpan, int, string>>();
                    //         ^ NGramName
                    //                       ^ Frequency 
                    //                             ^ Duration
                    //                                      ^ Size
                    //                                             ^ PersonIdentifier


                    // TODO: Find out, wyh the End-State is missing in the N-Gram Table

                    int MAX_SIZE = _completeSequenceOfStates.Count;
                    if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("ngrammaxsize"))       // TODO: Document Parameter
                    {
                        string _maxSize = ParsedCommandLineArguments.ParameterDictionary["ngrammaxsize"];
                        int _maxSizeParsed = -1;
                        if (int.TryParse(_maxSize, out _maxSizeParsed))
                        {
                            if (MAX_SIZE > _maxSizeParsed)
                                MAX_SIZE = _maxSizeParsed;
                        }

                    }

                    for (int _size = 1; _size <= MAX_SIZE; _size++)
                    {
                        for (int _startPoint = 0; _startPoint <= _completeSequenceOfStates.Count - _size; _startPoint++)
                        {
                            string _NGram_Name = _completeSequenceOfStates[_startPoint].Item2;
                            string _person = _completeSequenceOfStates[_startPoint].Item1;

                            TimeSpan _NGram_Duration = _completeSequenceOfStates[_startPoint].Item3;
                            for (int j = 1; j < _size; j++)
                            {
                                _NGram_Name += ";" + _completeSequenceOfStates[_startPoint + j].Item2;
                                _NGram_Duration += _completeSequenceOfStates[_startPoint + j].Item3;
                            }
                            if (!_dictionaryOfNGrams.ContainsKey(_NGram_Name))
                                _dictionaryOfNGrams.Add(_NGram_Name, new Tuple<int, TimeSpan, int, string>(1, _NGram_Duration, _size, _person));
                            else
                                _dictionaryOfNGrams[_NGram_Name] = new Tuple<int, TimeSpan, int, string>(_dictionaryOfNGrams[_NGram_Name].Item1 + 1, _dictionaryOfNGrams[_NGram_Name].Item2 + _NGram_Duration, _size, _person);
                        }
                    }


                    DataTable _nGramTable = new DataTable("NGramTable");

                    DataColumn _nGramPersonIdentifier = new DataColumn();
                    _nGramPersonIdentifier.DataType = System.Type.GetType("System.String");
                    _nGramPersonIdentifier.ColumnName = "PersonIdentifier";
                    _nGramPersonIdentifier.Caption = "PersonIdentifier";
                    _nGramPersonIdentifier.ReadOnly = false;
                    _nGramPersonIdentifier.Unique = false;
                    _nGramTable.Columns.Add(_nGramPersonIdentifier);

                    DataColumn _nGramSize = new DataColumn();
                    _nGramSize.DataType = System.Type.GetType("System.String");
                    _nGramSize.ColumnName = "Size";
                    _nGramSize.Caption = "Size";
                    _nGramSize.ReadOnly = false;
                    _nGramSize.Unique = false;
                    _nGramTable.Columns.Add(_nGramSize);

                    DataColumn _nGramName = new DataColumn();
                    _nGramName.DataType = System.Type.GetType("System.String");
                    _nGramName.ColumnName = "Sequence";
                    _nGramName.Caption = "Sequence";
                    _nGramName.ReadOnly = false;
                    _nGramName.Unique = false;
                    _nGramTable.Columns.Add(_nGramName);

                    DataColumn _nGramFrequency = new DataColumn();
                    _nGramFrequency.DataType = System.Type.GetType("System.String");
                    _nGramFrequency.ColumnName = "Frequency";
                    _nGramFrequency.Caption = "Frequency";
                    _nGramFrequency.ReadOnly = false;
                    _nGramFrequency.Unique = false;
                    _nGramTable.Columns.Add(_nGramFrequency);

                    DataColumn _nGramTotalTime = new DataColumn();
                    _nGramTotalTime.DataType = System.Type.GetType("System.String");
                    _nGramTotalTime.ColumnName = "TotalTime";
                    _nGramTotalTime.Caption = "TotalTime";
                    _nGramTotalTime.ReadOnly = false;
                    _nGramTotalTime.Unique = false;
                    _nGramTable.Columns.Add(_nGramTotalTime);

                    foreach (var _k in _dictionaryOfNGrams.Keys)
                    {
                        DataRow _row = _nGramTable.NewRow();
                        Tuple<int, TimeSpan, int, string> _element = _dictionaryOfNGrams[_k];
                        _row["PersonIdentifier"] = _element.Item4;
                        _row["Size"] = _element.Item3;
                        _row["Sequence"] = _k;
                        _row["Frequency"] = _element.Item1;
                        _row["TotalTime"] = _element.Item2.TotalMilliseconds.ToString();
                        _nGramTable.Rows.Add(_row);
                    }
                    #endregion

                    _resultJsonObject.Tables.Add("SequenceTable_" + i, _sequenceTable);
                    _resultJsonObject.Tables.Add("TransitionFrequencyTable_" + i, _transitionFrequencyTable);
                    _resultJsonObject.Tables.Add("StateSummaryTable_" + i, _stateSummaryTable);
                    _resultJsonObject.Tables.Add("NGramTable_" + i, _nGramTable);
                    _resultJsonObject.Tables.Add("StateEventFrequencyTable_" + i, _stateEventFrequencyTable);

                }

                #region  VariableValueTable
                DataTable _variableValueTable = new DataTable("VariableValueTable");

                DataColumn _personIdentifierVariableValueTable = new DataColumn();
                _personIdentifierVariableValueTable.DataType = System.Type.GetType("System.String");
                _personIdentifierVariableValueTable.ColumnName = "PersonIdentifier";
                _personIdentifierVariableValueTable.Caption = "PersonIdentifier";
                _personIdentifierVariableValueTable.ReadOnly = false;
                _personIdentifierVariableValueTable.Unique = false;
                _variableValueTable.Columns.Add(_personIdentifierVariableValueTable);

                DataColumn _variableVariableValueTable = new DataColumn();
                _variableVariableValueTable.DataType = System.Type.GetType("System.String");
                _variableVariableValueTable.ColumnName = "Variable";
                _variableVariableValueTable.Caption = "Variable";
                _variableVariableValueTable.ReadOnly = false;
                _variableVariableValueTable.Unique = false;
                _variableValueTable.Columns.Add(_variableVariableValueTable);

                DataColumn _valueVariableValueTable = new DataColumn();
                _valueVariableValueTable.DataType = System.Type.GetType("System.String");
                _valueVariableValueTable.ColumnName = "Value";
                _valueVariableValueTable.Caption = "Value";
                _valueVariableValueTable.ReadOnly = false;
                _valueVariableValueTable.Unique = false;
                _variableValueTable.Columns.Add(_valueVariableValueTable);

                DataColumn _changesVariableValueTable = new DataColumn();
                _changesVariableValueTable.DataType = System.Type.GetType("System.String");
                _changesVariableValueTable.ColumnName = "NumberOfChanges";
                _changesVariableValueTable.Caption = "NumberOfChanges";
                _changesVariableValueTable.ReadOnly = false;
                _changesVariableValueTable.Unique = false;
                _variableValueTable.Columns.Add(_changesVariableValueTable);

                foreach (var _var in _fsmVariables)
                {
                    DataRow _row = _variableValueTable.NewRow();
                    _row["PersonIdentifier"] = _vartablePersonIdentifier;
                    _row["Variable"] = _var;
                    _row["Value"] = _lastVariableValue[_var];
                    _row["NumberOfChanges"] = _numberOfVariableChanges[_var];
                    _variableValueTable.Rows.Add(_row);
                }

                _resultJsonObject.Tables.Add("VariableValueTable", _variableValueTable);

                // replace null values with ##NA##

                if (ParsedCommandLineArguments.Flags.Contains("REPLACENULLWITHNA")) //TODO: Document Flag
                {
                    foreach (var key in _resultJsonObject.Tables.Keys)
                        replaceNullValuesInDataTable(_resultJsonObject.Tables[key], "##NA##"); // TODO: Make the string a parameter
                }

                #endregion

                string _newfile = Path.Combine(ParsedCommandLineArguments.TempPath, _personIdentifier) + ".json";
                using (StreamWriter file = File.CreateText(_newfile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, _resultJsonObject);
                }

                /*
                string json = JsonConvert.SerializeObject(_resultJsonObject, Newtonsoft.Json.Formatting.Indented);
                string _file = Path.Combine(ParsedCommandLineArguments.TempPath, _personIdentifier) + ".json";
                File.WriteAllText(_file, json);
                */

                _tmpReturn.Add(_newfile);
            }
             
            return _tmpReturn;
        }

    }
}

