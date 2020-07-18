namespace LogFSM
{
    #region usings
    using LogFSMShared;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    #endregion


    public class FSMSimpleSyntax : IFSMSyntaxReader
    {
        public List<FSMIgnore> Ignores { get; }
        public List<FSMTransition> Transitions { get; }
        public Dictionary<string, FSMOperator> Operators { get; }

        public List<string> Errors { get; }

        //private List<string> states = new List<string>();
        // public string[] States { get { return states.ToArray(); } }

        // TODO: TEST
        private Dictionary<int, List<string>> states =  new Dictionary<int, List<string>>();
        public Dictionary<int, List<string>> States { get { return states; } }
     
        private Dictionary<int, string> start = new Dictionary<int, string>();
        public Dictionary<int, string> Start { get { return start; } }

        private Dictionary<int, string[]> end = new Dictionary<int, string[]>();
        public Dictionary<int, string[]> End { get { return end; } }

        private Dictionary<string, FSMTrigger> triggers = new Dictionary<string, FSMTrigger>();
        public FSMTrigger[] Triggers { get { return triggers.Values.ToArray(); } }

        private int numberOfMachines { get; }
        public int NumberOfMachines { get { return numberOfMachines; } }

        /*
        public string[] AllStates
        {
            get
            {
                List<string> _ret = new List<string>();
                _ret.AddRange(states);
                foreach (int _key in start.Keys)
                {
                    _ret.Add(start[_key]);
                }
                foreach (int _key in end.Keys)
                {
                    _ret.AddRange(end[_key]);
                }
                return _ret.ToArray();
            }
        }
        */

        public FSMSimpleSyntax(string FSMDefinition)
        {
            numberOfMachines = 1;

            Ignores = new List<FSMIgnore>();
            Transitions = new List<FSMTransition>();
            Operators = new Dictionary<string, FSMOperator>();

            string[] _lines = FSMDefinition.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Errors = new List<string>();

            foreach (string _line in _lines)
            {
                var _parsedLine = ParseSimpleSyntaxLine(_line);

                if (_parsedLine != null)
                { 
                    #region START-STATE

                    if (_parsedLine.Command.RemoveWhitespace().ToLower().StartsWith("start", StringComparison.OrdinalIgnoreCase))
                    {
                        List<string> _startStates = _parsedLine.GetStateList1;
                        int _machineIndex = _parsedLine.Index;

                        if (_machineIndex > numberOfMachines)
                            numberOfMachines = _machineIndex;

                        if (!start.ContainsKey(_machineIndex - 1))
                            start.Add(_machineIndex - 1, "");

                        if (!states.ContainsKey(_machineIndex - 1))
                            states.Add(_machineIndex - 1, new List<string>());

                        if (_startStates.Count == 1 && start[_machineIndex - 1] == "")
                        {
                            start[_machineIndex - 1] = _startStates[0];
                            if (!states[_machineIndex - 1].Contains(_startStates[0]))
                                states[_machineIndex - 1].Add(_startStates[0]);
                        }
                        else
                        {
                            Errors.Add("More than one start state defined.");
                        }
                    }
                    #endregion

                    #region END-STATES

                    if (_parsedLine.Command.RemoveWhitespace().ToLower().StartsWith("end", StringComparison.OrdinalIgnoreCase))
                    {
                        List<string> _endStates = _parsedLine.GetStateList1;
                        int _machineIndex = _parsedLine.Index;
                        if (_machineIndex > numberOfMachines)
                            numberOfMachines = _machineIndex;

                        if (!end.ContainsKey(_machineIndex))
                            end.Add(_machineIndex, new string[] { });

                        end[_machineIndex] = _endStates.ToArray();

                        if (!states.ContainsKey(_machineIndex - 1))
                            states.Add(_machineIndex - 1, new List<string>());

                        foreach (string s in _endStates)
                        {
                            if (!states[_machineIndex - 1].Contains(s))
                                states[_machineIndex - 1].Add(s);
                        }
                    }
                    #endregion

                    #region TRANSITIONS
                    if (_parsedLine.Command.RemoveWhitespace().ToLower().StartsWith("transition", StringComparison.OrdinalIgnoreCase))
                    {
                        int _machineIndex = _parsedLine.Index;
                        if (_machineIndex > numberOfMachines)
                            numberOfMachines = _machineIndex;

                        List<string> _listOfFromStates = _parsedLine.GetStateList1;
                        List<string> _lisOfToStates = _parsedLine.GetStateList2;

                        if (_parsedLine.Trigger == "")
                        {
                            _parsedLine.Trigger = "EventName=";
                        }

                        List<string> _listOfTriggerStrings = _parsedLine.Trigger.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                        List<string> _guards = _parsedLine.Guard.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                        List<string> _operators = _parsedLine.Operator.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();

                        if (!states.ContainsKey(_machineIndex - 1))
                            states.Add(_machineIndex - 1, new List<string>());
                        
                        foreach (string _trigger in _listOfTriggerStrings)
                        {
                            FSMTrigger _fsmtrigger = new FSMTrigger(_trigger.Trim(), _parsedLine.Guard, _parsedLine.Operator, _machineIndex - 1, "");

                            /*
                            if (!triggers.ContainsKey(_fsmtrigger.ConditionString))
                                triggers.Add(_fsmtrigger.ConditionString, _fsmtrigger); 
                            */

                            if (!triggers.ContainsKey(_fsmtrigger.GetTriggerName))
                                triggers.Add(_fsmtrigger.GetTriggerName, _fsmtrigger);

                            foreach (string _from in _listOfFromStates)
                            {
                                if (!_fsmtrigger.States.Contains(_from + "_logfsm_id_" + _machineIndex))
                                    _fsmtrigger.States.Add(_from + "_logfsm_id_" + _machineIndex);
                                 
                                FSMOperator _fsmoperator = new FSMOperator() { OperatorString = _parsedLine.Operator, State = _from, TriggerName = _fsmtrigger.GetTriggerName, MachineIndex = _machineIndex - 1 };
  
                                string _key = _fsmoperator.GetKey.RemoveWhitespace() + CreateValidIdentifier(_parsedLine.Guard);
                                if (!Operators.ContainsKey(_key))
                                {
                                    Operators.Add(_key, _fsmoperator);
                                }
                                else
                                {
                                    Errors.Add("Operator already defined (state: " + _from + ", trigger: " + _parsedLine.Trigger.Trim() + ", machine: " + _machineIndex + ")");
                                }

                                foreach (string _to in _lisOfToStates)
                                {
                                    if (_from.Trim() != _to.Trim())
                                    {
                                        FSMTransition _t = new FSMTransition()
                                        {
                                            MachineIndex = _machineIndex - 1,
                                            From = _from.Trim(),
                                            To = _to.Trim()
                                        };

                                        if (!states[_machineIndex - 1].Contains(_from.Trim()))
                                            states[_machineIndex - 1].Add(_from.Trim());

                                        if (!states[_machineIndex - 1].Contains(_to.Trim()))
                                            states[_machineIndex - 1].Add(_to.Trim());

                                        _t.Triggers.Add(_fsmtrigger);

                                        Transitions.Add(_t);

                                    }
                                    else
                                    {
                                        FSMIgnore _i = new FSMIgnore()
                                        {
                                            MachineIndex = _machineIndex - 1,
                                            State = _from.Trim(),
                                        };

                                        if (!states[_machineIndex - 1].Contains(_from.Trim()))
                                            states[_machineIndex - 1].Add(_from.Trim());

                                        _i.Triggers.Add(_fsmtrigger);

                                        Ignores.Add(_i);

                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region IGNORE
                    if (_parsedLine.Command.RemoveWhitespace().ToLower().StartsWith("ignore", StringComparison.OrdinalIgnoreCase))
                    {
                        int _machineIndex = _parsedLine.Index;
                        if (_machineIndex > numberOfMachines)
                            numberOfMachines = _machineIndex;

                        List<string> _listOfIgnoredStates = _parsedLine.GetStateList1;

                        List<string> _listOfTriggerStrings = _parsedLine.Trigger.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                        List<string> _guards = _parsedLine.Guard.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                        List<string> _operators = _parsedLine.Operator.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();

                        if (!states.ContainsKey(_machineIndex - 1))
                            states.Add(_machineIndex - 1, new List<string>());

                        foreach (string _trigger in _listOfTriggerStrings)
                        { 
                            FSMTrigger _fsmtrigger = new FSMTrigger(_trigger.Trim(), _parsedLine.Guard, _parsedLine.Operator, _machineIndex - 1, "");

                            if (!triggers.ContainsKey(_fsmtrigger.GetTriggerName))
                            {
                                triggers.Add(_fsmtrigger.GetTriggerName, _fsmtrigger);
                            }
                            else
                            {
                                _fsmtrigger = triggers[_fsmtrigger.GetTriggerName];
                            }


                            foreach (string _state in _listOfIgnoredStates)
                            {
                                if (!_fsmtrigger.States.Contains(_state + "_logfsm_id_" + _machineIndex))
                                    _fsmtrigger.States.Add(_state + "_logfsm_id_" + _machineIndex);


                                FSMOperator _fsmoperator = new FSMOperator() { OperatorString = _parsedLine.Operator, State = _state, TriggerName = _fsmtrigger.GetTriggerName, MachineIndex = _machineIndex - 1 };

                                string _key = _fsmoperator.GetKey;
                                if (!Operators.ContainsKey(_key))
                                {
                                    Operators.Add(_key, _fsmoperator);
                                }
                                else
                                {
                                    Errors.Add("Operator already defined (state: " + _state + ", trigger: " + _parsedLine.Trigger.Trim() + ", machine: " + _machineIndex + ")");
                                }

                                FSMIgnore _i = new FSMIgnore()
                                {
                                    MachineIndex = _machineIndex - 1,
                                    State = _state.Trim(),
                                };

                                if (!states[_machineIndex - 1].Contains(_state.Trim()))
                                    states[_machineIndex - 1].Add(_state.Trim());

                                _i.Triggers.Add(_fsmtrigger);

                                Ignores.Add(_i);

                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    Errors.Add("Error parsing line:" + _line);
                }
            }

        }
        public static string CreateValidIdentifier(string Identifier)
        {
            return Identifier.Trim().Replace("(", "paropen").Replace(")", "parclose").
                Replace("&", "and").Replace(",", "comma").Replace("=", "equal").
                Replace("$", "dollar").Replace("*", "star").Replace("0", "zero").
                Replace("1", "one").Replace("2", "two").Replace("3", "three").
                Replace("4", "four").Replace("5", "five").Replace("6", "six").
                Replace("7", "seven").Replace("8", "eight").Replace("9", "nine").
                Replace("|", "bar").Replace("<", "gt").Replace(">", "st").
                Replace("-", "minus").Replace("+", "plus").Replace(".", "dot").Replace(" ", "");
            // return String.Join("", Identifier.Split(' ', '&', '|', '$', '*', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '(', '(', ',', '=', '\'', '\"', '|', '>', '<'));
        }

        private List<string> getListOfStrings(string input, string key)
        {
            int i;
            int a = 0;
            int count = 0;
            List<string> _return = new List<string>();
            for (i = 0; i < input.Length; i++)
            {
                if (input[i] == ';' || input[i] == ':' || input[i] == '>')
                {
                    if ((count & 1) == 0)
                    {
                        _return.Add(input.Substring(a, i - a).Trim());
                        a = i + 1;
                    }
                }
                else if (input[i] == '\'')
                {
                    count++;
                }
            }
            _return.Add(input.Substring(a).Trim());

            if (_return.Count > 0)
            {
                if (_return[0].ToLower().RemoveWhitespace() == key)
                {
                    _return.RemoveAt(0);
                }
            }
            return _return;
        }

        class SimpleSyntaxLine
        {        
            public int Index { get; set; }
            public string Command { get; set; }

            public string StateList1 { get; set; }
            public List<string> GetStateList1 { get { return trimListFromSemecolonString(StateList1); } }

            public string StateList2 { get; set; }
            public List<string> GetStateList2 { get { return trimListFromSemecolonString(StateList2); } }

            public string Trigger { get; set; }
            public string Operator { get; set; }
            public string Guard { get; set; }

            private List<string> trimListFromSemecolonString (string SemecolonString)
            {
                List<string> _tmp = SemecolonString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();
                List<string> _ret = new List<string>();
                foreach (string s in _tmp)
                    _ret.Add(s.Trim());
                return _ret;
            }

        }

        static SimpleSyntaxLine ParseSimpleSyntaxLine(string input)
        {
            try
            {
                // examples:
                // =========
                // start: {State}
                // 1 start: {State}
                // end: {State 1};  {State 2}
                // 1 end: {State 1};  {State 2}
                // transition: {State 1}; {State 2} ->  {State 1};  {State 2} @ {Trigger 1} ; {Trigger 2}  ! {Operator 1} ; {Operator 2} | {Guard 1} ; {Guard 2}
                // 1 transition: {State 1}; {State 2} ->  {State 1};  {State 2} @ {Trigger 1} ; {Trigger 2}  ! {Operator 1} ; {Operator 2} | {Guard 1} ; {Guard 2}
                // ignore: {State 1};  {State 2} @ {Trigger 1} ; {Trigger 2}  ! {Operator 1} ; {Operator 2} | {Guard 1} ; {Guard 2}
                // 1 ignore: {State 1};  {State 2} @ {Trigger 1} ; {Trigger 2}  ! {Operator 1} ; {Operator 2} | {Guard 1} ; {Guard 2}

                // schema:
                // =======
                // ________  __________________ :  _________________ -> _______________ @ _________________ ! ____________________ | ________________
                //           ^ _digitSeparator  ^ _commandSeparator  ^ _stateSeparator  ^ _triggerSepartor  ^ _operatorSeparator   ^ _guardSeparator
                // COUNTER   COMMAND              STATE-LIST-1         STATE-LIST-2       TRIGGER             OPERATOR              GUARD

                char[] chars = input.ToCharArray();

                #region Digit-Separator 
                int _counter = 1;
                int _digitSeparator = 0;
                for (int i = 0; i < chars.Length; i++)
                {
                    if (!Char.IsDigit(chars[i]))
                    {
                        _digitSeparator = i;
                        break;
                    }
                }
                if (_digitSeparator > 0)
                    _counter = int.Parse(new string(chars, 0, _digitSeparator));
                #endregion

                #region Command-Separator
                int _commandSeparator = 0;
                string _commandString = "";
                for (int i = _digitSeparator; i < chars.Length; i++)
                {
                    if (chars[i] == ':')
                    {
                        _commandSeparator = i;
                        break;
                    }
                }
                if (_commandSeparator > 0)
                    _commandString = input.Substring(_digitSeparator, _commandSeparator - _digitSeparator).Trim();
                #endregion

                #region State-Separator
                int _stateSeparator = 0;
                for (int i = _commandSeparator; i < chars.Length - 1; i++)
                {
                    if (chars[i] == '-' && chars[i + 1] == '>')
                    {
                        _stateSeparator = i;
                        break;
                    }
                }
                #endregion

                #region Trigger-Separator
                int _triggerSepartor = 0;
                for (int i = _commandSeparator; i < chars.Length; i++)
                {
                    if (chars[i] == '@')
                    {
                        _triggerSepartor = i;
                        break;
                    }
                }
                #endregion

                #region Operator-Separator
                int _operatorSeparator = 0;
                for (int i = _commandSeparator; i < chars.Length; i++)
                {
                    if (chars[i] == '!')
                    {
                        _operatorSeparator = i;
                        break;
                    }
                }
                #endregion

                #region Guard-Separator
                int _guardSeparator = 0;
                for (int i = _commandSeparator; i < chars.Length; i++)
                {
                    if (chars[i] == '|')
                    {
                        _guardSeparator = i;
                        break;
                    }
                }
                #endregion

                if (_guardSeparator == 0)
                    _guardSeparator = input.Length;
                if (_operatorSeparator == 0)
                    _operatorSeparator = _guardSeparator;
                if (_triggerSepartor == 0)
                    _triggerSepartor = _operatorSeparator;
                if (_stateSeparator == 0)
                    _stateSeparator = _triggerSepartor;


                string _guardString = input.Substring(_guardSeparator, input.Length - _guardSeparator);
                if (_guardString.StartsWith("|"))
                    _guardString = _guardString.Substring(1, _guardString.Length - 1);
                _guardString = _guardString.TrimStart().TrimEnd();

                string _operatorString = input.Substring(_operatorSeparator, _guardSeparator - _operatorSeparator);
                if (_operatorString.StartsWith("!"))
                    _operatorString = _operatorString.Substring(1, _operatorString.Length - 1);
                _operatorString = _operatorString.TrimStart().TrimEnd();


                string _triggerString = input.Substring(_triggerSepartor, _operatorSeparator - _triggerSepartor);
                if (_triggerString.StartsWith("@"))
                    _triggerString = _triggerString.Substring(1, _triggerString.Length - 1);
                _triggerString = _triggerString.TrimStart().TrimEnd();

                string _statelist2String = input.Substring(_stateSeparator, _triggerSepartor - _stateSeparator);
                if (_statelist2String.StartsWith("->"))
                    _statelist2String = _statelist2String.Substring(2, _statelist2String.Length - 2);
                _statelist2String = _statelist2String.TrimStart().TrimEnd();

                string _statelist1String = input.Substring(_commandSeparator, _stateSeparator - _commandSeparator);
                if (_statelist1String.StartsWith(":"))
                    _statelist1String = _statelist1String.Substring(1, _statelist1String.Length - 1);
                _statelist1String = _statelist1String.TrimStart().TrimEnd();

                return new SimpleSyntaxLine
                {
                    Command = _commandString,
                    Index = _counter,
                    Guard = _guardString,
                    Operator = _operatorString,
                    StateList1 = _statelist1String,
                    StateList2 = _statelist2String,
                    Trigger = _triggerString
                };
            }
            catch
            {
                return null;
            }
           

        }

    }

}
