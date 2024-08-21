#region usings
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace LogFSMShared
{ 
    public interface ILogFSM
    {
        List<FSMTrigger> Triggers { get; set; }
        Dictionary<string, string> Operators { get; set; }
        void ProcessEvent(List<EventData> Data, int EventIndex);
        void UpdateVariables(List<EventData> Data, int EventIndex);
        List<string> GetVarNames { get; }
    }

    public interface IFSMProvider
    {
        System.Reflection.Assembly GetAssembly();
    }

    public enum EProccEventResult
    {
        Unknown,
        EventAccepted,
        EventNotDefinedAsTrigger,
        EventNotPermitted,
        ErrorProcessingEventAsTrigger,
    }

    public class LogFSMBase : ILogFSM
    {
        public virtual List<FSMTrigger> Triggers { get; set; }
        public virtual Dictionary<string, string> Operators { get; set; }
        public virtual void ProcessEvent(List<EventData> Data, int EventIndex) { }

        public void ExecuteOperators(string OperatorString, List<EventData> Data, int EventIndex)
        { 

            EventData e = Data[EventIndex];
            List<string> _operators = OperatorString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();

            foreach (string _o in _operators)
            {
                if (_o.ToLower().Trim().StartsWith("setvariable"))
                {
                    string[] _operatorParameters = _o.Split('(', ')')[1].Split(',');
                    if (_operatorParameters.Length == 2)
                    {
                        SetVariableValue(e, _operatorParameters[0], _operatorParameters[1]);
                    }

                }
                else if (_o.ToLower().Trim().StartsWith("increaseintvalue"))
                {
                    string[] _operatorParameters = _o.Split('(', ')')[1].Split(',');
                    if (_operatorParameters.Length == 2)
                    {
                        IncreaseIntValue(Data, EventIndex, _operatorParameters[0], _operatorParameters[1]);
                    }
                }
                else if (_o.ToLower().Trim().StartsWith("copyattributetovariable"))
                {
                    string[] _operatorParameters = _o.Split('(', ')')[1].Split(',');
                    if (_operatorParameters.Length == 2)
                    {
                        CopyAttributeToVariable(e, _operatorParameters[0], _operatorParameters[1]);
                    }
                }
                else
                {
                    Console.WriteLine("Operator not recognized: " + _o);
                }

            }

        }
         
        private static bool guardVariableIs(string _sourceValue, string _targetValue, string _operator)
        {
            if (_operator == "id")
            {
                if (_sourceValue != _targetValue)
                    return false;
            }
            else if (_operator == "not")
            {
                if (_sourceValue == _targetValue)
                    return false;
            }
            else
            {
                double _source = double.NaN;
                double _target = double.NaN;
                if (double.TryParse(_sourceValue, out _source) && double.TryParse(_targetValue, out _target))
                {
                    if (_operator == "eq")
                    {
                        if (!(_source == _target))
                            return false;
                    }
                    else if (_operator == "gt")
                    {
                        if (!(_source > _target))
                            return false;
                    }
                    else if (_operator == "ge")
                    {
                        if (!(_source >= _target))
                            return false;
                    }
                    else if (_operator == "lt")
                    {
                        if (!(_source < _target))
                            return false;
                    }
                    else if (_operator == "le")
                    {
                        if (!(_source <= _target))
                            return false;
                    }
                    else if (_operator == "neq")
                    {
                        if (!(_source != _target))
                            return false;
                    }
                }
                else
                {
                    return false;
                }

            }

            return true;
        }

        public List<Tuple<string, string>> GetActiveTriggerNames(List<EventData> Data, int EventIndex, List<FSMTrigger> Trigger, int MachineIndex, string CurrentState)
        { 
            List<Tuple<string, string>> _return = new List<Tuple<string, string>>();

            EventData e = Data[EventIndex];
             
            //bool _isLookAheadFirst = true;
            //bool _useAciveTriggersFoGuardGroup = true;

            //SortedDictionary<int, Tuple<bool, FSMTrigger>> _activeTriggersForGuardGroup = new SortedDictionary<int, Tuple<bool, FSMTrigger>>();
            // key: distance to the current EventIndex
            // Tuple<bool, FSMTrigger>
            //    bool: First = true or Last = false
            //    FSMTrigger: active trigger 

            SortedDictionary<int, List<FSMTrigger>> _activeTriggersForGuardGroup = new SortedDictionary<int, List<FSMTrigger>>();
            // key: distance to the current EventIndex
            // value: list of (valid) triggers

            foreach (FSMTrigger t in Trigger)
            {
                if (t.MachineIndex != MachineIndex)
                    continue;
                
                if (!t.States.Contains(CurrentState))
                    continue;

                bool _isValidCondition = true;
                foreach (var c in t.Condition)
                {
                    if (e.EventValues.ContainsKey(c.Key))
                    {
                        if (e.EventValues[c.Key].ToLower() != c.Value.ToLower())
                        {
                            _isValidCondition = false;
                        }
                    }
                    else if (c.Key == "EventName")
                    {
                        if (e.EventName.ToLower() != c.Value.ToLower())
                        {
                            _isValidCondition = false;
                        }
                    }
                    else if (c.Key == "Element")
                    {
                        if (e.Element.ToLower() != c.Value.ToLower())
                        {
                            _isValidCondition = false;
                        }
                    }
                    else
                    {
                        _isValidCondition = false;
                    }
                }

                if (_isValidCondition)
                {
                    bool _guardIsTrue = true;
                    if (t.GuardString != "")
                    {
                        List<string> _guards = t.GuardString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList<string>();

                        // Two types of guards

                        // A: guards that are ACTIVE or INACTIVE (without evaluating all guards)
                        // B: guards that can be ACTIVE or INACTIVE (taking either the first or the last upcomming guard)

                        // TODO: Possible to handle (the combination) of multiple guards??

                        // TODO: Write test cases for all guards

                        foreach (string _g in _guards)
                        {
                            if (_g.ToLower().Trim().StartsWith("variablesare") || _g.ToLower().Trim().StartsWith("compareattributes"))
                            {
                                // VariablesAre / CompareAttributes
                                // 1 = AttributeName1
                                // 2 = AttributeName2

                                #region GUARD: "VariableIs" / "CompareAttribute" 

                                string[] _guardParameters = _g.Split('(', ')')[1].Split(',');
                                string _targetAttributeName = _guardParameters[1];
                                string _sourceAttributeName = _guardParameters[0];
                                string _operator = "id";
                                int _targetEventIndex = EventIndex;

                                if (_guardParameters.Length >= 3)
                                {
                                    if (_guardParameters[2].Trim().ToLower() == "le") // <=
                                        _operator = "le";
                                    else if (_guardParameters[2].Trim().ToLower() == "lt") // <
                                        _operator = "lt";
                                    else if (_guardParameters[2].Trim().ToLower() == "ge") // >=
                                        _operator = "ge";
                                    else if (_guardParameters[2].Trim().ToLower() == "gt") // >
                                        _operator = "gt";
                                    else if (_guardParameters[2].Trim().ToLower() == "eq") // =
                                        _operator = "eq";
                                    else if (_guardParameters[2].Trim().ToLower() == "neq") // != (numbers)
                                        _operator = "neq";
                                    else if (_guardParameters[2].Trim().ToLower() == "not") // != (strings)
                                        _operator = "not";
                                }

                                if (_guardParameters.Length >= 4)
                                {
                                    if (_guardParameters[3].Trim().ToLower() == "first")
                                    {
                                        _targetEventIndex = 0;
                                    }
                                    else if (_guardParameters[3].Trim().ToLower() == "last")
                                    {
                                        _targetEventIndex = Data.Count - 1;
                                    }
                                    else
                                    {
                                        int _offset = 0;
                                        if (int.TryParse(_guardParameters[3], out _offset))
                                        {
                                            if (_targetEventIndex + _offset < 0 || _targetEventIndex + _offset > Data.Count - 1)
                                            {
                                                _guardIsTrue = false;
                                            }
                                            else
                                            {
                                                _targetEventIndex = _targetEventIndex + _offset;
                                            }
                                        };
                                    }
                                }

                                string _sourceValue = Data[EventIndex].GetEventValue(_sourceAttributeName);
                                string _targetValue = Data[_targetEventIndex].GetEventValue(_targetAttributeName);
                                if (_guardParameters.Length > 1 && _guardParameters.Length <= 4)
                                {
                                    if (!guardVariableIs(_sourceValue, _targetValue, _operator))
                                        _guardIsTrue = false;
                                }

                                #endregion
                            }
                            else if (_g.ToLower().Trim().StartsWith("variableis") || _g.ToLower().Trim().StartsWith("compareattribute"))
                            {
                                // VariableIs / CompareAttribute
                                // 1 = AttributeName
                                // 2 = TargetValue

                                #region GUARD: "VariableIs" / "CompareAttribute" 

                                string[] _guardParameters = _g.Split('(', ')')[1].Split(',');
                                string _targetValue = _guardParameters[1];
                                string _sourceAttributeName = _guardParameters[0];
                                string _operator = "id";
                                int _targetEventIndex = EventIndex;

                                if (_guardParameters.Length >= 3)
                                {
                                    if (_guardParameters[2].Trim().ToLower() == "le") // <=
                                        _operator = "le";
                                    else if (_guardParameters[2].Trim().ToLower() == "lt") // <
                                        _operator = "lt";
                                    else if (_guardParameters[2].Trim().ToLower() == "ge") // >=
                                        _operator = "ge";
                                    else if (_guardParameters[2].Trim().ToLower() == "gt") // >
                                        _operator = "gt";
                                    else if (_guardParameters[2].Trim().ToLower() == "eq") // =
                                        _operator = "eq";
                                    else if (_guardParameters[2].Trim().ToLower() == "neq") // != (numbers)
                                        _operator = "neq";
                                    else if (_guardParameters[2].Trim().ToLower() == "not") // != (strings)
                                        _operator = "not";
                                }

                                if (_guardParameters.Length >= 4)
                                {
                                    if (_guardParameters[3].Trim().ToLower() == "first")
                                    {
                                        _targetEventIndex = 0;
                                    }
                                    else if (_guardParameters[3].Trim().ToLower() == "last")
                                    {
                                        _targetEventIndex = Data.Count - 1;
                                    }
                                    else
                                    {
                                        int _offset = 0;
                                        if (int.TryParse(_guardParameters[3], out _offset))
                                        {
                                            if (_targetEventIndex + _offset < 0 || _targetEventIndex + _offset > Data.Count - 1)
                                            {
                                                _guardIsTrue = false;
                                            }
                                            else
                                            {
                                                _targetEventIndex = _targetEventIndex + _offset;
                                            }
                                        };
                                    }
                                }

                                string _sourceValue = Data[_targetEventIndex].GetEventValue(_sourceAttributeName);
                                if (_guardParameters.Length > 1 && _guardParameters.Length <= 4)
                                {
                                    if (!guardVariableIs(_sourceValue, _targetValue, _operator))
                                        _guardIsTrue = false;
                                }

                                #endregion
                            }
                            else if (_g.ToLower().Trim().StartsWith("islasttrigger"))
                            {
                                #region GUARD: "IsLastTrigger"
                                string[] _guardParameters = _g.Split('(', ')')[1].Split(',');

                                var ConditionFilter = new Dictionary<string, string>();
                                string[] querySegmentsFilter = _guardParameters[0].Split('&');
                                foreach (string segment in querySegmentsFilter)
                                {
                                    string[] _parts = segment.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    if (_parts.Length > 1)
                                    {
                                        ConditionFilter.Add(_parts[0].Trim(), _parts[1].Trim());
                                    }
                                }

                                // islasttrigger: 
                                // - check remaining events. if found an event with this trigger, 
                                //   the guard is false. otherwise, the guard is true (default).
                                // - a row is true if all conditions are met, i.e., if any condition
                                //   if false, the row is false

                                for (int k = EventIndex + 1; k < Data.Count; k++)
                                {
                                    bool _rowIsTrue = true;
                                    foreach (var _key in ConditionFilter.Keys)
                                    {
                                        string _value = ConditionFilter[_key];
                                        if (Data[k].GetEventValue(_key) != _value)
                                        {
                                            _rowIsTrue = false;
                                            break;
                                        }
                                    }

                                    if (_rowIsTrue)
                                    {
                                        _guardIsTrue = false;
                                        break;
                                    }
                                }
                                #endregion
                            }
                            else if (_g.ToLower().Trim().StartsWith("isnotlasttrigger"))
                            {
                                #region GUARD: "IsLastTrigger"
                                string[] _guardParameters = _g.Split('(', ')')[1].Split(',');

                                var ConditionFilter = new Dictionary<string, string>();
                                string[] querySegmentsFilter = _guardParameters[0].Split('&');
                                foreach (string segment in querySegmentsFilter)
                                {
                                    string[] _parts = segment.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    if (_parts.Length > 1)
                                    {
                                        ConditionFilter.Add(_parts[0].Trim(), _parts[1].Trim());
                                    }
                                }

                                // isnotlasttrigger: 
                                // - check remaining events. if found an event with this trigger, 
                                //   the guard is true. otherwise, the guard is fals (default).
                                // - a row is true if all conditions are met, i.e., if any condition
                                //   if false, the row is false

                                _guardIsTrue = false;
                                for (int k = EventIndex + 1; k < Data.Count; k++)
                                {
                                    bool _rowIsTrue = true;
                                    foreach (var _key in ConditionFilter.Keys)
                                    {
                                        string _value = ConditionFilter[_key];
                                        if (Data[k].GetEventValue(_key) != _value)
                                        {
                                            _rowIsTrue = false;
                                            break;
                                        }
                                    }

                                    if (_rowIsTrue)
                                    {
                                        _guardIsTrue = true;
                                        break;
                                    }
                                }
                                #endregion
                            }
                            else if (_g.ToLower().Trim().StartsWith("isfirsttrigger"))
                            {
                                #region GUARD: "IsFirstTrigger"
                                string[] _guardParameters = _g.Split('(', ')')[1].Split(',');

                                var ConditionFilter = new Dictionary<string, string>();
                                string[] querySegmentsFilter = _guardParameters[0].Split('&');
                                foreach (string segment in querySegmentsFilter)
                                {
                                    string[] _parts = segment.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    if (_parts.Length > 1)
                                    {
                                        ConditionFilter.Add(_parts[0].Trim(), _parts[1].Trim());
                                    }
                                }

                                // isfirsttrigger: 
                                // - check previous events. if found an event with this trigger, 
                                //   the guard is false. otherwise, the guard is true (default).
                                // - a row is true if all conditions are met, i.e., if any condition
                                //   if false, the row is false

                                for (int k = 0; k < EventIndex; k++)
                                {
                                    bool _rowIsTrue = true;
                                    foreach (var _key in ConditionFilter.Keys)
                                    {
                                        string _value = ConditionFilter[_key];
                                        if (Data[k].GetEventValue(_key) != _value)
                                        {
                                            _rowIsTrue = false;
                                            break;
                                        }
                                    }

                                    if (_rowIsTrue)
                                    {
                                        _guardIsTrue = false;
                                        break;
                                    }
                                }
                                #endregion
                            }
                            else if (_g.ToLower().Trim().StartsWith("isnotfirsttrigger"))
                            {
                                #region GUARD: "IsFirstTrigger"
                                string[] _guardParameters = _g.Split('(', ')')[1].Split(',');

                                var ConditionFilter = new Dictionary<string, string>();
                                string[] querySegmentsFilter = _guardParameters[0].Split('&');
                                foreach (string segment in querySegmentsFilter)
                                {
                                    string[] _parts = segment.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    if (_parts.Length > 1)
                                    {
                                        ConditionFilter.Add(_parts[0].Trim(), _parts[1].Trim());
                                    }
                                }

                                // isnotfirsttrigger: 
                                // - check previous events. if found an event with this trigger, 
                                //   the guard is true. otherwise, the guard is false (default).
                                // - a row is true if all conditions are met, i.e., if any condition
                                //   if false, the row is false

                                _guardIsTrue = false;
                                for (int k = 0; k < EventIndex; k++)
                                {
                                    bool _rowIsTrue = true;
                                    foreach (var _key in ConditionFilter.Keys)
                                    {
                                        string _value = ConditionFilter[_key];
                                        if (Data[k].GetEventValue(_key) != _value)
                                        {
                                            _rowIsTrue = false;
                                            break;
                                        }
                                    }

                                    if (_rowIsTrue)
                                    {
                                        _guardIsTrue = true;
                                        break;
                                    }
                                }
                                #endregion
                            }
                            else if (_g.ToLower().Trim().StartsWith("look_ahead_first") /*|| _g.ToLower().Trim().StartsWith("look_ahead_last")*/)
                            {
                                #region GUARD: "look_ahead_first" / "look_ahead_last"
                                // | look_ahead_first({Filter}, {Condition}) 


                                // | look_ahead_last({Filter}, {Condition})  <-- removed 2019-09-08                            
                                //_isLookAheadFirst = _g.ToLower().Trim().StartsWith("look_ahead_first");

                                string[] _guardParameters = _g.Split('(', ')')[1].Split(',');

                                var ConditionFilter = new Dictionary<string, string>();
                                string[] querySegmentsFilter = _guardParameters[0].Split('&');
                                foreach (string segment in querySegmentsFilter)
                                {
                                    string[] _parts = segment.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    if (_parts.Length > 1)
                                    {
                                        ConditionFilter.Add(_parts[0].Trim(), _parts[1].Trim());
                                    }
                                }

                                var ConditionTrigger = new Dictionary<string, string>();
                                if (_guardParameters.Length > 1)
                                {
                                    string[] querySegmentsTrigger = _guardParameters[1].Split('&');
                                    foreach (string segment in querySegmentsTrigger)
                                    {
                                        string[] _parts = segment.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                        if (_parts.Length > 1)
                                        {
                                            ConditionTrigger.Add(_parts[0].Trim(), _parts[1].Trim());
                                        }
                                    }
                                }

                                for (int k = EventIndex + 1; k < Data.Count; k++)
                                {
                                    bool _rowIsTrue = true;
                                    foreach (var _key in ConditionFilter.Keys)
                                    {
                                        if (_key == "EventName")
                                        {
                                            if (Data[k].EventName != ConditionFilter[_key])
                                            {
                                                _rowIsTrue = false;
                                                break;
                                            }
                                        }
                                        else if (_key == "Element")
                                        {
                                            if (Data[k].Element != ConditionFilter[_key])
                                            {
                                                _rowIsTrue = false;
                                                break;
                                            }
                                        }
                                        else if (Data[k].EventValues[_key] != Data[k].EventValues[ConditionFilter[_key]])
                                        {
                                            _rowIsTrue = false;
                                            break;
                                        }
                                    }

                                    if (_rowIsTrue)
                                    {
                                        foreach (var _key in ConditionTrigger.Keys)
                                        {
                                            if (_key == "EventName")
                                            {
                                                if (Data[k].EventName != ConditionFilter[_key])
                                                {
                                                    _rowIsTrue = false;
                                                    break;
                                                }
                                            }
                                            else if (_key == "Element")
                                            {
                                                if (Data[k].Element != ConditionFilter[_key])
                                                {
                                                    _rowIsTrue = false;
                                                    break;
                                                }
                                            }
                                            else if (Data[k].EventValues[_key] != ConditionTrigger[_key])
                                            {
                                                _rowIsTrue = false;
                                                break;
                                            }
                                        }
                                        if (_rowIsTrue)
                                        {
                                            /*
                                            int _depth = k - EventIndex;
                                            if (!_activeTriggersForGuardGroup.ContainsKey(_depth))
                                            {
                                                _activeTriggersForGuardGroup.Add(_depth, new Tuple<bool, FSMTrigger>(_isLookAheadFirst, t));
                                            }
                                            else
                                            {
                                                // multiple guards with the identical distance --> Can't decide which guard is active ;-(
                                                _useAciveTriggersFoGuardGroup = false;
                                            }
                                            */
                                            int _depth = k - EventIndex;
                                            if (!_activeTriggersForGuardGroup.ContainsKey(_depth))
                                                _activeTriggersForGuardGroup.Add(_depth, new List<FSMTrigger>());

                                            _activeTriggersForGuardGroup[_depth].Add(t);

                                        }

                                    }
                                }

                                _guardIsTrue = false;
                                #endregion
                            }
                            else if (_g.ToLower().Trim().StartsWith("previous_state") || _g.ToLower().Trim().StartsWith("previousstate"))
                            {
                                #region GUARD: "previous_state"
                                string[] _guardParameters = _g.Split('(', ')')[1].Split(',');
                                string _value = _guardParameters[0];
                                string _state = "";
                                if (_guardParameters.Length == 1)
                                {
                                    _state = "StateBefore_1";
                                }
                                else if (_guardParameters.Length == 2)
                                {
                                    _state = "StateBefore_" + _guardParameters[1];
                                }
                                else
                                {
                                    throw new Exception("Guard not recognized: ' " + _g + "'");
                                }

                                string _currentState = Data[EventIndex].EventValues[_state];
                                int k = EventIndex - 1;
                                while (k >= 0)
                                {
                                    string _checkValue = Data[k].EventValues[_state];
                                    if (_checkValue != _currentState)
                                    {
                                        if (_checkValue.Replace(" ", "") != _value.Replace(" ", ""))
                                        {
                                            _guardIsTrue = false;
                                        }
                                        else
                                        {
                                            break;
                                        }

                                    }
                                    k--;
                                }
                                if (k == 0)
                                    _guardIsTrue = false;

                                #endregion
                            }
                            else if (_g.ToLower().Trim().StartsWith("look_ahead_condition") || _g.ToLower().Trim().StartsWith("not_look_ahead_condition"))
                            {
                                #region GUARD: "look_ahead_condition" / "not_look_ahead_condition"

                                /* 
                                 *  The guard 'look_ahead_condition' is defined as follows. It evaluates if a trigger (<-- _triggerOfInterest) (e.g., EventName=Response)
                                 *  is found in any / none of the following events, as long as the condition (<-- _conditionToInvestigate) (e.g., Page=SQ123) is met.
                                 */

                                bool _negation = _g.ToLower().Trim().StartsWith("not_look_ahead_condition");

                                string[] _guardParameters = _g.Split('(', ')')[1].Split(',');

                                var _triggerOfInterest = new Dictionary<string, string>();
                                if (_guardParameters.Length > 1)
                                {
                                    string[] querySegmentsTrigger = _guardParameters[0].Split('&');
                                    foreach (string segment in querySegmentsTrigger)
                                    {
                                        string[] _parts = segment.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                        if (_parts.Length > 1)
                                        {
                                            _triggerOfInterest.Add(_parts[0].Trim(), _parts[1].Trim());
                                        }
                                    }
                                }

                                var _conditionToBeInvestigated = new Dictionary<string, string>();
                                string[] querySegmentsFilter = _guardParameters[1].Split('&');
                                foreach (string segment in querySegmentsFilter)
                                {
                                    string[] _parts = segment.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    if (_parts.Length > 1)
                                    {
                                        _conditionToBeInvestigated.Add(_parts[0].Trim(), _parts[1].Trim());
                                    }
                                }
                                  
                                // Step 1: For how many rows is the condition valid?
                                //         EventIndex ... _EventIndexRangeEnd

                                bool _conditionIsValid = true;
                                int _EventIndexRangeEnd = EventIndex;
                                while (_conditionIsValid)
                                {
                                    foreach (var _key in _conditionToBeInvestigated.Keys)
                                    {
                                        if (_key == "EventName")
                                        {
                                            if (Data[_EventIndexRangeEnd + 1].EventName != _conditionToBeInvestigated[_key])
                                            {
                                                _conditionIsValid = false;
                                                break;
                                            }
                                        }
                                        else if (_key == "Element")
                                        {
                                            if (Data[_EventIndexRangeEnd + 1].Element != _conditionToBeInvestigated[_key])
                                            {
                                                _conditionIsValid = false;
                                                break;
                                            }
                                        }
                                        else if (Data[_EventIndexRangeEnd + 1].EventValues[_key] != _conditionToBeInvestigated[_key])
                                        {
                                            _conditionIsValid = false;
                                            break;
                                        }

                                        if (_conditionIsValid && _EventIndexRangeEnd + 2 < Data.Count)
                                        {
                                            _EventIndexRangeEnd++;
                                        }
                                        else
                                        {
                                            _conditionIsValid = false;
                                        }

                                    }
                                }

                                // Step 2: Check if the condition is fullfilled in any (or none, when _negation is true) of the rows in the range  EventIndex  ... _EventIndexRangeEnd.

                                if (_EventIndexRangeEnd > EventIndex + 1)
                                {
                                    bool _foundCondition = false;

                                    for (int k = EventIndex + 1; k < _EventIndexRangeEnd; k++)
                                    {
                                        bool _isTrueForThisRow = true;
                                        foreach (var _key in _triggerOfInterest.Keys)
                                        {
                                            if (_key == "EventName")
                                            {
                                                if (Data[k].EventName != _triggerOfInterest[_key])
                                                {
                                                    _isTrueForThisRow = false;
                                                    break;
                                                }
                                            }
                                            else if (_key == "Element")
                                            {
                                                if (Data[k].Element != _triggerOfInterest[_key])
                                                {
                                                    _isTrueForThisRow = false;
                                                    break;
                                                }
                                            }
                                            else if (Data[k].EventValues[_key] != _triggerOfInterest[_key])
                                            {
                                                _isTrueForThisRow = false;
                                                break;
                                            }
                                        }

                                        if (_isTrueForThisRow)
                                        {
                                            _foundCondition = true;
                                            break;
                                        }
                                    }

                                    if (!_foundCondition && !_negation)
                                    {
                                        _guardIsTrue = false;
                                    }
                                    else if (_foundCondition && _negation)
                                    {
                                        _guardIsTrue = false;
                                    }

                                }
                                else
                                {
                                    // condition not met for the current event.

                                    _guardIsTrue = false;
                                }  
                                 
                                #endregion
                            }
                            else
                            {
                                throw new Exception("Guard not recognized: ' " + _g + "'");
                            }

                        }
                    }

                    if (_guardIsTrue)
                    {
                        if (_activeTriggersForGuardGroup.Count == 0)
                        {
                            _return.Add(new Tuple<string,string>(t.GetTriggerName, t.OperatorString));
                        }
                        else
                        {
                            if (!_activeTriggersForGuardGroup.ContainsKey(0))
                                _activeTriggersForGuardGroup.Add(0, new List<FSMTrigger>());

                            _activeTriggersForGuardGroup[0].Add(t);
                        }
                    }

                }

            }

            // return list of active triggers, ordered by distance 

            if (_activeTriggersForGuardGroup.Count > 0)
            {
                List<int> _keys = _activeTriggersForGuardGroup.Keys.ToList();
                for (int i = 0; i < _keys.Count; i += 1)
                {
                    foreach (FSMTrigger t in _activeTriggersForGuardGroup[_keys[i]])
                    {
                        _return.Add(new Tuple<string, string>(t.GetTriggerName, t.OperatorString));
                    }
                    //_return.Add(new Tuple<string, string>(_activeTriggersForGuardGroup[_keys[i]].Item2.GetTriggerName, _activeTriggersForGuardGroup[_keys[i]].Item2.OperatorString));
                }
            }
             
            return _return;
        }

        public List<string> GetVarNames { get { return varNames; } }

        private List<string> varNames = new List<string>();
         
        public void SetVariableValue(EventData e, string VarName, string ValueOrAttributeName)
        {
            string _value = ValueOrAttributeName;
            if (e.EventValues.ContainsKey(ValueOrAttributeName))
            {
                _value = e.EventValues[ValueOrAttributeName];
            }
             
            if (e.EventValues.ContainsKey(VarName))
            {
                e.EventValues[VarName] = _value;
            }
            else
            {
                if (!varNames.Contains(VarName))
                    varNames.Add(VarName);

                e.AddEventValue(VarName, ValueOrAttributeName);
            }
        }

        public void CopyAttributeToVariable(EventData e, string VarName, string AttributeName)
        {
            if (e.EventValues.ContainsKey(VarName))
            {
                e.AddEventValue(VarName,e.GetEventValue(AttributeName));
            }
            else
            {
                if (!varNames.Contains(VarName))
                    varNames.Add(VarName);

                e.AddEventValue(VarName, e.GetEventValue(AttributeName));
            }
        }

        public void IncreaseIntValue(List<EventData> Data, int EventIndex, string VarName, string ValueOrAttributeName)
        {
            string _value = ValueOrAttributeName;
            if (Data[EventIndex].EventValues.ContainsKey(ValueOrAttributeName))
            {
                _value = Data[EventIndex].EventValues[ValueOrAttributeName];
            }


            if (!Data[EventIndex].EventValues.ContainsKey(VarName) && !varNames.Contains(VarName))
                varNames.Add(VarName);

            int _newValue = 0;
            bool _newValueIsIntValue = int.TryParse(_value, out _newValue);
            if (!_newValueIsIntValue)
            {
                Data[EventIndex].AddEventValue(VarName, "0");
            }
            else
            {
                if (EventIndex == 0)
                {
                    Data[EventIndex].AddEventValue(VarName, _newValue.ToString());
                }
                else
                {
                    string _previousValue = Data[EventIndex - 1].GetEventValue(VarName);
                    int _previousIntValue = 0;
                    bool _previousValueIsInt = int.TryParse(_previousValue, out _previousIntValue);
                    if (_previousValueIsInt)
                    {
                        Data[EventIndex].AddEventValue(VarName, (_previousIntValue + _newValue).ToString());
                    }
                    else
                    {
                        Data[EventIndex].AddEventValue(VarName, (_newValue).ToString());
                    }
                }
            }


        }
         
        public void UpdateVariables(List<EventData> Data, int EventIndex)
        { 
            if (EventIndex > 0)
            {
                // Compute time in state 

                int _m = 1;
                while (Data[EventIndex - 1].EventValues.ContainsKey("StateBefore_" + _m) && Data[EventIndex - 1].EventValues.ContainsKey("StateAfter_" + _m))
                {
                    if (!Data[EventIndex - 1].EventValues.ContainsKey("TimeInState_" + _m))
                        Data[EventIndex - 1].AddEventValue("TimeInState_" + _m, "0");

                    if (Data[EventIndex - 1].EventValues["StateBefore_" + _m] == Data[EventIndex - 1].EventValues["StateAfter_" + _m])
                        Data[EventIndex].AddEventValue("TimeInState_" + _m, (Data[EventIndex].TimeDifferencePrevious.TotalMilliseconds + double.Parse(Data[EventIndex - 1].EventValues["TimeInState_" + _m])).ToString());
                    else
                        Data[EventIndex].AddEventValue("TimeInState_" + _m, Data[EventIndex].TimeDifferencePrevious.TotalMilliseconds.ToString());
                    _m++;
                }

                // Carry values of variables forward

                foreach (string v in varNames)
                    Data[EventIndex].AddEventValue(v, Data[EventIndex - 1].GetEventValue(v));                 
            }            

        }

        public void ExportTextFile(string FileName, string Text)
        {
            File.WriteAllText(FileName, Text);
        }
    }


    public class FSMIgnore
    { 
        public int MachineIndex { get; set; }
        public string State { get; set; }
        public List<FSMTrigger> Triggers { get; set; }
        public FSMIgnore()
        {
            Triggers = new List<FSMTrigger>();
        }
    }


    public class FSMTransition
    {
        public int MachineIndex { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public List<FSMTrigger> Triggers { get; set; }
        public FSMTransition()
        {
            Triggers = new List<FSMTrigger>();
        }
    }

    public class FSMOperator
    {
        public int MachineIndex { get; set; }
        public string TriggerName { get; set; }

        public string State { get; set; }

        public string OperatorString { get; set; }

        public string GetKey
        {
            get { return State + "_logfsm_id_" + MachineIndex + "_" + TriggerName; }
        }

    }

    public class FSMTrigger
    {
        public int MachineIndex { get; set; }

        public string ConditionString { get; set; }

        public string OperatorString { get; set; }

        public string GuardString { get; set; }

        public string GetComment
        {
            get
            {
                string _ret = "";
                foreach (string _k in Condition.Keys)
                    _ret = _ret + _k + "=" + Condition[_k] + ";";

                if (_ret.EndsWith(";"))
                    _ret = _ret.Substring(0, _ret.Length - 1);

                return _ret;
            }
        }

        public string GetTriggerName
        {
            get
            {
                string _ret = "t_";
                foreach (string _k in Condition.Keys)
                    _ret = _ret + _k + "_" + CreateValidIdentifier(Condition[_k]) + "_";
               
                _ret = _ret + CreateValidIdentifier(GuardString) + "_" + CreateValidIdentifier(OperatorString) + "_" + MachineIndex;

                return _ret;
            }
        }

        private string CreateValidIdentifier(string Identifier)
        {
            return Identifier.Trim().Replace("(", "paropen").Replace(")", "parclose").
                Replace("&", "and").Replace(",", "comma").Replace("=", "equal").
                Replace("$", "dollar").Replace("*", "star").Replace("0", "zero").
                Replace("1", "one").Replace("2", "two").Replace("3", "three").
                Replace("4", "four").Replace("5", "five").Replace("6", "six").
                Replace("7", "seven").Replace("8", "eight").Replace("9", "nine").
                Replace("|", "bar").Replace("<", "gt").Replace(">", "st").
                Replace("-", "minus").Replace("+","plus").Replace(".","dot").Replace(" ", "").Replace(";", "semicolon");
            // return String.Join("", Identifier.Split(' ', '&', '|', '$', '*', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '(', '(', ',', '=', '\'', '\"', '|', '>', '<'));
        }

        public List<string> States { get; set; }

        public string GetStatesString
        {
            get
            {
                return String.Join(";", States);
            }
        }

        public Dictionary<string, string> Condition { get; set; }
         
        public FSMTrigger(string ConditionString, string GuardString, string OperatorString, int MachineIndex, string StatesString)
        {
            this.ConditionString = ConditionString;
            this.OperatorString = OperatorString; 
            this.GuardString = GuardString;
            this.MachineIndex = MachineIndex;

            Condition = new Dictionary<string, string>();

            if (!ConditionString.Contains("="))
            {
                ConditionString = "EventName=" + ConditionString;
            }

            string[] querySegments = ConditionString.Split('&');
            foreach (string segment in querySegments)
            { 
                string[] _parts = segment.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (_parts.Length > 1)
                {
                    Condition.Add(_parts[0], _parts[1]);
                }
            }

            if (StatesString.Trim() != "")
                States = StatesString.Split(';').ToList<string>();
            else
                States = new List<string>();
        }
    }
}
