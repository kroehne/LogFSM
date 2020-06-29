namespace LogDataTransformer_IB_REACT_8_12__8_13
{
 
    #region usings 
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    #endregion

    #region Reader

    public static class JSON_IB_8_12__8_13_helper
    {

        public static List<Log_IB_8_12__8_13> ParseTraceLogs(string line)
        {
            List<Log_IB_8_12__8_13> _ret = new List<Log_IB_8_12__8_13>();

            var _trace = JsonConvert.DeserializeObject<LogDataTransformer_IRTlibPlayer_V01.TraceLog>(line);
            if (_trace.Log == null)
                return _ret;

            string _element ="";
            string _test = "";
            string _task = "";
            string _bookklet = "";
            string _preview = "";

            if (_trace.Context!= null)
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
                  
            } 

            _ret.Add(new PlatformTraceLog() { Trigger = _trace.Trigger, Log = _trace.Log, Sender = _trace.Sender, TimeStamp = _trace.Timestamp, SessonId = _trace.SessionId , Element = _element, EventName = nameof(PlatformTraceLog) , Booklet = _bookklet, Preview  = _preview});

            return _ret;
        }

        public static List<Log_IB_8_12__8_13> ParseLogElements(string line, string source)
        {
            List<Log_IB_8_12__8_13> _ret = new List<Log_IB_8_12__8_13>();

            string _IBTraceJSON = "";
            string _element = "";
            string _test = "";
            string _task = "";
            if (source == "IRTlibPlayer_V01")
            {
                var _trace = JsonConvert.DeserializeObject<LogDataTransformer_IRTlibPlayer_V01.TraceEvent>(line);
                if (_trace.Trace == null)
                    return _ret;

                 _element = _trace.Context.Item;
                 _test = _trace.Context.Test;
                 _task = _trace.Context.Task;

                _IBTraceJSON = _trace.Trace;
            }
            else if (source == "IBSD_V01")
            {
                _IBTraceJSON = line;
            } 

            var logFragment = JsonConvert.DeserializeObject<json_IB_8_12__8_13>(_IBTraceJSON); 
            string _personIdentifier = logFragment.metaData.userId;

            foreach (var entry in logFragment.logEntriesList)
            {
                string _pageAreaType = "";
                string _pageAreaName = "";
                string _page = "";

                if (entry.Details.ContainsKey("indexPath"))
                {
                    string ret = entry.Details["indexPath"].ToString();
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

                if (_element == null)
                {

                }
                // TODO: Add page, task, pageAreaType and pageAreaName to EventDetails!

                // TODO: Add Item to TasksViewVisible / ItemSwitch / TaskSwitch

                if (entry.Type == "TasksViewVisible")
                {
                    #region TasksViewVisible
                    TasksViewVisible details = new TasksViewVisible()
                    {
                        Element = _element,
                        EventID = int.Parse(entry.EntryId),
                        EventName = entry.Type,
                        PersonIdentifier = _personIdentifier,
                        TimeStamp = DateTime.Parse(entry.Timestamp),
                         
                        AllowScoreDebugging = bool.Parse(entry.Details["settings"]["AllowScoreDebugging"].ToString()),
                        AllowFSMDebugging = bool.Parse(entry.Details["settings"]["AllowFSMDebugging"].ToString()),
                        AllowTraceDebugging = bool.Parse(entry.Details["settings"]["AllowTraceDebugging"].ToString()),
                        ShowTaskNavigationBars = bool.Parse(entry.Details["settings"]["ShowTaskNavigationBars"].ToString()),
                    };
                    _ret.Add(details);
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

                        user = entry.Details["user"].ToString(),
                        loginTimestamp = entry.Details["loginTimestamp"].ToString(),
                        runtimeVersion = entry.Details["runtimeVersion"].ToString(),
                        webClientUserAgent = entry.Details["webClientUserAgent"].ToString(),
                    };
                    _ret.Add(details);
                    #endregion
                }
                else if (entry.Type == "ItemSwitch")
                {
                    #region ItemSwitch
                    ItemSwitch details = new ItemSwitch()
                    {
                        Element = _element,
                        EventID = int.Parse(entry.EntryId),
                        EventName = entry.Type,
                        PersonIdentifier = _personIdentifier,
                        TimeStamp = DateTime.Parse(entry.Timestamp),

                        name = entry.Details["item"]["name"].ToString(),
                    };
                    _ret.Add(details);
                    #endregion
                }
                else if (entry.Type == "TaskSwitch")
                {
                    #region TaskSwitch

                    if (entry.Details.ContainsKey("newTask"))
                        _element = entry.Details["newItem"].ToString();
                     
                    TaskSwitch details = new TaskSwitch()
                    {
                        Element = _element,
                        EventID = int.Parse(entry.EntryId),
                        EventName = entry.Type,
                        PersonIdentifier = _personIdentifier,
                        TimeStamp = DateTime.Parse(entry.Timestamp),

                        newTask = entry.Details["newTask"].ToString(),
                        newItem = entry.Details["newItem"].ToString(),
                        newTest = entry.Details["newTest"].ToString(),
                    };

                    if (entry.Details.ContainsKey("oldTask"))
                        details.oldTask = entry.Details["oldTask"].ToString();
                    if (entry.Details.ContainsKey("oldItem"))
                        details.oldItem = entry.Details["oldItem"].ToString();
                    if (entry.Details.ContainsKey("oldTest"))
                        details.oldTest = entry.Details["oldTest"].ToString();


                    if (entry.Details.ContainsKey("taskResult"))
                    {
                        // TaskSwitch not contains results!

                        string foo = entry.Details["taskResult"].ToString();
                        if (foo != "{}")
                        {

                        }
                        JObject taskResult = (entry.Details["taskResult"] as JObject);

                        if (taskResult.ContainsKey("hitsAccumulated"))
                            details.hitsAccumulated = long.Parse(entry.Details["taskResult"]["hitsAccumulated"].ToString());
                        if (taskResult.ContainsKey("hitsCount"))
                            details.hitsCount = long.Parse(entry.Details["taskResult"]["hitsCount"].ToString());
                        if (taskResult.ContainsKey("missesAccumulated"))
                            details.missesAccumulated = long.Parse(entry.Details["taskResult"]["missesAccumulated"].ToString());
                        if (taskResult.ContainsKey("missesCount"))
                            details.missesCount = long.Parse(entry.Details["taskResult"]["missesCount"].ToString());
                        if (taskResult.ContainsKey("classMaxWeighed"))
                            details.classMaxWeighed = double.Parse(entry.Details["taskResult"]["classMaxWeighed"].ToString());
                        if (taskResult.ContainsKey("classMaxName"))
                            details.classMaxName = entry.Details["taskResult"]["classMaxName"].ToString();
                        if (taskResult.ContainsKey("totalResult"))
                            details.totalResult = double.Parse(entry.Details["taskResult"]["totalResult"].ToString());
                        if (taskResult.ContainsKey("nbUserInteractions"))
                            details.nbUserInteractions = long.Parse(entry.Details["taskResult"]["nbUserInteractions"].ToString());
                        if (taskResult.ContainsKey("nbUserInteractionsTotal"))
                            details.nbUserInteractionsTotal = long.Parse(entry.Details["taskResult"]["nbUserInteractionsTotal"].ToString());
                        if (taskResult.ContainsKey("firstReactionTime"))
                            details.firstReactionTime = double.Parse(entry.Details["taskResult"]["firstReactionTime"].ToString());
                        if (taskResult.ContainsKey("firstReactionTimeTotal"))
                            details.firstReactionTimeTotal = double.Parse(entry.Details["taskResult"]["firstReactionTimeTotal"].ToString());
                        if (taskResult.ContainsKey("taskExecutionTime"))
                            details.taskExecutionTime = double.Parse(entry.Details["taskResult"]["taskExecutionTime"].ToString());
                        if (taskResult.ContainsKey("taskExecutionTimeTotal"))
                            details.taskExecutionTimeTotal = double.Parse(entry.Details["taskResult"]["taskExecutionTimeTotal"].ToString());

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
                                    taskResultDict[_parts[1]].Text = pair.Value.ToString();
                                }
                                else if (_parts[0].EndsWith("Weighed"))
                                {
                                    taskResultDict[_parts[1]].Weight = double.Parse(pair.Value.ToString());
                                }
                                else
                                {
                                    taskResultDict[_parts[1]].Typ = _parts[0];
                                    taskResultDict[_parts[1]].Value = pair.Value.ToString();
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

                        pageAreaType = entry.Details["pageAreaType"].ToString(),
                        pageAreaName = entry.Details["pageAreaName"].ToString(),
                        newPageName = entry.Details["newPageName"].ToString(),
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

                        indexPath = entry.Details["indexPath"].ToString(),
                        newPageName = entry.Details["newPageName"].ToString(),
                    };

                    if (entry.Details.ContainsKey("tab"))
                        details.tab = entry.Details["tab"].ToString();

                    if (entry.Details.ContainsKey("historyMove"))
                        details.historyMove = entry.Details["historyMove"].ToString();

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        oldSelected = bool.Parse(entry.Details["oldSelected"].ToString()),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());
                    if (entry.Details.ContainsKey("subtype"))
                        details.subtype = entry.Details["subtype"].ToString();

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        oldSelected = bool.Parse(entry.Details["oldSelected"].ToString()),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        oldSelected = bool.Parse(entry.Details["oldSelected"].ToString()),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        oldSelected = bool.Parse(entry.Details["oldSelected"].ToString()),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

                    _ret.Add(details);
                    #endregion
                }
                else if (entry.Type == "ExternalPageFrame")
                {
                    if (entry.Details.Count > 9)
                    {
                        // TODO: Check for additional event-specific attributes
                    }

                    #region ExternalPageFrame
                    ExternalPageFrame details = new ExternalPageFrame()
                    {
                        Element = _element,
                        EventID = int.Parse(entry.EntryId),
                        EventName = entry.Type,
                        PersonIdentifier = _personIdentifier,
                        TimeStamp = DateTime.Parse(entry.Timestamp),

                        indexPath = entry.Details["indexPath"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        oldSelected = int.Parse(entry.Details["oldSelected"].ToString()),
                        newSelected = int.Parse(entry.Details["newSelected"].ToString()),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());
                    if (entry.Details.ContainsKey("oldSelectedUserDefId"))
                        details.oldSelectedUserDefId = entry.Details["oldSelectedUserDefId"].ToString();
                    if (entry.Details.ContainsKey("newSelectedUserDefId"))
                        details.newSelectedUserDefId = entry.Details["newSelectedUserDefId"].ToString();

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        row = int.Parse(entry.Details["row"].ToString()),
                        column = int.Parse(entry.Details["column"].ToString()),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());
                    if (entry.Details.ContainsKey("tableUserDefIdPath"))
                        details.tableUserDefIdPath = entry.Details["tableUserDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("tableUserDefId"))
                        details.tableUserDefId = entry.Details["tableUserDefId"].ToString();
                    if (entry.Details.ContainsKey("oldSelected"))
                        details.oldSelected = bool.Parse(entry.Details["oldSelected"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        row = int.Parse(entry.Details["row"].ToString()),
                        column = int.Parse(entry.Details["column"].ToString()),
                        cellType = entry.Details["cellType"].ToString(),
                        oldValue = entry.Details["oldValue"].ToString(),
                        newValue = entry.Details["newValue"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());
                    if (entry.Details.ContainsKey("tableUserDefIdPath"))
                        details.tableUserDefIdPath = entry.Details["tableUserDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("tableUserDefId"))
                        details.tableUserDefId = entry.Details["tableUserDefId"].ToString();
                    if (entry.Details.ContainsKey("oldEvaluatedValue"))
                        details.oldEvaluatedValue = double.Parse(entry.Details["oldEvaluatedValue"].ToString());
                    if (entry.Details.ContainsKey("newEvaluatedValue"))
                        details.newEvaluatedValue = double.Parse(entry.Details["newEvaluatedValue"].ToString());
                    if (entry.Details.ContainsKey("errorInFormula"))
                        details.errorInFormula = entry.Details["errorInFormula"].ToString();

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        tab = entry.Details["tab"].ToString(),
                        page = entry.Details["page"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        oldSelected = bool.Parse(entry.Details["oldSelected"].ToString()),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        currentTextValue = entry.Details["currentTextValue"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        oldTextValue = entry.Details["oldTextValue"].ToString(),
                        newTextValue = entry.Details["newTextValue"].ToString(),
                        origin = entry.Details["origin"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());
                    if (entry.Details.ContainsKey("validationPattern"))
                        details.validationPattern = entry.Details["validationPattern"].ToString();
                    if (entry.Details.ContainsKey("invalidTextValue"))
                        details.invalidTextValue = entry.Details["invalidTextValue"].ToString();

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        currentTextValue = entry.Details["currentTextValue"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        oldTextValue = entry.Details["oldTextValue"].ToString(),
                        newTextValue = entry.Details["newTextValue"].ToString(),
                        origin = entry.Details["origin"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());
                    if (entry.Details.ContainsKey("validationPattern"))
                        details.validationPattern = entry.Details["validationPattern"].ToString();
                    if (entry.Details.ContainsKey("invalidTextValue"))
                        details.invalidTextValue = entry.Details["invalidTextValue"].ToString();

                    _ret.Add(details);
                    #endregion
                }
                else if (entry.Type == "ValueInput")
                {
                    // TODO: Implement final version
                }
                else if (entry.Type == "ValueInputFieldModified")
                {
                    // TODO: Implement final version
                }
                else if (entry.Type == "ScaleValueInput")
                {
                    // TODO: Implement final version
                }
                else if (entry.Type == "SpinnerValueInput")
                {
                    // TODO: Implement final version
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

                        indexPath = entry.Details["indexPath"].ToString(),
                        displayType = entry.Details["displayType"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        oldSelected = bool.Parse(entry.Details["oldSelected"].ToString()),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

                    _ret.Add(details);
                    #endregion
                }
                else if (entry.Type == "RichText" || entry.Type == "RichTextField")
                {
                    // TODO: Check with PL and nagarro
                    /*
                    if (entry.Type == "RichTextField")
                    {
                        Console.WriteLine("Waring: Event Type 'RichTextField' expected as 'RechText': " + entry.Details.ToString());
                    }
                    */

                    #region RichText 
                    RichText details = new RichText()
                    {
                        Element = _element,
                        EventID = int.Parse(entry.EntryId),
                        EventName = entry.Type,
                        PersonIdentifier = _personIdentifier,
                        TimeStamp = DateTime.Parse(entry.Timestamp),

                        indexPath = entry.Details["indexPath"].ToString(),
                        oldSelected = bool.Parse(entry.Details["oldSelected"].ToString()),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                    };

                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        userDefId = entry.Details["userDefId"].ToString(),
                    };

                    if (entry.Details.ContainsKey("oldSelections"))
                    {
                        details.oldSelections = JsonConvert.DeserializeObject<List<RichTextHighlightFragment>>(entry.Details["oldSelections"].ToString());  
                    }
                    if (entry.Details.ContainsKey("newSelections"))
                    {
                        details.newSelections = JsonConvert.DeserializeObject<List<RichTextHighlightFragment>>(entry.Details["newSelections"].ToString());
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

                            indexPath = entry.Details["indexPath"].ToString(),
                        };

                        if (entry.Details.ContainsKey("maxPlay"))
                            details.maxPlay = int.Parse(entry.Details["maxPlay"].ToString());
                        if (entry.Details.ContainsKey("currentPlayNo"))
                            details.currentPlayNo = int.Parse(entry.Details["currentPlayNo"].ToString());
                        if (entry.Details.ContainsKey("automaticStart"))
                            details.automaticStart = bool.Parse(entry.Details["automaticStart"].ToString());
                        if (entry.Details.ContainsKey("hideControls"))
                            details.hideControls = bool.Parse(entry.Details["hideControls"].ToString());
                        if (entry.Details.ContainsKey("volumeLevel"))
                            details.volumeLevel = double.Parse(entry.Details["volumeLevel"].ToString());
                        if (entry.Details.ContainsKey("operation"))
                            details.operation = entry.Details["operation"].ToString();
                        if (entry.Details.ContainsKey("userDefId"))
                            details.userDefId = entry.Details["userDefId"].ToString();
                        if (entry.Details.ContainsKey("userDefIdPath"))
                            details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                        if (entry.Details.ContainsKey("isStatemachineTriggered"))
                            details.isStatemachineTriggered = bool.Parse(entry.Details["isStatemachineTriggered"].ToString());

                        _ret.Add(details);
                    }
                    catch
                    {
                        Console.WriteLine("AudioPlayer -- Attribute Error: " + entry.Details.ToString());
                    }

                    #endregion
                }
                else if (entry.Type == "VideoPlayer")
                {
                    #region VideoPlayer
                    try
                    {
                        VideoPlayer details = new VideoPlayer()
                        {
                            Element = _element,
                            EventID = int.Parse(entry.EntryId),
                            EventName = entry.Type,
                            PersonIdentifier = _personIdentifier,
                            TimeStamp = DateTime.Parse(entry.Timestamp),

                            indexPath = entry.Details["indexPath"].ToString(),
                        };

                        /* TODO: The following attributes should not be missing*/

                        if (entry.Details.ContainsKey("maxPlay"))
                            details.maxPlay = int.Parse(entry.Details["maxPlay"].ToString());
                        if (entry.Details.ContainsKey("currentPlayNo"))
                            details.currentPlayNo = int.Parse(entry.Details["currentPlayNo"].ToString());
                        if (entry.Details.ContainsKey("automaticStart"))
                            details.automaticStart = bool.Parse(entry.Details["automaticStart"].ToString());
                        if (entry.Details.ContainsKey("hideControls"))
                            details.hideControls = bool.Parse(entry.Details["hideControls"].ToString());
                        if (entry.Details.ContainsKey("volumeLevel"))
                            details.volumeLevel = double.Parse(entry.Details["volumeLevel"].ToString());
                        if (entry.Details.ContainsKey("operation"))
                            details.operation = entry.Details["operation"].ToString();

                        /* END-TODO*/

                        if (entry.Details.ContainsKey("userDefId"))
                            details.userDefId = entry.Details["userDefId"].ToString();
                        if (entry.Details.ContainsKey("userDefIdPath"))
                            details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                        if (entry.Details.ContainsKey("isStatemachineTriggered"))
                            details.isStatemachineTriggered = bool.Parse(entry.Details["isStatemachineTriggered"].ToString());

                        _ret.Add(details);
                    }
                    catch
                    {
                        Console.WriteLine("VideoPlayer -- Attribute Error: " + entry.Details.ToString());
                    }
                    #endregion
                }
                else if (entry.Type == "ScrollbarMove")
                {
                    /* TODO: Is this still a valid event?
                     * 
                     **/
                    #region ScrollbarMove
                    ScrollbarMove details = new ScrollbarMove()
                    {
                        Element = _element,
                        EventID = int.Parse(entry.EntryId),
                        EventName = entry.Type,
                        PersonIdentifier = _personIdentifier,
                        TimeStamp = DateTime.Parse(entry.Timestamp),

                        indexPath = entry.Details["indexPath"].ToString()
                    };

                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefId = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("orientation"))
                        details.orientation = entry.Details["orientation"].ToString();
                    if (entry.Details.ContainsKey("horizontalScroll"))
                        details.horizontalScroll = entry.Details["horizontalScroll"].ToString();
                    if (entry.Details.ContainsKey("verticalScroll"))
                        details.verticalScroll = entry.Details["verticalScroll"].ToString();
                    if (entry.Details.ContainsKey("direction"))
                        details.direction = entry.Details["direction"].ToString();

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

                        indexPath = entry.Details["indexPath"].ToString()
                    };

                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefId = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("orientation"))
                        details.orientation = entry.Details["orientation"].ToString();
                    if (entry.Details.ContainsKey("horizontalScroll"))
                        details.horizontalScroll = entry.Details["horizontalScroll"].ToString();
                    if (entry.Details.ContainsKey("verticalScroll"))
                        details.verticalScroll = entry.Details["verticalScroll"].ToString();
                    if (entry.Details.ContainsKey("direction"))
                        details.direction = entry.Details["direction"].ToString();

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

                        indexPath = entry.Details["indexPath"].ToString(),
                        orientation = entry.Details["orientation"].ToString(),
                        position = double.Parse(entry.Details["position"].ToString()),
                    };

                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefId = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();

                    _ret.Add(details);
                    #endregion
                }
                else if (entry.Type == "DragAndDropReceive")
                {
                    #region DragAndDropReceive

                    if (entry.Details.ContainsKey("senderIndexPath")|| entry.Details.ContainsKey("receiverIndexPath"))
                    {
                        string ret = ""; 
                        if (entry.Details.ContainsKey("senderIndexPath"))
                            ret =  entry.Details["senderIndexPath"].ToString();
                        else  
                            ret = entry.Details["receiverIndexPath"].ToString();

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

                        senderIndexPath = entry.Details["senderIndexPath"].ToString(),
                        senderUserDefIdPath = entry.Details["senderUserDefIdPath"].ToString(),
                        senderUserDefId = entry.Details["senderUserDefId"].ToString(),
                        receiverIndexPath = entry.Details["receiverIndexPath"].ToString(), 
                        receiverUserDefIdPath = entry.Details["receiverUserDefIdPath"].ToString(),
                        receiverUserDefId = entry.Details["receiverUserDefId"].ToString(),
                        sendingType = entry.Details["sendingType"].ToString(),
                        receivingType = entry.Details["receivingType"].ToString(),
                        operation = entry.Details["operation"].ToString(),
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

                        indexPath = entry.Details["indexPath"].ToString(),
                        oldTextValue = entry.Details["oldTextValue"].ToString(),
                        newTextValue = entry.Details["newTextValue"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();

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

                        index = int.Parse(entry.Details["index"].ToString()),
                    };

                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        navigationType = entry.Details["navigationType"].ToString(),
                        navigationTarget = entry.Details["navigationTarget"].ToString(),
                    };

                    if (entry.Details.ContainsKey("clientX"))
                        details.clientX = long.Parse(entry.Details["clientX"].ToString());
                    if (entry.Details.ContainsKey("clientY"))
                        details.clientY = long.Parse(entry.Details["clientY"].ToString());
                    if (entry.Details.ContainsKey("pageX"))
                        details.pageX = long.Parse(entry.Details["pageX"].ToString());
                    if (entry.Details.ContainsKey("pageY"))
                        details.pageY = long.Parse(entry.Details["pageY"].ToString());
                    if (entry.Details.ContainsKey("screenX"))
                        details.screenX = long.Parse(entry.Details["screenX"].ToString());
                    if (entry.Details.ContainsKey("screenY"))
                        details.screenY = long.Parse(entry.Details["screenY"].ToString());

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

                        origin = entry.Details["origin"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("indexPath"))
                        details.indexPath = entry.Details["indexPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefId = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("message"))
                        details.message = entry.Details["message"].ToString();

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

                        actionType = entry.Details["actionType"].ToString(),
                        details = entry.Details["details"].ToString(),
                    }; 
                    
                    _ret.Add(details);
                    #endregion

                }
                else if (entry.Type == "CutCopyPaste")
                {
                    // TODO: Implement with example data
                }
                else if (entry.Type == "Bookmark")
                {
                    // TODO: Implement with example data
                }
                else if (entry.Type == "Fullscreen")
                {
                    // TODO: Implement with example data
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

                        type = entry.Details["type"].ToString(),
                        alternateStateDuration = double.Parse(entry.Details["alternateStateDuration"].ToString()),
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
                         
                        text = entry.Details["text"].ToString(),
                    };

                    _ret.Add(details);
                    #endregion

                }
                else if (entry.Type == "OperatorTraceSnapshot")
                {
                    // TODO: Implement with example data
                }
                else if (entry.Type == "Snapshot")
                {
                    // TODO: Implement with example data (or ignore!?)
                }
                else if (entry.Type == "Recommend")
                {
                    // TODO: Implement with example data
                }
                else
                {

                }

               

            }


      

            return _ret;
        }


        public static string XmlSerializeToString(this object objectInstance)
        {
            var serializer = new XmlSerializer(objectInstance.GetType());
            var sb = new StringBuilder();

            using (TextWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, objectInstance);
            }

            return sb.ToString();
        }

        public static ItemScore_IB_8_12__8_13 ParseItemScore(string itemscorejson, string task, string item, string personIdentifier)
        {
            ItemScore_IB_8_12__8_13 _ret = new ItemScore_IB_8_12__8_13();

            Dictionary<string, string> _rawDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(itemscorejson);
            Dictionary<string, List<hitEntry>> _classResults = new Dictionary<string, List<hitEntry>>();

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


    public class json_IB_8_12__8_13
    {
        public metaData metaData { get; set; }
        public logEntries[] logEntriesList { get; set; }
    }

    public class metaData
    {
        public string cbaVers { get; set; }

        public string sendTimestamp { get; set; }

        public string sessionId { get; set; }

        public string loginTimestamp { get; set; }

        public string userId { get; set; }


    }

    public partial class logEntries
    {
        public string EntryId { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; }
        public JObject Details { get; set; }
    }

    public class ItemScore_IB_8_12__8_13
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

        public ItemScore_IB_8_12__8_13()
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
        [XmlAttribute] public string Trigger { get; set; }

        [XmlAttribute] public string Sender { get; set; }

        [XmlAttribute] public string Log { get; set; }
        [XmlAttribute] public string SessonId { get; set; }
        [XmlAttribute] public string Booklet { get; set; }

        [XmlAttribute] public string Preview { get; set; }

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
        [XmlAttribute] public string indexPath { get; set; }
          
        [XmlAttribute] public string Task { get; set; }
        [XmlAttribute] public string PageAreaType { get; set; }
        [XmlAttribute] public string PageAreaName { get; set; }
        [XmlAttribute] public string Page { get; set; }
         
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public long clientX { get; set; }
        [XmlAttribute] public long clientY { get; set; }
        [XmlAttribute] public long pageX { get; set; }
        [XmlAttribute] public long pageY { get; set; }
        [XmlAttribute] public long screenX { get; set; }
        [XmlAttribute] public long screenY { get; set; }

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
        [XmlAttribute] public string subtype { get; set; }

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
        [XmlAttribute] public string oldSelectedUserDefId { get; set; }
        [XmlAttribute] public int newSelected { get; set; }
        [XmlAttribute] public string newSelectedUserDefId { get; set; }

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
        [XmlAttribute] public string tableUserDefIdPath { get; set; }
        [XmlAttribute] public string tableUserDefId { get; set; }
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
        [XmlAttribute] public string tableUserDefIdPath { get; set; }
        [XmlAttribute] public string tableUserDefId { get; set; }
        [XmlAttribute] public int row { get; set; }
        [XmlAttribute] public int column { get; set; }
        [XmlAttribute] public string cellType { get; set; }
        [XmlAttribute] public string oldValue { get; set; }
        [XmlAttribute] public string newValue { get; set; }
        [XmlAttribute] public double oldEvaluatedValue { get; set; }
        [XmlAttribute] public double newEvaluatedValue { get; set; }
        [XmlAttribute] public string errorInFormula { get; set; }

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
        [XmlAttribute] public string tab { get; set; }
        [XmlAttribute] public string page { get; set; }

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
        [XmlAttribute] public string currentTextValue { get; set; }

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
        [XmlAttribute] public string oldTextValue { get; set; }
        [XmlAttribute] public string newTextValue { get; set; }
        [XmlAttribute] public string origin { get; set; }
        [XmlAttribute] public string validationPattern { get; set; }
        [XmlAttribute] public string invalidTextValue { get; set; }

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
        [XmlAttribute] public string currentTextValue { get; set; }

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
        [XmlAttribute] public string oldTextValue { get; set; }
        [XmlAttribute] public string newTextValue { get; set; }
        [XmlAttribute] public string origin { get; set; }
        [XmlAttribute] public string validationPattern { get; set; }
        [XmlAttribute] public string invalidTextValue { get; set; }

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
    public class ValueDisplay : VisualEventBase
    {
        [XmlAttribute] public string displayType { get; set; }

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

    public class NavigationButton : VisualEventBase
    {
        [XmlAttribute] public string navigationType { get; set; }
        [XmlAttribute] public string navigationTarget { get; set; }

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
        
        [XmlAttribute] public string actionType { get; set; }
        [XmlAttribute] public string details { get; set; }

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
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
         
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
    public class VideoPlayer : Log_IB_8_12__8_13
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

        public override string GetType() => nameof(VideoPlayer);
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
        [XmlAttribute] public string pageAreaType { get; set; }
        [XmlAttribute] public string pageAreaName { get; set; }
        [XmlAttribute] public string newPageName { get; set; }

        public override string GetType() => nameof(PageSwitchTopLevel);
        public override Dictionary<string, string> GetPropertyList()
        {
            var result = base.GetPropertyList();
            result.Add(nameof(pageAreaType), pageAreaType);
            result.Add(nameof(pageAreaName), pageAreaName);
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

    public class JavaScriptInjected : Log_IB_8_12__8_13
    {
        [XmlAttribute] public string origin { get; set; }
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string message { get; set; }
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
        [XmlAttribute] public string oldTask { get; set; }
        [XmlAttribute] public string oldItem { get; set; }
        [XmlAttribute] public string oldTest { get; set; }
        [XmlAttribute] public string newTask { get; set; }
        [XmlAttribute] public string newItem { get; set; }
        [XmlAttribute] public string newTest { get; set; }
        [XmlAttribute] public long hitsAccumulated { get; set; }
        [XmlAttribute] public long hitsCount { get; set; }
        [XmlAttribute] public long missesAccumulated { get; set; }
        [XmlAttribute] public long missesCount { get; set; }
        [XmlAttribute] public double classMaxWeighed { get; set; }
        [XmlAttribute] public string classMaxName { get; set; }
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
}