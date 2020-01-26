using Newtonsoft.Json;
using Newtonsoft.Json.Linq; 
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using LogFSM_LogX2019;

namespace LogDataTransformer_IBSD_Module_V0
{ 
    #region Reader

    public static class JSON_IB_8_12_beta3_helper
    {
        public static void AddProcessedLogDataToContainer(logXContainer _ret, string line)
        {
            var logFragment = JsonConvert.DeserializeObject<JSON_IB_8_12_beta3>(line);
            string _personIdentifier = logFragment.metaData.UserId;

            foreach (var entry in logFragment.logEntriesList)
            {
                string _item = "(testlevel)";
                string _test = "";
                string _task = "";
                string _pageAreaType = "";
                string _pageAreaName = "";
                string _page = "";

                if (entry.Details.ContainsKey("indexPath"))
                {
                    string ret = entry.Details["indexPath"].ToString();
                    string[] parts = ret.Split("/", StringSplitOptions.RemoveEmptyEntries);
                    _test = parts[0].Replace("test=", "");
                    if (parts.Length > 1)
                        _item = parts[1].Replace("item=", "");
                    if (parts.Length > 2)
                        _task = parts[2].Replace("task=", "");
                    if (parts.Length > 3)
                        _pageAreaType = parts[3].Replace("task=", "");
                    if (parts.Length > 4)
                        _pageAreaName = parts[4].Replace("task=", "");
                    if (parts.Length > 5)
                        _page = parts[5].Replace("task=", "");
                }

                // TODO: Add page, task, pageAreaType and pageAreaName to EventDetails!

                // TODO: Add Item to TasksViewVisible / ItemSwitch / TaskSwitch

                var g = new logxGenericLogElement()
                {
                    Item = _item,
                    EventID = int.Parse(entry.EntryId),
                    EventName = entry.Type,
                    PersonIdentifier = _personIdentifier,
                    TimeStamp = DateTime.Parse(entry.Timestamp)
                };

                if (entry.Type == "TasksViewVisible")
                {
                    #region TasksViewVisible
                    TasksViewVisible details = new TasksViewVisible()
                    {
                        AllowScoreDebugging = bool.Parse(entry.Details["settings"]["AllowScoreDebugging"].ToString()),
                        AllowFSMDebugging = bool.Parse(entry.Details["settings"]["AllowFSMDebugging"].ToString()),
                        AllowTraceDebugging = bool.Parse(entry.Details["settings"]["AllowTraceDebugging"].ToString()),
                        ShowTaskNavigationBars = bool.Parse(entry.Details["settings"]["ShowTaskNavigationBars"].ToString()),
                    };
                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion

                    // TODO: add 'headerButtons', 'upperHeaderMenu', 'lowerHeaderMenu'
                }
                else if (entry.Type == "UserLogin")
                {
                    #region UserLogin
                    UserLogin details = new UserLogin()
                    {
                        user = entry.Details["user"].ToString(),
                        loginTimestamp = entry.Details["loginTimestamp"].ToString(),
                        runtimeVersion = entry.Details["runtimeVersion"].ToString(),
                        webClientUserAgent = entry.Details["webClientUserAgent"].ToString(),
                    };
                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "ItemSwitch")
                {
                    #region ItemSwitch
                    ItemSwitch details = new ItemSwitch()
                    {
                        name = entry.Details["item"]["name"].ToString(),
                    };
                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "TaskSwitch")
                {
                    #region TaskSwitch
                    TaskSwitch details = new TaskSwitch()
                    {
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
                    g.EventDataXML = Helper.XmlSerializeToString(details);

                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "PageSwitchTopLevel")
                {
                    #region PageSwitchTopLevel
                    PageSwitchTopLevel details = new PageSwitchTopLevel()
                    {
                        pageAreaType = entry.Details["pageAreaType"].ToString(),
                        pageAreaName = entry.Details["pageAreaName"].ToString(),
                        newPageName = entry.Details["newPageName"].ToString(),
                    };

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "PageSwitchEmbedded")
                {
                    #region PageSwitchEmbedded
                    PageSwitchEmbedded details = new PageSwitchEmbedded()
                    {
                        indexPath = entry.Details["indexPath"].ToString(),
                        newPageName = entry.Details["newPageName"].ToString(),
                    };

                    if (entry.Details.ContainsKey("tab"))
                        details.tab = entry.Details["tab"].ToString();

                    if (entry.Details.ContainsKey("historyMove"))
                        details.historyMove = entry.Details["historyMove"].ToString();

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "Button")
                {
                    #region Button
                    Button details = new Button()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "Checkbox")
                {
                    #region Checkbox
                    Checkbox details = new Checkbox()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "RadioButton")
                {
                    #region RadioButton
                    RadioButton details = new RadioButton()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "Link")
                {
                    #region Link
                    Link details = new Link()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "SimpleTextField")
                {
                    #region SimpleTextField
                    SimpleTextField details = new SimpleTextField()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "ImageField")
                {
                    #region ImageField
                    ImageField details = new ImageField()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "Panel")
                {
                    #region Panel
                    Panel details = new Panel()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "Container")
                {
                    #region Container
                    Container details = new Container()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "Combobox")
                {
                    #region Combobox
                    Combobox details = new Combobox()
                    {
                        indexPath = entry.Details["indexPath"].ToString(),
                        oldSelected = bool.Parse(entry.Details["oldSelected"].ToString()),
                        newSelected = bool.Parse(entry.Details["newSelected"].ToString()),
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "TableCell")
                {
                    #region TableCell
                    TableCell details = new TableCell()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "TableCellModified")
                {
                    #region TableCellModified
                    TableCellModified details = new TableCellModified()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "BrowserTab")
                {
                    #region BrowserTab
                    BrowserTab details = new BrowserTab()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "RegionMap")
                {
                    #region RegionMap
                    RegionMap details = new RegionMap()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "ImageArea")
                {
                    #region ImageArea
                    ImageArea details = new ImageArea()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "SingleLineInputField")
                {
                    #region SingleLineInputField
                    SingleLineInputField details = new SingleLineInputField()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "SingleLineInputFieldModified")
                {
                    #region SingleLineInputFieldModified
                    SingleLineInputFieldModified details = new SingleLineInputFieldModified()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "InputField")
                {
                    #region InputField
                    InputField details = new InputField()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "InputFieldModified")
                {
                    #region InputFieldModified
                    InputFieldModified details = new InputFieldModified()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "PageArea")
                {
                    #region PageArea
                    PageArea details = new PageArea()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "ImageTextField")
                {
                    #region ImageTextField 
                    ImageTextField details = new ImageTextField()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "EmbeddedLink")
                {
                    #region EmbeddedLink
                    EmbeddedLink details = new EmbeddedLink()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "RichTextHighlight")
                {
                    // TODO: Implement with example data
                }
                else if (entry.Type == "AudioPlayer")
                {
                    #region AudioPlayer
                    try
                    {
                        AudioPlayer details = new AudioPlayer()
                        {
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

                        g.EventDataXML = Helper.XmlSerializeToString(details);
                        _ret.AddEvent(g);
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

                        g.EventDataXML = Helper.XmlSerializeToString(details);
                        _ret.AddEvent(g);
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion

                }
                else if (entry.Type == "Scrollbar")
                {
                    #region Scrollbar
                    Scrollbar details = new Scrollbar()
                    {
                        indexPath = entry.Details["indexPath"].ToString(),
                        orientation = entry.Details["orientation"].ToString(),
                        position = double.Parse(entry.Details["position"].ToString()),
                    };

                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefId = entry.Details["userDefId"].ToString();
                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "DragAndDropReceive")
                {
                    // TODO: Implement with example data
                }
                else if (entry.Type == "OperatorSetTextValue")
                {
                    #region OperatorSetTextValue
                    OperatorSetTextValue details = new OperatorSetTextValue()
                    {
                        indexPath = entry.Details["indexPath"].ToString(),
                        oldTextValue = entry.Details["oldTextValue"].ToString(),
                        newTextValue = entry.Details["newTextValue"].ToString(),
                    };

                    if (entry.Details.ContainsKey("userDefIdPath"))
                        details.userDefIdPath = entry.Details["userDefIdPath"].ToString();
                    if (entry.Details.ContainsKey("userDefId"))
                        details.userDefIdPath = entry.Details["userDefId"].ToString();

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "HeaderButton")
                {
                    #region HeaderButton
                    HeaderButton details = new HeaderButton()
                    {
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


                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "NavigationButton")
                {
                    #region NavigationButton
                    NavigationButton details = new NavigationButton()
                    {
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

                    g.EventDataXML = Helper.XmlSerializeToString(details);
                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "JavaScriptInjected")
                {
                    #region JavaScriptInjected
                    JavaScriptInjected details = new JavaScriptInjected()
                    {
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




                    string detailsXMLString = Helper.XmlSerializeToString(details);
                    g.EventDataXML = detailsXMLString;

                    /*
                    string messageXMLString = "";

                    try
                    {
                        XmlDocument messageXML = (XmlDocument)JsonConvert.DeserializeXmlNode(details.message, "Details");
                        messageXMLString = messageXML.OuterXml;


                        if (messageXMLString != "")
                        {
                            XmlDocument detailsXML = new XmlDocument();
                            detailsXML.LoadXml(detailsXMLString);
                            XmlNode detailsXMLRoot = detailsXML.DocumentElement;
                             
                            XmlNode tempNode = detailsXML.ImportNode(messageXML.FirstChild, true);

                            detailsXMLRoot.InsertAfter(tempNode, detailsXMLRoot.FirstChild);
                            g.EventDataXML = detailsXML.OuterXml;


                        }
                        else
                        {
                            g.EventDataXML = detailsXMLString;
                        }

                    }
                    catch (Exception _ex)
                    {
                        Console.WriteLine(messageXMLString);
                        Console.WriteLine(details.message);
                        g.EventDataXML = detailsXMLString;
                    }
                    */


                    _ret.AddEvent(g);
                    #endregion
                }
                else if (entry.Type == "RuntimeController")
                {
                    // TODO: Implement with example data
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
                    // TODO: Implement with example data
                }
                else if (entry.Type == "OperatorTraceText")
                {
                    // TODO: Implement with example data
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
        }

    }
    public class JSON_IB_8_12_beta3
    {
        public metaData metaData { get; set; }
        public logEntries[] logEntriesList { get; set; }
    }

    public class metaData
    {
        public string CbaVers { get; set; }

        public string SendTimestamp { get; set; }

        public string SessionId { get; set; }

        public string LoginTimestamp { get; set; }

        public string UserId { get; set; }


    }

    public partial class logEntries
    {
        public string EntryId { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; }
        public JObject Details { get; set; }
    }

    #endregion

    #region Customized Classes

    public class VisualEventBase
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public long clientX { get; set; }
        [XmlAttribute] public long clientY { get; set; }
        [XmlAttribute] public long pageX { get; set; }
        [XmlAttribute] public long pageY { get; set; }
        [XmlAttribute] public long screenX { get; set; }
        [XmlAttribute] public long screenY { get; set; }
    }

    public class Button : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }
        [XmlAttribute] public string subtype { get; set; }
    }
    public class Checkbox : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }
    }

    public class RadioButton : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }
    }
    public class Link : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }
    }

