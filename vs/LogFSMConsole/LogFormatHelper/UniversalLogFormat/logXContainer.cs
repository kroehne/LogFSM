﻿#region usings
using CsvHelper;
using CsvHelper.Configuration;
using ICSharpCode.SharpZipLib.GZip;
using Ionic.Zip;
using LogFSM;
using LogFSMConsole;
using LogFSMShared;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using SixLabors.ImageSharp.ColorSpaces;
using SpssLib.DataReader;
using SpssLib.SpssDataset;
using StataLib;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Unicode;
using System.Xml.Linq;
using static NPOI.HSSF.Util.HSSFColor;
#endregion

namespace LogFSM_LogX2019
{
    public class logXContainer
    {

        public bool PersonIdentifierIsNumber { get; set; }
        public string PersonIdentifierName { get; set; }

        public bool RelativeTimesAreSeconds { get; set; }

        #region Result Data

        public bool flagShortenResultDataNames = false;
        public bool ContainsResultData { get; set; }
        public Dictionary<string, logxResultDataRow> resultDataTable { get; set; }
        public Dictionary<string, string> resultDataTableColnames { get; set; }

        #endregion

        #region Concordance Table 

        public Dictionary<string, string> CondordanceTable { get; set; }

        #endregion

        #region Log Data 

        public const string rootLogName = "Log";
        public bool flagIgnoreXSDandXSI = true;

        public Dictionary<string, List<logxLogDataRow>> logDataTables { get; set; }
        public Dictionary<string, List<string>> logDataTableColnames { get; set; }

        public Dictionary<string, Dictionary<string, int>> uniqueValues = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<int, string>> uniqueValuesLookup = new Dictionary<string, Dictionary<int, string>>();

        public List<string> ExportErrors = new List<string>();

        public logxCodebookDictionary CodebookDictionary { get; set; }

        private Dictionary<string, int> maxEventIDByPerson = new Dictionary<string, int>();

        #endregion

        public logXContainer()
        {
            PersonIdentifierIsNumber = true;

            resultDataTable = new Dictionary<string, logxResultDataRow>();
            resultDataTableColnames = new Dictionary<string, string>();

            logDataTables = new Dictionary<string, List<logxLogDataRow>>();
            logDataTables.Add(rootLogName, new List<logxLogDataRow>());

            logDataTableColnames = new Dictionary<string, List<string>>();
            logDataTableColnames.Add(rootLogName, new List<string>());

            PersonIdentifierName = "PersonIdentifier";

            if (!uniqueValues.ContainsKey("PersonIdentifier"))
                uniqueValues.Add("PersonIdentifier", new Dictionary<string, int>());
            if (!uniqueValues.ContainsKey("Element"))
                uniqueValues.Add("Element", new Dictionary<string, int>());
            if (!uniqueValues.ContainsKey("Path"))
                uniqueValues.Add("Path", new Dictionary<string, int>());
            if (!uniqueValues.ContainsKey("ParentPath"))
                uniqueValues.Add("ParentPath", new Dictionary<string, int>());
            if (!uniqueValues.ContainsKey("EventName"))
                uniqueValues.Add("EventName", new Dictionary<string, int>());

            CondordanceTable = new Dictionary<string, string>();
        }

        public void CreateLookup()
        {
            foreach (string table in uniqueValues.Keys)
            {
                uniqueValuesLookup.Add(table, new Dictionary<int, string>());
                foreach (string key in uniqueValues[table].Keys)
                {
                    uniqueValuesLookup[table].Add(uniqueValues[table][key], key);
                }

            }
        }

        public void AddResults(logxGenericResultElement element)
        {
            ContainsResultData = true;

            if (CondordanceTable.Count != 0)
            {
                if (!CondordanceTable.ContainsKey(element.PersonIdentifier))
                    return;
                else
                    element.PersonIdentifier = CondordanceTable[element.PersonIdentifier];
            }

            long _personIdentifier = -1;
            if (!PersonIdentifierIsNumber)
            {
                if (!uniqueValues["PersonIdentifier"].ContainsKey(element.PersonIdentifier))
                    uniqueValues["PersonIdentifier"].Add(element.PersonIdentifier, uniqueValues["PersonIdentifier"].Count);

                _personIdentifier = uniqueValues["PersonIdentifier"][element.PersonIdentifier];
            }
            else
            {
                long.TryParse(element.PersonIdentifier, out _personIdentifier);
                if (_personIdentifier == -1)
                {
                    throw new Exception("Personidentifier was expected do be a number");
                }
            }

            if (!resultDataTable.ContainsKey(element.PersonIdentifier))
                resultDataTable.Add(element.PersonIdentifier, new logxResultDataRow() { AttributValues = new List<Tuple<int, int>>(), PersonIdentifier = _personIdentifier });

            foreach (var _name in element.Results.Keys)
            {
                if (!resultDataTableColnames.ContainsKey(_name))
                {
                    List<string> _shortnames = resultDataTableColnames.Values.ToList<string>();
                    if (flagShortenResultDataNames && _name.Length > 25)
                    {
                        string _shortname = _name.Substring(0, 25);
                        int _i = 1;
                        while (_shortnames.Contains(_shortname + "_" + _i))
                            _i++;

                        resultDataTableColnames.Add(_name, _shortname + "_" + _i);
                    }
                    else
                    {
                        resultDataTableColnames.Add(_name, _name);
                    }
                }

                string _value = "";
                if (element.Results[_name] != null)
                    _value = element.Results[_name].ToString();

                AddValueToResultDataTable(_name, _value, resultDataTable[element.PersonIdentifier]);
            }
        }

        private void AddValueToResultDataTable(string name, string value, logxResultDataRow row)
        {
            string path = "Results";
            if (!uniqueValues.ContainsKey(name))
                uniqueValues.Add(name, new Dictionary<string, int>());

            if (!uniqueValues[name].ContainsKey(value))
                uniqueValues[name].Add(value, uniqueValues[name].Count);

            if (!logDataTableColnames.ContainsKey(path))
                logDataTableColnames.Add(path, new List<string>());

            if (!logDataTableColnames[path].Contains(name))
                logDataTableColnames[path].Add(name);

            row.AttributValues.Add(new Tuple<int, int>(logDataTableColnames[path].IndexOf(name), uniqueValues[name][value]));

        }

        public void AddEvent(logxGenericLogElement element, string[] Elements, string[] Events, string[] ExcludedElements, string[] ExcludedEvents)
        {
            bool _add = true;
            if (Elements.Length != 0 && !Elements.Contains<string>(element.Item))
                _add = false;
            else if (Events.Length != 0 && !Events.Contains<string>(element.EventName))
                _add = false;
            else if (ExcludedElements.Length != 0 && ExcludedElements.Contains<string>(element.Item))
                _add = false;
            else if (ExcludedEvents.Length != 0 && ExcludedEvents.Contains<string>(element.EventName))
                _add = false;

            if (_add) 
                AddEvent(element);
           
        }

        public void AddEvent(logxGenericLogElement element)
        {
            if (CondordanceTable.Count != 0)
            {
                if (!CondordanceTable.ContainsKey(element.PersonIdentifier))
                    return;
                else
                    element.PersonIdentifier = CondordanceTable[element.PersonIdentifier];
            }

            if (!uniqueValues["Element"].ContainsKey(element.Item))
                uniqueValues["Element"].Add(element.Item, uniqueValues["Element"].Count);

            if (!uniqueValues["Path"].ContainsKey(rootLogName))
                uniqueValues["Path"].Add(rootLogName, uniqueValues["Path"].Count);

            if (!uniqueValues["ParentPath"].ContainsKey("(no parent)"))
                uniqueValues["ParentPath"].Add("(no parent)", uniqueValues["ParentPath"].Count);

            if (!uniqueValues["EventName"].ContainsKey(element.EventName))
                uniqueValues["EventName"].Add(element.EventName, uniqueValues["EventName"].Count);

            long _personIdentifier = -1;
            if (!PersonIdentifierIsNumber)
            {
                if (!uniqueValues["PersonIdentifier"].ContainsKey(element.PersonIdentifier))
                    uniqueValues["PersonIdentifier"].Add(element.PersonIdentifier, uniqueValues["PersonIdentifier"].Count);

                _personIdentifier = uniqueValues["PersonIdentifier"][element.PersonIdentifier];
            }
            else
            {
                long.TryParse(element.PersonIdentifier, out _personIdentifier);
                if (_personIdentifier == -1)
                {
                    throw new Exception("Personidentifier was expected do be a number");
                }
            }
             
            if (!maxEventIDByPerson.ContainsKey(element.PersonIdentifier))
                maxEventIDByPerson.Add(element.PersonIdentifier, 0);

            if (element.EventID > maxEventIDByPerson[element.PersonIdentifier])
                maxEventIDByPerson[element.PersonIdentifier] = element.EventID;

            TimeSpan _relativeTimeSpan = RelativeTimesAreSeconds ? TimeSpan.FromSeconds(element.RelativeTime) : TimeSpan.FromMilliseconds(element.RelativeTime);

            

            logxLogDataRow rootParentLine = new logxLogDataRow()
            {
                PersonIdentifier = _personIdentifier,
                Element = uniqueValues["Element"][element.Item],
                TimeStamp = element.TimeStamp,
                RelativeTime = _relativeTimeSpan,
                ParentEventID = -1,
                Path = uniqueValues["Path"][rootLogName],
                ParentPath = uniqueValues["ParentPath"]["(no parent)"],
                EventID = element.EventID,
                EventName = uniqueValues["EventName"][element.EventName],
                ChildEvents = new List<logxLogDataRow>(),
                AttributValues = new List<Tuple<int, int>>(),
            };

            logDataTables[rootLogName].Add(rootParentLine);

            if (element.EventDataXML != "")
            {
                element.EventDataXML = element.EventDataXML.Replace("´&#x8;", "");
                using (TextReader tr = new StringReader(element.EventDataXML))
                {
                    XDocument doc = XDocument.Load(tr);
                    ProcessXMLData(doc.Root, rootLogName, element.PersonIdentifier, rootParentLine, 0);
                    doc = null;
                }
            }   
        }

        private void ProcessXMLData(XElement xmlelement, string path, string PersonIdentifier, logxLogDataRow parentLine, int id)
        {
            if (path == rootLogName)
                path = rootLogName + "." + xmlelement.Name.LocalName;

            int split = path.LastIndexOf(".");
            string parentPath = "(no parent)";
            if (split > 0)
                parentPath = path.Substring(0, split);

            if (!uniqueValues["Path"].ContainsKey(path))
                uniqueValues["Path"].Add(path, uniqueValues["Path"].Count);

            if (!uniqueValues["ParentPath"].ContainsKey(parentPath))
                uniqueValues["ParentPath"].Add(parentPath, uniqueValues["ParentPath"].Count);

            logxLogDataRow newChildLine = new logxLogDataRow()
            {
                PersonIdentifier = parentLine.PersonIdentifier,
                Element = parentLine.Element,
                TimeStamp = parentLine.TimeStamp,
                RelativeTime = parentLine.RelativeTime,
                ParentEventID = parentLine.EventID,
                EventID = id,
                Path = uniqueValues["Path"][path],
                ParentPath = uniqueValues["ParentPath"][parentPath],
                EventName = parentLine.EventName,
                ChildEvents = new List<logxLogDataRow>(),
                AttributValues = new List<Tuple<int, int>>()
            };

            parentLine.ChildEvents.Add(newChildLine);

            if (!logDataTables.ContainsKey(path))
                logDataTables.Add(path, new List<logxLogDataRow>());

            if (!logDataTableColnames.ContainsKey(path))
                logDataTableColnames.Add(path, new List<string>());

            logDataTables[path].Add(newChildLine);

            if (!xmlelement.HasElements && xmlelement.Value.Trim() != "")
                AddValueToLogDataTable(path, xmlelement.Name.LocalName, xmlelement.Value, newChildLine);
             
            foreach (var a in xmlelement.Attributes())
            {
                if (!flagIgnoreXSDandXSI || (a.Name.LocalName != "xsi" && a.Name.LocalName != "xsd"))
                    AddValueToLogDataTable(path, a.Name.LocalName, a.Value, newChildLine);
            }

            int i = 0;
            foreach (XElement x in xmlelement.Elements())
            {
                ProcessXMLData(x, path + "." + x.Name.LocalName, PersonIdentifier, newChildLine, i);
                i++;
            } 
        }

        private void AddValueToLogDataTable(string path, string name, string value, logxLogDataRow row)
        {
            string _localPath = name;
            if (path.Trim() != "")
                _localPath = path + "." + _localPath;

            if (!uniqueValues.ContainsKey(name))
                uniqueValues.Add(name, new Dictionary<string, int>());

            if (!uniqueValues[name].ContainsKey(value))
                uniqueValues[name].Add(value, uniqueValues[name].Count);

            if (!logDataTableColnames.ContainsKey(path))
                logDataTableColnames.Add(path, new List<string>());

            if (!logDataTableColnames[path].Contains(name))
                logDataTableColnames[path].Add(name);

            row.AttributValues.Add(new Tuple<int, int>(logDataTableColnames[path].IndexOf(name), uniqueValues[name][value]));
        }

        public void UpdateRelativeTimes()
        { 
            // Get first time stamp for each PersonIdentifier

            Dictionary<long, DateTime> _startByPersonIdentifier = new Dictionary<long, DateTime>();
            foreach (var v in logDataTables["Log"])
            {
                if (!_startByPersonIdentifier.ContainsKey(v.PersonIdentifier))
                    _startByPersonIdentifier.Add(v.PersonIdentifier, DateTime.MaxValue);
                if (v.TimeStamp < _startByPersonIdentifier[v.PersonIdentifier])
                    _startByPersonIdentifier[v.PersonIdentifier] = v.TimeStamp; 
            }
             
            foreach (string _id in logDataTables.Keys)
            {
                foreach (var v in logDataTables[_id])
                {
                    v.RelativeTime  = (v.TimeStamp - _startByPersonIdentifier[v.PersonIdentifier]);
                }

                logDataTables[_id].Sort((x, y) =>
                {
                    int result = decimal.Compare(x.PersonIdentifier, y.PersonIdentifier);
                    if (result == 0)
                        result = decimal.Compare((decimal)x.RelativeTime.TotalMilliseconds, (decimal)y.RelativeTime.TotalMilliseconds);
                    return result;
                });
            } 
        }

