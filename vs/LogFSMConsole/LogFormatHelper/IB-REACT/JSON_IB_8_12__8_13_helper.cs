#region usings 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
#endregion

namespace LogDataTransformer_IB_REACT_8_12__8_13
{
    #region Reader

    public static class JSON_IB_8_12__8_13_helper
    { 
        public static List<Log_IB_8_12__8_13> ParseTraceLogs(string line, double UTCOffset)
        {
            List<Log_IB_8_12__8_13> _ret = new List<Log_IB_8_12__8_13>();

            var _trace = JsonConvert.DeserializeObject<LogDataTransformer_IRTlibPlayer_V01.TraceLog>(line);
            if (_trace.Log == null)
                return _ret;

            string _element = "";
            string _test = "";
            string _task = "";
            string _bookklet = "";
            string _preview = "";
            int _traceId = -1;

            if (_trace.Context != null)
            {
                if (_trace.Context.Item != null)
                    _element = _trace.Context.Item;
                if (_trace.Context.Test != null)
                    _test = _trace.Context.Test;
                if (_trace.Context.Task != null)
                    _task = _trace.Context.Task;
                if (_trace.Context.Booklet != null)
                    _bookklet = _trace.Context.Booklet;
                if (_trace.Context.Preview != null)
                    _preview = _trace.Context.Preview;
                _traceId = _trace.TraceID;

            }

            // Hint: Timestamp is currently UTC 
            _ret.Add(new PlatformTraceLog() { Trigger = _trace.Trigger, Log = _trace.Log, Sender = _trace.Sender, TimeStamp = _trace.Timestamp.AddHours(UTCOffset), SessonId = _trace.SessionId, Element = _element, EventName = nameof(PlatformTraceLog), Booklet = _bookklet, Preview = _preview, TraceId = _traceId });

            return _ret;
        }

        public static List<Log_IB_8_12__8_13> ParseLogElements(string line, string source, bool check, string _personIdentifier)
        {
            List<Log_IB_8_12__8_13> _ret = new List<Log_IB_8_12__8_13>();

            ItemBuilder_React_Runtime_trace logFragment = null;
            
            string _element = "";
            string _test = "";
            string _task = "";
            int _tracId = -1;

            if (source == "IRTlibPlayer_V01")
            {
                try
                {
                    // TODO: Make MaxDepth a Parameter
                    // var _settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, MaxDepth = 128 };
                    // var _jsonSerializer = Newtonsoft.Json.JsonSerializer.Create(_settings);
                    var _trace = JsonConvert.DeserializeObject<LogDataTransformer_IRTlibPlayer_V01.TraceEvent>(line, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore, MaxDepth = 512 });
                    if (_trace.Trace == null)
                        return _ret;

                    _element = _trace.Context.Item;
                    _test = _trace.Context.Test;
                    _task = _trace.Context.Task;
                    _tracId = _trace.TraceID;

                    logFragment = JsonConvert.DeserializeObject<ItemBuilder_React_Runtime_trace>(_trace.Trace, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore, MaxDepth = 512 });

                    if (_personIdentifier == "")
                        _personIdentifier = logFragment.metaData.userId;
                    if (_personIdentifier.Contains("\r"))
                        _personIdentifier = _personIdentifier.Replace("\r", "");

                }
                catch (Exception _innerException1)
                {
                    Console.WriteLine(_innerException1.ToString());
                }
            }
            else if (source == "IBSD_V01")
            {
                logFragment = JsonConvert.DeserializeObject<ItemBuilder_React_Runtime_trace>(line, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore, MaxDepth = 512 });