    public class SimpleTextField : VisualEventBase
    {
    }

    public class ImageField : VisualEventBase
    {
    }

    public class Panel : VisualEventBase
    {
    }

    public class ExternalPageFrame : VisualEventBase
    {
    }

    public class Container : VisualEventBase
    {
    }

    public class Combobox : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }
        [XmlAttribute] public string oldSelectedUserDefId { get; set; }
        [XmlAttribute] public bool newSelected { get; set; }
        [XmlAttribute] public string newSelectedUserDefId { get; set; }
    }
    public class TableCell : VisualEventBase
    {
        [XmlAttribute] public string tableUserDefIdPath { get; set; }
        [XmlAttribute] public string tableUserDefId { get; set; }
        [XmlAttribute] public int row { get; set; }
        [XmlAttribute] public int column { get; set; }
        [XmlAttribute] public bool oldSelected { get; set; }
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

    }

    public class BrowserTab : VisualEventBase
    {
        [XmlAttribute] public string tab { get; set; }
        [XmlAttribute] public string page { get; set; }
    }

    public class RegionMap : VisualEventBase
    {
    }

    public class ImageArea : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }
    }

    public class SingleLineInputField : VisualEventBase
    {
        [XmlAttribute] public string currentTextValue { get; set; }
    }

    public class SingleLineInputFieldModified : VisualEventBase
    {
        [XmlAttribute] public string oldTextValue { get; set; }
        [XmlAttribute] public string newTextValue { get; set; }
        [XmlAttribute] public string origin { get; set; }
        [XmlAttribute] public string validationPattern { get; set; }
        [XmlAttribute] public string invalidTextValue { get; set; }
    }
    public class InputField : VisualEventBase
    {
        [XmlAttribute] public string currentTextValue { get; set; }
    }

    public class InputFieldModified : VisualEventBase
    {
        [XmlAttribute] public string oldTextValue { get; set; }
        [XmlAttribute] public string newTextValue { get; set; }
        [XmlAttribute] public string origin { get; set; }
        [XmlAttribute] public string validationPattern { get; set; }
        [XmlAttribute] public string invalidTextValue { get; set; }
    }
    public class ValueDisplay : VisualEventBase
    {
        [XmlAttribute] public string displayType { get; set; }
    }

    public class PageArea : VisualEventBase
    {
    }

    public class ImageTextField : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }
    }
    public class RichText : VisualEventBase
    {
        [XmlAttribute] public bool oldSelected { get; set; }
    }
    public class HeaderButton : VisualEventBase
    {
        [XmlAttribute] public int index { get; set; }
    }

    public class NavigationButton : VisualEventBase
    {
        [XmlAttribute] public string navigationType { get; set; }
        [XmlAttribute] public string navigationTarget { get; set; }
    }

    public class EmbeddedLink
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public long clientX { get; set; }
        [XmlAttribute] public long clientY { get; set; }
        [XmlAttribute] public long pageX { get; set; }
        [XmlAttribute] public long pageY { get; set; }
        [XmlAttribute] public long screenX { get; set; }
        [XmlAttribute] public long screenY { get; set; }
    }
    public class AudioPlayer
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
    }
    public class VideoPlayer
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
    }

    public class Scrollbar
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string orientation { get; set; }
        [XmlAttribute] public double position { get; set; }
    }

    public class ScrollbarMove
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string horizontalScroll { get; set; }
        [XmlAttribute] public string verticalScroll { get; set; }
        [XmlAttribute] public string orientation { get; set; }
        [XmlAttribute] public string direction { get; set; }
    }
    public class OperatorSetTextValue
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string oldTextValue { get; set; }
        [XmlAttribute] public string newTextValue { get; set; }
    }

    public class PageSwitchTopLevel
    {
        [XmlAttribute] public string pageAreaType { get; set; }
        [XmlAttribute] public string pageAreaName { get; set; }
        [XmlAttribute] public string newPageName { get; set; }
    }

    public class PageSwitchEmbedded
    {
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string newPageName { get; set; }
        [XmlAttribute] public string tab { get; set; }
        [XmlAttribute] public string historyMove { get; set; }
    }

    public class ItemSwitch
    {
        [XmlAttribute] public string name { get; set; }
    }

    public class JavaScriptInjected
    {
        [XmlAttribute] public string origin { get; set; }
        [XmlAttribute] public string indexPath { get; set; }
        [XmlAttribute] public string userDefIdPath { get; set; }
        [XmlAttribute] public string userDefId { get; set; }
        [XmlAttribute] public string message { get; set; }
        public List<KeyValueDim> Data { get; set; }

    }

    public class KeyValueDim
    {
        [XmlAttribute] public string key { get; set; }

        [XmlAttribute] public string value { get; set; }

        [XmlAttribute] public int dim { get; set; }
    }

    public class UserLogin
    {
        [XmlAttribute] public string user { get; set; }
        [XmlAttribute] public string loginTimestamp { get; set; }
        [XmlAttribute] public string runtimeVersion { get; set; }
        [XmlAttribute] public string webClientUserAgent { get; set; }
    }

    public class TasksViewVisible
    {
        [XmlAttribute] public bool AllowScoreDebugging { get; set; }
        [XmlAttribute] public bool AllowFSMDebugging { get; set; }
        [XmlAttribute] public bool AllowTraceDebugging { get; set; }
        [XmlAttribute] public bool ShowTaskNavigationBars { get; set; }
    }

    public class TaskSwitch
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

        [XmlElement(Order = 1)]
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

    public static class Helper
    {
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
    }


    #endregion 
}