        public void ExportStata(CommandLineArguments ParsedCommandLineArguments)
        {
            string filename = ParsedCommandLineArguments.Transform_OutputStata;

            string _attributeNotDefinded = "(attribute not defined)";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputstringattributenotdefined"))
                _attributeNotDefinded = ParsedCommandLineArguments.ParameterDictionary["outputstringattributenotdefined"];

            string _attributeNotExpected = "(attribute not expected)";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputstringattributenotexpected"))
                _attributeNotExpected = ParsedCommandLineArguments.ParameterDictionary["outputstringattributenotdefined"];

            bool _skipRootTable = false;
            if (ParsedCommandLineArguments.Flags.Contains("SKIPROOT"))
                _skipRootTable = true;

            bool _includeFlatAndSparseLogDataTable = false;
            if (ParsedCommandLineArguments.Flags.Contains("INCLUDEFLAT"))
                _includeFlatAndSparseLogDataTable = true;

            string _language = "ENG";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("language"))
                _language = ParsedCommandLineArguments.ParameterDictionary["language"];

            DateTime dt1960 = new DateTime(1960, 1, 1, 0, 0, 0, 0);

            // Add attribute variables as string, if number of characters exceeds 32000 characters
            Dictionary<string, string> listOfLabelContainers = new Dictionary<string, string>();
            foreach (var k in uniqueValues.Keys)
            {
                if (!listOfLabelContainers.ContainsKey(k))
                    listOfLabelContainers.Add(k, "l_" + listOfLabelContainers.Count);

                foreach (var c in uniqueValues[k].Keys)
                {
                    if (c == null)
                        continue;

                    if (c.Length >= 3200)
                    {
                        listOfLabelContainers.Remove(k);
                        break;
                    }
                }
            }