                _personIdentifier = logFragment.metaData.userId;
                if (_personIdentifier.Contains("\r"))
                    _personIdentifier = _personIdentifier.Replace("\r", "");

            }
            else if (source == "Firebase_V01")
            {
                try
                {
                    var _trace = JsonConvert.DeserializeObject<LogDataTransformer_Firebase_V01.TraceDatePoint>(line);
                    if (_trace.trace == null)
                        return _ret;

                    _element = _trace.item;
                    _test = _trace.test;
                    _task = _trace.task;

                    ItemBuilder_React_Runtime_trace_package package = JsonConvert.DeserializeObject<ItemBuilder_React_Runtime_trace_package>(_trace.trace, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore });
                    logFragment = package.traceLogData;

                    _personIdentifier = _trace.code;
                }
                catch (Exception _innerException1)
                {
                    Console.WriteLine(_innerException1.ToString());
                }
            }
            else if (source == "TAOPCI_V01")
            {
                logFragment = JsonConvert.DeserializeObject<ItemBuilder_React_Runtime_trace>(line, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore, MaxDepth = 512 });
                _personIdentifier = logFragment.metaData.userId;
            }
            else if (source == "TAOPCI_V02")
            {
                logFragment = new ItemBuilder_React_Runtime_trace();
                var _fragment = JsonConvert.DeserializeObject<ItemBuilder_React_Runtime_trace_element>(line, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore, MaxDepth = 512 });
                logFragment.logEntriesList = new ItemBuilder_React_Runtime_trace_element[] { _fragment };
                logFragment.metaData = new metaData();
                logFragment.metaData.loginTimestamp = DateTime.MinValue.ToString();
                logFragment.metaData.sendTimestamp = DateTime.MinValue.ToString();
            }
            else if (source == "Alea_V01")
            {
                logFragment = JsonConvert.DeserializeObject<ItemBuilder_React_Runtime_trace>(line);
            }

            if (logFragment != null)
            {
                foreach (var entry in logFragment.logEntriesList)
                {
                    string _pageAreaType = "";
                    string _pageAreaName = "";
                    string _page = "";

                    if (!int.TryParse(entry.EntryId, out _tracId))
                        _tracId = -1;

                    string _cbaVers = logFragment.metaData.cbaVers;
                    string _sessionId = logFragment.metaData.sessionId;
                    DateTime _loginTimestamp = DateTime.Parse(logFragment.metaData.loginTimestamp);
                    DateTime _sendTimestamp = DateTime.Parse(logFragment.metaData.sendTimestamp);
                     
                    if (entry.Details != null && entry.Details.ContainsKey("indexPath"))
                    {
                        string ret = entry.Details["indexPath"].ToXmlSafeString();
                        string[] parts = ret.Split("/", StringSplitOptions.RemoveEmptyEntries);
                        _test = parts[0].Replace("test=", "");

                        if (parts.Length > 1)
                            _element = parts[1].Replace("item=", "");
                        if (parts.Length > 2)
                            _task = parts[2].Replace("task=", "");
                        if (parts.Length > 3)
                            _pageAreaType = parts[3].Replace("pageAreaType=", "");
                        if (parts.Length > 4)
                            _pageAreaName = parts[4].Replace("pageAreaName=", "");
                        if (parts.Length > 5)
                            _page = parts[5].Replace("page=", "");
                    } 
                    else
                    {                        
                        _task = "(not specified)";
                        _pageAreaType = "(not specified)";
                        _page = "(not specified)";
                    }

                    if (_element == null)
                    {
                        Console.WriteLine("Not implemented: No Element specified");
                    } 
                    else if (_element == "")
                    {
                        _element = "(Platform)";
                    }

                    if (entry.Type == "TasksViewVisible")
                    {
                        #region TasksViewVisible

                        if (entry.Details.ContainsKey("settings"))
                        {
                            var _s = entry.Details["settings"];
                            TasksViewVisible details = new TasksViewVisible()
                            {
                                Element = _element,
                                EventID = int.Parse(entry.EntryId),
                                EventName = entry.Type,
                                PersonIdentifier = _personIdentifier,
                                TimeStamp = DateTime.Parse(entry.Timestamp),
                                TraceId = _tracId,
                                Task = _task,
                                Test = _test,
                                PageAreaName = _pageAreaName,
                                Page = _page,
                                PageAreaType = _pageAreaType,
                                CbaVers = _cbaVers,
                                SessionId = _sessionId,
                                LoginTimestamp = _loginTimestamp,
                                SendTimestamp = _sendTimestamp
                            };

                            if (_s.Contains("AllowScoreDebugging"))
                                details.AllowScoreDebugging = bool.Parse(_s["AllowScoreDebugging"].ToXmlSafeString());
                            
                            if (_s.Contains("AllowFSMDebugging"))
                                details.AllowFSMDebugging = bool.Parse(_s["AllowFSMDebugging"].ToXmlSafeString());

                            if (_s.Contains("AllowTraceDebugging"))
                                details.AllowTraceDebugging = bool.Parse(_s["AllowTraceDebugging"].ToXmlSafeString());

                            if (_s.Contains("ShowTaskNavigationBars"))
                                details.ShowTaskNavigationBars = bool.Parse(_s["ShowTaskNavigationBars"].ToXmlSafeString());

                            _ret.Add(details);
                        }

                        #endregion

                        // Info: 'headerButtons', 'upperHeaderMenu', 'lowerHeaderMenu' ignored
                    }
                    else if (entry.Type == "UserLogin")
                    {
                        #region UserLogin
                        UserLogin details = new UserLogin()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            user = entry.Details.ContainsKey("Details") ? entry.Details["user"].ToXmlSafeString() : "",
                            loginTimestamp = entry.Details["loginTimestamp"].ToXmlSafeString(),
                            runtimeVersion = entry.Details["runtimeVersion"].ToXmlSafeString(),
                            webClientUserAgent = entry.Details["webClientUserAgent"].ToXmlSafeString(),

                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };
                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "ItemSwitch")
                    {
                        #region ItemSwitch

                        string _name = "";
                        if (entry.Details != null && entry.Details.ContainsKey("name"))
                            _name = entry.Details["item"]["name"].ToXmlSafeString();

                        ItemSwitch details = new ItemSwitch()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,
                            name = _name,
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };
                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "TaskSwitch")
                    {
                        #region TaskSwitch

                        if (entry.Details.ContainsKey("oldItem"))
                            _element = entry.Details["oldItem"].ToXmlSafeString();

                        TaskSwitch details = new TaskSwitch()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("newTask"))
                            details.newTask = entry.Details["newTask"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("newItem"))
                            details.newItem = entry.Details["newItem"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("newTest"))
                            details.newTest = entry.Details["newTest"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("oldTask"))
                            details.oldTask = entry.Details["oldTask"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("oldItem"))
                            details.oldItem = entry.Details["oldItem"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("oldTest"))
                            details.oldTest = entry.Details["oldTest"].ToXmlSafeString();

                        if (entry.Details.ContainsKey("taskResult"))
                        {
                            JObject taskResult = (entry.Details["taskResult"] as JObject);

                            if (taskResult.ContainsKey("hitsAccumulated"))
                                details.hitsAccumulated = long.Parse(entry.Details["taskResult"]["hitsAccumulated"].ToXmlSafeString());
                            if (taskResult.ContainsKey("hitsCount"))
                                details.hitsCount = long.Parse(entry.Details["taskResult"]["hitsCount"].ToXmlSafeString());
                            if (taskResult.ContainsKey("missesAccumulated"))
                                details.missesAccumulated = long.Parse(entry.Details["taskResult"]["missesAccumulated"].ToXmlSafeString());
                            if (taskResult.ContainsKey("missesCount"))
                                details.missesCount = long.Parse(entry.Details["taskResult"]["missesCount"].ToXmlSafeString());
                            if (taskResult.ContainsKey("classMaxWeighed"))
                                details.classMaxWeighed = double.Parse(entry.Details["taskResult"]["classMaxWeighed"].ToXmlSafeString());
                            if (taskResult.ContainsKey("classMaxName"))
                                details.classMaxName = entry.Details["taskResult"]["classMaxName"].ToXmlSafeString();
                            if (taskResult.ContainsKey("totalResult"))
                                details.totalResult = double.Parse(entry.Details["taskResult"]["totalResult"].ToXmlSafeString());
                            if (taskResult.ContainsKey("nbUserInteractions"))
                                details.nbUserInteractions = long.Parse(entry.Details["taskResult"]["nbUserInteractions"].ToXmlSafeString());
                            if (taskResult.ContainsKey("nbUserInteractionsTotal"))
                                details.nbUserInteractionsTotal = long.Parse(entry.Details["taskResult"]["nbUserInteractionsTotal"].ToXmlSafeString());
                            if (taskResult.ContainsKey("firstReactionTime"))
                                details.firstReactionTime = double.Parse(entry.Details["taskResult"]["firstReactionTime"].ToXmlSafeString());
                            if (taskResult.ContainsKey("firstReactionTimeTotal"))
                                details.firstReactionTimeTotal = double.Parse(entry.Details["taskResult"]["firstReactionTimeTotal"].ToXmlSafeString());
                            if (taskResult.ContainsKey("taskExecutionTime"))
                                details.taskExecutionTime = double.Parse(entry.Details["taskResult"]["taskExecutionTime"].ToXmlSafeString());
                            if (taskResult.ContainsKey("taskExecutionTimeTotal"))
                                details.taskExecutionTimeTotal = double.Parse(entry.Details["taskResult"]["taskExecutionTimeTotal"].ToXmlSafeString());

                            // extract scoring

                            Dictionary<string, HitList> taskResultDict = new Dictionary<string, HitList>();
                            foreach (var pair in taskResult)
                            {
                                string[] _parts = pair.Key.Split('.', StringSplitOptions.RemoveEmptyEntries);

                                if (_parts.Length > 1)
                                {
                                    if (!taskResultDict.ContainsKey(_parts[1]))
                                        taskResultDict.Add(_parts[1], new HitList() { Name = _parts[1], Text = "" });

                                    if (_parts[0].EndsWith("Text"))
                                    {
                                        taskResultDict[_parts[1]].Text = pair.Value.ToXmlSafeString();
                                    }
                                    else if (_parts[0].EndsWith("Weighed"))
                                    {
                                        taskResultDict[_parts[1]].Weight = double.Parse(pair.Value.ToXmlSafeString());
                                    }
                                    else
                                    {
                                        taskResultDict[_parts[1]].Typ = _parts[0];
                                        taskResultDict[_parts[1]].Value = pair.Value.ToXmlSafeString();
                                    }
                                }
                            }
                            details.HitList = taskResultDict.Values.ToArray<HitList>();
                        }


                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "PageSwitchTopLevel")
                    {
                        #region PageSwitchTopLevel
                        PageSwitchTopLevel details = new PageSwitchTopLevel()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            newPageAreaType = entry.Details["pageAreaType"].ToXmlSafeString(),
                            newPageAreaName = entry.Details["pageAreaName"].ToXmlSafeString(),
                            newPageName = entry.Details["newPageName"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };
                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "PageSwitchEmbedded")
                    {
                        #region PageSwitchEmbedded
                        PageSwitchEmbedded details = new PageSwitchEmbedded()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            newPageName = entry.Details["newPageName"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("tab"))
                            details.tab = entry.Details["tab"].ToXmlSafeString();

                        if (entry.Details.ContainsKey("historyMove"))
                            details.historyMove = entry.Details["historyMove"].ToXmlSafeString();

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "Button")
                    {
                        #region Button
                        Button details = new Button()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,
                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            userDefId = entry.Details["userDefId"].ToXmlSafeString(),
                            oldSelected = bool.Parse(entry.Details["oldSelected"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("subtype"))
                            details.subtype = entry.Details["subtype"].ToXmlSafeString();

                        RetrievedEventDetials(entry, details, check);
                         
                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "Checkbox")
                    {
                        #region Checkbox
                        Checkbox details = new Checkbox()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            oldSelected = bool.Parse(entry.Details["oldSelected"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "RadioButton")
                    {
                        #region RadioButton
                        RadioButton details = new RadioButton()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            oldSelected = bool.Parse(entry.Details["oldSelected"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "Link")
                    {
                        #region Link
                        Link details = new Link()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            oldSelected = bool.Parse(entry.Details["oldSelected"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "SimpleTextField")
                    {
                        #region SimpleTextField
                        SimpleTextField details = new SimpleTextField()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "ImageField")
                    {
                        #region ImageField
                        ImageField details = new ImageField()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "ExternalPageFrame")
                    { 
                        #region ExternalPageFrame
                        ExternalPageFrame details = new ExternalPageFrame()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "Panel")
                    {
                        #region Panel
                        Panel details = new Panel()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "Container")
                    {
                        #region Container
                        Container details = new Container()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "Combobox")
                    {
                        #region Combobox
                        Combobox details = new Combobox()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            oldSelected = int.Parse(entry.Details["oldSelected"].ToXmlSafeString()),
                            newSelected = int.Parse(entry.Details["newSelected"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("oldSelectedUserDefId"))
                            details.oldSelectedUserDefId = entry.Details["oldSelectedUserDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("newSelectedUserDefId"))
                            details.newSelectedUserDefId = entry.Details["newSelectedUserDefId"].ToXmlSafeString();

                        RetrievedEventDetials(entry, details, check);
                         
                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "TableCell")
                    {
                        #region TableCell
                        TableCell details = new TableCell()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            row = int.Parse(entry.Details["row"].ToXmlSafeString()),
                            column = int.Parse(entry.Details["column"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("tableUserDefIdPath"))
                            details.tableUserDefIdPath = entry.Details["tableUserDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("tableUserDefId"))
                            details.tableUserDefId = entry.Details["tableUserDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("oldSelected"))
                            details.oldSelected = bool.Parse(entry.Details["oldSelected"].ToXmlSafeString());

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "TableCellModified")
                    {
                        #region TableCellModified
                        TableCellModified details = new TableCellModified()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            row = int.Parse(entry.Details["row"].ToXmlSafeString()),
                            column = int.Parse(entry.Details["column"].ToXmlSafeString()),
                            cellType = entry.Details["cellType"].ToXmlSafeString(),
                            oldValue = entry.Details["oldValue"].ToXmlSafeString(),
                            newValue = entry.Details["newValue"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                       if (entry.Details.ContainsKey("tableUserDefIdPath"))
                            details.tableUserDefIdPath = entry.Details["tableUserDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("tableUserDefId"))
                            details.tableUserDefId = entry.Details["tableUserDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("oldEvaluatedValue"))
                            details.oldEvaluatedValue = double.Parse(entry.Details["oldEvaluatedValue"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("newEvaluatedValue"))
                            details.newEvaluatedValue = double.Parse(entry.Details["newEvaluatedValue"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("errorInFormula"))
                            details.errorInFormula = entry.Details["errorInFormula"].ToXmlSafeString();

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "BrowserTab")
                    {
                        #region BrowserTab
                        BrowserTab details = new BrowserTab()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            tab = entry.Details["tab"].ToXmlSafeString(),
                            page = entry.Details["page"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "RegionMap")
                    {
                        #region RegionMap
                        RegionMap details = new RegionMap()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "ImageArea")
                    {
                        #region ImageArea
                        ImageArea details = new ImageArea()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            oldSelected = bool.Parse(entry.Details["oldSelected"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "SingleLineInputField")
                    {
                        #region SingleLineInputField
                        SingleLineInputField details = new SingleLineInputField()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(), 
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("currentTextValue"))
                            details.currentTextValue = entry.Details["currentTextValue"].ToXmlSafeString();

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "SingleLineInputFieldModified")
                    {
                        #region SingleLineInputFieldModified
                        SingleLineInputFieldModified details = new SingleLineInputFieldModified()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,
                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };
                         
                        if (entry.Details.ContainsKey("validationPattern"))
                            details.validationPattern = entry.Details["validationPattern"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("invalidTextValue"))
                            details.invalidTextValue = entry.Details["invalidTextValue"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("oldTextValue"))
                            details.oldTextValue = entry.Details["oldTextValue"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("newTextValue"))
                            details.newTextValue = entry.Details["newTextValue"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("origin"))
                            details.origin = entry.Details["origin"].ToXmlSafeString();

                        RetrievedEventDetials(entry, details, check);
                         
                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "InputField")
                    {
                        #region InputField
                        InputField details = new InputField()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            currentTextValue = entry.Details["currentTextValue"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "InputFieldModified")
                    {
                        #region InputFieldModified
                        InputFieldModified details = new InputFieldModified()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            oldTextValue = entry.Details["oldTextValue"].ToXmlSafeString(),
                            newTextValue = entry.Details["newTextValue"].ToXmlSafeString(),
                            origin = entry.Details["origin"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("validationPattern"))
                            details.validationPattern = entry.Details["validationPattern"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("invalidTextValue"))
                            details.invalidTextValue = entry.Details["invalidTextValue"].ToXmlSafeString();

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "ValueInput")
                    { 
                        #region ValueInput

                        ValueInput details = new ValueInput()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion                        
                        
                    }
                    else if (entry.Type == "ValueInputFieldModified")
                    {
                        // TODO: Implement final version

                        Console.WriteLine("Not implemented: " + entry.Type);

                    }
                    else if (entry.Type == "ScaleValueInput")
                    {
                        #region ScaleValueInput
                        ScaleValueInput details = new ScaleValueInput()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(), 
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion 
                    }
                    else if (entry.Type == "SpinnerValueInput")
                    {
                        #region SpinnerValueInput

                        SpinnerValueInput details = new SpinnerValueInput()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(), 
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };
  
                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion                        
                    }
                    else if (entry.Type == "ValueDisplay")
                    {
                        #region ValueDisplay

                        ValueDisplay details = new ValueDisplay()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            displayType = entry.Details["displayType"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "PageArea")
                    {
                        #region PageArea
                        PageArea details = new PageArea()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };
                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "ImageTextField")
                    {
                        #region ImageTextField 
                        ImageTextField details = new ImageTextField()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            oldSelected = bool.Parse(entry.Details["oldSelected"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "RichText" || entry.Type == "RichTextField") // Note: "RichTextField" is outdated
                    {
                        #region RichText 
                        RichText details = new RichText()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            oldSelected = bool.Parse(entry.Details["oldSelected"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "EmbeddedLink")
                    {
                        #region EmbeddedLink
                        EmbeddedLink details = new EmbeddedLink()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };
                         
                        if (entry.Details.ContainsKey("clientX"))
                            details.clientX = long.Parse(entry.Details["clientX"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("clientY"))
                            details.clientY = long.Parse(entry.Details["clientY"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("pageX"))
                            details.pageX = long.Parse(entry.Details["pageX"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("pageY"))
                            details.pageY = long.Parse(entry.Details["pageY"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("screenX"))
                            details.screenX = long.Parse(entry.Details["screenX"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("screenY"))
                            details.screenY = long.Parse(entry.Details["screenY"].ToXmlSafeString());

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "RichTextHighlight")
                    {
                        #region RichTextHighlight
                        RichTextHighlight details = new RichTextHighlight()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            userDefId = entry.Details["userDefId"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("oldSelections"))
                        {
                            details.oldSelections = JsonConvert.DeserializeObject<List<RichTextHighlightFragment>>(entry.Details["oldSelections"].ToXmlSafeString());
                        }
                        if (entry.Details.ContainsKey("newSelections"))
                        {
                            details.newSelections = JsonConvert.DeserializeObject<List<RichTextHighlightFragment>>(entry.Details["newSelections"].ToXmlSafeString());
                        }

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "AudioPlayer" || entry.Type == "AudioPlayerControl")
                    {
                        #region AudioPlayer
                        try
                        {
                            AudioPlayerControl details = new AudioPlayerControl()
                            {
                                Element = _element,
                                EventID = int.Parse(entry.EntryId),
                                EventName = entry.Type,
                                PersonIdentifier = _personIdentifier,
                                TimeStamp = DateTime.Parse(entry.Timestamp),
                                TraceId = _tracId,
                                Task = _task,
                                Test = _test,
                                PageAreaName = _pageAreaName,
                                Page = _page,
                                PageAreaType = _pageAreaType,

                                indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                                CbaVers = _cbaVers,
                                SessionId = _sessionId,
                                LoginTimestamp = _loginTimestamp,
                                SendTimestamp = _sendTimestamp
                            };

                            if (entry.Details.ContainsKey("maxPlay"))
                                details.maxPlay = int.Parse(entry.Details["maxPlay"].ToXmlSafeString());
                            if (entry.Details.ContainsKey("currentPlayNo"))
                                details.currentPlayNo = int.Parse(entry.Details["currentPlayNo"].ToXmlSafeString());
                            if (entry.Details.ContainsKey("automaticStart"))
                                details.automaticStart = bool.Parse(entry.Details["automaticStart"].ToXmlSafeString());
                            if (entry.Details.ContainsKey("hideControls"))
                                details.hideControls = bool.Parse(entry.Details["hideControls"].ToXmlSafeString());
                            if (entry.Details.ContainsKey("volumeLevel"))
                                details.volumeLevel = double.Parse(entry.Details["volumeLevel"].ToXmlSafeString());
                            if (entry.Details.ContainsKey("operation"))
                                details.operation = entry.Details["operation"].ToXmlSafeString();
                            if (entry.Details.ContainsKey("userDefId"))
                                details.userDefId = entry.Details["userDefId"].ToXmlSafeString();
                            if (entry.Details.ContainsKey("userDefIdPath"))
                                details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();
                            if (entry.Details.ContainsKey("isStatemachineTriggered"))
                                details.isStatemachineTriggered = bool.Parse(entry.Details["isStatemachineTriggered"].ToXmlSafeString());

                            _ret.Add(details);
                        }
                        catch
                        {
                            Console.WriteLine("AudioPlayer -- Attribute Error: " + entry.Details.ToXmlSafeString());
                        }

                        #endregion
                    }
                    else if (entry.Type == "VideoPlayer" || entry.Type == "VideoPlayerControl")
                    {
                        #region VideoPlayer
                        try
                        {
                            VideoPlayerControl details = new VideoPlayerControl()
                            {
                                Element = _element,
                                EventID = int.Parse(entry.EntryId),
                                EventName = entry.Type,
                                PersonIdentifier = _personIdentifier,
                                TimeStamp = DateTime.Parse(entry.Timestamp),
                                TraceId = _tracId,
                                Task = _task,
                                Test = _test,
                                PageAreaName = _pageAreaName,
                                Page = _page,
                                PageAreaType = _pageAreaType,

                                indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                                CbaVers = _cbaVers,
                                SessionId = _sessionId,
                                LoginTimestamp = _loginTimestamp,
                                SendTimestamp = _sendTimestamp
                            };

                            /* TODO: The following attributes should not be missing*/

                            if (entry.Details.ContainsKey("maxPlay"))
                                details.maxPlay = int.Parse(entry.Details["maxPlay"].ToXmlSafeString());
                            if (entry.Details.ContainsKey("currentPlayNo"))
                                details.currentPlayNo = int.Parse(entry.Details["currentPlayNo"].ToXmlSafeString());
                            if (entry.Details.ContainsKey("automaticStart"))
                                details.automaticStart = bool.Parse(entry.Details["automaticStart"].ToXmlSafeString());
                            if (entry.Details.ContainsKey("hideControls"))
                                details.hideControls = bool.Parse(entry.Details["hideControls"].ToXmlSafeString());
                            if (entry.Details.ContainsKey("volumeLevel"))
                                details.volumeLevel = double.Parse(entry.Details["volumeLevel"].ToXmlSafeString());
                            if (entry.Details.ContainsKey("operation"))
                                details.operation = entry.Details["operation"].ToXmlSafeString();

                            /* END-TODO*/

                            if (entry.Details.ContainsKey("userDefId"))
                                details.userDefId = entry.Details["userDefId"].ToXmlSafeString();
                            if (entry.Details.ContainsKey("userDefIdPath"))
                                details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();
                            if (entry.Details.ContainsKey("isStatemachineTriggered"))
                                details.isStatemachineTriggered = bool.Parse(entry.Details["isStatemachineTriggered"].ToXmlSafeString());

                            _ret.Add(details);
                        }
                        catch
                        {
                            Console.WriteLine("VideoPlayer -- Attribute Error: " + entry.Details.ToXmlSafeString());
                        }
                        #endregion
                    }
                    else if (entry.Type == "ScrollbarMove")
                    {
                        #region ScrollbarMove
                        ScrollbarMove details = new ScrollbarMove()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("userDefId"))
                            details.userDefId = entry.Details["userDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("userDefIdPath"))
                            details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("userDefIdPath"))
                            details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("orientation"))
                            details.orientation = entry.Details["orientation"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("horizontalScroll"))
                            details.horizontalScroll = entry.Details["horizontalScroll"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("verticalScroll"))
                            details.verticalScroll = entry.Details["verticalScroll"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("direction"))
                            details.direction = entry.Details["direction"].ToXmlSafeString();

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "ScrollbarLogEntry")
                    {
                        /* TODO: Is this still a valid event?
                         * 
                         **/
                        #region ScrollbarLogEntry -> ScrollbarMove
                        ScrollbarMove details = new ScrollbarMove()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("userDefId"))
                            details.userDefId = entry.Details["userDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("userDefIdPath"))
                            details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("userDefIdPath"))
                            details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("orientation"))
                            details.orientation = entry.Details["orientation"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("horizontalScroll"))
                            details.horizontalScroll = entry.Details["horizontalScroll"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("verticalScroll"))
                            details.verticalScroll = entry.Details["verticalScroll"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("direction"))
                            details.direction = entry.Details["direction"].ToXmlSafeString();

                        _ret.Add(details);
                        #endregion

                    }
                    else if (entry.Type == "Scrollbar")
                    {
                        #region Scrollbar
                        Scrollbar details = new Scrollbar()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            orientation = entry.Details["orientation"].ToXmlSafeString(),
                            position = double.Parse(entry.Details["position"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("userDefId"))
                            details.userDefId = entry.Details["userDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("userDefIdPath"))
                            details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "DragAndDropReceive")
                    {
                        #region DragAndDropReceive

                        if (entry.Details.ContainsKey("senderIndexPath") || entry.Details.ContainsKey("receiverIndexPath"))
                        {
                            string ret = "";
                            if (entry.Details.ContainsKey("senderIndexPath"))
                                ret = entry.Details["senderIndexPath"].ToXmlSafeString();
                            else
                                ret = entry.Details["receiverIndexPath"].ToXmlSafeString();

                            string[] parts = ret.Split("/", StringSplitOptions.RemoveEmptyEntries);
                            _test = parts[0].Replace("test=", "");

                            if (parts.Length > 1)
                                _element = parts[1].Replace("item=", "");
                            if (parts.Length > 2)
                                _task = parts[2].Replace("task=", "");
                            if (parts.Length > 3)
                                _pageAreaType = parts[3].Replace("pageAreaType=", "");
                            if (parts.Length > 4)
                                _pageAreaName = parts[4].Replace("pageAreaName=", "");
                            if (parts.Length > 5)
                                _page = parts[5].Replace("page=", "");
                        }

                        DragAndDropReceive details = new DragAndDropReceive()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            senderIndexPath = entry.Details["senderIndexPath"].ToXmlSafeString(),
                            senderUserDefIdPath = entry.Details["senderUserDefIdPath"].ToXmlSafeString(),
                            senderUserDefId = entry.Details["senderUserDefId"].ToXmlSafeString(),
                            receiverIndexPath = entry.Details["receiverIndexPath"].ToXmlSafeString(),
                            receiverUserDefIdPath = entry.Details["receiverUserDefIdPath"].ToXmlSafeString(),
                            receiverUserDefId = entry.Details["receiverUserDefId"].ToXmlSafeString(),
                            sendingType = entry.Details["sendingType"].ToXmlSafeString(),
                            receivingType = entry.Details["receivingType"].ToXmlSafeString(),
                            operation = entry.Details["operation"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "OperatorSetTextValue")
                    {
                        #region OperatorSetTextValue
                        OperatorSetTextValue details = new OperatorSetTextValue()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            oldTextValue = entry.Details["oldTextValue"].ToXmlSafeString(),
                            newTextValue = entry.Details["newTextValue"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("userDefIdPath"))
                            details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("userDefId"))
                            details.userDefIdPath = entry.Details["userDefId"].ToXmlSafeString();

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "HeaderButton")
                    {
                        #region HeaderButton
                        HeaderButton details = new HeaderButton()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            index = int.Parse(entry.Details["index"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("clientX"))
                            details.clientX = long.Parse(entry.Details["clientX"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("clientY"))
                            details.clientY = long.Parse(entry.Details["clientY"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("pageX"))
                            details.pageX = long.Parse(entry.Details["pageX"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("pageY"))
                            details.pageY = long.Parse(entry.Details["pageY"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("screenX"))
                            details.screenX = long.Parse(entry.Details["screenX"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("screenY"))
                            details.screenY = long.Parse(entry.Details["screenY"].ToXmlSafeString());

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "NavigationButton")
                    {
                        #region NavigationButton
                        NavigationButton details = new NavigationButton()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            navigationType = entry.Details["navigationType"].ToXmlSafeString(),
                            navigationTarget = entry.Details["navigationTarget"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "JavaScriptInjected")
                    {
                        #region JavaScriptInjected
                        JavaScriptInjected details = new JavaScriptInjected()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            origin = entry.Details["origin"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("userDefIdPath"))
                            details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("indexPath"))
                            details.indexPath = entry.Details["indexPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("userDefId"))
                            details.userDefId = entry.Details["userDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("message"))
                            details.message = entry.Details["message"].ToXmlSafeString();

                        if (entry.Details.ContainsKey("type"))
                            details.type = entry.Details["type"].ToXmlSafeString();

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "RuntimeController")
                    {
                        #region RuntimeController
                        RuntimeController details = new RuntimeController()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            actionType = entry.Details["actionType"].ToXmlSafeString(),
                            details = entry.Details["details"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "CutCopyPaste")
                    {
                        #region CutCopyPaste 
                        CutCopyPaste details = new CutCopyPaste()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("userDefIdPath"))
                            details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("indexPath"))
                            details.indexPath = entry.Details["indexPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("userDefId"))
                            details.userDefId = entry.Details["userDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("triggerType"))
                            details.triggerType = entry.Details["triggerType"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("triggerIndexPath"))
                            details.triggerIndexPath = entry.Details["triggerIndexPath"].ToXmlSafeString();

                        if (entry.Details.ContainsKey("triggerUserDefIdPath"))
                            details.triggerUserDefIdPath = entry.Details["triggerUserDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("triggerUserDefId"))
                            details.triggerUserDefId = entry.Details["triggerUserDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("operation"))
                            details.operation = entry.Details["operation"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("contentIndexPath"))
                            details.contentIndexPath = entry.Details["contentIndexPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("contentUserDefIdPath"))
                            details.contentUserDefIdPath = entry.Details["contentUserDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("contentUserDefId"))
                            details.contentUserDefId = entry.Details["contentUserDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("content"))
                            details.content = entry.Details["content"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("isPerformed"))
                            details.isPerformed = entry.Details["isPerformed"].ToXmlSafeString();
                         
                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion                         
                    }
                    else if (entry.Type == "Bookmark")
                    {
                        #region Bookmark 
                        Bookmark details = new Bookmark()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("userDefIdPath"))
                            details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("indexPath"))
                            details.indexPath = entry.Details["indexPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("userDefId"))
                            details.userDefId = entry.Details["userDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("triggerType"))
                            details.triggerType = entry.Details["triggerType"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("triggerIndexPath"))
                            details.triggerIndexPath = entry.Details["triggerIndexPath"].ToXmlSafeString();

                        if (entry.Details.ContainsKey("triggerUserDefIdPath"))
                            details.triggerUserDefIdPath = entry.Details["triggerUserDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("triggerUserDefId"))
                            details.triggerUserDefId = entry.Details["triggerUserDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("operation"))
                            details.operation = entry.Details["operation"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("ownerIndexPath"))
                            details.ownerIndexPath = entry.Details["ownerIndexPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("ownerUserDefIdPath"))
                            details.ownerUserDefIdPath = entry.Details["ownerUserDefIdPath"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("ownerUserDefId"))
                            details.ownerUserDefId = entry.Details["ownerUserDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("pageName"))
                            details.pageName = entry.Details["pageName"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("pageUrl"))
                            details.pageUrl = entry.Details["pageUrl"].ToXmlSafeString();

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion                         
                         
                    }
                    else if (entry.Type == "Fullscreen")
                    {
                        // TODO: Implement with example data

                        Console.WriteLine("Not implemented: " + entry.Type);
                    }
                    else if (entry.Type == "ApplicationVisibility")
                    {
                        #region ApplicationVisibility
                        ApplicationVisibility details = new ApplicationVisibility()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            type = entry.Details["type"].ToXmlSafeString(),
                            alternateStateDuration = double.Parse(entry.Details["alternateStateDuration"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "OperatorTraceText")
                    {
                        #region OperatorTraceText
                        OperatorTraceText details = new OperatorTraceText()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            text = entry.Details["text"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "OperatorTraceSnapshot")
                    {
                        #region OperatorTraceText
                        OperatorTraceSnapshot details = new OperatorTraceSnapshot()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            text = entry.Details["text"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "Snapshot")
                    {
                        #region Snapshot
                        Snapshot details = new Snapshot()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        details.snapShot = entry.Details.ToXmlSafeString();

                        _ret.Add(details);
                        #endregion 
                    }
                    else if (entry.Type == "Recommend") // TODO: Implement with example data (or ignore!?)
                    {
                        Console.WriteLine("Not implemented: " + entry.Type);
                    }
                    else if (entry.Type == "Rectangle")  
                    {
                        #region Rectangle
                        Rectangle details = new Rectangle()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("clientX"))
                            details.clientX = long.Parse(entry.Details["clientX"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("clientY"))
                            details.clientY = long.Parse(entry.Details["clientY"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("pageX"))
                            details.pageX = long.Parse(entry.Details["pageX"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("pageY"))
                            details.pageY = long.Parse(entry.Details["pageY"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("screenX"))
                            details.screenX = long.Parse(entry.Details["screenX"].ToXmlSafeString());
                        if (entry.Details.ContainsKey("screenY"))
                            details.screenY = long.Parse(entry.Details["screenY"].ToXmlSafeString());

                        _ret.Add(details);
                        #endregion
                    }
                    else if (entry.Type == "ApplicationFullScreen")
                    {
                        #region ApplicationFullScreen
                        ApplicationFullScreen details = new ApplicationFullScreen()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            type = entry.Details["type"].ToXmlSafeString(),
                            alternateStateDuration = double.Parse(entry.Details["alternateStateDuration"].ToXmlSafeString()),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        _ret.Add(details);
                        #endregion                         
                    }
                    else if (entry.Type == "PauseResume")   
                    {
                        #region PauseResume
                        PauseResume details = new PauseResume()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            type = entry.Details["type"].ToXmlSafeString(), 
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        _ret.Add(details);
                        #endregion              
                    }
                    else if (entry.Type == "GridArea")
                    { 
                        #region GridArea 
                        GridArea details = new GridArea()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(), 
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        RetrievedEventDetials(entry, details, check);
                        #endregion

                    }
                    else if (entry.Type == "TreeNode")
                    {
                        #region TreeNode 
                        TreeNode details = new TreeNode()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };                         
                        if (entry.Details.ContainsKey("operation"))
                            details.operation = entry.Details["operation"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("nodeName"))
                            details.nodeName = entry.Details["nodeName"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("nodeType"))
                            details.nodeType = entry.Details["nodeType"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("nodePathId"))
                            details.nodePathId = entry.Details["nodePathId"].ToXmlSafeString(); 

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion         
                    }
                    else if (entry.Type == "TreeViewNode")
                    { 
                        #region TreeViewNode 
                        TreeViewNode details = new TreeViewNode()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };
                          
                        if (entry.Details.ContainsKey("operation"))
                            details.operation = entry.Details["operation"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("oldValue"))
                            details.oldValue = entry.Details["oldValue"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("newValue"))
                            details.newValue = entry.Details["newValue"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("columnName"))
                            details.columnName = entry.Details["columnName"].ToXmlSafeString(); 
                        if (entry.Details.ContainsKey("nodeName"))
                            details.nodeName = entry.Details["nodeName"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("nodeType"))
                            details.nodeType = entry.Details["nodeType"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("nodePathId"))
                            details.nodePathId = entry.Details["nodePathId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("triggeredEvent"))
                            details.triggeredEvent = entry.Details["triggeredEvent"].ToXmlSafeString();
                         
                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion                             
                    }
                    else if (entry.Type == "TreeViewSort")
                    { 
                        #region TreeViewSort 
                        TreeViewSort details = new TreeViewSort()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };
                          
                        if (entry.Details.ContainsKey("sortDirection"))
                            details.sortDirection = entry.Details["sortDirection"].ToXmlSafeString(); 

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion                    
                    }
                    else if (entry.Type == "TreeChildArea")
                    { 
                        #region TreeChildArea 
                        TreeChildArea details = new TreeChildArea()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };
 
                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion     
                    }
                    else if (entry.Type == "ListItem")
                    {
                        #region ListItem 
                        ListItem details = new ListItem()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("newSelected"))
                            details.newSelected = entry.Details["newSelected"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("newSelectedUserDefId"))
                            details.newSelectedUserDefId = entry.Details["newSelectedUserDefId"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("oldSelected"))
                            details.oldSelected = entry.Details["oldSelected"].ToXmlSafeString();
                        if (entry.Details.ContainsKey("oldSelectedUserDefId"))
                            details.oldSelectedUserDefId = entry.Details["oldSelectedUserDefId"].ToXmlSafeString();

                        RetrievedEventDetials(entry, details, check);

                        _ret.Add(details);
                        #endregion     
                         
                    }
                    else if (entry.Type == "ValueInputModified")
                    {
                        #region ValueInputModified 
                        ValueInputModified details = new ValueInputModified()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),
                            TraceId = _tracId,
                            Task = _task,
                            Test = _test,
                            PageAreaName = _pageAreaName,
                            Page = _page,
                            PageAreaType = _pageAreaType,

                            indexPath = entry.Details["indexPath"].ToXmlSafeString(),
                            CbaVers = _cbaVers,
                            SessionId = _sessionId,
                            LoginTimestamp = _loginTimestamp,
                            SendTimestamp = _sendTimestamp
                        };

                        if (entry.Details.ContainsKey("newValue"))
                            details.newValue = entry.Details["newValue"].ToXmlSafeString();
                        else
                            details.newValue = "";

                        RetrievedEventDetials(entry, details, check);
                         
                        _ret.Add(details);
                        #endregion    
                    }
                    else
                    {
                        Console.WriteLine("Not implemented: " + entry.Type);
                    }
                }
            }
         

            return _ret;
        }

        private static void RetrievedEventDetials(ItemBuilder_React_Runtime_trace_element entry, VisualEventBase details, bool check)
        {
            if (entry.Details.ContainsKey("userDefIdPath"))
                details.userDefIdPath = entry.Details["userDefIdPath"].ToXmlSafeString();
            if (entry.Details.ContainsKey("userDefId"))
                details.userDefId = entry.Details["userDefId"].ToXmlSafeString();
            else
                details.userDefId = "";

            if (entry.Details.ContainsKey("clientX"))
                details.clientX = double.Parse(entry.Details["clientX"].ToXmlSafeString());
            if (entry.Details.ContainsKey("clientY"))
                details.clientY = double.Parse(entry.Details["clientY"].ToXmlSafeString());
            if (entry.Details.ContainsKey("pageX"))
                details.pageX = double.Parse(entry.Details["pageX"].ToXmlSafeString());
            if (entry.Details.ContainsKey("pageY"))
                details.pageY = double.Parse(entry.Details["pageY"].ToXmlSafeString());
            if (entry.Details.ContainsKey("screenX"))
                details.screenX = double.Parse(entry.Details["screenX"].ToXmlSafeString());
            if (entry.Details.ContainsKey("screenY"))
                details.screenY = double.Parse(entry.Details["screenY"].ToXmlSafeString());
              
            if (check)
            {
                var p = details.GetAllPropertyAsString();
                foreach (JProperty property in entry.Details.Properties())
                {
                    if (!p.Contains(property.Value.ToString()))
                    {
                        Console.WriteLine("For an event of type '" + entry.Type + "' the property  '" + property.Name + "' was not expected. \n Details: " + entry.ToString());
                    }
                }
            }
                         
        }


        public static List<string> GetAllPropertyAsString(this object obj)
        {
            return obj.GetType()
                .GetProperties() 
                .Select(pi => pi.GetValue(obj).ToString())
                .ToList();
        }

        public static List<TProperty> GetAllPropertyValuesOfType<TProperty>(this object obj)
        {
            return obj.GetType()
                .GetProperties()
                .Where(prop => prop.PropertyType == typeof(TProperty))
                .Select(pi => (TProperty)pi.GetValue(obj))
                .ToList();
        }
        
        public static string XmlSerializeToString(this object objectInstance)
        {
            //var serializer = new System.Xml.Serialization.XmlSerializer(objectInstance.GetType());
            XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(objectInstance.GetType());
            //XmlSerializer serializer = XmlSerializer.FromTypes(new[] { objectInstance.GetType() })[0];
            var sb = new StringBuilder();

            using (TextWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, objectInstance);
            }

            return sb.ToString();
        }

        public static itemScore ParseItemScore(string itemscorejson, string task, string item, string personIdentifier)
        {
            itemScore _ret = new itemScore();

            Dictionary<string, string> _rawDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(itemscorejson);

            Dictionary<string, List<hitEntry>> _classResults = new Dictionary<string, List<hitEntry>>();

            if  (_rawDict == null)
            {
                return _ret;
            }
            // hitsAccumulated
            if (_rawDict.ContainsKey("hitsAccumulated"))
                _ret.hitsAccumulated = int.Parse(_rawDict["hitsAccumulated"]);
            else
                _ret.hitsAccumulated = -1;

            // hitsCount
            if (_rawDict.ContainsKey("hitsCount"))
                _ret.hitsCount = int.Parse(_rawDict["hitsCount"]);
            else
                _ret.hitsCount = -1;

            // missesAccumulated
            if (_rawDict.ContainsKey("missesAccumulated"))
                _ret.missesAccumulated = int.Parse(_rawDict["missesAccumulated"]);
            else
                _ret.missesAccumulated = -1;

            // missesCount
            if (_rawDict.ContainsKey("missesCount"))
                _ret.missesCount = int.Parse(_rawDict["missesCount"]);
            else
                _ret.missesCount = -1;

            // classMaxWeighed
            if (_rawDict.ContainsKey("classMaxWeighed"))
                _ret.classMaxWeighed = double.Parse(_rawDict["classMaxWeighed"]);
            else
                _ret.classMaxWeighed = -1;

            // classMaxName
            if (_rawDict.ContainsKey("classMaxName"))
                _ret.classMaxName = _rawDict["classMaxName"];
            else
                _ret.classMaxName = "";

            // totalResult
            if (_rawDict.ContainsKey("totalResult"))
                _ret.totalResult = double.Parse(_rawDict["totalResult"]);
            else
                _ret.totalResult = -1;

            // nbUserInteractions
            if (_rawDict.ContainsKey("nbUserInteractions"))
                _ret.nbUserInteractions = int.Parse(_rawDict["nbUserInteractions"]);
            else
                _ret.nbUserInteractions = -1;

            // nbUserInteractionsTotal
            if (_rawDict.ContainsKey("nbUserInteractionsTotal"))
                _ret.nbUserInteractionsTotal = int.Parse(_rawDict["nbUserInteractionsTotal"]);
            else
                _ret.nbUserInteractionsTotal = -1;

            // firstReactionTimeTotal
            if (_rawDict.ContainsKey("firstReactionTimeTotal"))
                _ret.firstReactionTimeTotal = long.Parse(_rawDict["firstReactionTimeTotal"]);
            else
                _ret.firstReactionTimeTotal = -1;

            // taskExecutionTime
            if (_rawDict.ContainsKey("taskExecutionTime"))
                _ret.taskExecutionTime = long.Parse(_rawDict["taskExecutionTime"]);
            else
                _ret.taskExecutionTime = -1;

            // taskExecutionTimeTotal
            if (_rawDict.ContainsKey("taskExecutionTimeTotal"))
                _ret.taskExecutionTimeTotal = long.Parse(_rawDict["taskExecutionTimeTotal"]);
            else
                _ret.taskExecutionTimeTotal = -1;

            foreach (string _k in _rawDict.Keys)
            {
                if (_k.StartsWith("hit."))
                {
                    string _hitName = _k.Replace("hit.", "");
                    bool _value = bool.Parse(_rawDict[_k]);
                    if (!_ret.Hits.ContainsKey(_hitName))
                    {
                        _ret.Hits.Add(_hitName, new hitEntry() { Name = _hitName, IsHit = true, IsTrue = _value });
                    }
                    else
                    {
                        _ret.Hits[_hitName].IsHit = true;
                        _ret.Hits[_hitName].IsTrue = _value;
                    }
                }
                else if (_k.StartsWith("miss."))
                {
                    // TODO CHECK
                    string _hitName = _k.Replace("miss.", "");
                    bool _value = bool.Parse(_rawDict[_k]);
                    if (!_ret.Hits.ContainsKey(_hitName))
                    {
                        _ret.Hits.Add(_hitName, new hitEntry() { Name = _hitName, IsHit = false, IsTrue = _value });
                    }
                    else
                    {
                        _ret.Hits[_hitName].IsHit = false;
                        _ret.Hits[_hitName].IsTrue = _value;
                    }
                }
                else if (_k.StartsWith("hitText."))
                {
                    string _hitName = _k.Replace("hitText.", "");
                    if (!_ret.Hits.ContainsKey(_hitName))
                        _ret.Hits.Add(_hitName, new hitEntry() { Name = _hitName, ResultText = _rawDict[_k] });
                    else
                        _ret.Hits[_hitName].ResultText = _rawDict[_k];
                }
                else if (_k.StartsWith("missText."))
                {
                    // TODO CHECK
                    string _hitName = _k.Replace("missText.", "");
                    if (!_ret.Hits.ContainsKey(_hitName))
                        _ret.Hits.Add(_hitName, new hitEntry() { Name = _hitName, ResultText = _rawDict[_k] });
                    else
                        _ret.Hits[_hitName].ResultText = _rawDict[_k];
                }
                else if (_k.StartsWith("hitWeighed."))
                {
                    string _hitName = _k.Replace("hitWeighed.", "");
                    double _weight = double.Parse(_rawDict[_k]);
                    if (!_ret.Hits.ContainsKey(_hitName))
                        _ret.Hits.Add(_hitName, new hitEntry() { Name = _hitName, Weight = _weight });
                    else
                        _ret.Hits[_hitName].Weight = _weight;
                }
                else if (_k.StartsWith("missWeighed."))
                {
                    // TODO CHECK
                    string _hitName = _k.Replace("missWeighed.", "");
                    double _weight = double.Parse(_rawDict[_k]);
                    if (!_ret.Hits.ContainsKey(_hitName))
                        _ret.Hits.Add(_hitName, new hitEntry() { Name = _hitName, Weight = _weight });
                    else
                        _ret.Hits[_hitName].Weight = _weight;

                }
                else if (_k.StartsWith("hitClass."))
                {
                    string _hitName = _k.Replace("hitClass.", "");
                    string _className = _rawDict[_k];
                    if (!_ret.Hits.ContainsKey(_hitName))
                        _ret.Hits.Add(_hitName, new hitEntry() { Name = _hitName, HitMissClass = _className });
                    else
                        _ret.Hits[_hitName].HitMissClass = _className;

                    if (!_classResults.ContainsKey(_className))
                        _classResults.Add(_className, new List<hitEntry>());

                    if (_classResults[_className].FindAll(x => x.Name == _hitName).Count == 0)
                        _classResults[_className].Add(_ret.Hits[_hitName]);
                }
                else if (_k.StartsWith("missClass."))
                {
                    // TODO CHECK
                    string _hitName = _k.Replace("missClass.", "");
                    string _className = _rawDict[_k];
                    if (!_ret.Hits.ContainsKey(_hitName))
                        _ret.Hits.Add(_hitName, new hitEntry() { Name = _hitName, HitMissClass = _className });
                    else
                        _ret.Hits[_hitName].HitMissClass = _className;

                    if (!_classResults.ContainsKey(_className))
                        _classResults.Add(_className, new List<hitEntry>());

                    if (_classResults[_className].FindAll(x => x.Name == _hitName).Count == 0)
                        _classResults[_className].Add(_ret.Hits[_hitName]);
                }
            }

            foreach (string _className in _classResults.Keys)
            {
                // select hits within each class which are true

                var _activeHits = _classResults[_className].Where(hit => hit.IsTrue).OrderBy(hit => hit.Weight).ToList();
                if (_activeHits.Count == 0)
                {
                    _ret.ClassResults.Add(_className, null);
                }
                else if (_activeHits.Count == 1)
                {
                    _ret.ClassResults.Add(_className, _activeHits[0]);
                }
                else
                {
                    if (_activeHits[0].Weight > _activeHits[1].Weight)
                    {
                        _ret.ClassResults.Add(_className, _activeHits[0]);
                    }
                    else
                    {
                        _ret.ClassResults.Add(_className, null);
                    }
                }

            }

            return _ret;
        }

    }
 
    public class ItemBuilder_React_Runtime_trace_package
    {
        public string eventType { get; set; }

        public ItemBuilder_React_Runtime_trace traceLogData { get; set; }
    }

    public class ItemBuilder_React_Runtime_trace
    {
        public metaData metaData { get; set; }
        public ItemBuilder_React_Runtime_trace_element[] logEntriesList { get; set; }
    }

    public class ItemBuilder_React_Runtime_trace_collectionpciV02
    {
        public string _id { get; set; }
        public int __v { get; set; }
        
        public string uuid { get; set; }
        public string itemUrl { get; set; }
        public ItemBuilder_React_Runtime_trace_element[] logEntriesList { get; set; }
    }

    public class ItemBuilder_React_Runtime_trace_collection
    {
        public ItemBuilder_React_Runtime_trace[] logs { get; set; }
    }

    public class metaData
    {
        public string cbaVers { get; set; }

        public string sendTimestamp { get; set; }

        public string sessionId { get; set; }

        public string loginTimestamp { get; set; }

        public string userId { get; set; }
         
    }

    public partial class ItemBuilder_React_Runtime_trace_element
    {
        public string EntryId { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; }
        public JObject Details { get; set; }
    }

    public class ItemBuilder_React_Runtime_itemscore_package
    {
        public string eventType { get; set; }

        public JObject result { get; set; }
    }

    public class itemScore
    {
        public int hitsAccumulated { get; set; }
        public int hitsCount { get; set; }
        public int missesAccumulated { get; set; }
        public int missesCount { get; set; }
        public double classMaxWeighed { get; set; }
        public string classMaxName { get; set; }
        public double totalResult { get; set; }
        public int nbUserInteractions { get; set; }
        public int nbUserInteractionsTotal { get; set; }
        public long firstReactionTimeTotal { get; set; }
        public long taskExecutionTime { get; set; }
        public long taskExecutionTimeTotal { get; set; }
        public Dictionary<string, hitEntry> Hits { get; set; }

        public Dictionary<string, hitEntry> ClassResults { get; set; }

        public itemScore()
        {
            // key: HitName
            Hits = new Dictionary<string, hitEntry>();

            // key: ClassName
            ClassResults = new Dictionary<string, hitEntry>();
        }
    }

    public class hitEntry
    {
        public string Name { get; set; }
        public bool IsHit { get; set; }
        public string ResultText { get; set; }
        public bool IsTrue { get; set; }
        public string HitMissClass { get; set; }
        public double Weight { get; set; }
    }


    #endregion

    #region Customized Classes IB 8.12 / IB 8.13

    public class Log_IB_8_12__8_13
    {
        [XmlIgnore] public string PersonIdentifier { get; set; }
        [XmlIgnore] public DateTime TimeStamp { get; set; }
        [XmlIgnore] public long RelativeTime { get; set; }
        [XmlIgnore] public int EventID { get; set; }
        [XmlIgnore] public string Element { get; set; }
        [XmlIgnore] public string EventName { get; set; }

        [XmlAttribute] public long TraceId { get; set; }
        [XmlAttribute] public string Task { get; set; } = "";
        [XmlAttribute] public string Test { get; set; } = "";
        [XmlAttribute] public string PageAreaName { get; set; } = "";
        [XmlAttribute] public string Page { get; set; } = "";
        [XmlAttribute] public string PageAreaType { get; set; } = "";

        [XmlAttribute] public string CbaVers { get; set; } = "";

        [XmlAttribute] public string SessionId { get; set; } = "";

        [XmlAttribute] public DateTime LoginTimestamp { get; set; }

        [XmlAttribute] public DateTime SendTimestamp { get; set; }

        public virtual new  string GetType() => nameof(Log_IB_8_12__8_13);
        public virtual Dictionary<string, string> GetPropertyList()
        {
            var result = new Dictionary<string, string>();
            result.Add(nameof(PersonIdentifier), PersonIdentifier);
            result.Add(nameof(TimeStamp), TimeStamp.ToString());
            result.Add(nameof(RelativeTime), RelativeTime.ToString());
            result.Add(nameof(EventID), EventID.ToString());
            result.Add(nameof(Element), Element);
            result.Add(nameof(EventName), EventName);
            return result;
        }
    }

    public class PlatformTraceLog : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string Trigger { get; set; } = "";

        [XmlAttribute] public string Sender { get; set; } = "";

        [XmlAttribute] public string Log { get; set; } = "";
        [XmlAttribute] public string SessonId { get; set; } = "";
        [XmlAttribute] public string Booklet { get; set; } = "";

        [XmlAttribute] public string Preview { get; set; } = "";

        public virtual new  Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(Trigger), Trigger);
            result.Add(nameof(Sender), Sender);
            result.Add(nameof(Log), Log);
            result.Add(nameof(SessonId), SessonId);
            result.Add(nameof(Booklet), Booklet);
            result.Add(nameof(Preview), Preview);
            return result;
        }
    }

    public class VisualEventBase : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string indexPath { get; set; } = "";
        [XmlAttribute] public string userDefIdPath { get; set; } = "";
        [XmlAttribute] public string userDefId { get; set; } = "";
        [XmlAttribute] public double clientX { get; set; }
        [XmlAttribute] public double clientY { get; set; }
        [XmlAttribute] public double pageX { get; set; }
        [XmlAttribute] public double pageY { get; set; }
        [XmlAttribute] public double screenX { get; set; }
        [XmlAttribute] public double screenY { get; set; }

        public override string GetType() => nameof(VisualEventBase);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(indexPath), indexPath);
            result.Add(nameof(userDefIdPath), userDefIdPath);
            result.Add(nameof(userDefId), userDefId);
            result.Add(nameof(clientX), clientX.ToString());
            result.Add(nameof(clientY), clientY.ToString());
            result.Add(nameof(pageX), pageX.ToString());
            result.Add(nameof(pageY), pageY.ToString());
            result.Add(nameof(screenX), screenX.ToString());
            result.Add(nameof(screenY), screenY.ToString());
            return result;
        }
    }

    public class Button : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }
        [XmlAttribute] public string subtype { get; set; } = "";

        public override string GetType() => nameof(Button);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldSelected), oldSelected.ToString());
            result.Add(nameof(subtype), subtype);
            return result;
        }
    }
    public class Checkbox : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }

        public override string GetType() => nameof(Checkbox);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldSelected), oldSelected.ToString());
            return result;
        }
    }

    public class RadioButton : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }

        public override string GetType() => nameof(RadioButton);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldSelected), oldSelected.ToString());
            return result;
        }
    }
    public class Link : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }

        public override string GetType() => nameof(Link);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldSelected), oldSelected.ToString());
            return result;
        }
    }

    public class SimpleTextField : VisualEventBase
    {
        public override string GetType() => nameof(SimpleTextField);
    }

    public class ImageField : VisualEventBase
    {
        public override string GetType() => nameof(ImageField);
    }

    public class Panel : VisualEventBase
    {
        public override string GetType() => nameof(Panel);
    }

    public class ExternalPageFrame : VisualEventBase
    {
        public override string GetType() => nameof(ExternalPageFrame);
    }

    public class Container : VisualEventBase
    {
        public override string GetType() => nameof(Container);
    }

    public class Combobox : VisualEventBase
    {
        [XmlAttribute] public int oldSelected { get; set; }
        [XmlAttribute] public string oldSelectedUserDefId { get; set; } = "";
        [XmlAttribute] public int newSelected { get; set; }
        [XmlAttribute] public string newSelectedUserDefId { get; set; } = "";

        public override string GetType() => nameof(Combobox);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldSelected), oldSelected.ToString());
            result.Add(nameof(oldSelectedUserDefId), oldSelectedUserDefId);
            result.Add(nameof(newSelected), newSelected.ToString());
            result.Add(nameof(newSelectedUserDefId), newSelectedUserDefId);
            return result;
        }
    }
    public class TableCell : VisualEventBase
    {
        [XmlAttribute] public string tableUserDefIdPath { get; set; } = "";
        [XmlAttribute] public string tableUserDefId { get; set; } = "";
        [XmlAttribute] public int row { get; set; }
        [XmlAttribute] public int column { get; set; }
        [XmlAttribute] public bool oldSelected { get; set; }

        public override string GetType() => nameof(TableCell);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(tableUserDefIdPath), tableUserDefIdPath);
            result.Add(nameof(tableUserDefId), tableUserDefId);
            result.Add(nameof(row), row.ToString());
            result.Add(nameof(column), column.ToString());
            result.Add(nameof(oldSelected), oldSelected.ToString());
            return result;
        }
    }

    public class TableCellModified : VisualEventBase
    {
        [XmlAttribute] public string tableUserDefIdPath { get; set; } = "";
        [XmlAttribute] public string tableUserDefId { get; set; } = "";
        [XmlAttribute] public int row { get; set; }
        [XmlAttribute] public int column { get; set; }
        [XmlAttribute] public string cellType { get; set; } = "";
        [XmlAttribute] public string oldValue { get; set; } = "";
        [XmlAttribute] public string newValue { get; set; } = "";
        [XmlAttribute] public double oldEvaluatedValue { get; set; }
        [XmlAttribute] public double newEvaluatedValue { get; set; }
        [XmlAttribute] public string errorInFormula { get; set; } = "";

        public override string GetType() => nameof(TableCellModified);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(tableUserDefIdPath), tableUserDefIdPath);
            result.Add(nameof(tableUserDefId), tableUserDefId);
            result.Add(nameof(row), row.ToString());
            result.Add(nameof(column), column.ToString());
            result.Add(nameof(cellType), cellType);
            result.Add(nameof(oldValue), oldValue);
            result.Add(nameof(oldEvaluatedValue), oldEvaluatedValue.ToString());
            result.Add(nameof(newEvaluatedValue), newEvaluatedValue.ToString());
            result.Add(nameof(errorInFormula), errorInFormula);
            return result;
        }
    }

    public class BrowserTab : VisualEventBase
    {
        [XmlAttribute] public string tab { get; set; } = "";
        [XmlAttribute] public string page { get; set; } = "";

        public override string GetType() => nameof(BrowserTab);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(tab), tab);
            result.Add(nameof(page), page);
            return result;
        }
    }

    public class RegionMap : VisualEventBase
    {
        public override string GetType() => nameof(RegionMap);
    }

    public class ImageArea : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }

        public override string GetType() => nameof(ImageArea);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldSelected), oldSelected.ToString());
            return result;
        }
    }

    public class SingleLineInputField : VisualEventBase
    {
        [XmlAttribute] public string currentTextValue { get; set; } = "";

        public override string GetType() => nameof(SingleLineInputField);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(currentTextValue), currentTextValue.ToString());
            return result;
        }
    }

    public class SingleLineInputFieldModified : VisualEventBase
    {
        [XmlAttribute] public string oldTextValue { get; set; } = "";
        [XmlAttribute] public string newTextValue { get; set; } = "";
        [XmlAttribute] public string origin { get; set; } = "";
        [XmlAttribute] public string validationPattern { get; set; } = "";
        [XmlAttribute] public string invalidTextValue { get; set; } = "";

        public override string GetType() => nameof(SingleLineInputFieldModified);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldTextValue), oldTextValue);
            result.Add(nameof(newTextValue), newTextValue);
            result.Add(nameof(origin), origin);
            result.Add(nameof(validationPattern), validationPattern);
            result.Add(nameof(invalidTextValue), invalidTextValue);
            return result;
        }
    }
    public class InputField : VisualEventBase
    {
        [XmlAttribute] public string currentTextValue { get; set; } = "";

        public override string GetType() => nameof(InputField);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(currentTextValue), currentTextValue.ToString());
            return result;
        }
    }
 
    public class InputFieldModified : VisualEventBase
    {
        [XmlAttribute] public string oldTextValue { get; set; } = "";
        [XmlAttribute] public string newTextValue { get; set; } = "";
        [XmlAttribute] public string origin { get; set; } = "";
        [XmlAttribute] public string validationPattern { get; set; } = "";
        [XmlAttribute] public string invalidTextValue { get; set; } = "";

        public override string GetType() => nameof(InputFieldModified);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldTextValue), oldTextValue);
            result.Add(nameof(newTextValue), newTextValue);
            result.Add(nameof(origin), origin);
            result.Add(nameof(validationPattern), validationPattern);
            result.Add(nameof(invalidTextValue), invalidTextValue);
            return result;
        }
    }


    public class ScaleValueInput : VisualEventBase
    {
        [XmlAttribute] public string oldTextValue { get; set; } = "";
        [XmlAttribute] public string newTextValue { get; set; } = "";
        [XmlAttribute] public string origin { get; set; } = "";
        [XmlAttribute] public string validationPattern { get; set; } = "";
        [XmlAttribute] public string invalidTextValue { get; set; } = "";

        public override string GetType() => nameof(InputFieldModified);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldTextValue), oldTextValue);
            result.Add(nameof(newTextValue), newTextValue);
            result.Add(nameof(origin), origin);
            result.Add(nameof(validationPattern), validationPattern);
            result.Add(nameof(invalidTextValue), invalidTextValue);
            return result;
        }
    }
     

    public class ValueInput : VisualEventBase
    {
        public override string GetType() => nameof(ValueInput);
    }

    public class SpinnerValueInput : VisualEventBase
    { 
        public override string GetType() => nameof(SpinnerValueInput);        
    }
     
    public class TreeNode : VisualEventBase
    {
        [XmlAttribute] public string operation { get; set; } = "";
        [XmlAttribute] public string nodeName { get; set; } = "";
        [XmlAttribute] public string nodeType { get; set; } = "";
        [XmlAttribute] public string nodePathId { get; set; } = "";

        public override string GetType() => nameof(TreeNode);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(operation), operation);
            result.Add(nameof(nodeName), nodeName);
            result.Add(nameof(nodeType), nodeType);
            result.Add(nameof(nodePathId), nodePathId);
  
            return result;
        }
    }
     
    public class TreeViewNode : VisualEventBase
    { 
        [XmlAttribute] public string operation { get; set; } = "";
        [XmlAttribute] public string oldValue { get; set; } = "";
        [XmlAttribute] public string newValue { get; set; } = "";
        [XmlAttribute] public string columnName { get; set; } = "";
        [XmlAttribute] public string nodeType { get; set; } = "";
        [XmlAttribute] public string nodePathId { get; set; } = "";
        [XmlAttribute] public string nodeName { get; set; } = ""; 
        [XmlAttribute] public string triggeredEvent { get; set; } = "";

        public override string GetType() => nameof(TreeViewNode);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(operation), operation);
            result.Add(nameof(oldValue), oldValue);
            result.Add(nameof(newValue), newValue);
            result.Add(nameof(columnName), columnName);
            result.Add(nameof(nodeType), nodeType);
            result.Add(nameof(nodePathId), nodePathId);
            result.Add(nameof(nodeName), nodeName);
            result.Add(nameof(triggeredEvent), triggeredEvent);

            return result;
        }
    }
        
   public class TreeViewSort : VisualEventBase
    {
        [XmlAttribute] public string sortDirection { get; set; } = "";

        public override string GetType() => nameof(TreeViewSort);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(sortDirection), sortDirection); 
            return result;
        }
    }
     
    public class ListItem : VisualEventBase
    {
        [XmlAttribute] public string newSelected { get; set; } = "";
        [XmlAttribute] public string newSelectedUserDefId { get; set; } = "";
        [XmlAttribute] public string oldSelected { get; set; } = "";
        [XmlAttribute] public string oldSelectedUserDefId { get; set; } = "";
        public override string GetType() => nameof(ListItem);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(newSelected), newSelected);
            result.Add(nameof(newSelectedUserDefId), newSelectedUserDefId);
            result.Add(nameof(oldSelected), oldSelected);
            result.Add(nameof(oldSelectedUserDefId), oldSelectedUserDefId);
            return result;
        }
    }
     
    public class ValueInputModified : VisualEventBase
    {
        [XmlAttribute] public string newValue { get; set; } = "";
        public override string GetType() => nameof(ValueInputModified);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(newValue), newValue); 
            return result;
        }
    }


    public class TreeChildArea : VisualEventBase
    {
        [XmlAttribute] public string sortDirection { get; set; } = "";

        public override string GetType() => nameof(TreeChildArea);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(sortDirection), sortDirection);
            return result;
        }
    }
     
    public class CutCopyPaste : VisualEventBase
    {
        [XmlAttribute] public string triggerType { get; set; } = "";
        [XmlAttribute] public string triggerIndexPath { get; set; } = "";
        [XmlAttribute] public string triggerUserDefIdPath { get; set; } = "";
        [XmlAttribute] public string triggerUserDefId { get; set; } = "";
        [XmlAttribute] public string operation { get; set; } = "";
        [XmlAttribute] public string contentIndexPath { get; set; } = "";
        [XmlAttribute] public string contentUserDefIdPath { get; set; } = "";
        [XmlAttribute] public string contentUserDefId { get; set; } = "";
        [XmlAttribute] public string content { get; set; } = "";
        [XmlAttribute] public string isPerformed { get; set; } = "";

        public override string GetType() => nameof(CutCopyPaste);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(triggerType), triggerType);
            result.Add(nameof(triggerIndexPath), triggerIndexPath);
            result.Add(nameof(triggerUserDefIdPath), triggerUserDefIdPath);
            result.Add(nameof(triggerUserDefId), triggerUserDefId);
            result.Add(nameof(operation), operation);
            result.Add(nameof(contentIndexPath), contentIndexPath);
            result.Add(nameof(contentUserDefIdPath), contentUserDefIdPath);
            result.Add(nameof(contentUserDefId), contentUserDefId);
            result.Add(nameof(content), content);
            result.Add(nameof(isPerformed), isPerformed);
            return result;
        }
    }
     
    public class Bookmark : VisualEventBase
    {
        [XmlAttribute] public string triggerType { get; set; } = "";
        [XmlAttribute] public string triggerIndexPath { get; set; } = "";
        [XmlAttribute] public string triggerUserDefIdPath { get; set; } = "";
        [XmlAttribute] public string triggerUserDefId { get; set; } = "";
        [XmlAttribute] public string operation { get; set; } = "";
        [XmlAttribute] public string ownerIndexPath { get; set; } = "";
        [XmlAttribute] public string ownerUserDefIdPath { get; set; } = "";
        [XmlAttribute] public string ownerUserDefId { get; set; } = "";
        [XmlAttribute] public string pageName { get; set; } = "";
        [XmlAttribute] public string pageUrl { get; set; } = "";

        public override string GetType() => nameof(Bookmark);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(triggerType), triggerType);
            result.Add(nameof(triggerIndexPath), triggerIndexPath);
            result.Add(nameof(triggerUserDefIdPath), triggerUserDefIdPath);
            result.Add(nameof(triggerUserDefId), triggerUserDefId);
            result.Add(nameof(operation), operation);
            result.Add(nameof(ownerIndexPath), ownerIndexPath);
            result.Add(nameof(ownerUserDefIdPath), ownerUserDefIdPath);
            result.Add(nameof(ownerUserDefId), ownerUserDefId);
            result.Add(nameof(pageName), pageName);
            result.Add(nameof(pageUrl), pageUrl);
            return result;
        }
    }
     

    public class ValueDisplay : VisualEventBase
    {
        [XmlAttribute] public string displayType { get; set; } = "";

        public override string GetType() => nameof(ValueDisplay);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(displayType), displayType);
            return result;
        }
    }

    public class PageArea : VisualEventBase
    {
        public override string GetType() => nameof(PageArea);
    }

    public class ImageTextField : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }

        public override string GetType() => nameof(ImageTextField);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldSelected), oldSelected.ToString());
            return result;
        }
    }
    public class RichText : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }

        public override string GetType() => nameof(RichText);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldSelected), oldSelected.ToString());
            return result;
        }
    }

    public class GridArea : VisualEventBase
    { 
        public override string GetType() => nameof(GridArea);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList(); 
            return result;
        }
    }

    

    public class HeaderButton : VisualEventBase
    {
        [XmlAttribute] public int index { get; set; }

        public override string GetType() => nameof(HeaderButton);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(index), index.ToString());
            return result;
        }
    }

    public class Rectangle: VisualEventBase
    { 
        public override string GetType() => nameof(Rectangle);       
    }

    public class NavigationButton : VisualEventBase
    {
        [XmlAttribute] public string navigationType { get; set; } = "";
        [XmlAttribute] public string navigationTarget { get; set; } = "";

        public override string GetType() => nameof(NavigationButton);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(navigationType), navigationType);
            result.Add(nameof(navigationTarget), navigationTarget);
            return result;
        }
    }
     
 
    public class RuntimeController : Log_IB_8_12__8_13
    {
        
        [XmlAttribute] public string actionType { get; set; } = "";
        [XmlAttribute] public string details { get; set; } = "";

        public override string GetType() => nameof(RuntimeController);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(actionType), actionType);
            result.Add(nameof(details), details);
            return result;
        }
    }

    
    public class RichTextHighlight : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string indexPath { get; set; } = "";
        [XmlAttribute] public string userDefId { get; set; } = "";

        // TODO: Create data structure for selections

        [XmlArray("oldSelections")]
        public List<RichTextHighlightFragment> oldSelections { get; set; }

        [XmlArray("newSelections")]
        public List<RichTextHighlightFragment> newSelections { get; set; }
      
        public override string GetType() => nameof(RichTextHighlight);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(indexPath), indexPath);
            result.Add(nameof(userDefId), userDefId);
            //result.Add(nameof(oldSelections), oldSelections);
           // result.Add(nameof(newSelections), newSelections); 
            return result;
        }
    }

    public class RichTextHighlightFragment
    {
        [XmlAttribute] public int startKey{ get; set; }
        [XmlAttribute] public int endKey { get; set; }
        [XmlAttribute] public int startOffset { get; set; }
        [XmlAttribute] public int EndOffset { get; set; }
    }

    public class EmbeddedLink : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public long clientX { get; set; }
        [XmlAttribute] public long clientY { get; set; }
        [XmlAttribute] public long pageX { get; set; }
        [XmlAttribute] public long pageY { get; set; }
        [XmlAttribute] public long screenX { get; set; }
        [XmlAttribute] public long screenY { get; set; }

        public override string GetType() => nameof(EmbeddedLink);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(indexPath), indexPath);
            result.Add(nameof(clientX), clientX.ToString());
            result.Add(nameof(clientY), clientY.ToString());
            result.Add(nameof(pageX), pageX.ToString());
            result.Add(nameof(pageY), pageY.ToString());
            result.Add(nameof(screenX), screenX.ToString());
            result.Add(nameof(screenY), screenY.ToString());
            return result;
        }
    }
    public class AudioPlayerControl : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string operation { get; set; }
        [XmlAttribute] public int maxPlay { get; set; }
        [XmlAttribute] public int currentPlayNo { get; set; }
        [XmlAttribute] public bool automaticStart { get; set; }
        [XmlAttribute] public bool hideControls { get; set; }
        [XmlAttribute] public double volumeLevel { get; set; }
        [XmlAttribute] public bool isStatemachineTriggered { get; set; }

        public override string GetType() => nameof(EmbeddedLink);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(indexPath), indexPath);
            result.Add(nameof(userDefIdPath), userDefIdPath);
            result.Add(nameof(userDefId), userDefId);
            result.Add(nameof(operation), operation);
            result.Add(nameof(maxPlay), maxPlay.ToString());
            result.Add(nameof(currentPlayNo), currentPlayNo.ToString());
            result.Add(nameof(automaticStart), automaticStart.ToString());
            result.Add(nameof(hideControls), hideControls.ToString());
            result.Add(nameof(volumeLevel), volumeLevel.ToString());
            result.Add(nameof(isStatemachineTriggered), isStatemachineTriggered.ToString());
            return result;
        }
    }
    public class VideoPlayerControl : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string operation { get; set; }
        [XmlAttribute] public int maxPlay { get; set; }
        [XmlAttribute] public int currentPlayNo { get; set; }
        [XmlAttribute] public bool automaticStart { get; set; }
        [XmlAttribute] public bool hideControls { get; set; }
        [XmlAttribute] public double volumeLevel { get; set; }
        [XmlAttribute] public bool isStatemachineTriggered { get; set; }

        public override string GetType() => nameof(VideoPlayerControl);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(indexPath), indexPath);
            result.Add(nameof(userDefIdPath), userDefIdPath);
            result.Add(nameof(userDefId), userDefId);
            result.Add(nameof(operation), operation);
            result.Add(nameof(maxPlay), maxPlay.ToString());
            result.Add(nameof(currentPlayNo), currentPlayNo.ToString());
            result.Add(nameof(automaticStart), automaticStart.ToString());
            result.Add(nameof(hideControls), hideControls.ToString());
            result.Add(nameof(volumeLevel), volumeLevel.ToString());
            result.Add(nameof(isStatemachineTriggered), isStatemachineTriggered.ToString());
            return result;
        }
    }

    public class Scrollbar : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string orientation { get; set; }
        [XmlAttribute] public double position { get; set; }

        public override string GetType() => nameof(Scrollbar);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(indexPath), indexPath);
            result.Add(nameof(userDefIdPath), userDefIdPath);
            result.Add(nameof(userDefId), userDefId);
            result.Add(nameof(orientation), orientation);
            result.Add(nameof(position), position.ToString());
            return result;
        }
    }

    public class ScrollbarMove : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string horizontalScroll { get; set; }
        [XmlAttribute] public string verticalScroll { get; set; }
        [XmlAttribute] public string orientation { get; set; }
        [XmlAttribute] public string direction { get; set; }

        public override string GetType() => nameof(ScrollbarMove);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(indexPath), indexPath);
            result.Add(nameof(userDefIdPath), userDefIdPath);
            result.Add(nameof(userDefId), userDefId);
            result.Add(nameof(horizontalScroll), horizontalScroll);
            result.Add(nameof(verticalScroll), verticalScroll);
            result.Add(nameof(orientation), orientation);
            result.Add(nameof(direction), direction);
            return result;
        }
    }
    public class OperatorSetTextValue : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string oldTextValue { get; set; }
        [XmlAttribute] public string newTextValue { get; set; }

        public override string GetType() => nameof(OperatorSetTextValue);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(indexPath), indexPath);
            result.Add(nameof(userDefIdPath), userDefIdPath);
            result.Add(nameof(userDefId), userDefId);
            result.Add(nameof(oldTextValue), oldTextValue);
            result.Add(nameof(newTextValue), newTextValue);
            return result;
        }
    }

    public class DragAndDropReceive : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string senderIndexPath { get; set; }
        [XmlAttribute] public string senderUserDefIdPath { get; set; }
        [XmlAttribute] public string senderUserDefId { get; set; }
        [XmlAttribute] public string receiverIndexPath { get; set; }
        [XmlAttribute] public string receiverUserDefIdPath { get; set; }
        [XmlAttribute] public string receiverUserDefId { get; set; }
        [XmlAttribute] public string sendingType { get; set; }
        [XmlAttribute] public string receivingType { get; set; }
        [XmlAttribute] public string operation { get; set; }

        public override string GetType() => nameof(DragAndDropReceive);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(senderIndexPath), senderIndexPath);
            result.Add(nameof(senderUserDefIdPath), senderUserDefIdPath);
            result.Add(nameof(senderUserDefId), senderUserDefId);
            result.Add(nameof(receiverIndexPath), receiverIndexPath);
            result.Add(nameof(receiverUserDefIdPath), receiverUserDefIdPath);
            result.Add(nameof(receiverUserDefId), receiverUserDefId);
            result.Add(nameof(sendingType), sendingType);
            result.Add(nameof(receivingType), receivingType);
            result.Add(nameof(operation), operation);
            return result;
        }
    }
 
    public class PageSwitchTopLevel : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string newPageAreaType { get; set; }
        [XmlAttribute] public string newPageAreaName { get; set; }
        [XmlAttribute] public string newPageName { get; set; }

        public override string GetType() => nameof(PageSwitchTopLevel);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(newPageAreaType), newPageAreaType);
            result.Add(nameof(newPageAreaName), newPageAreaName);
            result.Add(nameof(newPageName), newPageName);
            return result;
        }
    }

    public class PageSwitchEmbedded : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string newPageName { get; set; }
        [XmlAttribute] public string tab { get; set; }
        [XmlAttribute] public string historyMove { get; set; }

        public override string GetType() => nameof(PageSwitchEmbedded);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(indexPath), indexPath);
            result.Add(nameof(newPageName), newPageName);
            result.Add(nameof(tab), tab);
            result.Add(nameof(historyMove), historyMove);
            return result;
        }
    }

    public class ItemSwitch : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string name { get; set; }

        public override string GetType() => nameof(ItemSwitch);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(name), name);
            return result;
        }
    }
     

    public class ApplicationVisibility : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string type { get; set; }
        [XmlAttribute] public double alternateStateDuration { get; set; } 

        public override string GetType() => nameof(ApplicationVisibility);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(type), type);
            result.Add(nameof(alternateStateDuration), alternateStateDuration.ToString()); 
            return result;
        }
    }
     
    public class OperatorTraceText : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string text { get; set; }

        public override string GetType() => nameof(OperatorTraceText);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(text), text); 
            return result;
        }
    }

    public class OperatorTraceSnapshot : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string text { get; set; }

        public override string GetType() => nameof(OperatorTraceText);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(text), text);
            return result;
        }
    }


    public class PauseResume : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string type { get; set; } 

        public override string GetType() => nameof(PauseResume);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(type), type); 
            return result;
        }
    }

    public class ApplicationFullScreen : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string type { get; set; }
        [XmlAttribute] public double alternateStateDuration { get; set; }

        public override string GetType() => nameof(ApplicationFullScreen);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(type), type);
            result.Add(nameof(alternateStateDuration), alternateStateDuration.ToString());
            return result;
        }
    }

    public class JavaScriptInjected : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string origin { get; set; }
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string message { get; set; }
        [XmlAttribute] public string type { get; set; }

        public List<KeyValueDim> Data { get; set; }

        public override string GetType() => nameof(JavaScriptInjected);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(origin), origin);
            result.Add(nameof(indexPath), indexPath);
            result.Add(nameof(userDefIdPath), userDefIdPath);
            result.Add(nameof(userDefId), userDefId);
            result.Add(nameof(message), message);
            result.Add(nameof(type), type);
            foreach (var entry in Data)
            {
                foreach (var keyVal in entry.GetPropertyList())
                {
                    result.Add(nameof(keyVal.Key), keyVal.Value);
                }
            }
            return result;
        }
    }

    public class KeyValueDim : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string key { get; set; }

        [XmlAttribute] public string value { get; set; }

        [XmlAttribute] public int dim { get; set; }

        public override string GetType() => nameof(KeyValueDim);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(key), key);
            result.Add(nameof(value), value);
            result.Add(nameof(dim), dim.ToString());
            return result;
        }
    }
     
    public class Snapshot : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string origin { get; set; }
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string snapShot { get; set; }

        public override string GetType() => nameof(JavaScriptInjected);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(origin), origin);
            result.Add(nameof(indexPath), indexPath);
            result.Add(nameof(userDefIdPath), userDefIdPath);
            result.Add(nameof(userDefId), userDefId);
            result.Add(nameof(snapShot), snapShot);
            return result;
        }
    }

    public class UserLogin : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string user { get; set; }
        [XmlAttribute] public string loginTimestamp { get; set; }
        [XmlAttribute] public string runtimeVersion { get; set; }
        [XmlAttribute] public string webClientUserAgent { get; set; }

        public override string GetType() => nameof(UserLogin);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(user), user);
            result.Add(nameof(loginTimestamp), loginTimestamp);
            result.Add(nameof(runtimeVersion), runtimeVersion);
            result.Add(nameof(webClientUserAgent), webClientUserAgent);
            return result;
        }
    }

    public class TasksViewVisible : Log_IB_8_12__8_13
    {
        [XmlAttribute] public bool AllowScoreDebugging { get; set; }
        [XmlAttribute] public bool AllowFSMDebugging { get; set; }
        [XmlAttribute] public bool AllowTraceDebugging { get; set; }
        [XmlAttribute] public bool ShowTaskNavigationBars { get; set; }

        public override string GetType() => nameof(TasksViewVisible);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(AllowScoreDebugging), AllowScoreDebugging.ToString());
            result.Add(nameof(AllowFSMDebugging), AllowFSMDebugging.ToString());
            result.Add(nameof(AllowTraceDebugging), AllowTraceDebugging.ToString());
            result.Add(nameof(ShowTaskNavigationBars), ShowTaskNavigationBars.ToString());
            return result;
        }
    }

    public class TaskSwitch : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string oldTask { get; set; } = "";
        [XmlAttribute] public string oldItem { get; set; } = "";
        [XmlAttribute] public string oldTest { get; set; } = "";
        [XmlAttribute] public string newTask { get; set; } = "";
        [XmlAttribute] public string newItem { get; set; } = "";
        [XmlAttribute] public string newTest { get; set; } = "";
        [XmlAttribute] public long hitsAccumulated { get; set; }
        [XmlAttribute] public long hitsCount { get; set; }
        [XmlAttribute] public long missesAccumulated { get; set; }
        [XmlAttribute] public long missesCount { get; set; }
        [XmlAttribute] public double classMaxWeighed { get; set; }
        [XmlAttribute] public string classMaxName { get; set; } = "";
        [XmlAttribute] public double totalResult { get; set; }
        [XmlAttribute] public long nbUserInteractions { get; set; }
        [XmlAttribute] public long nbUserInteractionsTotal { get; set; }
        [XmlAttribute] public double firstReactionTime { get; set; }
        [XmlAttribute] public double firstReactionTimeTotal { get; set; }
        [XmlAttribute] public double taskExecutionTime { get; set; }
        [XmlAttribute] public double taskExecutionTimeTotal { get; set; }

        public override string GetType() => nameof(TaskSwitch);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(oldTask), oldTask);
            result.Add(nameof(oldItem), oldItem);
            result.Add(nameof(oldTest), oldTest);
            result.Add(nameof(newTask), newTask);
            result.Add(nameof(newItem), newItem);
            result.Add(nameof(newTest), newTest);
            result.Add(nameof(hitsAccumulated), hitsAccumulated.ToString());
            result.Add(nameof(hitsCount), hitsCount.ToString());
            result.Add(nameof(missesAccumulated), missesAccumulated.ToString());
            result.Add(nameof(missesCount), missesCount.ToString());
            result.Add(nameof(classMaxWeighed), classMaxWeighed.ToString());
            result.Add(nameof(classMaxName), classMaxName.ToString());
            result.Add(nameof(totalResult), totalResult.ToString());
            result.Add(nameof(nbUserInteractions), nbUserInteractions.ToString());
            result.Add(nameof(nbUserInteractionsTotal), nbUserInteractionsTotal.ToString());
            result.Add(nameof(firstReactionTime), firstReactionTime.ToString());
            result.Add(nameof(firstReactionTimeTotal), firstReactionTimeTotal.ToString());
            result.Add(nameof(taskExecutionTime), taskExecutionTime.ToString());
            result.Add(nameof(taskExecutionTimeTotal), taskExecutionTimeTotal.ToString());
            return result;
        }

        // [XmlElement(Order = 1)]
        public HitList[] HitList { get; set; }

    }

    public class HitList
    {
        [XmlAttribute] public string Name { get; set; }
        [XmlAttribute] public double Weight { get; set; }
        [XmlAttribute] public string Text { get; set; }
        [XmlAttribute] public string Typ { get; set; }
        [XmlAttribute] public string Value { get; set; }
    }

    #endregion


    public static class XmlExtensions
    {
        /// <summary>
        /// Converts an object to a string and replaces characters that are not allowed in XML.
        /// </summary>
        /// <param name="obj">The object to convert and sanitize.</param>
        /// <returns>A sanitized string suitable for XML content.</returns>
        public static string ToXmlSafeString(this object obj)
        {
            if (obj == null)
                return string.Empty;

            // Convert the object to a string
            string str = obj.ToString();

            // StringBuilder to hold the sanitized string
            StringBuilder sb = new StringBuilder();

            // Iterate through each character in the string
            foreach (char c in str)
            {
                if (IsValidXmlChar(c))
                {
                    sb.Append(c);
                }
                else
                {
                    // Optionally replace invalid character with a valid one or remove it
                    // Here, we simply remove it
                    // To replace, you could use something like: sb.Append(' ');
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines if a character is valid in XML.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>true if the character is valid; otherwise, false.</returns>
        private static bool IsValidXmlChar(char ch)
        {
            // Reference: https://www.w3.org/TR/xml/#charsets
            // This checks if the character is valid according to XML 1.0 specification.
            return
                (ch == 0x9 /* == '\t' == 9   */) || (ch == 0xA /* == '\n' == 10  */) || (ch == 0xD /* == '\r' == 13  */) ||
                (ch >= 0x20 && ch <= 0xD7FF) ||
                (ch >= 0xE000 && ch <= 0xFFFD) ||
                (ch >= 0x10000 && ch <= 0x10FFFF);
        }
    }
}