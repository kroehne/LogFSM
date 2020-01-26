namespace LogFSM
{
    #region usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Reflection;
    using LogFSM;
    using LogFSMShared;
    using System.Runtime.Loader;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Stateless;
    using Stateless.Graph;
    using System.Diagnostics;
    using System.Data;
    using Ionic.Zip;
    using Newtonsoft.Json;
    #endregion

    public class FSMFactory : IFSMProvider
    {
        private string sourceFileName { get; }
        public string CodeNameSpace { get; }
        public string ClassName { get; }

        public List<string> CompileErrors { get; }

        public bool HasErrors { get{ return CompileErrors.Count > 0; }}

        #region definitions 

        private const string ENUMENAMESTATES = "State";
        private const string ENUMENAMETRIGGERS = "Trigger";

        #endregion

        private LogFSMConsole.CommandLineArguments parsedCommandLineArguments;
        private int numberOfMachines = 1;

        public FSMFactory(string SourceFileName, string CodeNameSpace, string ClassName, int NumberOfMachines, LogFSMConsole.CommandLineArguments ParsedCommandLineArguments )
        {
            sourceFileName = SourceFileName;
            this.CodeNameSpace = CodeNameSpace;
            this.ClassName = ClassName;
            parsedCommandLineArguments = ParsedCommandLineArguments;
            this.CompileErrors = new List<string>();
            this.numberOfMachines = NumberOfMachines;
        }

        #region State Definition 

        private string enumDefinitionCodeFragment = "";

        private Dictionary<int, string> StartStateName = new Dictionary<int, string>();
        private Dictionary<int, string[]> EndStateNames;
        private Dictionary<int, List<string>> StateNames;

        public void DefineStates(Dictionary<int, List<string>> ListOfStates, Dictionary<int, string> StartState, Dictionary<int, string[]> ListOfEndStates)
        {
            StartStateName = StartState;
            EndStateNames = ListOfEndStates;
            StateNames = ListOfStates;

            stateLabelDictionary = new Dictionary<string, string>();

            enumDefinitionCodeFragment += "public enum " + ENUMENAMESTATES + Environment.NewLine;
            enumDefinitionCodeFragment += "{" + Environment.NewLine;

            for (int i=0; i < numberOfMachines; i++)
            {
                foreach (var _s in ListOfStates[i])
                {
                    // state name is (valid name) + "_" + (machine index)
                    string _key = _s + "_logfsm_id_" + i;
                    if (!stateLabelDictionary.ContainsKey(_key))
                        stateLabelDictionary.Add(_key, _s.CreateValidEnumValue() + "_logfsm_id_" + i);

                    enumDefinitionCodeFragment += "  // State '" + _s + "' in machine " + i + Environment.NewLine;
                    enumDefinitionCodeFragment += "  " + stateLabelDictionary[_key] + "," + Environment.NewLine;
                    enumDefinitionCodeFragment += Environment.NewLine;
                }
            }
          

            enumDefinitionCodeFragment += "}" + Environment.NewLine;
        }

        Dictionary<string, string> stateLabelDictionary;

        #endregion

        #region Trigger Definition

        private string triggerDefinitionCodeFragment = "";

        public void DefineTriggers(FSMTrigger[] ListOfTriggers)
        {

            triggerDefinitionCodeFragment += "public enum " + ENUMENAMETRIGGERS + Environment.NewLine;
            triggerDefinitionCodeFragment += "{" + Environment.NewLine;

            foreach (var trigger in ListOfTriggers)
            {
              //  triggerDefinitionCodeFragment += "  [Description(\"" + trigger.GetComment + "\")]" + Environment.NewLine;
                triggerDefinitionCodeFragment += "  " + trigger.GetTriggerName + "," + Environment.NewLine;
                triggerDefinitionCodeFragment += Environment.NewLine;
            }

            triggerDefinitionCodeFragment += "}" + Environment.NewLine;
        }

        #endregion

        #region Define Rules

        private string createRulesFunctionCodeFragment = "";

        // machineIndexDict
        // ----------------
        // key: 0, ..., Number_of_Machines : Internal index, starting with 0 and free from ony gaps
        // value: 1, 3, 4,... X: Integer value as used in the syntax to define mulitple fsms, or 1 (default value)

        Dictionary<int, int> machineIndexDict = new Dictionary<int, int>();
        public void CreateRules(List<FSMTransition> Transitions, List<FSMIgnore> Ignores, Dictionary<string, FSMOperator> Operators)
        {

            createRulesFunctionCodeFragment += "public void CreateRules()" + Environment.NewLine;
            createRulesFunctionCodeFragment += "{" + Environment.NewLine;
            createRulesFunctionCodeFragment += "     // Add state machine rules." + Environment.NewLine;

            foreach (var transition in Transitions)
            {
                foreach (var trigger in transition.Triggers)
                {
                    if (!machineIndexDict.ContainsKey(transition.MachineIndex))
                        machineIndexDict.Add(transition.MachineIndex, machineIndexDict.Values.Count);

                    createRulesFunctionCodeFragment += "machines[" + machineIndexDict[transition.MachineIndex] + 
                            "].Configure(State." + stateLabelDictionary[transition.From + "_logfsm_id_" + trigger.MachineIndex] + 
                            ").Permit(Trigger." + trigger.GetTriggerName  + 
                            ", State." + stateLabelDictionary[transition.To + "_logfsm_id_" + trigger.MachineIndex] + ");" + Environment.NewLine;
                }
            }

            foreach (var ignore in Ignores)
            {
                foreach (var trigger in ignore.Triggers)
                {
                    if (!machineIndexDict.ContainsKey(ignore.MachineIndex))
                        machineIndexDict.Add(ignore.MachineIndex, machineIndexDict.Values.Count);

                    createRulesFunctionCodeFragment += "machines[" + machineIndexDict[ignore.MachineIndex] +
                        "].Configure(State." + stateLabelDictionary[ignore.State + "_logfsm_id_" + trigger.MachineIndex] +
                        ").Ignore(Trigger." + trigger.GetTriggerName + ");" + Environment.NewLine;
                }
            }

            createRulesFunctionCodeFragment += "     // Add transition operators." + Environment.NewLine;
  
            foreach (var key in Operators.Keys)
            {
                if (Operators[key].OperatorString.Trim() != "")
                {
                    createRulesFunctionCodeFragment += "Operators.Add(\"" + key + "\",\"" + Operators[key].OperatorString.Trim() + "\");" + Environment.NewLine;   
                }
            }

            createRulesFunctionCodeFragment += "}" + Environment.NewLine;
        }

        #endregion

        #region Define Triggers

        private string createTriggerFunctionCodeFragment = "";

        public void CreateTriggerDefinition(FSMTrigger[] Trigger)
        {
            createTriggerFunctionCodeFragment += "public void CreateTriggers()" + Environment.NewLine;
            createTriggerFunctionCodeFragment += "{" + Environment.NewLine;
            createTriggerFunctionCodeFragment += "     //  Add Triggers." + Environment.NewLine;
 
            foreach (var trigger in Trigger)
            {
                createTriggerFunctionCodeFragment += "     Triggers.Add(new FSMTrigger(\"" + trigger.ConditionString + "\", \"" + trigger.GuardString + "\", \"" + trigger.OperatorString  + "\", " + trigger.MachineIndex + "));" + Environment.NewLine;
            }
            createTriggerFunctionCodeFragment += "}" + Environment.NewLine;
        }
        #endregion

        #region CreateConstructor

        private string constructorCodeFragment = "";
        public void CreateConstructor()
        {

            constructorCodeFragment += "public " + ClassName + "()" + Environment.NewLine;
            constructorCodeFragment += "{" + Environment.NewLine;
            constructorCodeFragment += "    machines = new Dictionary<int, Stateless.StateMachine<State, Trigger>>();" + Environment.NewLine;
            for (int i = 0; i < machineIndexDict.Values.Count; i++)
            {
                int _syntaxStateIndexKey = machineIndexDict.Keys.ToArray()[i];
                string _stateState = StartStateName[_syntaxStateIndexKey];
                constructorCodeFragment += "    machines.Add(" + i + ", new Stateless.StateMachine<State, Trigger>(State." + _stateState + "_logfsm_id_" + i + ")); " + Environment.NewLine;
            }
            constructorCodeFragment += "    Triggers = new List<FSMTrigger>();" + Environment.NewLine;
            constructorCodeFragment += "    Operators = new Dictionary<string, string>();" + Environment.NewLine;
            constructorCodeFragment += "    this.CreateRules();" + Environment.NewLine;
            constructorCodeFragment += "    this.CreateTriggers();" + Environment.NewLine;

            constructorCodeFragment += "    for (int mIndex = 0; mIndex < machines.Keys.Count; mIndex++)" + Environment.NewLine;
            constructorCodeFragment += "    {" + Environment.NewLine;
            constructorCodeFragment += "         string _stateMachineExportFolder = string.Concat(@\""  +  parsedCommandLineArguments.OutputPath +  "\");" + Environment.NewLine;
            constructorCodeFragment += "         string _stateMachineExportFileName = string.Concat(\"logfsmmachine_\", mIndex, \".txt\");" + Environment.NewLine;
            constructorCodeFragment += "         base.ExportTextFile(Path.Combine(_stateMachineExportFolder,_stateMachineExportFileName), UmlDotGraph.Format(machines[mIndex].GetInfo()));" + Environment.NewLine;
            constructorCodeFragment += "    }" + Environment.NewLine;
            constructorCodeFragment += "}" + Environment.NewLine;
        }
        #endregion

        #region Impement Interface

        private string createProcessEventMethodCodeFragment = "";
  
        public void ImplementInterface()
        {
            
            createProcessEventMethodCodeFragment += "public override void ProcessEvent(System.Collections.Generic.List<LogFSMShared.EventData> Data, int EventIndex)" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "{" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "     //  Access current event as variable 'e': " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "     LogFSMShared.EventData e = Data[EventIndex]; " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "     for (int mIndex = 0; mIndex < machines.Keys.Count; mIndex++)" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "     {" + Environment.NewLine;

            createProcessEventMethodCodeFragment += "          //  Define column names for state machine state (before/after) and result: " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          string _stateBeforeEventPropteryName = string.Concat(\"StateBefore_\", (mIndex + 1));" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          string _stateAfterEventPropteryName = string.Concat(\"StateAfter_\", (mIndex + 1));" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          string _resultEventPropteryName = string.Concat(\"Result_\", (mIndex + 1));" + Environment.NewLine;

            createProcessEventMethodCodeFragment += "          //  Store the current state before the trigger is fired: " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          e.AddEventValue(_stateBeforeEventPropteryName, machines[mIndex].State.ToString().Replace(\"_logfsm_id_\" + mIndex,\"\"));" + Environment.NewLine;

            createProcessEventMethodCodeFragment += "          //  Declare a variable for the result of the attempt to process the trigger: " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          EProccEventResult _res = EProccEventResult.Unknown;" + Environment.NewLine;

            createProcessEventMethodCodeFragment += "          //  Get list of active triggers" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          List<Tuple<string,string>> _listOfTriggers = base.GetActiveTriggerNames(Data, EventIndex, Triggers, mIndex);" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          if (_listOfTriggers.Count > 0)" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          {" + Environment.NewLine; 
            createProcessEventMethodCodeFragment += "               foreach (Tuple<string,string> _triggerGuardTuple in _listOfTriggers)" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "               {" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                   //  Declare a variable for the trigger generated from the event: " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                   Trigger _t;" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                   //  Try to parse the event-name as trigger: " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                   if (Enum.TryParse<Trigger>(_triggerGuardTuple.Item1, out _t))" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                   {" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                        //  Check if the trigger will be accepted in the current state.  " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                        if (machines[mIndex].PermittedTriggers.Contains(_t))" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                        {" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                            //  Try to fire the trigger...  " + Environment.NewLine;             
            createProcessEventMethodCodeFragment += "                            try" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                            {" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                                   //  Execute operators for the activated trigger: " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                                   if (_triggerGuardTuple.Item2 != \"\")" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                                       base.ExecuteOperators(_triggerGuardTuple.Item2, Data, EventIndex);" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                                   //  Fire the trigger in the FSM: " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                                  machines[mIndex].Fire(_t);" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                                 //  If no error occurs, the trigger is accepted." + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                                _res = EProccEventResult.EventAccepted;" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                               break;" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                            }" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                            catch (System.Exception ex)" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                            {" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                               System.Console.WriteLine(\"Error processing event:\" + ex.Message);" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                               _res = EProccEventResult.ErrorProcessingEventAsTrigger;" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                               break;" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                            }" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                        }" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                   }" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                }" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                if (_res == EProccEventResult.Unknown)" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                {" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                    //  Handle, that the trigger is not permitted in the current state: " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                    _res = EProccEventResult.EventNotPermitted;" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                }" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          }" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          else" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          {" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "               //  Handle, that the log-event is not a valid trigger. " + Environment.NewLine;
            createProcessEventMethodCodeFragment += "                _res = EProccEventResult.EventNotDefinedAsTrigger;" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          }" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          e.AddEventValue(_stateAfterEventPropteryName, machines[mIndex].State.ToString().Replace(\"_logfsm_id_\" + mIndex,\"\"));" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "          e.AddEventValue(_resultEventPropteryName, _res.ToString());" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "     }" + Environment.NewLine;
            createProcessEventMethodCodeFragment += "}" + Environment.NewLine;
        }
 
        #endregion

        public Assembly GetAssembly()
        {
            ImplementInterface();
            CreateConstructor();

            string _source = "//------------------------------------------------------------------------------" + Environment.NewLine + 
                             "// <auto-generated>" + Environment.NewLine +
                             "//     Dynamically created source code. Provisionally created as C# syntax, " + Environment.NewLine +
                             "//     because CodeDom is not supported under .NET Core. " + Environment.NewLine +
                             "// </auto-generated>" + Environment.NewLine +
                             "//------------------------------------------------------------------------------" + Environment.NewLine;

            _source += "namespace " + CodeNameSpace + Environment.NewLine;
            _source += "{" + Environment.NewLine;
            _source += "    using System;" + Environment.NewLine;
            _source += "    using Stateless;" + Environment.NewLine;
            _source += "    using Stateless.Graph;" + Environment.NewLine;
            _source += "    using LogFSMShared;" + Environment.NewLine;
            _source += "    using System.Linq;" + Environment.NewLine; 
           
            //_source += "    using System.ComponentModel.Primitives;" + Environment.NewLine;
            _source += "    using System.IO;" + Environment.NewLine; 
            _source += "    using System.Collections;" + Environment.NewLine;
            _source += "    using System.Collections.Generic;" + Environment.NewLine;
              
            _source += "    public class " + ClassName +  ": LogFSMShared.LogFSMBase, LogFSMShared.ILogFSM " + Environment.NewLine;
            _source += "    {" + Environment.NewLine;
            _source += "        private Dictionary<int,Stateless.StateMachine<State,Trigger>> machines;" + Environment.NewLine;
          
            _source += constructorCodeFragment;

            _source += createProcessEventMethodCodeFragment;

            _source += createRulesFunctionCodeFragment;
 
            _source += createTriggerFunctionCodeFragment;

            _source += enumDefinitionCodeFragment;
             
            _source += triggerDefinitionCodeFragment;

            _source += "     } " + Environment.NewLine;
            _source += " } " + Environment.NewLine;

            // export generated cs code for inspection
            if (parsedCommandLineArguments.Flags.Contains("EXPORT_GENERATED_SOURCE") || parsedCommandLineArguments.IsDebug)
            { 
                using (StreamWriter sourceWriter = new StreamWriter(sourceFileName))
                {
                    sourceWriter.Write(_source);
                }
            }

            Assembly createdAssembly = null;

            var options = new CSharpCompilationOptions(
               OutputKind.DynamicallyLinkedLibrary,
               optimizationLevel: OptimizationLevel.Release,
               allowUnsafe: false);

            var compilation = CSharpCompilation.Create(Path.GetRandomFileName(), options: options);

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(_source);
            compilation = compilation.AddSyntaxTrees(syntaxTree);

            // TODO: Optimize!

            #region Add Rferences

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            List<MetadataReference> references = new List<MetadataReference>();

            references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Console.dll")));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            var usings = compilation.SyntaxTrees.Select(tree => tree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>()).SelectMany(s => s).ToArray();

            foreach (var u in usings)
            {
                if (File.Exists(Path.Combine(assemblyPath, u.Name.ToString() + ".dll")))
                    references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, u.Name.ToString() + ".dll")));
            }
             
            references.Add(MetadataReference.CreateFromFile(Path.Combine(parsedCommandLineArguments.RuntimePath, "Stateless.dll")));
            references.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(parsedCommandLineArguments.RuntimePath, "LogFSMShared.dll")));

            #endregion

            compilation = compilation.AddReferences(references);

            //compile

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        CompileErrors.Add(diagnostic.Id + ";" + diagnostic.GetMessage() + ";" + diagnostic.Location);
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    AssemblyLoadContext context = AssemblyLoadContext.Default;
                    createdAssembly = context.LoadFromStream(ms);
                }
            }

            if (HasErrors)
            {
                foreach (string l in CompileErrors)
                    File.AppendAllText(Path.Combine(parsedCommandLineArguments.OutputPath, "logfsmlasterror.txt"), l);
            }


            return createdAssembly;

         
        }

    }
}