            using (ZipFile zip = new ZipFile())
            {
                #region Log Data

                foreach (string _id in logDataTables.Keys)
                {

                    if (_skipRootTable && _id == "Log")
                        continue;

                    var _varlist = new List<StataVariable>();
                    _varlist.Add(new StataVariable() { Name = "Line", VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%12.0g", Description = CodebookDictionary.GetColumnNameDescription("Line", _language) });

                    if (PersonIdentifierIsNumber)
                        _varlist.Add(new StataVariable() { Name = PersonIdentifierName, VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("PersonIdentifier", _language) });
                    else
                        _varlist.Add(new StataVariable() { Name = PersonIdentifierName, VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("PersonIdentifier", _language), ValueLabelName = "l_" + "PersonIdentifier" });

                    _varlist.Add(new StataVariable() { Name = "Element", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("Element", _language), ValueLabelName = "l_" + "Element" });
                    _varlist.Add(new StataVariable() { Name = "TimeStamp", VarType = StataVariable.StataVarType.Double, DisplayFormat = @"%tcMonth_dd,_CCYY_HH:MM:SS.sss", Description = CodebookDictionary.GetColumnNameDescription("TimeStamp", _language) , ValueLabelName = "l_" + "Timestamp" });
                    _varlist.Add(new StataVariable() { Name = "RelativeTime", VarType = StataVariable.StataVarType.Double, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("RelativeTime", _language), ValueLabelName = "l_" + "RelativeTime" });

                    _varlist.Add(new StataVariable() { Name = "EventID", VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("EventID", _language) });
                    _varlist.Add(new StataVariable() { Name = "ParentEventID", VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("ParentEventID", _language), ValueLabelName = "l_" + "ParentEventID" });
                    _varlist.Add(new StataVariable() { Name = "Path", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%40.0g", Description = CodebookDictionary.GetColumnNameDescription("Path", _language), ValueLabelName = "l_" + "Path" });
                    _varlist.Add(new StataVariable() { Name = "ParentPath", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%40.0g", Description = CodebookDictionary.GetColumnNameDescription("ParentPath", _language), ValueLabelName = "l_" + "ParentPath" });
                    _varlist.Add(new StataVariable() { Name = "EventName", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("EventName", _language), ValueLabelName = "l_" + "EventName" });
                    
                    if (logDataTableColnames.ContainsKey(_id))
                    {
                        foreach (var _colname in logDataTableColnames[_id])
                        {
                            if (!listOfLabelContainers.ContainsKey(_colname))
                            {  
                                _varlist.Add(new StataVariable() { Name = "a_" + _colname, VarType = StataVariable.StataVarType.String, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetAttributeDescrition(_id, "a_" + _colname, _language, 64)});
                            }
                            else
                            {
                                _varlist.Add(new StataVariable() { Name = "a_" + _colname, VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetAttributeDescrition(_id, "a_" + _colname, _language, 64), ValueLabelName = listOfLabelContainers [_colname] });
                            }

                        }
                    }

                    string _tmpfile = GetTempFileName("dta");
                    string _dataSetName = CodebookDictionary.GetMetaData("StudyName", "", _language);
                    
                    var _dtaFile = new StataFileWriter(_tmpfile, _varlist, _dataSetName, true);
                     
                    if (!PersonIdentifierIsNumber)
                    {
                        for (int _i = 0; _i < uniqueValues["PersonIdentifier"].Count; _i++)
                            _dtaFile.AddValueLabel("l_" + "PersonIdentifier", _i, uniqueValuesLookup["PersonIdentifier"][_i]);
                    }

                    int _linecounter = 0;
                    foreach (var v in logDataTables[_id])
                    {

                        object[] _line = new object[_varlist.Count];

                        _line[0] = _linecounter;
                        _line[1] = v.PersonIdentifier;

                        _line[2] = v.Element;
                        if (v.TimeStamp.Year == 1)
                        {
                            _line[3] = StataLib.StataMissingValues._empty;
                        }
                        else
                        {
                            _line[3] = Math.Round((v.TimeStamp - dt1960).TotalMilliseconds, 0);
                        }
                        _line[4] = Math.Round(v.RelativeTime.TotalMilliseconds,0);
                        _line[5] = v.EventID;
                        _line[6] = v.ParentEventID;
                        _line[7] = v.Path;
                        _line[8] = v.ParentPath;
                        _line[9] = v.EventName;

                        if (logDataTableColnames.ContainsKey(_id))
                        {
                            for (int _i = 0; _i < logDataTableColnames[_id].Count; _i++)
                            {
            
                                if (!listOfLabelContainers.ContainsKey(logDataTableColnames[_id][_i]))
                                 { 
                                    string _value = "";
                                    foreach (var p in v.AttributValues)
                                    {
                                        if (p.Item1 == _i)
                                        {
                                            _value = uniqueValuesLookup[logDataTableColnames[_id][_i]][p.Item2];
                                            break;
                                        }
                                    }
                                    _line[10 + _i] = _value;
                                }
                                else
                                {
                                    int _value = -1;
                                    foreach (var p in v.AttributValues)
                                    {
                                        if (p.Item1 == _i)
                                        {
                                            _value = p.Item2;
                                            break;
                                        }
                                    }
                                    _line[10 + _i] = _value;
                                }

                            }
                        }

                        _dtaFile.AppendDataLine(_line);

                        _linecounter++;
                    }
                     
                    _dtaFile.AddValueLabel("l_" + "ParentEventID", -1, "(no parent)");
                    _dtaFile.AddValueLabel("l_" + "Timestamp", -1, "(missing)");

                    foreach (string _labelSetName in new string[] { "Element", "Path", "ParentPath", "EventName" })
                    {
                        for (int _i = 0; _i < uniqueValues[_labelSetName].Count; _i++)
                            _dtaFile.AddValueLabel("l_" + _labelSetName, _i, uniqueValuesLookup[_labelSetName][_i]);
                    }

                    if (logDataTableColnames.ContainsKey(_id))
                    {
                        foreach (var _colname in logDataTableColnames[_id])
                        {
                            if (listOfLabelContainers.ContainsKey(_colname))
                            { 
                                for (int _i = 0; _i < uniqueValues[_colname].Count; _i++)
                                    _dtaFile.AddValueLabel(listOfLabelContainers[_colname], _i, uniqueValuesLookup[_colname][_i]);
                                _dtaFile.AddValueLabel("l_" + _colname, -1, _attributeNotDefinded);
                            }                           
                        }
                    }

                    _dtaFile.Close();
                    zip.AddFile(_tmpfile).FileName = _id + ".dta"; 
                }

                #endregion

                #region Log Data / Flat and Sparse Log Data Table 

                if (_includeFlatAndSparseLogDataTable)
                { 
                    string _tmpfile = GetTempFileName("dta");
                    string _dataSetName = CodebookDictionary.GetMetaData("StudyName", "", _language);

                    var _varlist = new List<StataVariable>();
                    _varlist.Add(new StataVariable() { Name = "Line", VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%12.0g", Description = CodebookDictionary.GetColumnNameDescription("Line", _language) });

                    if (PersonIdentifierIsNumber)
                        _varlist.Add(new StataVariable() { Name = PersonIdentifierName, VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("PersonIdentifier", _language) });
                    else
                        _varlist.Add(new StataVariable() { Name = PersonIdentifierName, VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("PersonIdentifier", _language), ValueLabelName = "l_" + "PersonIdentifier" });

                    _varlist.Add(new StataVariable() { Name = "Element", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("Element", _language), ValueLabelName = "l_" + "Element" });
                    _varlist.Add(new StataVariable() { Name = "TimeStamp", VarType = StataVariable.StataVarType.Double, DisplayFormat = @"%tcMonth_dd,_CCYY_HH:MM:SS.sss", Description = CodebookDictionary.GetColumnNameDescription("TimeStamp", _language), ValueLabelName = "l_" + "Timestamp" });
                    _varlist.Add(new StataVariable() { Name = "RelativeTime", VarType = StataVariable.StataVarType.Double, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("RelativeTime", _language), ValueLabelName = "l_" + "RelativeTime" });

                    _varlist.Add(new StataVariable() { Name = "EventID", VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("EventID", _language) });
                    _varlist.Add(new StataVariable() { Name = "ParentEventID", VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("ParentEventID", _language), ValueLabelName = "l_" + "ParentEventID" });
                    _varlist.Add(new StataVariable() { Name = "Path", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%40.0g", Description = CodebookDictionary.GetColumnNameDescription("Path", _language), ValueLabelName = "l_" + "Path" });
                    _varlist.Add(new StataVariable() { Name = "ParentPath", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%40.0g", Description = CodebookDictionary.GetColumnNameDescription("ParentPath", _language), ValueLabelName = "l_" + "ParentPath" });
                    _varlist.Add(new StataVariable() { Name = "EventName", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("EventName", _language), ValueLabelName = "l_" + "EventName" });

                    List<string> _colnames = new List<string>();
                    foreach (var _table in logDataTableColnames)
                    {
                        if (_table.Key != "Results")
                        {
                            foreach (var _colname in _table.Value)
                                if (!_colnames.Contains(_colname))
                                    _colnames.Add(_colname);
                        }
                    }

                    foreach (var _colname in _colnames)
                    {
                        if (!listOfLabelContainers.ContainsKey(_colname))
                            _varlist.Add(new StataVariable() { Name = "a_" + _colname, VarType = StataVariable.StataVarType.String, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetResultVariableLabel(_colname, _language, 64)});
                        else
                            _varlist.Add(new StataVariable() { Name = "a_" + _colname, VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetResultVariableLabel(_colname, _language, 64), ValueLabelName = listOfLabelContainers[_colname] });
                    }                       

                    var _dtaFile = new StataFileWriter(_tmpfile, _varlist, _dataSetName, true);
                     
                    // Data

                    int _linecounter = 0; 
                    for (int i = 0; i < logDataTables[rootLogName].Count; i++)
                    {
                        foreach (var rootChild in logDataTables[rootLogName][i].ChildEvents)
                        { 
                            _linecounter = dtaFlatExportEvent(_attributeNotDefinded, _attributeNotExpected, dt1960, _varlist, _colnames, _dtaFile, _linecounter, rootChild);
                        }
                    }
                     
                    _dtaFile.AddValueLabel("l_" + "ParentEventID", -1, "(no parent)");
                    _dtaFile.AddValueLabel("l_" + "Timestamp", -1, "(missing)");

                    foreach (string _labelSetName in new string[] { "Element", "Path", "ParentPath", "EventName" })
                    {
                        for (int _i = 0; _i < uniqueValues[_labelSetName].Count; _i++)
                            _dtaFile.AddValueLabel("l_" + _labelSetName, _i, uniqueValuesLookup[_labelSetName][_i]);
                    }

                    foreach (var _colname in _colnames)
                    {
                       if (listOfLabelContainers.ContainsKey(_colname))
                       {
                            for (int _i = 0; _i < uniqueValues[_colname].Count; _i++)
                                _dtaFile.AddValueLabel(listOfLabelContainers[_colname], _i, uniqueValuesLookup[_colname][_i].Replace("ë", "e"));

                            _dtaFile.AddValueLabel(listOfLabelContainers[_colname], -1, _attributeNotDefinded);
                           _dtaFile.AddValueLabel(listOfLabelContainers[_colname], -2, _attributeNotExpected);
                       }
                   }

                   _dtaFile.Close();
                   zip.AddFile(_tmpfile).FileName = "FlatAndSparseLogDataTable.dta";
               }

               #endregion

               #region Result Data

               if (ContainsResultData)
               { 
                   var _varlist = new Dictionary<string,StataVariable>();
                   _varlist.Add("Line", new StataVariable() { Name = "Line", VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%12.0g", Description = CodebookDictionary.GetColumnNameDescription("Line", _language) });

                   if (PersonIdentifierIsNumber)
                       _varlist.Add("PersonIdentifier",new StataVariable() { Name = PersonIdentifierName, VarType = StataVariable.StataVarType.Long, DisplayFormat = @" % 20.0g", Description = CodebookDictionary.GetColumnNameDescription("PersonIdentifier", _language) });
                   else
                       _varlist.Add("PersonIdentifier", new StataVariable() { Name = PersonIdentifierName, VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("PersonIdentifier", _language), ValueLabelName = "l_" + "PersonIdentifier" });

                   string _id = "Results";

                   // Head
                   for (int _i = 0; _i < resultDataTableColnames.Count; _i++)
                   {
                       string _key = resultDataTableColnames.Keys.ToArray<string>()[_i];

                       if (!CodebookDictionary.IgnoreResultVariable(_key))
                       {
                           string _colname = resultDataTableColnames.Values.ToArray<string>()[_i];
                           if (!listOfLabelContainers.ContainsKey(_key))
                               _varlist.Add(_key, new StataVariable() { Name = CodebookDictionary.GetResultVariableName(_key), VarType = StataVariable.StataVarType.String, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetResultVariableLabel(_key, _language, 64) });
                           else
                               _varlist.Add(_key, new StataVariable() { Name = CodebookDictionary.GetResultVariableName(_key), VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetResultVariableLabel(_key, _language, 64), ValueLabelName = listOfLabelContainers[_key] });
                       } 
                   }

                   string _tmpfile = GetTempFileName("dta");
                   string _dataSetName = CodebookDictionary.GetMetaData("StudyName", "", _language);
                   var _dtaFile = new StataFileWriter(_tmpfile, _varlist.Values.ToList<StataVariable>(), _dataSetName, true);

                   if (!PersonIdentifierIsNumber)
                   {
                       for (int _i = 0; _i < uniqueValues["PersonIdentifier"].Count; _i++)
                           _dtaFile.AddValueLabel("l_" + "PersonIdentifier", _i, uniqueValuesLookup["PersonIdentifier"][_i]);
                   }

                   // Data 
                   int id = 0;
                   foreach (var v in resultDataTable.Keys)
                   {
                       object[] _line = new object[_varlist.Count];
                       _line[0] = id;
                       _line[1] = resultDataTable[v].PersonIdentifier;

                       int j = 0;
                       for (int _i = 0; _i < resultDataTableColnames.Count; _i++)
                       { 
                           string _key = resultDataTableColnames.Keys.ToList<string>()[_i];
                           if (!CodebookDictionary.IgnoreResultVariable(_key))
                           {
                               if (!listOfLabelContainers.ContainsKey(_key))
                               {
                                   string _value = "";
                                   foreach (var p in resultDataTable[v].AttributValues)
                                   {
                                       if (p.Item1 == _i)
                                       {
                                           _value = uniqueValuesLookup[logDataTableColnames[_id][_i]][p.Item2];
                                           break;
                                       }
                                   }
                                   _line[2 + j++] = _value;
                               }
                               else
                               {
                                   int _value = -1;
                                   foreach (var p in resultDataTable[v].AttributValues)
                                   {
                                       if (p.Item1 == _i)
                                       {
                                           _value = p.Item2;
                                           break;
                                       }
                                   }
                                   _line[2 + j++] = _value;
                               }
                           }
                       }

                       _dtaFile.AppendDataLine(_line);
                       id++;
                   }


                   for (int _j = 0; _j < resultDataTableColnames.Count; _j++)
                   {
                       string _key = resultDataTableColnames.Keys.ToList<string>()[_j];
                       if (!CodebookDictionary.IgnoreResultVariable(_key))
                       {
                           if (listOfLabelContainers.ContainsKey(_key))
                           {
                               for (int _i = 0; _i < uniqueValues[_key].Count; _i++)
                                   _dtaFile.AddValueLabel(listOfLabelContainers[_key], _i, uniqueValuesLookup[_key][_i]);

                               _dtaFile.AddValueLabel(listOfLabelContainers[_key], -1, _attributeNotDefinded);
                           }
                       }
                   } 

                   _dtaFile.Close();
                   zip.AddFile(_tmpfile).FileName = _id + ".dta";
               }

               #endregion

               zip.UseZip64WhenSaving = Zip64Option.Always;
               zip.Save(filename);
           }
       }

        private int dtaFlatExportEvent(string _attributeNotDefinded, string _attributeNotExpected, DateTime dt1960, List<StataVariable> _varlist, List<string> _colnames, StataFileWriter _dtaFile, int _linecounter, logxLogDataRow rootChild)
        {
            object[] _line = new object[_varlist.Count];

            _line[0] = _linecounter++;
            _line[1] = rootChild.PersonIdentifier;

            _line[2] = rootChild.Element;
            if (rootChild.TimeStamp.Year == 1)
                _line[3] = StataLib.StataMissingValues._empty;
            else
                _line[3] = Math.Round((rootChild.TimeStamp - dt1960).TotalMilliseconds, 0);

            _line[4] = Math.Round(rootChild.RelativeTime.TotalMilliseconds, 0);
            _line[5] = rootChild.EventID;
            _line[6] = rootChild.ParentEventID;
            _line[7] = rootChild.Path;
            _line[8] = rootChild.ParentPath;
            _line[9] = rootChild.EventName;

            for (int j = 0; j < _colnames.Count; j++)
            {
                string _colname = _colnames[j];
                string _id = uniqueValuesLookup["Path"][rootChild.Path];

                if (logDataTableColnames[_id].Contains(_colname))
                {
                    int _i = logDataTableColnames[_id].IndexOf(_colname);
                    int _value = -1;
                    foreach (var p in rootChild.AttributValues)
                    {
                        if (p.Item1 == _i)
                        {
                            _value = p.Item2;
                            break;
                        }
                    }
                    if (_value == -1)
                    {
                        if (_varlist[10 + j].VarType == StataVariable.StataVarType.String || _varlist[10 + j].VarType == StataVariable.StataVarType.FixedString)
                            _line[10 + j] = _attributeNotExpected;
                        else if (_varlist[10 + j].VarType == StataVariable.StataVarType.Int || _varlist[10 + j].VarType == StataVariable.StataVarType.Long || _varlist[10 + j].VarType == StataVariable.StataVarType.Byte)
                        {
                            _line[10 + j] = _value;
                        }
                    }
                    else
                    {
                        _line[10 + j] = _value;
                    }
                }
                else
                {
                    if (_varlist[10 + j].VarType == StataVariable.StataVarType.String || _varlist[10 + j].VarType == StataVariable.StataVarType.FixedString)
                        _line[10 + j] = _attributeNotDefinded;
                    else if (_varlist[10 + j].VarType == StataVariable.StataVarType.Int || _varlist[10 + j].VarType == StataVariable.StataVarType.Long || _varlist[10 + j].VarType == StataVariable.StataVarType.Byte)
                    {
                        _line[10 + j] = -2;
                    }
                }
            }

            _dtaFile.AppendDataLine(_line);

            foreach (var child in rootChild.ChildEvents)
            {
                _linecounter = dtaFlatExportEvent(_attributeNotDefinded, _attributeNotExpected, dt1960, _varlist, _colnames, _dtaFile, _linecounter, child);
            }

            return _linecounter;
        }

        public void ExportSPSS(CommandLineArguments ParsedCommandLineArguments)
        {
            string filename = ParsedCommandLineArguments.Transform_OutputSPSS;

            string _attributeNotDefinded = "(attribute not defined)";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputstringattributenotdefined"))
                _attributeNotDefinded = ParsedCommandLineArguments.ParameterDictionary["outputstringattributenotdefined"];

            string _attributeNotExpected = "(attribute not expected)";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputstringattributenotexpected"))
                _attributeNotExpected = ParsedCommandLineArguments.ParameterDictionary["outputstringattributenotdefined"];

            bool _skipRootTable = false;
            if (ParsedCommandLineArguments.Flags.Contains("SKIPROOT"))
                _skipRootTable = true;

            bool _includeFlatAndSparseLogDataTable = false;
            if (ParsedCommandLineArguments.Flags.Contains("INCLUDEFLAT"))
                _includeFlatAndSparseLogDataTable = true;

            string _language = "ENG";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("language"))
                _language = ParsedCommandLineArguments.ParameterDictionary["language"];

            DateTime dt1960 = new DateTime(1960, 1, 1, 0, 0, 0, 0);

            Dictionary<string, Dictionary<double, string>> labelContainers = new Dictionary<string, Dictionary<double, string>>();
            foreach (var k in uniqueValues.Keys)
            {
                if (!labelContainers.ContainsKey(k))
                    labelContainers.Add(k, new Dictionary<double, string>()); ;

                for (int i = 0; i < uniqueValues[k].Count; i++)
                    labelContainers[k].Add((double)i, uniqueValuesLookup[k][i]);

                labelContainers[k].Add((double)(-1), _attributeNotDefinded);
            }

            using (ZipFile zip = new ZipFile())
            {
                #region Log Data

                foreach (string _id in logDataTables.Keys)
                {
                    if (_skipRootTable && _id == "Log")
                        continue;

                    var _varlist = new List<SpssLib.SpssDataset.Variable>();
                    _varlist.Add(new SpssLib.SpssDataset.Variable("Line") { Label = CodebookDictionary.GetColumnNameDescription("Line", _language) });

                    if (PersonIdentifierIsNumber || !labelContainers.ContainsKey("PersonIdentifier"))
                        _varlist.Add(new SpssLib.SpssDataset.Variable(PersonIdentifierName) { Label = CodebookDictionary.GetResultVariableLabel("PersonIdentifier", _language, 64) });
                    else
                        _varlist.Add(new SpssLib.SpssDataset.Variable(PersonIdentifierName) { ValueLabels = labelContainers["PersonIdentifier"], Label = CodebookDictionary.GetResultVariableLabel("PersonIdentifier", _language, 64) });

                    _varlist.Add(new SpssLib.SpssDataset.Variable("Element") { ValueLabels = labelContainers["Element"], Label = CodebookDictionary.GetResultVariableLabel("Element", _language, 64) });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("TimeStamp")
                    { /*ValueLabels = labelContainers["TimeStamp"],*/
                        Label = CodebookDictionary.GetResultVariableLabel("TimeStamp", _language, 64)
                    });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("RelativeTime") { /*ValueLabels = labelContainers["RelativeTime"], */Label = CodebookDictionary.GetResultVariableLabel("RelativeTime", _language, 64) });

                    _varlist.Add(new SpssLib.SpssDataset.Variable("EventID") { /*ValueLabels = labelContainers["EventID"],*/ Label = CodebookDictionary.GetResultVariableLabel("EventID", _language, 64) });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("ParentEventID") { Label = CodebookDictionary.GetResultVariableLabel("ParentEventID", _language, 64) });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("Path") { ValueLabels = labelContainers["Path"], Label = CodebookDictionary.GetResultVariableLabel("Path", _language, 64) });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("ParentPath") { ValueLabels = labelContainers["ParentPath"], Label = CodebookDictionary.GetResultVariableLabel("ParentPath", _language, 64) });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("EventName") { ValueLabels = labelContainers["EventName"], Label = CodebookDictionary.GetResultVariableLabel("EventName", _language, 64) });


                    if (logDataTableColnames.ContainsKey(_id))
                    {
                        foreach (var _colname in logDataTableColnames[_id])
                        {
                            if (!labelContainers.ContainsKey(_colname))
                                _varlist.Add(new SpssLib.SpssDataset.Variable("a_" + _colname) { Label = CodebookDictionary.GetResultVariableLabel(_colname, _language, 64) });
                            else
                                _varlist.Add(new SpssLib.SpssDataset.Variable("a_" + _colname) { ValueLabels = labelContainers[_colname], Label = CodebookDictionary.GetResultVariableLabel(_colname, _language, 64) });
                        }
                    }
                     
                    string _tmpfile = GetTempFileName("sav");
                    string _dataSetName = CodebookDictionary.GetMetaData("StudyName", "", _language);
                    var options = new SpssLib.DataReader.SpssOptions();

                    int _linecounter = 0;
                    using (FileStream fileStream = new FileStream(_tmpfile, FileMode.Create, FileAccess.Write))
                    {
                        using (var writer = new SpssLib.DataReader.SpssWriter(fileStream, _varlist, options))
                        {
                            foreach (var v in logDataTables[_id])
                            {

                                var _line = writer.CreateRecord();
                                _line[0] = (double)_linecounter;
                                _line[1] = (double)v.PersonIdentifier;

                                _line[2] = (double)v.Element;
                                if (v.TimeStamp.Year == 1)
                                {
                                    _line[3] = (double)-1;
                                }
                                else
                                {
                                    _line[3] = (double)Math.Round((v.TimeStamp - dt1960).TotalMilliseconds, 0);
                                }
                                _line[4] = (double)(Math.Round(v.RelativeTime.TotalMilliseconds, 0));
                                _line[5] = (double)v.EventID;
                                _line[6] = (double)v.ParentEventID;
                                _line[7] = (double)v.Path;
                                _line[8] = (double)v.ParentPath;
                                _line[9] = (double)v.EventName;

                                if (logDataTableColnames.ContainsKey(_id))
                                {
                                    for (int _i = 0; _i < logDataTableColnames[_id].Count; _i++)
                                    {

                                        if (!labelContainers.ContainsKey(logDataTableColnames[_id][_i]))
                                        {
                                            double _value = -1;
                                            foreach (var p in v.AttributValues)
                                            {
                                                if (p.Item1 == _i)
                                                {
                                                    _value = double.Parse(uniqueValuesLookup[logDataTableColnames[_id][_i]][p.Item2]);
                                                    break;
                                                }
                                            }
                                            _line[10 + _i] = _value;
                                        }
                                        else
                                        {
                                            double _value = -1;
                                            foreach (var p in v.AttributValues)
                                            {
                                                if (p.Item1 == _i)
                                                {
                                                    _value = (double)p.Item2;
                                                    break;
                                                }
                                            }
                                            _line[10 + _i] = _value;
                                        }

                                    }
                                }

                                writer.WriteRecord(_line);
                                _linecounter++;
                            }
                        }
                        zip.AddFile(_tmpfile).FileName = _id + ".sav";

                    }
                }

                #endregion
                 
                #region Log Data / Flat and Sparse Log Data Table 

                if (_includeFlatAndSparseLogDataTable)
                { 
                    var _varlist = new List<SpssLib.SpssDataset.Variable>();
                    _varlist.Add(new SpssLib.SpssDataset.Variable("Line") { Label = CodebookDictionary.GetColumnNameDescription("Line", _language) });
                     
                    foreach( var l in labelContainers.Keys)
                    {
                        if (!labelContainers[l].ContainsKey((double)-2))
                            labelContainers[l].Add((double)-2, _attributeNotExpected);
                    }

                    if (PersonIdentifierIsNumber || !labelContainers.ContainsKey("PersonIdentifier"))
                        _varlist.Add(new SpssLib.SpssDataset.Variable(PersonIdentifierName) { Label = CodebookDictionary.GetResultVariableLabel("PersonIdentifier", _language, 64) });
                    else
                        _varlist.Add(new SpssLib.SpssDataset.Variable(PersonIdentifierName) { ValueLabels = labelContainers["PersonIdentifier"], Label = CodebookDictionary.GetResultVariableLabel("PersonIdentifier", _language, 64) });

                    _varlist.Add(new SpssLib.SpssDataset.Variable("Element") { ValueLabels = labelContainers["Element"], Label = CodebookDictionary.GetResultVariableLabel("Element", _language, 64) });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("TimeStamp") {   Label = CodebookDictionary.GetResultVariableLabel("TimeStamp", _language, 64) });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("RelativeTime") { Label = CodebookDictionary.GetResultVariableLabel("RelativeTime", _language, 64) });

                    _varlist.Add(new SpssLib.SpssDataset.Variable("EventID") { Label = CodebookDictionary.GetResultVariableLabel("EventID", _language, 64) });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("ParentEventID") { Label = CodebookDictionary.GetResultVariableLabel("ParentEventID", _language, 64) });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("Path") { ValueLabels = labelContainers["Path"], Label = CodebookDictionary.GetResultVariableLabel("Path", _language, 64) });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("ParentPath") { ValueLabels = labelContainers["ParentPath"], Label = CodebookDictionary.GetResultVariableLabel("ParentPath", _language, 64) });
                    _varlist.Add(new SpssLib.SpssDataset.Variable("EventName") { ValueLabels = labelContainers["EventName"], Label = CodebookDictionary.GetResultVariableLabel("EventName", _language, 64) });

                    List<string> _colnames = new List<string>();
                    foreach (var _table in logDataTableColnames)
                    {
                        if (_table.Key != "Results")
                        {
                            foreach (var _colname in _table.Value)
                                if (!_colnames.Contains(_colname))
                                    _colnames.Add(_colname);
                        }
                    }

                    foreach (var _colname in _colnames)
                    {
                        if (!labelContainers.ContainsKey(_colname))
                            _varlist.Add(new SpssLib.SpssDataset.Variable("a_" + _colname) { Label = CodebookDictionary.GetResultVariableLabel(_colname, _language, 64) });
                        else
                            _varlist.Add(new SpssLib.SpssDataset.Variable("a_" + _colname) { ValueLabels = labelContainers[_colname], Label = CodebookDictionary.GetResultVariableLabel(_colname, _language, 64) });
                         
                    }

                    string _tmpfile = GetTempFileName("sav");
                    string _dataSetName = CodebookDictionary.GetMetaData("StudyName", "", _language);
                    var options = new SpssLib.DataReader.SpssOptions();

                    
                    using (FileStream fileStream = new FileStream(_tmpfile, FileMode.Create, FileAccess.Write))
                    {
                        using (var writer = new SpssLib.DataReader.SpssWriter(fileStream, _varlist, options))
                        {
                            int _linecounter = 0;
                            for (int i = 0; i < logDataTables[rootLogName].Count; i++)
                            {
                                foreach (var rootChild in logDataTables[rootLogName][i].ChildEvents)
                                {                                   
                                    _linecounter = savFlatExportEvent(_attributeNotDefinded, _attributeNotExpected, dt1960, _varlist, _colnames, writer, _linecounter, rootChild);
                                }
                            }                                                        
                        }
                    }
                    zip.AddFile(_tmpfile).FileName = "FlatAndSparseLogDataTable.sav";
                }

                #endregion

                #region Result Data

                if (ContainsResultData)
                {
                    string _id = "Results";
                    var _varlist = new List<SpssLib.SpssDataset.Variable>();
                    _varlist.Add(new SpssLib.SpssDataset.Variable("Line") { Label = CodebookDictionary.GetColumnNameDescription("Line", _language) });

                    if (PersonIdentifierIsNumber || !labelContainers.ContainsKey("PersonIdentifier"))
                        _varlist.Add(new SpssLib.SpssDataset.Variable(PersonIdentifierName) { Label = CodebookDictionary.GetResultVariableLabel("PersonIdentifier", _language, 64) });
                    else
                        _varlist.Add(new SpssLib.SpssDataset.Variable(PersonIdentifierName) { ValueLabels = labelContainers["PersonIdentifier"], Label = CodebookDictionary.GetResultVariableLabel("PersonIdentifier", _language, 64) });

                    // Head
                    for (int _i = 0; _i < resultDataTableColnames.Count; _i++)
                    {
                        string _key = resultDataTableColnames.Keys.ToArray<string>()[_i];

                        if (!CodebookDictionary.IgnoreResultVariable(_key))
                        {
                            string _colname = resultDataTableColnames.Values.ToArray<string>()[_i];
                            if (!labelContainers.ContainsKey(_key))
                            {
                                _varlist.Add(new SpssLib.SpssDataset.Variable(CodebookDictionary.GetResultVariableName(_key)) { Label = CodebookDictionary.GetResultVariableLabel(_key, _language, 64) });
                            }
                            else
                            {
                                _varlist.Add(new SpssLib.SpssDataset.Variable(CodebookDictionary.GetResultVariableName(_key)) { ValueLabels = labelContainers[_key], Label = CodebookDictionary.GetResultVariableLabel(_key, _language, 64) });
                            }
                        }
                    }

                    string _tmpfile = GetTempFileName("sav");
                    string _dataSetName = CodebookDictionary.GetMetaData("StudyName", "", _language);
                    var options = new SpssLib.DataReader.SpssOptions();

                    using (FileStream fileStream = new FileStream(_tmpfile, FileMode.Create, FileAccess.Write))
                    {
                        using (var writer = new SpssLib.DataReader.SpssWriter(fileStream, _varlist, options))
                        {
                            int id = 0;
                            foreach (var v in resultDataTable.Keys)
                            {
                                var _line = writer.CreateRecord();
                                _line[0] = (double)id;
                                _line[1] = (double)resultDataTable[v].PersonIdentifier;

                                int j = 0;
                                for (int _i = 0; _i < resultDataTableColnames.Count; _i++)
                                {
                                    string _key = resultDataTableColnames.Keys.ToList<string>()[_i];
                                    if (!CodebookDictionary.IgnoreResultVariable(_key))
                                    {
                                        if (!labelContainers.ContainsKey(_key))
                                        {
                                            double _value = -1;
                                            foreach (var p in resultDataTable[v].AttributValues)
                                            {
                                                if (p.Item1 == _i)
                                                {
                                                    _value = double.Parse(uniqueValuesLookup[logDataTableColnames[_id][_i]][p.Item2]);
                                                    break;
                                                }
                                            }
                                            _line[2 + j++] = _value;
                                        }
                                        else
                                        {
                                            double _value = -1;
                                            foreach (var p in resultDataTable[v].AttributValues)
                                            {
                                                if (p.Item1 == _i)
                                                {
                                                    _value = p.Item2;
                                                    break;
                                                }
                                            }
                                            _line[2 + j++] = _value;
                                        }
                                    }
                                }

                                writer.WriteRecord(_line);
                                id++;
                            }
                        }
                    }

                    zip.AddFile(_tmpfile).FileName = _id + ".sav";
                }

                #endregion

                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.Save(filename);
            }
        }

        private int savFlatExportEvent(string _attributeNotDefinded, string _attributeNotExpected, DateTime dt1960, List<Variable> _varlist, List<string> _colnames, SpssWriter writer, int _linecounter, logxLogDataRow rootChild)
        {
            var _line = writer.CreateRecord();
            _line[0] = (double)_linecounter++;
            _line[1] = (double)rootChild.PersonIdentifier;

            _line[2] = (double)rootChild.Element;
            if (rootChild.TimeStamp.Year == 1)
            {
                _line[3] = (double)-1;
            }
            else
            {
                _line[3] = (double)Math.Round((rootChild.TimeStamp - dt1960).TotalMilliseconds, 0);
            }
            _line[4] = (double)(Math.Round(rootChild.RelativeTime.TotalMilliseconds, 0));
            _line[5] = (double)rootChild.EventID;
            _line[6] = (double)rootChild.ParentEventID;
            _line[7] = (double)rootChild.Path;
            _line[8] = (double)rootChild.ParentPath;
            _line[9] = (double)rootChild.EventName;

            string _id = uniqueValuesLookup["Path"][rootChild.Path];
            for (int j = 0; j < _colnames.Count; j++)
            {
                string _colname = _colnames[j];
                if (logDataTableColnames[_id].Contains(_colname))
                {
                    int _i = logDataTableColnames[_id].IndexOf(_colname);
                    int _value = -1;
                    foreach (var p in rootChild.AttributValues)
                    {
                        if (p.Item1 == _i)
                        {
                            _value = p.Item2;
                            break;
                        }
                    }
                    if (_value == -1)
                    {
                        if (_varlist[10 + j].Type == SpssLib.SpssDataset.DataType.Text)
                            _line[10 + j] = _attributeNotExpected;
                        else if (_varlist[10 + j].Type == SpssLib.SpssDataset.DataType.Numeric)
                            _line[10 + j] = (double)_value;
                    }
                    else
                    {
                        _line[10 + j] = (double)_value;
                    }
                }
                else
                {
                    if (_varlist[10 + j].Type == SpssLib.SpssDataset.DataType.Text)
                        _line[10 + j] = _attributeNotDefinded;
                    else if (_varlist[10 + j].Type == SpssLib.SpssDataset.DataType.Numeric)
                        _line[10 + j] = (double)-2;

                }
            }

            writer.WriteRecord(_line);

            foreach (var child in rootChild.ChildEvents)
            {
                _linecounter = savFlatExportEvent(_attributeNotDefinded, _attributeNotExpected, dt1960, _varlist, _colnames, writer, _linecounter, child); ;
            }

            return _linecounter;
        }

        // TODO: Check Quotation

        public void ExportCSV(CommandLineArguments ParsedCommandLineArguments)
        {
            string filename = ParsedCommandLineArguments.Transform_OutputZCSV;

            string _outputTimeStampFormatString = "dd.MM.yyyy HH:mm:ss.fff";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputtimestampformatstring"))
                _outputTimeStampFormatString = ParsedCommandLineArguments.ParameterDictionary["outputtimestampformatstring"];

            string _outputRelativeTimeFormatString = "";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputrelativetimeformatstring"))
                _outputRelativeTimeFormatString = ParsedCommandLineArguments.ParameterDictionary["outputrelativetimeformatstring"];

            string _attributeNotDefinded = "(attribute not defined)";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputstringattributenotdefined"))
                _attributeNotDefinded = ParsedCommandLineArguments.ParameterDictionary["outputstringattributenotdefined"];

            string _attributeNotExpected = "(attribute not expected)";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputstringattributenotexpected"))
                _attributeNotExpected = ParsedCommandLineArguments.ParameterDictionary["outputstringattributenotdefined"];

            bool _includeFlatAndSparseLogDataTable = false;
            if (ParsedCommandLineArguments.Flags.Contains("INCLUDEFLAT"))
                _includeFlatAndSparseLogDataTable = true;

            bool _skipRootTable = false;
            if (ParsedCommandLineArguments.Flags.Contains("SKIPROOT"))
                _skipRootTable = true;

            bool _skipResultTable = false;
            if (ParsedCommandLineArguments.Flags.Contains("SKIPRESULTS"))
                _skipResultTable = true;

            string _columnDelimiter = ";";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("columndelimiter"))
                _columnDelimiter = ParsedCommandLineArguments.ParameterDictionary["columndelimiter"];

            string _language = "ENG";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("language"))
                _language = ParsedCommandLineArguments.ParameterDictionary["language"];

            var _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = args => true,
                Delimiter = _columnDelimiter
            };

            using (ZipFile zip = new ZipFile())
            { 
                #region Log Data / Universal Format

                foreach (string _id in logDataTables.Keys)
                {
                    if (_skipRootTable && _id == "Log")
                        continue;

                    string _tmpfile = GetTempFileName("csv");

                    using (StreamWriter writer = new StreamWriter(new FileStream(_tmpfile, FileMode.OpenOrCreate), Encoding.UTF8))
                    using (var csv = new CsvWriter(writer,  _config))
                    {
                        // header 

                        List<string> _header = new List<string>() { "Line", "PersonIdentifier", "Element", "TimeStamp", "RelativeTime", "EventID", "ParentEventID", "Path", "ParentPath", "EventName" };
                        foreach (var _colname in logDataTableColnames[_id])
                            _header.Add("a_" + _colname);
                         
                        foreach (var h in _header)
                            csv.WriteField(h);

                        csv.NextRecord();

                        // data

                        int _linecounter = 0;
                        foreach (var v in logDataTables[_id])
                        {
                            csv.WriteField(_linecounter);
                             
                            if (PersonIdentifierIsNumber)
                                csv.WriteField(v.PersonIdentifier.ToString());
                            else
                                csv.WriteField(uniqueValuesLookup["PersonIdentifier"][(int)v.PersonIdentifier]);

                            csv.WriteField(uniqueValuesLookup["Element"][v.Element] );
                            csv.WriteField(v.TimeStamp.ToString(_outputTimeStampFormatString));
                         
                            if (_outputRelativeTimeFormatString.Trim() == "")
                                csv.WriteField(Math.Round(v.RelativeTime.TotalMilliseconds, 0).ToString() );                            
                            else
                                csv.WriteField(v.RelativeTime.ToString(_outputRelativeTimeFormatString) );

                            csv.WriteField(v.EventID.ToString() );
                            csv.WriteField(v.ParentEventID.ToString() );
                            csv.WriteField(uniqueValuesLookup["Path"][v.Path] );
                            csv.WriteField(uniqueValuesLookup["ParentPath"][v.ParentPath] );
                            csv.WriteField(uniqueValuesLookup["EventName"][v.EventName]);
                              
                            if (logDataTableColnames.ContainsKey(_id))
                            {
                                for (int _i = 0; _i < logDataTableColnames[_id].Count; _i++)
                                {                                     
                                    int _value = -1;
                                    foreach (var p in v.AttributValues)
                                    {
                                        if (p.Item1 == _i)
                                        {
                                            _value = p.Item2;
                                            break;
                                        }
                                    }
                                
                                    if (_value == -1)
                                        csv.WriteField(_attributeNotDefinded);
                                    else
                                        csv.WriteField(uniqueValuesLookup[logDataTableColnames[_id][_i]][_value]);                                     
                                }
                            }
                            _linecounter++;

                            csv.NextRecord();

                        }
                         
                    }
                     
                    zip.AddFile(_tmpfile).FileName = _id + ".csv";

                }

                #endregion

                #region Log Data / Flat and Sparse Log Data Table 

                if (_includeFlatAndSparseLogDataTable)
                { 
                    string _tmpFlatFile = GetTempFileName("csv");
                    using (StreamWriter writer = new StreamWriter(new FileStream(_tmpFlatFile, FileMode.OpenOrCreate), Encoding.UTF8))
                    using (var csv = new CsvWriter(writer, _config))
                    {
                        // header 

                        List<string> _header = new List<string>() { "Line", "PersonIdentifier", "Element", "TimeStamp", "RelativeTime", "EventID", "ParentEventID", "Path", "ParentPath", "EventName" };
                        List<string> _colnames = new List<string>();
                        foreach (var _table in logDataTableColnames)
                        {
                            if (_table.Key != "Results")
                            {
                                foreach (var _colname in _table.Value)
                                    if (!_colnames.Contains(_colname))
                                        _colnames.Add(_colname);
                            }
                        }
                        foreach (var _colname in _colnames)
                            _header.Add("a_" + _colname);

                        foreach (var h in _header)
                            csv.WriteField(h);

                        csv.NextRecord();

                        // data
                         
                        int _linecounter = 0;
                        for (int i = 0; i < logDataTables[rootLogName].Count; i++)
                        {
                            foreach (var rootChild in logDataTables[rootLogName][i].ChildEvents)
                            {
                                _linecounter = csvFlatExportEvent(_outputTimeStampFormatString, _outputRelativeTimeFormatString, _attributeNotDefinded, _attributeNotExpected, csv, _colnames, _linecounter, rootChild);
                            }
                        }
                    }
 
                    zip.AddFile(_tmpFlatFile).FileName = "FlatAndSparseLogDataTable.csv";
                }

                #endregion 
                 
                #region Result Data

                if (ContainsResultData && !_skipResultTable)
                {
                    string _id = "Results";
                    string _tmpfile = GetTempFileName("csv"); 
                    using (StreamWriter writer = new StreamWriter(new FileStream(_tmpfile, FileMode.OpenOrCreate), Encoding.UTF8))
                    using (var csv = new CsvWriter(writer, _config))
                    {
                        // header 

                        List<string> _header = new List<string>() { "Line", "PersonIdentifier"};
                        if (logDataTableColnames.ContainsKey(_id))
                        {
                            for (int _i = 0; _i < resultDataTableColnames.Count; _i++)
                            {
                                string _key = resultDataTableColnames.Keys.ToArray<string>()[_i];
                                if (!CodebookDictionary.IgnoreResultVariable(_key))
                                    _header.Add(CodebookDictionary.GetResultVariableName(_key));
                            }
                        }

                        foreach (var h in _header)
                            csv.WriteField(h);

                        csv.NextRecord();

                        // data

                        int _linecounter = 0;  
                        foreach (var v in resultDataTable.Keys)
                        {
                            csv.WriteField(_linecounter);

                            if (PersonIdentifierIsNumber)
                                csv.WriteField(resultDataTable[v].PersonIdentifier);
                            else
                                csv.WriteField(uniqueValuesLookup["PersonIdentifier"][(int)resultDataTable[v].PersonIdentifier]);

                            for (int _i = 0; _i < resultDataTableColnames.Count; _i++)
                            {
                                string _key = resultDataTableColnames.Keys.ToArray<string>()[_i];
                                if (!CodebookDictionary.IgnoreResultVariable(_key))
                                {
                                    int _value = -1;
                                    foreach (var p in resultDataTable[v].AttributValues)
                                    {
                                        if (p.Item1 == _i)
                                        {
                                            _value = p.Item2;
                                            break;
                                        }
                                    }
                                    if (_value == -1)
                                        csv.WriteField(_attributeNotDefinded);
                                    else
                                        csv.WriteField(uniqueValuesLookup[logDataTableColnames[_id][_i]][_value]);
                                }
                            }
                            csv.NextRecord();

                            _linecounter++;
                        }
                    }

                     
                    zip.AddFile(_tmpfile).FileName = _id + ".csv";
                }

                #endregion
                 
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.Save(filename);
            }
        }

        private int csvFlatExportEvent(string _outputTimeStampFormatString, string _outputRelativeTimeFormatString, string _attributeNotDefinded, string _attributeNotExpected, CsvWriter csv, List<string> _colnames, int _linecounter, logxLogDataRow rootChild)
        {
            csv.WriteField(_linecounter++);

            if (PersonIdentifierIsNumber)
                csv.WriteField(rootChild.PersonIdentifier.ToString());
            else
                csv.WriteField(uniqueValuesLookup["PersonIdentifier"][(int)rootChild.PersonIdentifier]);

            csv.WriteField(uniqueValuesLookup["Element"][rootChild.Element]);
            csv.WriteField(rootChild.TimeStamp.ToString(_outputTimeStampFormatString));

            if (_outputRelativeTimeFormatString.Trim() == "")
                csv.WriteField(Math.Round(rootChild.RelativeTime.TotalMilliseconds, 0).ToString());
            else
                csv.WriteField(rootChild.RelativeTime.ToString(_outputRelativeTimeFormatString));

            csv.WriteField(rootChild.EventID.ToString());
            csv.WriteField(rootChild.ParentEventID.ToString());
            csv.WriteField(uniqueValuesLookup["Path"][rootChild.Path]);
            csv.WriteField(uniqueValuesLookup["ParentPath"][rootChild.ParentPath]);
            csv.WriteField(uniqueValuesLookup["EventName"][rootChild.EventName]);

            string _id = uniqueValuesLookup["Path"][rootChild.Path];
            foreach (var _colname in _colnames)
            {
                if (logDataTableColnames[_id].Contains(_colname))
                {
                    int _i = logDataTableColnames[_id].IndexOf(_colname);
                    int _value = -1;
                    foreach (var p in rootChild.AttributValues)
                    {
                        if (p.Item1 == _i)
                        {
                            _value = p.Item2;
                            break;
                        }
                    }
                    if (_value == -1)
                        csv.WriteField(_attributeNotDefinded);
                    else
                        csv.WriteField(uniqueValuesLookup[logDataTableColnames[_id][_i]][_value]);
                }
                else
                    csv.WriteField(_attributeNotExpected);
            }

            csv.NextRecord();
          
            foreach (var child in rootChild.ChildEvents)
                _linecounter = csvFlatExportEvent(_outputTimeStampFormatString, _outputRelativeTimeFormatString, _attributeNotDefinded, _attributeNotExpected, csv, _colnames, _linecounter, child);

            return _linecounter;
        }


        public void ExportXLSX(CommandLineArguments ParsedCommandLineArguments)
        {
            string filename = ParsedCommandLineArguments.Transform_OutputXLSX;

            string _outputTimeStampFormatString = "dd.MM.yyyy HH:mm:ss.fff";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputtimestampformatstring"))
                _outputTimeStampFormatString = ParsedCommandLineArguments.ParameterDictionary["outputtimestampformatstring"];

            string _outputRelativeTimeFormatString = "";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputrelativetimeformatstring"))
                _outputRelativeTimeFormatString = ParsedCommandLineArguments.ParameterDictionary["outputrelativetimeformatstring"];

            string _attributeNotDefinded = "(attribute not defined)";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputstringattributenotdefined"))
                _attributeNotDefinded = ParsedCommandLineArguments.ParameterDictionary["outputstringattributenotdefined"];

            string _attributeNotExpected = "(attribute not expected)";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("outputstringattributenotexpected"))
                _attributeNotExpected = ParsedCommandLineArguments.ParameterDictionary["outputstringattributenotdefined"];

            bool _skipRootTable = false;
            if (ParsedCommandLineArguments.Flags.Contains("SKIPROOT"))
                _skipRootTable = true;

            bool _includeFlatAndSparseLogDataTable = false;
            if (ParsedCommandLineArguments.Flags.Contains("INCLUDEFLAT"))
                _includeFlatAndSparseLogDataTable = true;

            bool _skipResultTable = false;
            if (ParsedCommandLineArguments.Flags.Contains("SKIPRESULTS"))
                _skipResultTable = true;

            string _language = "ENG";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("language"))
                _language = ParsedCommandLineArguments.ParameterDictionary["language"];


            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                Dictionary<string, string> _sheetIndex = new Dictionary<string, string>();
                IWorkbook workbook = new XSSFWorkbook();

                #region Log Data

                foreach (string _id in logDataTables.Keys)
                {
                    if (_skipRootTable && _id == "Log")
                        continue;

                    _sheetIndex.Add(_id, _id);
                    if (_id.Length > 29)
                    {
                        int i = 0;
                        foreach (string v in _sheetIndex.Keys)
                        {
                            if (v.Length > 29)
                                if (v.Substring(0, 28) == _id.Substring(0, 28))
                                    i++;
                        }
                        _sheetIndex[_id] = _id.Substring(0, 28) + i.ToString();

                    }
                    ISheet sheet = workbook.CreateSheet(_sheetIndex[_id]);

                    var rowIndex = 0;
                    IRow firstrow = sheet.CreateRow(rowIndex++);
                    firstrow.CreateCell(0).SetCellValue("Line");
                    firstrow.CreateCell(1).SetCellValue("PersonIdentifier");
                    firstrow.CreateCell(2).SetCellValue("Element");
                    firstrow.CreateCell(3).SetCellValue("TimeStamp");
                    firstrow.CreateCell(4).SetCellValue("RelativeTime");
                    firstrow.CreateCell(5).SetCellValue("EventID");
                    firstrow.CreateCell(6).SetCellValue("ParentEventID");   
                    firstrow.CreateCell(7).SetCellValue("Path");
                    firstrow.CreateCell(8).SetCellValue("ParentPath");
                    firstrow.CreateCell(9).SetCellValue("EventName");

                    if (logDataTableColnames.ContainsKey(_id))
                    {
                        int _colIndex = 10;
                        foreach (var _colname in logDataTableColnames[_id])
                        {
                            firstrow.CreateCell(_colIndex++).SetCellValue("a_" + _colname);
                        }
                    }

                    int id = 0;
                    foreach (var v in logDataTables[_id])
                    {
                        IRow row = sheet.CreateRow(rowIndex++);
                        row.CreateCell(0).SetCellValue(id);
                        if (PersonIdentifierIsNumber)
                        {
                            row.CreateCell(1).SetCellValue(v.PersonIdentifier);
                        }
                        else
                        {
                            row.CreateCell(1).SetCellValue(uniqueValuesLookup["PersonIdentifier"][(int)v.PersonIdentifier]);
                        }
                      
                        row.CreateCell(2).SetCellValue(uniqueValuesLookup["Element"][v.Element]);
                        row.CreateCell(3).SetCellValue(v.TimeStamp.ToString());

                        if (_outputRelativeTimeFormatString.Trim() == "")
                            row.CreateCell(4).SetCellValue(Math.Round(v.RelativeTime.TotalMilliseconds, 0).ToString());
                        else
                            row.CreateCell(4).SetCellValue(v.RelativeTime.ToString(_outputRelativeTimeFormatString));
                                               
                        row.CreateCell(5).SetCellValue(v.EventID.ToString());
                        row.CreateCell(6).SetCellValue(v.ParentEventID.ToString());
                        row.CreateCell(7).SetCellValue(uniqueValuesLookup["Path"][v.Path]);
                        row.CreateCell(8).SetCellValue(uniqueValuesLookup["ParentPath"][v.ParentPath]);
                        row.CreateCell(9).SetCellValue(uniqueValuesLookup["EventName"][v.EventName]);
                         
                        if (logDataTableColnames.ContainsKey(_id))
                        {
                            int _colIndex = 10;
                            for (int _i = 0; _i < logDataTableColnames[_id].Count; _i++)
                            {
                                int _value = -1;
                                foreach (var p in v.AttributValues)
                                {
                                    if (p.Item1 == _i)
                                    {
                                        _value = p.Item2;
                                        break;
                                    }
                                }
                                if (_value == -1)
                                {
                                    row.CreateCell(_colIndex++).SetCellValue(_attributeNotDefinded);
                                }
                                else
                                {
                                    string _stringValue = uniqueValuesLookup[logDataTableColnames[_id][_i]][_value];
                                    if (_stringValue.Length > 32766)
                                    {
                                        
                                        if (PersonIdentifierIsNumber)
                                        {
                                            ExportErrors.Add("- XLSX: Shortened string of length " + _stringValue.Length + " for person identifier '" + v.PersonIdentifier + "', in table  '" + _id + "', for column '" + logDataTableColnames[_id][_i] + "'");
                                        }
                                        else
                                        {
                                            ExportErrors.Add("- XLSX: Shortened string of length " + _stringValue.Length + " for person identifier '" + uniqueValuesLookup["PersonIdentifier"][(int)v.PersonIdentifier] + "', in table  '" + _id + "', for column '" + logDataTableColnames[_id][_i] + "'");
                                        } 

                                        _stringValue = _stringValue.Substring(0, 32766);
                                    }

                                    row.CreateCell(_colIndex++).SetCellValue(_stringValue);
                                }
                            }
                        }

                        id++;
                    }

                }

                #endregion

                #region Log Data / Flat and Sparse Log Data Table 

                if (_includeFlatAndSparseLogDataTable)
                {
                    string _tableName = "FlatAndSparseLogDataTable";
                    _sheetIndex.Add(_tableName, _tableName);
                    ISheet sheet = workbook.CreateSheet(_sheetIndex[_tableName]);

                    // Head

                    var rowIndex = 0;
                    IRow firstrow = sheet.CreateRow(rowIndex++);
                    firstrow.CreateCell(0).SetCellValue("Line");
                    firstrow.CreateCell(1).SetCellValue("PersonIdentifier");
                    firstrow.CreateCell(2).SetCellValue("Element");
                    firstrow.CreateCell(3).SetCellValue("TimeStamp");
                    firstrow.CreateCell(4).SetCellValue("RelativeTime");
                    firstrow.CreateCell(5).SetCellValue("EventID");
                    firstrow.CreateCell(6).SetCellValue("ParentEventID");
                    firstrow.CreateCell(7).SetCellValue("Path");
                    firstrow.CreateCell(8).SetCellValue("ParentPath");
                    firstrow.CreateCell(9).SetCellValue("EventName");

                    List<string> _colnames = new List<string>();
                    foreach (var _table in logDataTableColnames)
                    {
                        if (_table.Key != "Results")
                        {
                            foreach (var _colname in _table.Value)
                                if (!_colnames.Contains(_colname))
                                    _colnames.Add(_colname);
                        }
                    }

                    int _colIndex = 10;
                    foreach (var _colname in _colnames)
                    {
                        firstrow.CreateCell(_colIndex).SetCellValue("a_" + _colname);
                        _colIndex++;
                    }

                    // Data 

                    int _linecounter = 0;
                    foreach (string _id in logDataTables.Keys)
                    {
                        if (_id != rootLogName)
                        {
                            foreach (var v in logDataTables[_id])
                            {
                                IRow row = sheet.CreateRow(rowIndex++);
                                row.CreateCell(0).SetCellValue(_linecounter);
                                if (PersonIdentifierIsNumber)
                                    row.CreateCell(1).SetCellValue(v.PersonIdentifier);
                                else
                                    row.CreateCell(1).SetCellValue(uniqueValuesLookup["PersonIdentifier"][(int)v.PersonIdentifier]);

                                row.CreateCell(2).SetCellValue(StringToCSVCell(uniqueValuesLookup["Element"][v.Element]));

                                if (_outputRelativeTimeFormatString.Trim() == "")
                                    row.CreateCell(3).SetCellValue((Math.Round(v.RelativeTime.TotalMilliseconds, 0).ToString()));
                                else
                                    row.CreateCell(3).SetCellValue(v.TimeStamp.ToString(_outputTimeStampFormatString));
                                                                 
                                row.CreateCell(4).SetCellValue(StringToCSVCell(Math.Round(v.RelativeTime.TotalMilliseconds, 0).ToString()));
                                row.CreateCell(5).SetCellValue(StringToCSVCell(v.EventID.ToString()));
                                row.CreateCell(6).SetCellValue(StringToCSVCell(v.ParentEventID.ToString()));
                                row.CreateCell(7).SetCellValue(StringToCSVCell(uniqueValuesLookup["Path"][v.Path]));
                                row.CreateCell(8).SetCellValue(StringToCSVCell(uniqueValuesLookup["ParentPath"][v.ParentPath]));
                                row.CreateCell(9).SetCellValue(StringToCSVCell(uniqueValuesLookup["EventName"][v.EventName]));

                                _colIndex = 10;
                                foreach (var _colname in _colnames)
                                {
                                    if (logDataTableColnames[_id].Contains(_colname))
                                    {
                                        int _i = logDataTableColnames[_id].IndexOf(_colname);
                                        int _value = -1;
                                        foreach (var p in v.AttributValues)
                                        {
                                            if (p.Item1 == _i)
                                            {
                                                _value = p.Item2;
                                                break;
                                            }
                                        }
                                        if (_value == -1)
                                            row.CreateCell(_colIndex).SetCellValue(_attributeNotDefinded);
                                        else
                                            row.CreateCell(_colIndex).SetCellValue(uniqueValuesLookup[logDataTableColnames[_id][_i]][_value]);
                                    }
                                    else
                                        row.CreateCell(_colIndex).SetCellValue(_attributeNotExpected);

                                    _colIndex++;
                                } 
                                _linecounter++; 
                            }

                        }
                    } 
                }

                #endregion

                #region Result Data

                if (ContainsResultData && !_skipResultTable)
                {
                    string _tableName = "Results";
                    _sheetIndex.Add(_tableName, _tableName);
                    ISheet sheet = workbook.CreateSheet(_sheetIndex[_tableName]);

                    // Head

                    var rowIndex = 0;
                    IRow firstrow = sheet.CreateRow(rowIndex++);
                    firstrow.CreateCell(0).SetCellValue("Line");
                    firstrow.CreateCell(1).SetCellValue("PersonIdentifier");
                    int _colIndex = 2;
                    for (int _i = 0; _i < resultDataTableColnames.Count; _i++)
                    {
                        string _key = resultDataTableColnames.Keys.ToArray<string>()[_i];
                        if (!CodebookDictionary.IgnoreResultVariable(_key))
                            firstrow.CreateCell(_colIndex++).SetCellValue(CodebookDictionary.GetResultVariableName(_key));
                    }
                       
                    // Data 
                    int _linecounter = 0;
                    foreach (var v in resultDataTable.Keys)
                    { 
                        IRow row = sheet.CreateRow(rowIndex++);
                        row.CreateCell(0).SetCellValue(_linecounter);
                        if (PersonIdentifierIsNumber)
                        {
                            row.CreateCell(1).SetCellValue(resultDataTable[v].PersonIdentifier);
                        }
                        else
                        {
                            row.CreateCell(1).SetCellValue(uniqueValuesLookup["PersonIdentifier"][(int)resultDataTable[v].PersonIdentifier]);
                        }

                        _colIndex = 2;
                        for (int _i = 0; _i < resultDataTableColnames.Count; _i++)
                        {
                            string _key = resultDataTableColnames.Keys.ToArray<string>()[_i];
                            if (!CodebookDictionary.IgnoreResultVariable(_key))
                            {
                                int _value = -1;
                                foreach (var p in resultDataTable[v].AttributValues)
                                {
                                    if (p.Item1 == _i)
                                    {
                                        _value = p.Item2;
                                        break;
                                    }
                                }
                                if (_value == -1)
                                {
                                    row.CreateCell(_colIndex++).SetCellValue(_attributeNotDefinded);
                                }
                                else
                                {
                                    string _stringValue = uniqueValuesLookup[logDataTableColnames[_tableName][_i]][_value];
                                    if (_stringValue.Length > 32766)
                                    {

                                        if (PersonIdentifierIsNumber)
                                        {
                                            ExportErrors.Add("- XLSX: Shortened string of length " + _stringValue.Length + " for person identifier '" + resultDataTable[v].PersonIdentifier + "', in table  '" + _tableName + "', for column '" + logDataTableColnames[_tableName][_i] + "'");
                                        }
                                        else
                                        {
                                            ExportErrors.Add("- XLSX: Shortened string of length " + _stringValue.Length + " for person identifier '" + uniqueValuesLookup["PersonIdentifier"][(int)resultDataTable[v].PersonIdentifier] + "', in table  '" + _tableName + "', for column '" + logDataTableColnames[_tableName][_i] + "'");
                                        }

                                        _stringValue = _stringValue.Substring(0, 32766);
                                    }

                                    row.CreateCell(_colIndex++).SetCellValue(_stringValue);
                                }
                            } 
                        } 
                        _linecounter++;
                    }
                     
                }

                #endregion

                workbook.Write(fs, false);
            }
        }

        public void ExportXES(CommandLineArguments ParsedCommandLineArguments)
        {
            string filename = ParsedCommandLineArguments.Transform_OutputXES;
            DateTime dt1960 = new DateTime(1960, 1, 1, 0, 0, 0, 0);

            EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.Time;
            if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                sort = EventDataListExtension.ESortType.None;

            if (ParsedCommandLineArguments.Flags.Contains("ORDER_WITHIN_ELEMENTS"))
                sort = EventDataListExtension.ESortType.ElementAndTime;

            bool serializeRawXML = false;
            if (ParsedCommandLineArguments.Flags.Contains("SERIALIZERAWXML"))
                serializeRawXML = true;

            bool serializeRawXMLZIP = false;
            if (ParsedCommandLineArguments.Flags.Contains("SERIALIZERAWXMLZIP"))
                serializeRawXMLZIP = true;
              
            Dictionary<string, List<EventData>> _inMemoryTempData = new Dictionary<string, List<EventData>>();
             
            List<string> _eventTables = logDataTables.Keys.ToList<string>();
            _eventTables.Remove("Log");

            foreach (string _eventName in _eventTables)
            {
                foreach (var v in logDataTables[_eventName])
                {
                    string _personIdentifier = v.PersonIdentifier.ToString();
                    if (uniqueValuesLookup.ContainsKey("PersonIdentifier"))
                    {
                        if (uniqueValuesLookup["PersonIdentifier"].ContainsKey((int)v.PersonIdentifier))
                            _personIdentifier = uniqueValuesLookup["PersonIdentifier"][(int)v.PersonIdentifier];
                    }

                    if (!_inMemoryTempData.ContainsKey(_personIdentifier))
                        _inMemoryTempData.Add(_personIdentifier, new List<EventData>());

                    string _element = uniqueValuesLookup["Element"][v.Element];
                    DateTime _timeStamp = v.TimeStamp;

                    var _eventValues = new Dictionary<string, string>();
                    foreach (var p in v.AttributValues)
                    {
                        string _key = logDataTableColnames[_eventName][p.Item1];
                        string _value = uniqueValuesLookup[logDataTableColnames[_eventName][p.Item1]][p.Item2];
                        _eventValues.Add(_key, _value);
                    }

                    _inMemoryTempData[_personIdentifier].Add(new EventData()
                    {
                        Element = _element,
                        EventName = _eventName.Replace(rootLogName + ".",""),
                        PersonIdentifier = _personIdentifier,
                        TimeStamp = _timeStamp,
                        EventValues = _eventValues
                    });
                }
            }

            OpenXesNet.factory.IXFactory factory = OpenXesNet.factory.XFactoryRegistry.Instance.CurrentDefault;
            Stack<OpenXesNet.model.IXAttributable> attributableStack = new Stack<OpenXesNet.model.IXAttributable>();
            Stack<OpenXesNet.model.XAttribute> attributeStack = new Stack<OpenXesNet.model.XAttribute>();

            OpenXesNet.extension.XExtension ext_concept_name = OpenXesNet.extension.XExtensionManager.Instance.GetByPrefix("concept");
            OpenXesNet.extension.XExtension ext_time_timestamp = OpenXesNet.extension.XExtensionManager.Instance.GetByPrefix("time");

            OpenXesNet.model.XLog xesOutput = OpenXesNet.factory.XFactoryRegistry.Instance.CurrentDefault.CreateLog();
            xesOutput.Version = "2.0";
            xesOutput.Features = "nested-attributes";

            List<string> _persons = _inMemoryTempData.Keys.ToList<string>();
            foreach (string _personIdentifier in _persons)
            {
                // sort events by time stamp
                 
                var _l = _inMemoryTempData[_personIdentifier];

                if (ParsedCommandLineArguments.RelativeTime)
                {
                    _l = EventDataListExtension.SortByRelativeTime(_l, sort);
                    _l = EventDataListExtension.ComputeRelativeTimes(_l);
                    _l.ComputeTimedifferencePreviousWithRelativeTimes();
                }
                else
                {
                    _l = EventDataListExtension.SortByTimeStamp(_l, sort);
                    _l = EventDataListExtension.ComputeRelativeTimes(_l);
                    _l.ComputeTimedifferencePrevious();
                }
                 
                OpenXesNet.model.XTrace trace = OpenXesNet.factory.XFactoryRegistry.Instance.CurrentDefault.CreateTrace();
                 
                OpenXesNet.model.XAttribute attr = factory.CreateAttributeLiteral("concept:name", _personIdentifier, ext_concept_name);
                attributeStack.Push(attr);
                OpenXesNet.model.XAttributeMap m_trace = new OpenXesNet.model.XAttributeMap();
                m_trace.Add(attr.Key, attr);

                foreach (var e  in _l)
                {
                    OpenXesNet.model.XAttributeMap m_event = new OpenXesNet.model.XAttributeMap();
                    m_event.Add("concept:name", factory.CreateAttributeLiteral("concept:name", e.EventName, ext_concept_name));
                    m_event.Add("assessment:element", factory.CreateAttributeLiteral("assessment:element", e.Element, null)); 
                    m_event.Add("assessment:relativetime", factory.CreateAttributeLiteral("assessment:relativetime", Math.Round(e.RelativeTime.TotalMilliseconds, 0).ToString(), null));
                    m_event.Add("time:timestamp", factory.CreateAttributeTimestamp("time:timestamp", e.TimeStamp, ext_time_timestamp));
                    foreach (var a in e.EventValues)
                        m_event.Add(a.Key, factory.CreateAttributeLiteral(a.Key, a.Value, null));

                    OpenXesNet.model.XEvent evt = OpenXesNet.factory.XFactoryRegistry.Instance.CurrentDefault.CreateEvent();
                    evt.SetAttributes(m_event);
                    trace.Add(evt);

                }

                trace.SetAttributes(m_trace);
                xesOutput.Add(trace);

            }
                        
            if (serializeRawXML || serializeRawXMLZIP)
            {

                if (serializeRawXMLZIP)
                {
                    using (ZipFile zip = new ZipFile())
                    {
                        string _tmpfile = GetTempFileName("xml");
                        OpenXesNet.io.XesXmlSerializer serializer = new OpenXesNet.io.XesXmlSerializer();
                        FileStream fStream = File.Create(_tmpfile);
                        serializer.Serialize(xesOutput, fStream);
                        fStream.Close();
                        zip.AddFile(_tmpfile).FileName = "xes.xml";
                        zip.UseZip64WhenSaving = Zip64Option.Always;
                        zip.Save(filename);
                    }
                } 
                else
                {
                    OpenXesNet.io.XesXmlSerializer serializer = new OpenXesNet.io.XesXmlSerializer();
                    FileStream fStream = File.Create(filename);
                    serializer.Serialize(xesOutput, fStream);
                    fStream.Close();
                }

            } 
            else
            {
                OpenXesNet.io.XesXmlGZIPSerializer gzipSerializer = new OpenXesNet.io.XesXmlGZIPSerializer();
                FileStream fStream = File.Create(filename);
                gzipSerializer.Serialize(xesOutput, fStream);
                fStream.Close();
            }
        }

        public static string GetTempFileName(string extension)
        {
            int attempt = 0;
            while (true)
            {
                string fileName = Path.GetRandomFileName();
                fileName = Path.ChangeExtension(fileName, extension);
                fileName = Path.Combine(Path.GetTempPath(), fileName);

                try
                {
                    using (new FileStream(fileName, FileMode.CreateNew)) { }
                    return fileName;
                }
                catch (IOException ex)
                {
                    if (++attempt == 100)
                        throw new IOException("No unique temporary file name is available.", ex);
                }
            }
        }

        public static string StringToCSVCell(string str)
        {
            bool mustQuote = (str.Contains(";") || str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }

        public int GetNumberOfPersons
        {
            get
            {
                return uniqueValues["PersonIdentifier"].Count;
            }
        }

        public int GetMaxID(string personIdentifier)
        {
            if (maxEventIDByPerson.ContainsKey(personIdentifier))
                return maxEventIDByPerson[personIdentifier] + 1;
            else
                return 0;
        }

        public void ReadConcordanceTable(string filename, bool verbose)
        {
            if (filename.ToLower().EndsWith(".dta"))
            {
                // STATA 

                StataFileReader _stataLogFileReader = new StataFileReader(filename, true);
                foreach (var _line in _stataLogFileReader)
                {
                    if (!CondordanceTable.ContainsKey(_line[0].ToString()))
                        CondordanceTable.Add(_line[0].ToString().Trim(), _line[1].ToString().Trim());
                }
            }
            else if (filename.ToLower().EndsWith(".sav"))
            {
                // SPSS 

                using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 2048 * 10, FileOptions.SequentialScan))
                {
                    SpssLib.DataReader.SpssReader spssDataset = new SpssLib.DataReader.SpssReader(fileStream);
                    foreach (var _line in spssDataset.Records)
                    {
                        if (_line[0].ToString().Trim() != "" && _line[1].ToString().Trim() != "")
                            if (!CondordanceTable.ContainsKey(_line[0].ToString().Trim()))
                            CondordanceTable.Add(_line[0].ToString(), _line[1].ToString().Trim());
                    }
                } 
            }
            else if (filename.ToLower().EndsWith(".xlsx"))
            {
                // XLSX

                IWorkbook workbook = WorkbookFactory.Create(filename);
                int _concordanceTableSheetIndex = workbook.GetSheetIndex("ConcordancTable");
                


                if (_concordanceTableSheetIndex == -1)
                {
                    if (verbose)
                        Console.WriteLine("Found no table 'ConcordancTable', use the first sheet.");

                    _concordanceTableSheetIndex = 0;
                }

                var _sheet = workbook.GetSheetAt(_concordanceTableSheetIndex);
                if (_sheet.LastRowNum >= 1)
                {
                    for (int rowIndex = 1; rowIndex <= _sheet.LastRowNum; rowIndex++)
                    {
                        IRow row = _sheet.GetRow(rowIndex);
                        if (row.Cells.Count > 1)
                        {
                            string _old = row.Cells[0].ToString().Trim();
                            string _new = row.Cells[1].ToString().Trim();

                            if (!CondordanceTable.ContainsKey(_old))
                                CondordanceTable.Add(_old,_new);
                        }
                    }
                }
            }
            else if (filename.ToLower().EndsWith(".csv"))
            {
                // CSV

                using (var reader = new StreamReader(filename))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        if (!CondordanceTable.ContainsKey(csv.GetField(0)))
                            CondordanceTable.Add(csv.GetField(0).Trim(), csv.GetField(1).Trim());
                    }
                }
            }
            else
            {
                Console.WriteLine("File format for concordance table not supported.");
            }
        }

        public void CreateConcordanceTable(string filename)
        {
            // Export concordance table

            if (filename.ToLower().EndsWith(".dta"))
            {
                // STATA 

                var _varlist = new List<StataVariable>();
                _varlist.Add(new StataVariable() { Name = "OldPersonIdentifier", VarType = StataVariable.StataVarType.FixedString, FixedStringLength = 244, DisplayFormat = @"%20.0g", Description = "Old PersonIdentifier" });
                _varlist.Add(new StataVariable() { Name = "NewPersonIdentifier", VarType = StataVariable.StataVarType.FixedString, FixedStringLength = 244, DisplayFormat = @"%20.0g", Description = "New PersonIdentifier" });

                var _dtaFile = new StataFileWriter(filename, _varlist, "Template for concordance table", true);

                for (int _i = 0; _i < uniqueValues["PersonIdentifier"].Count; _i++)
                {
                    object[] _line = new object[_varlist.Count];

                    _line[0] = uniqueValuesLookup["PersonIdentifier"][_i].ToString();
                    _line[1] = uniqueValuesLookup["PersonIdentifier"][_i].ToString();
                    _dtaFile.AppendDataLine(_line);
                }

                _dtaFile.Close();
            }
            else if (filename.ToLower().EndsWith(".xlsx"))
            {
                // XLSX 

                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    IWorkbook workbook = new XSSFWorkbook();
                    ISheet sheet_concordance = workbook.CreateSheet("ConcordancTable");

                    int sheet_concordance_index = addRowValues(sheet_concordance, 0, new string[] { "OldPersonIdentifier", "NewPersonIdentifier" });
                    for (int _i = 0; _i < uniqueValues["PersonIdentifier"].Count; _i++)
                    {
                        sheet_concordance_index = addRowValues(sheet_concordance, sheet_concordance_index, new string[] { uniqueValuesLookup["PersonIdentifier"][_i].ToString(), uniqueValuesLookup["PersonIdentifier"][_i].ToString() });
                    }
                    workbook.Write(fs, false);
                }
            }
            else if (filename.ToLower().EndsWith(".csv"))
            {
                // CSV

                using (var writer = new StreamWriter(filename))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    for (int _i = 0; _i < uniqueValues["PersonIdentifier"].Count; _i++)
                    {
                        var r = new List<object> { new { OldPersonIdentifier = uniqueValuesLookup["PersonIdentifier"][_i].ToString(), NewPersonIdentifier = uniqueValuesLookup["PersonIdentifier"][_i].ToString() }, };
                        csv.WriteRecords(r);
                    } 
                }
            }
            else
            {
                Console.WriteLine("File format for concordance table not supported.");
            }
        }

        public void LoadCodebookDictionary(string filename)
        {
            CodebookDictionary = new logxCodebookDictionary();
            if (filename.Trim() != "")
                CodebookDictionary.LoadDictionaryExcelSheet(filename);
        }

        public void CreateCodebook(CommandLineArguments ParsedCommandLineArguments)
        {
            string filename = ParsedCommandLineArguments.Transform_Codebook;

            string _language = "ENG";
            if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("language"))
                _language = ParsedCommandLineArguments.ParameterDictionary["language"];


            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            { 
                IWorkbook workbook = new XSSFWorkbook();
                string[] _lng = new string[] { "ENG", "DE" };

                #region MetaData
                ISheet sheet_metadata = workbook.CreateSheet("MetaData");

                int sheet_metadata_index = addRowValues(sheet_metadata, 0, new string[] { "Attribute", "AttributeValue_ENG", "AttributeValue_DE" });
                sheet_metadata_index = addRowValues(sheet_metadata, sheet_metadata_index, CodebookDictionary.GetMetaDataLine("StudyName", "StudyName", _lng));
                sheet_metadata_index = addRowValues(sheet_metadata, sheet_metadata_index, CodebookDictionary.GetMetaDataLine("TestPlatform", "TestPlatform", _lng));

                #endregion
 
                #region Head
                ISheet sheet_head = workbook.CreateSheet("Head");
                  
                int sheet_head_index = addRowValues(sheet_head, 0, new string[] { "Column", "VariableLabel_ENG", "VariableLabel_DE", "Identifier" });
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("Line", _lng) );
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("PersonIdentifier", _lng));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("Element", _lng));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("TimeStamp", _lng));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("RelativeTime", _lng));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("ParentEventID", _lng));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("Path", _lng));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("ParentPath", _lng));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("EventName", _lng));

                #endregion

                #region Attributes
                ISheet sheet_data = workbook.CreateSheet("Attributes");
                int sheet_data_index = addRowValues(sheet_data, 0, new string[] { "Table", "Column", "Condition", "Description_DE", "Description_ENG", "Anonymity", "Purification" });

                foreach (string _id in logDataTables.Keys)
                {
                    if (logDataTableColnames.ContainsKey(_id))
                    {
                        foreach (var _colname in logDataTableColnames[_id])
                        { 
                            List<string[]> _conditions = CodebookDictionary.GetConditions(_id, "a_" + _colname, _lng);
                            foreach (var _c in _conditions)
                            {
                                sheet_data_index = addRowValues(sheet_data, sheet_data_index, _c);
                            } 
                        }
                    }
                }
                #endregion

                #region Events
                ISheet sheet_events = workbook.CreateSheet("Events");

                int sheet_events_index = addRowValues(sheet_events, 0, new string[] { "EventName", "Table", "EventDescription_ENG", "EventDescription_DE" });

                foreach (string tab in logDataTables.Keys)
                {
                    var _res = CodebookDictionary.GetEvent(tab, _lng);
                    foreach (var _r in _res)
                        sheet_events_index = addRowValues(sheet_events, sheet_events_index, _r );
                }

                #endregion

                #region Result Variables 

                ISheet sheet_resultvariables = workbook.CreateSheet("Results Variables");

                int sheet_resultvariables_index = addRowValues(sheet_resultvariables, 0, new string[] { "Key", "Variable", "Label_DE", "Label_ENG", "Ignore"});
                foreach (var v in resultDataTableColnames.Keys)
                {
                    if (CodebookDictionary.ResultVariables.ContainsKey(v))
                    {
                        var _old = CodebookDictionary.ResultVariables[v];
                        sheet_resultvariables_index = addRowValues(sheet_resultvariables, sheet_resultvariables_index, new string[] { v, _old.Name, _old.ValuesByLanguage["DE"], _old.ValuesByLanguage["ENG"], _old.Ignored.ToString() });
                    }
                    else
                    {
                        sheet_resultvariables_index = addRowValues(sheet_resultvariables, sheet_resultvariables_index, new string[] { v, resultDataTableColnames[v], "Beschreibung für '" + resultDataTableColnames[v] + "'", "Label for '" + resultDataTableColnames[v] + "'", "false" });
                    }
                }

                #endregion

                workbook.Write(fs, false);
            }
        }

        private static int addRowValues(ISheet sheet_head, int sheet_head_index, string[] row)
        {
            IRow sheet_head_row = sheet_head.CreateRow(sheet_head_index++);
            for (int i = 0; i < row.Length; i++)
                sheet_head_row.CreateCell(i).SetCellValue(row[i]);
            return sheet_head_index;
        }

        public static void ExportLogXContainerData(CommandLineArguments ParsedCommandLineArguments, logXContainer _ret)
        {
            bool _skipRelativeTimeUpdate = false; 
            if (ParsedCommandLineArguments.Flags.Contains("SKIPRELATIVETIMEUPDATE")  || ParsedCommandLineArguments.RelativeTime)
                _skipRelativeTimeUpdate = true;

            if (!_skipRelativeTimeUpdate)
                _ret.UpdateRelativeTimes();

            _ret.CreateLookup();

            // Export

            if (ParsedCommandLineArguments.Transform_OutputStata.Trim() != "")
            {
                if (ParsedCommandLineArguments.Verbose)
                    Console.WriteLine(" - Create ZIP archive with Stata file(s).");

                _ret.ExportStata(ParsedCommandLineArguments);
            }

            if (ParsedCommandLineArguments.Transform_OutputSPSS.Trim() != "")
            {
                if (ParsedCommandLineArguments.Verbose)
                    Console.WriteLine(" - Create ZIP archive with SPSS file(s).");

                _ret.ExportSPSS(ParsedCommandLineArguments);
            }

            if (ParsedCommandLineArguments.Transform_OutputXLSX.Trim() != "")
            {
                if (ParsedCommandLineArguments.Verbose)
                    Console.WriteLine(" - Create XLSX file.");

                _ret.ExportXLSX(ParsedCommandLineArguments); 
            }

            if (ParsedCommandLineArguments.Transform_OutputXES.Trim() != "")
            {
                if (ParsedCommandLineArguments.Verbose)
                    Console.WriteLine(" - Create XES file.");

                _ret.ExportXES(ParsedCommandLineArguments); 
            }

            if (ParsedCommandLineArguments.Transform_OutputZCSV.Trim() != "")
            {
                if (ParsedCommandLineArguments.Verbose)
                    Console.WriteLine(" - Create ZIP archive with CSV file(s).");

                _ret.ExportCSV(ParsedCommandLineArguments);
            }

            if (ParsedCommandLineArguments.Transform_Codebook.Trim() != "")
            {
                if (ParsedCommandLineArguments.Verbose)
                    Console.WriteLine(" - Create Codebook File.");

                _ret.CreateCodebook(ParsedCommandLineArguments);
            }

            if (_ret.ExportErrors.Count > 0)
            {
                Console.WriteLine(_ret.ExportErrors.Count + " error(s) creating output files.");
                if (ParsedCommandLineArguments.Verbose)
                {
                    for (int i = 0; i < _ret.ExportErrors.Count; i++)
                    {
                        Console.WriteLine(_ret.ExportErrors[i]);
                    } 
                }
            }
        }
    }

    public class logxResultDataRow
    {
        public long PersonIdentifier { get; set; }

        public List<Tuple<int, int>> AttributValues { get; set; }
    }

    public class logxLogDataRow
    {
        public long PersonIdentifier { get; set; }
        public int Element { get; set; }
        public DateTime TimeStamp { get; set; }
        public TimeSpan RelativeTime { get; set; }
        public int ParentEventID { get; set; }
        public int EventID { get; set; }
        public int Path { get; set; }
        public int ParentPath { get; set; }
        public int EventName { get; set; }
        public List<Tuple<int, int>> AttributValues { get; set; }

        public List<logxLogDataRow> ChildEvents { get; set; } 
    }

    public class logxGenericResultElement
    {
        public string PersonIdentifier { get; set; }

        public Dictionary<string, object>  Results { get; set; }

        public logxGenericResultElement()
        {
            Results = new Dictionary<string, object>();
        }

    }

    public class logxGenericLogElement
    {
        public string PersonIdentifier { get; set; }
        public DateTime TimeStamp { get; set; }
        public double RelativeTime { get; set; }
        public int EventID { get; set; }
        public string Item { get; set; }
        public string EventName { get; set; }
        public string EventDataXML { get; set; }
   
        public logxGenericLogElement()
        {
            EventDataXML = "";
        }
    }

    #region Codebook

    public class logxCodebookDictionary
    {
        public Dictionary<string,logxCodebookMetaData> MetaData { get; set; }
        public Dictionary<string, logxCodebookHead> Heads { get; set; }
        public Dictionary<string, logxCodebookEvent> Events { get; set; }
        public Dictionary<string, logxCodebookAttribute> Attributes { get; set; }

        public Dictionary<string, logxCodebookResultVariable> ResultVariables { get; set; }

        public logxCodebookDictionary()
        {
            MetaData = new Dictionary<string, logxCodebookMetaData>();
            Heads = new Dictionary<string, logxCodebookHead>();
            Events = new Dictionary<string, logxCodebookEvent>();
            Attributes = new Dictionary<string, logxCodebookAttribute>();
            ResultVariables = new Dictionary<string, logxCodebookResultVariable>();
        }

        public void LoadDictionaryExcelSheet(string CodebookDictionaryFile)
        {
            MetaData = new Dictionary<string, logxCodebookMetaData>();
            Heads = new Dictionary<string, logxCodebookHead>();
            Events = new Dictionary<string, logxCodebookEvent>();
            Attributes = new Dictionary<string, logxCodebookAttribute>();

            try
            {
                if (File.Exists(CodebookDictionaryFile))
                {
                    IWorkbook workbook = WorkbookFactory.Create(CodebookDictionaryFile);

                    #region Read Metadata
                    int _metaDataSheetIndex = workbook.GetSheetIndex("MetaData");
                    if (_metaDataSheetIndex != -1)
                    {
                        var _sheet = workbook.GetSheetAt(_metaDataSheetIndex);
                        if (_sheet.LastRowNum >= 1)
                        {
                            logxCodebookColumnNames cn = new logxCodebookColumnNames(_sheet.GetRow(0));
                            for (int rowIndex = 1; rowIndex <= _sheet.LastRowNum; rowIndex++)
                            {
                                IRow row = _sheet.GetRow(rowIndex);
                                string attribute = cn.GetValue(row, "Attribute");
                                MetaData.Add(attribute, new logxCodebookMetaData()
                                {
                                    Attribute = attribute,
                                    ValuesByLanguage = cn.GetValueByLanguage(row, "AttributeValue")
                                }); 
                            }
                        } 
                    }
                    #endregion

                    #region Read Head
                    int _headSheetIndex = workbook.GetSheetIndex("Head");
                    if (_headSheetIndex != -1)
                    {
                        var _sheet = workbook.GetSheetAt(_headSheetIndex);
                        if (_sheet.LastRowNum >= 1)
                        {
                            logxCodebookColumnNames cn = new logxCodebookColumnNames(_sheet.GetRow(0));
                            for (int rowIndex = 1; rowIndex <= _sheet.LastRowNum; rowIndex++)
                            {
                                IRow row = _sheet.GetRow(rowIndex);
                                string column = cn.GetValue(row, "Column");
                                Heads.Add(column, new logxCodebookHead()
                                {
                                    Column = column,
                                    Identifies = cn.GetValue(row, "Identifies"),
                                    Table = cn.GetValue(row, "Table"),
                                    VariableLabelByLanguage = cn.GetValueByLanguage(row, "VariableLabel")
                                });
                        }
                        }
                    }
                    #endregion
                    
                    #region Read Event
                    int _enventSheetIndex = workbook.GetSheetIndex("Events");
                    if (_enventSheetIndex != -1)
                    {
                        var _sheet = workbook.GetSheetAt(_enventSheetIndex);
                        if (_sheet.LastRowNum >= 1)
                        {
                            logxCodebookColumnNames cn = new logxCodebookColumnNames(_sheet.GetRow(0));
                            for (int rowIndex = 1; rowIndex <= _sheet.LastRowNum; rowIndex++)
                            {
                                IRow row = _sheet.GetRow(rowIndex);
                                string key = cn.GetValue(row, "Table");
                                Events.Add(key, new logxCodebookEvent()
                                {
                                    EventName = cn.GetValue(row, "EventName"),
                                    Table = cn.GetValue(row, "Table"),
                                    EventDescriptionByLanguage = cn.GetValueByLanguage(row, "EventDescription")
                                });
                            }
                        }
                    }
                    #endregion
 
                    #region Read Attributes
                    int _attributesSheetIndex = workbook.GetSheetIndex("Attributes");
                    if (_attributesSheetIndex != -1)
                    {
                        var _sheet = workbook.GetSheetAt(_attributesSheetIndex);
                        if (_sheet.LastRowNum >= 1)
                        {
                            logxCodebookColumnNames cn = new logxCodebookColumnNames(_sheet.GetRow(0));
                            for (int rowIndex = 1; rowIndex <= _sheet.LastRowNum; rowIndex++)
                            {
                                IRow row = _sheet.GetRow(rowIndex);
                                string key = cn.GetValue(row, "Table") + cn.GetValue(row, "Column");

                                if (!Attributes.ContainsKey(key))
                                {
                                    Attributes.Add(key, new logxCodebookAttribute()
                                    {
                                        TableColumn = key,
                                        Conditions = new List<logxCodebookAttributeCondition>()
                                    });
                                }

                                var _condition = new logxCodebookAttributeCondition()
                                {
                                    Table = cn.GetValue(row, "Table"),
                                    Condition = cn.GetValue(row, "Condition"),
                                    Column = cn.GetValue(row, "Column "),
                                    Anonymity = cn.GetValue(row, "Anonymity"),
                                    Purification = cn.GetValue(row, "Purification"),
                                    AttributeDescriptionByLanguage = cn.GetValueByLanguage(row, "Description")
                                };

                                Attributes[key].Conditions.Add(_condition);
                            }
                        }
                    }
                    #endregion

                    #region Read Variables

                    int _resultVariablesSheetIndex = workbook.GetSheetIndex("Results Variables");
                    if (_resultVariablesSheetIndex != -1)
                    {
                        var _sheet = workbook.GetSheetAt(_resultVariablesSheetIndex);
                        logxCodebookColumnNames cn = new logxCodebookColumnNames(_sheet.GetRow(0));

                        if (_sheet.LastRowNum >= 1)
                        {
                            for (int rowIndex = 1; rowIndex <= _sheet.LastRowNum; rowIndex++)
                            {
                                IRow row = _sheet.GetRow(rowIndex);
                                string key = cn.GetValue(row, "Key");
                                string name = cn.GetValue(row, "Variable");
                                var label = cn.GetValueByLanguage(row, "Label");
                                bool ignored  = bool.Parse(cn.GetValue(row, "Ignore"));
                                 
                                ResultVariables.Add(key, new logxCodebookResultVariable() { VariableKey = key, Name = name, ValuesByLanguage = label, Ignored = ignored });
                            }
                        }
                    }

                        //ResultVariables

                        #endregion
                    }
            } 
            catch (Exception _ex)
            {
                Console.WriteLine("Error reading codebook dictionary:  " + _ex.ToString());
            }
         }

        public string GetMetaData(string Attribute, string Default, string Language)
        {
            if (MetaData.ContainsKey(Attribute))
            {
                if (MetaData[Attribute].ValuesByLanguage.ContainsKey(Language))
                {
                    return MetaData[Attribute].ValuesByLanguage[Language];
                }
            }
            return Default;
        }

        public string[] GetMetaDataLine(string Attribute, string AttributeValue, string[] Language)
        {
            string[] _ret = new string[1+Language.Length];
            _ret[0] = Attribute;
            for (int i=0; i< Language.Length; i++)
            {
                _ret[1 + i] = AttributeValue;

                if (MetaData.ContainsKey(Attribute))
                {
                    if (MetaData[Attribute].ValuesByLanguage.ContainsKey(Language[i]))
                    {
                        _ret[1+i] = MetaData[Attribute].ValuesByLanguage[Language[i]];
                    }
                }
            }

       
            return _ret;
        }
        
        public string GetColumnNameDescription(string Column, string Language)
        { 
            if (Heads.ContainsKey(Column))
            {
                if (Heads[Column].VariableLabelByLanguage.ContainsKey(Language))
                    return Heads[Column].VariableLabelByLanguage[Language];
            }
            else
            {
                switch (Column)
                {
                    case "Line": 
                        return "Line counter (counter over all cases and events)";
                    case "PersonIdentifier":
                        return "ID of the person which triggered the event data in this row";
                    case "Element":
                        return "Item or page name (source of the event data in this row)";
                    case "TimeStamp":
                        return "Time stamp for the event data in this line";
                    case "RelativeTime":
                        return "Relative time for the log event (milliseconds relative to the start)";
                    case "ParentEventID":
                        return "ID of the parent event (used for the nested data structures of an event)";
                    case "Path":
                        return "Hierarchy of the nested data structure";
                    case "ParentPath":
                        return "Hierarchy of the parent element (empty for the elements at the root level)";
                    case "EventName":
                        return "ID for this line (counter over all cases and events)";
                }
            }  
            return Column;
           
        }

        public string GetColumnNameIdentifies(string Column)
        {
            
            if (Heads.ContainsKey(Column))
            {
                return Heads[Column].Identifies;
            }
            else
            {
                switch (Column)
                {
                    case "Line": 
                        return "line";
                    case "PersonIdentifier": 
                        return "target-person";
                    case "Element": 
                        return "instrument-part";
                    case "TimeStamp": 
                        return "assessment-time";
                    case "RelativeTime":
                        return "-";
                    case "ParentEventID": 
                        return "event";
                    case "Path":
                        return "table";
                    case "ParentPath":
                        return "table"; 
                    case "EventName": 
                        return "-";
 
                }
            }
          
            return Column;
        }

        public string[] GetHeadLine(string Column, string[] Language)
        {
            string[] _ret = new string[2 + Language.Length];
            _ret[0] = Column;
            for (int i = 0; i < Language.Length; i++)
            {
                _ret[1 + i] = GetColumnNameDescription(Column, Language[i]);
            }
            _ret[1 + Language.Length] = GetColumnNameIdentifies(Column);
            return _ret;
        }

        public string GetAttributeDescrition(string Table, string Column, string Language, int MaxLength)
        {
            string _ret = GetAttributeDescrition(Table, Column, Language);
            if (_ret.Length >= MaxLength)
                _ret = _ret.Substring(0, MaxLength) + " ...";

            return _ret;
        }

        public string GetResultVariableLabel(string Key, string Language, int MaxLength)
        {
            if (ResultVariables.ContainsKey(Key))
            {
                if (ResultVariables[Key].ValuesByLanguage.ContainsKey(Language))
                {
                    string _val = ResultVariables[Key].ValuesByLanguage[Language];
                    if (_val.Length > MaxLength)
                    {
                        return _val.Substring(0, MaxLength);
                    }
                    else
                    {
                        return _val;
                    }

                }
            }
            return Key;
        }

        public string GetResultVariableName(string Key)
        {
            if (ResultVariables.ContainsKey(Key))
            {
                return ResultVariables[Key].Name;
            }

            return Key;
        }

        public bool IgnoreResultVariable(string Key)
        {
            if (ResultVariables.ContainsKey(Key))
            {  
                return ResultVariables[Key].Ignored;
            }

            return false;

        }
        
        public string GetAttributeDescrition(string Table, string Column, string Language)
        {
            // Hack: Use first condition
            if (Attributes.ContainsKey(Table + Column))
            {
                foreach (var l in Attributes[Table + Column].Conditions)
                {  
                    if (l.AttributeDescriptionByLanguage.ContainsKey(Language))
                    {
                        return l.AttributeDescriptionByLanguage[Language];
                    }
                }
            }
            return Column;
        }

        public List<string[]> GetConditions(string Table, string Column, string[] Language)
        {
            List<string[]> _ret = new List<string[]>();
            if (Attributes.ContainsKey(Table + Column))
            { 
                foreach (var l in Attributes[Table + Column].Conditions)
                {
                    string[] _line = new string[5 + Language.Length];
                    _line[0] = Table;
                    _line[1] = Column;
                    _line[2] = l.Condition;
                    for (int i=0;i<Language.Length; i++)
                    {
                        if (l.AttributeDescriptionByLanguage.ContainsKey(Language[i]))
                            _line[3+i] = l.AttributeDescriptionByLanguage[Language[i]];
                        else
                            _line[3 + i] = "";
                    }
                    _line[3 + Language.Length] = l.Anonymity;
                    _line[4 + Language.Length] = l.Purification;

                    _ret.Add(_line);
                }
            }
            if (_ret.Count == 0)
                _ret.Add(new string[] { Table, Column, "-", "", "-", "-"});

            return _ret;            
        }

        public List<string[]> GetEvent(string TableName, string[] Language)
        {
            List<string[]> _ret = new List<string[]>();
             
            if (Events.ContainsKey(TableName))
            {
                string[] _l = new string[2 + Language.Length];
                _l[0] = Events[TableName].EventName;
                _l[1] = TableName;
                
                for (int i=0; i<Language.Length; i++)
                {
                    if (Events[TableName].EventDescriptionByLanguage.ContainsKey(Language[i]))
                        _l[2 + i] = Events[TableName].EventDescriptionByLanguage[Language[i]];
                }
                _ret.Add(_l);
            }
            return _ret;
        }

    }

    public class logxCodebookMetaData
    {
        public string Attribute { get; set; }
        public Dictionary<string,string> ValuesByLanguage { get; set; }
    }

    public class logxCodebookResultVariable
    {
        public string VariableKey { get; set; }
        public bool Ignored { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> ValuesByLanguage { get; set; }
    }

    public class logxCodebookHead
    {
        public string Table{ get; set; }
        public string Column { get; set; }
        public string Identifies { get; set; }
        public Dictionary<string, string> VariableLabelByLanguage { get; set; }
    }

    public class logxCodebookEvent
    {
        public string EventName { get; set; }
        public string Table { get; set; } 
        public Dictionary<string, string> EventDescriptionByLanguage { get; set; }
    }

    public class logxCodebookAttribute
    {
        public string TableColumn { get; set; }

        public List<logxCodebookAttributeCondition> Conditions { get; set; }
        public logxCodebookAttribute()
        {
            Conditions = new List<logxCodebookAttributeCondition>();
        }

    }

    public class logxCodebookAttributeCondition
    {
        public string Table { get; set; }
        public string Column { get; set; }
        public string Condition { get; set; }
        public string Anonymity { get; set; }
        public string Purification { get; set; }
        public Dictionary<string, string> AttributeDescriptionByLanguage { get; set; }
    }

    public class logxCodebookColumnNames
    {
        public Dictionary<string, int> colNameDict { get; set; }
        public logxCodebookColumnNames(IRow Headline)
        {
            colNameDict = new Dictionary<string, int>();
            int colIndex = 0;
            foreach (ICell cell in Headline.Cells)
            {
                colNameDict.Add(cell.StringCellValue, colIndex);
                colIndex++;
            }
        }

        public string GetValue(IRow Row, string ColumnName)
        {
            if (colNameDict.ContainsKey(ColumnName))
            {
                int _index = colNameDict[ColumnName];
                if (_index != -1 && Row.Cells.Count > _index)
                {
                    return Row.Cells[_index].StringCellValue;
                }
            }

            return ColumnName;
        }

        public Dictionary<string, string> GetValueByLanguage(IRow Row, string ColumnName)
        {
            Dictionary<string, string> _ret = new Dictionary<string, string>();
            foreach (string colName in colNameDict.Keys)
            {
                string[] colNameSplit = colName.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (colNameSplit[0] == ColumnName && colNameSplit.Length > 1)
                {
                    int _index = colNameDict[colName];
                    if (_index != -1 && Row.Cells.Count >= _index)
                    {
                        _ret.Add(colNameSplit[1], Row.Cells[_index].StringCellValue);
                    }
                } 
            }
            return _ret;
        }
    }
     
    #endregion

}
