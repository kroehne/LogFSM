namespace LogFSM_LogX2019
{
    #region usings
    using CsvHelper;
    using Ionic.Zip;
    using NPOI.SS.UserModel;
    using NPOI.XSSF.UserModel;
    using StataLib;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;
    #endregion

    public class logXContainer
    {

        public const string rootName = "Log";
        public bool PersonIdentifierIsNumber { get; set; }

        public string PersonIdentifierName { get; set; }

        public Dictionary<string, List<logxDataRow>> logDataTables { get; set; }
        public Dictionary<string, List<string>> logDataTableColnames { get; set; }

        public Dictionary<string, List<string>> uniqueValues = new Dictionary<string, List<string>>();

        public List<string> ExportErrors = new List<string>();

        public logxCodebookDictionary CodebookDictionary { get; set; }

        public Dictionary<string,string> CondordanceTable { get; set; }

        private Dictionary<string, int> maxEventIDByPerson = new Dictionary<string, int>();

        public logXContainer()
        {
            PersonIdentifierIsNumber = true;

            logDataTables = new Dictionary<string, List<logxDataRow>>();
            logDataTables.Add(rootName, new List<logxDataRow>());

            logDataTableColnames = new Dictionary<string, List<string>>();
            logDataTableColnames.Add(rootName, new List<string>());

            PersonIdentifierName = "PersonIdentifier";

            if (!uniqueValues.ContainsKey("PersonIdentifier"))
                uniqueValues.Add("PersonIdentifier", new List<string>());
            if (!uniqueValues.ContainsKey("Element"))
                uniqueValues.Add("Element", new List<string>());
            if (!uniqueValues.ContainsKey("Path"))
                uniqueValues.Add("Path", new List<string>());
            if (!uniqueValues.ContainsKey("ParentPath"))
                uniqueValues.Add("ParentPath", new List<string>());
            if (!uniqueValues.ContainsKey("EventName"))
                uniqueValues.Add("EventName", new List<string>());

            CondordanceTable = new Dictionary<string, string>();
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

            if (!uniqueValues["Element"].Contains(element.Item))
                uniqueValues["Element"].Add(element.Item);

            if (!uniqueValues["Path"].Contains(rootName))
                uniqueValues["Path"].Add(rootName);

            if (!uniqueValues["ParentPath"].Contains("(no parent)"))
                uniqueValues["ParentPath"].Add("(no parent)");

            if (!uniqueValues["EventName"].Contains(element.EventName))
                uniqueValues["EventName"].Add(element.EventName);

            long _personIdentifier = -1;
            if (!PersonIdentifierIsNumber)
            {
                if (!uniqueValues["PersonIdentifier"].Contains(element.PersonIdentifier))
                    uniqueValues["PersonIdentifier"].Add(element.PersonIdentifier);

                _personIdentifier = uniqueValues["PersonIdentifier"].IndexOf(element.PersonIdentifier);
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

            logxDataRow rootParentLine = new logxDataRow()
            {
                PersonIdentifier = _personIdentifier,
                Element = uniqueValues["Element"].IndexOf(element.Item),
                TimeStamp = element.TimeStamp,
                RelativeTime = element.RelativeTime,
                ParentEventID = -1,
                Path = uniqueValues["Path"].IndexOf(rootName),
                ParentPath = uniqueValues["ParentPath"].IndexOf("(no parent)"),
                EventID = element.EventID,
                EventName = uniqueValues["EventName"].IndexOf(element.EventName),

                AttributValues = new List<Tuple<int, int>>(),

            };

            logDataTables[rootName].Add(rootParentLine);

            if (element.EventDataXML != "")
            {
                using (TextReader tr = new StringReader(element.EventDataXML))
                {
                    XDocument doc = XDocument.Load(tr);
                    ProcessXMLData(doc.Root, rootName, element.PersonIdentifier, rootParentLine, 0);
                    doc = null;
                }
            } 

        }

        private void ProcessXMLData(XElement xmlelement, string path, string PersonIdentifier, logxDataRow parentLine, int id)
        {
            if (path == rootName)
                path = rootName + "." + xmlelement.Name.LocalName;

            int split = path.LastIndexOf(".");
            string parentPath = "(no parent)";
            if (split > 0)
                parentPath = path.Substring(0, split);

            if (!uniqueValues["Path"].Contains(path))
                uniqueValues["Path"].Add(path);

            if (!uniqueValues["ParentPath"].Contains(parentPath))
                uniqueValues["ParentPath"].Add(parentPath);

            logxDataRow newChildLine = new logxDataRow()
            {
                PersonIdentifier = parentLine.PersonIdentifier,
                Element = parentLine.Element,
                TimeStamp = parentLine.TimeStamp,
                RelativeTime = parentLine.RelativeTime,
                ParentEventID = parentLine.EventID,
                EventID = id,
                Path = uniqueValues["Path"].IndexOf(path),
                ParentPath = uniqueValues["ParentPath"].IndexOf(parentPath),
                EventName = parentLine.EventName,
                AttributValues = new List<Tuple<int, int>>()
            };


            if (!logDataTables.ContainsKey(path))
            {
                logDataTables.Add(path, new List<logxDataRow>());
            }

            logDataTables[path].Add(newChildLine);

            if (!xmlelement.HasElements && xmlelement.Value.Trim() != "")
                AddValueToTable(path, xmlelement.Name.LocalName, xmlelement.Value, newChildLine);
             
            foreach (var a in xmlelement.Attributes())
                AddValueToTable(path, a.Name.LocalName, a.Value, newChildLine);

            int i = 0;
            foreach (XElement x in xmlelement.Elements())
            {
                ProcessXMLData(x, path + "." + x.Name.LocalName, PersonIdentifier, newChildLine, i);
                i++;
            }

        }

        private void AddValueToTable(string path, string name, string value, logxDataRow row)
        {
            string _localPath = name;
            if (path.Trim() != "")
                _localPath = path + "." + _localPath;

            if (!uniqueValues.ContainsKey(name))
                uniqueValues.Add(name, new List<string>());

            if (!uniqueValues[name].Contains(value))
                uniqueValues[name].Add(value);

            if (!logDataTableColnames.ContainsKey(path))
                logDataTableColnames.Add(path, new List<string>());

            if (!logDataTableColnames[path].Contains(name))

                logDataTableColnames[path].Add(name);

            row.AttributValues.Add(new Tuple<int, int>(logDataTableColnames[path].IndexOf(name), uniqueValues[name].IndexOf(value)));

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
                    double _tmp = (v.TimeStamp - _startByPersonIdentifier[v.PersonIdentifier]).TotalMilliseconds;
                    v.RelativeTime = (long)_tmp;
                }

                logDataTables[_id].Sort((x, y) =>
                {
                    int result = decimal.Compare(x.PersonIdentifier, y.PersonIdentifier);
                    if (result == 0)
                        result = decimal.Compare(x.RelativeTime, y.RelativeTime);
                    return result;
                });
            }


        }

        public void ExportStata(string filename, string language)
        {

            DateTime dt1960 = new DateTime(1960, 1, 1, 0, 0, 0, 0);

            using (ZipFile zip = new ZipFile())
            {
                foreach (string _id in logDataTables.Keys)
                {
                    var _varlist = new List<StataVariable>();
                    _varlist.Add(new StataVariable() { Name = "ID", VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%12.0g", Description = CodebookDictionary.GetColumnNameDescription("ID", language) });

                    if (PersonIdentifierIsNumber)
                        _varlist.Add(new StataVariable() { Name = PersonIdentifierName, VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("PersonIdentifier", language) });
                    else
                        _varlist.Add(new StataVariable() { Name = PersonIdentifierName, VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("PersonIdentifier", language), ValueLabelName = "l_" + "PersonIdentifier" });

                    _varlist.Add(new StataVariable() { Name = "Element", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("Element", language), ValueLabelName = "l_" + "Element" });
                    _varlist.Add(new StataVariable() { Name = "TimeStamp", VarType = StataVariable.StataVarType.Double, DisplayFormat = @"%tcMonth_dd,_CCYY_HH:MM:SS.sss", Description = CodebookDictionary.GetColumnNameDescription("TimeStamp", language) , ValueLabelName = "l_" + "Timestamp" });
                    _varlist.Add(new StataVariable() { Name = "RelativeTime", VarType = StataVariable.StataVarType.Double, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("RelativeTime", language), ValueLabelName = "l_" + "RelativeTime" });

                    _varlist.Add(new StataVariable() { Name = "EventID", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("EventID", language) });
                    _varlist.Add(new StataVariable() { Name = "ParentEventID", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("ParentEventID", language), ValueLabelName = "l_" + "ParentEventID" });
                    _varlist.Add(new StataVariable() { Name = "Path", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%40.0g", Description = CodebookDictionary.GetColumnNameDescription("Path", language), ValueLabelName = "l_" + "Path" });
                    _varlist.Add(new StataVariable() { Name = "ParentPath", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%40.0g", Description = CodebookDictionary.GetColumnNameDescription("ParentPath", language), ValueLabelName = "l_" + "ParentPath" });
                    _varlist.Add(new StataVariable() { Name = "EventName", VarType = StataVariable.StataVarType.Int, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetColumnNameDescription("EventName", language), ValueLabelName = "l_" + "EventName" });
                  
                    // Add attribute variables as string, if number of characters exceeds 32000 characters 

                    List<string> listOfLongStringVariableNames = new List<string>();
                    foreach (var k in uniqueValues.Keys)
                    {
                       foreach (var c in uniqueValues[k])
                        {
                            if (c == null)
                                continue;

                            if (c.Length >= 3200)
                            {
                                listOfLongStringVariableNames.Add(k);
                                break;
                            }                             
                        }                        
                    }
                       
                    if (logDataTableColnames.ContainsKey(_id))
                    {
                        foreach (var _colname in logDataTableColnames[_id])
                        {
                            if (listOfLongStringVariableNames.Contains(_colname))
                            {  
                                _varlist.Add(new StataVariable() { Name = "a_" + _colname, VarType = StataVariable.StataVarType.String, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetAttributeDescrition(_id, "a_" + _colname, language, 64)});
                            }
                            else
                            {
                                _varlist.Add(new StataVariable() { Name = "a_" + _colname, VarType = StataVariable.StataVarType.Long, DisplayFormat = @"%20.0g", Description = CodebookDictionary.GetAttributeDescrition(_id, "a_" + _colname, language, 64), ValueLabelName = "l_" + _colname });
                            }

                        }
                    }

                    string _tmpfile = GetTempFileName("dta");
                    string _dataSetName = CodebookDictionary.GetMetaData("StudyName", "", language);
                    
                    var _dtaFile = new StataFileWriter(_tmpfile, _varlist, _dataSetName, true);

                    int id = 0;
                    foreach (var v in logDataTables[_id])
                    {

                        object[] _line = new object[_varlist.Count];

                        _line[0] = id;
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
                        _line[4] = v.RelativeTime;
                        _line[5] = v.EventID;
                        _line[6] = v.ParentEventID;
                        _line[7] = v.Path;
                        _line[8] = v.ParentPath;
                        _line[9] = v.EventName;

                        if (logDataTableColnames.ContainsKey(_id))
                        {
                            for (int _i = 0; _i < logDataTableColnames[_id].Count; _i++)
                            {
                                if (listOfLongStringVariableNames.Contains(logDataTableColnames[_id][_i]))
                                 { 
                                    string _value = "";
                                    foreach (var p in v.AttributValues)
                                    {
                                        if (p.Item1 == _i)
                                        {
                                            _value = uniqueValues[logDataTableColnames[_id][_i]][p.Item2];
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
                        id++;
                    }

                    if (!PersonIdentifierIsNumber)
                    {
                        for (int _i = 0; _i < uniqueValues["PersonIdentifier"].Count; _i++)
                            _dtaFile.AddValueLabel("l_" + "PersonIdentifier", _i, uniqueValues["PersonIdentifier"][_i]);
                    }

                    _dtaFile.AddValueLabel("l_" + "ParentEventID", -1, "(no parent)");
                    _dtaFile.AddValueLabel("l_" + "Timestamp", -1, "(missing)");

                    foreach (string _labelSetName in new string[] { "Element", "Path", "ParentPath", "EventName" })
                    {
                        for (int _i = 0; _i < uniqueValues[_labelSetName].Count; _i++)
                            _dtaFile.AddValueLabel("l_" + _labelSetName, _i, uniqueValues[_labelSetName][_i]);
                    }

                    if (logDataTableColnames.ContainsKey(_id))
                    {
                        foreach (var _colname in logDataTableColnames[_id])
                        {
                            if (!listOfLongStringVariableNames.Contains(_colname))
                            { 
                                for (int _i = 0; _i < uniqueValues[_colname].Count; _i++)
                                    _dtaFile.AddValueLabel("l_" + _colname, _i, uniqueValues[_colname][_i]);

                                _dtaFile.AddValueLabel("l_" + _colname, -1, "(attribute not defined)");
                            }                           
                        }
                    }

                    _dtaFile.Close();
                    zip.AddFile(_tmpfile).FileName = _id + ".dta";

                }
                
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.Save(filename);
            }
        }

        public void ExportCSV(string filename)
        {
            DateTime dt1960 = new DateTime(1960, 1, 1, 0, 0, 0, 0);
            string _sep = ";";

            using (ZipFile zip = new ZipFile())
            {

                foreach (string _id in logDataTables.Keys)
                {
                    string _tmpfile = GetTempFileName("csv");
                    using (StreamWriter sw = new StreamWriter(_tmpfile))
                    {
                        sw.Write("ID" + _sep + "PersonIdentifier" + _sep + "Element" + _sep + "TimeStamp" + _sep + "RelativeTime" + _sep + "EventID" + _sep + "ParentEventID" + _sep + "Path" + _sep + "ParentPath" + _sep + "EventName");
                        if (logDataTableColnames.ContainsKey(_id))
                        {
                            foreach (var _colname in logDataTableColnames[_id])
                                sw.Write(_sep + "a_" + _colname);
                        }
                        sw.WriteLine();

                        int id = 0;
                        foreach (var v in logDataTables[_id])
                        {
                            sw.Write(id);
                            sw.Write(_sep + StringToCSVCell(uniqueValues["PersonIdentifier"][(int)v.PersonIdentifier]));
                            sw.Write(_sep + StringToCSVCell(uniqueValues["Element"][v.Element]));
                            sw.Write(_sep + StringToCSVCell(v.TimeStamp.ToString())); // TODO: Format Statement
                            sw.Write(_sep + StringToCSVCell(v.RelativeTime.ToString()));
                            sw.Write(_sep + StringToCSVCell(v.EventID.ToString()));
                            sw.Write(_sep + StringToCSVCell(v.ParentEventID.ToString()));
                            sw.Write(_sep + StringToCSVCell(uniqueValues["Path"][v.Path]));
                            sw.Write(_sep + StringToCSVCell(uniqueValues["ParentPath"][v.ParentPath]));
                            sw.Write(_sep + StringToCSVCell(uniqueValues["EventName"][v.EventName]));

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
                                    {
                                        sw.Write(_sep + StringToCSVCell("(attribute not defined)"));
                                    }
                                    else
                                    {
                                        sw.Write(_sep + StringToCSVCell(uniqueValues[logDataTableColnames[_id][_i]][_value]));
                                    }
                                }
                            }

                            id++;
                            sw.WriteLine();
                        }
                    }

                    zip.AddFile(_tmpfile).FileName = _id + ".csv";

                }
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.Save(filename);
            }
        }

        public void ExportXLSX(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                Dictionary<string, string> _sheetIndex = new Dictionary<string, string>();

                IWorkbook workbook = new XSSFWorkbook();
                foreach (string _id in logDataTables.Keys)
                {
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
                    firstrow.CreateCell(0).SetCellValue("ID");
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
                            row.CreateCell(1).SetCellValue(uniqueValues["PersonIdentifier"][(int)v.PersonIdentifier]);
                        }
                      
                        row.CreateCell(2).SetCellValue(uniqueValues["Element"][v.Element]);
                        row.CreateCell(3).SetCellValue(v.TimeStamp.ToString());
                        row.CreateCell(4).SetCellValue(v.RelativeTime.ToString());
                        row.CreateCell(5).SetCellValue(v.EventID.ToString());
                        row.CreateCell(6).SetCellValue(v.ParentEventID.ToString());
                        row.CreateCell(7).SetCellValue(uniqueValues["Path"][v.Path]);
                        row.CreateCell(8).SetCellValue(uniqueValues["ParentPath"][v.ParentPath]);
                        row.CreateCell(9).SetCellValue(uniqueValues["EventName"][v.EventName]);


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
                                    row.CreateCell(_colIndex++).SetCellValue("(attribute not defined)");
                                }
                                else
                                {
                                    string _stringValue = uniqueValues[logDataTableColnames[_id][_i]][_value];
                                    if (_stringValue.Length > 32766)
                                    {
                                        
                                        if (PersonIdentifierIsNumber)
                                        {
                                            ExportErrors.Add("- XLSX: Shortened string of length " + _stringValue.Length + " for person identifier '" + v.PersonIdentifier + "', in table  '" + _id + "', for column '" + logDataTableColnames[_id][_i] + "'");
                                        }
                                        else
                                        {
                                            ExportErrors.Add("- XLSX: Shortened string of length " + _stringValue.Length + " for person identifier '" + uniqueValues["PersonIdentifier"][(int)v.PersonIdentifier] + "', in table  '" + _id + "', for column '" + logDataTableColnames[_id][_i] + "'");
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
                workbook.Write(fs);
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

        public void ReadConcordanceTable(string filename)
        {
            if (filename.ToLower().EndsWith(".dta"))
            {
                // STATA 

                StataFileReader _stataLogFileReader = new StataFileReader(filename, true);
                foreach (var _line in _stataLogFileReader)
                {
                    if (!CondordanceTable.ContainsKey(_line[0].ToString()))
                        CondordanceTable.Add(_line[0].ToString(), _line[1].ToString());
                }
            }
            else if (filename.ToLower().EndsWith(".xlsx"))
            {
                // XLSX

                IWorkbook workbook = WorkbookFactory.Create(filename);
                int _concordanceTableSheetIndex = workbook.GetSheetIndex("ConcordancTable");
                if (_concordanceTableSheetIndex != -1)
                {
                    var _sheet = workbook.GetSheetAt(_concordanceTableSheetIndex);
                    if (_sheet.LastRowNum >= 1)
                    {
                        for (int rowIndex = 1; rowIndex <= _sheet.LastRowNum; rowIndex++)
                        {
                            IRow row = _sheet.GetRow(rowIndex);
                            if (row.Cells.Count > 1)
                            {
                                if (!CondordanceTable.ContainsKey(row.Cells[0].StringCellValue))
                                    CondordanceTable.Add(row.Cells[0].StringCellValue, row.Cells[1].StringCellValue);
                            } 
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
                            CondordanceTable.Add(csv.GetField(0), csv.GetField(1));
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

                    _line[0] = uniqueValues["PersonIdentifier"][_i].ToString();
                    _line[1] = uniqueValues["PersonIdentifier"][_i].ToString();
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
                        sheet_concordance_index = addRowValues(sheet_concordance, sheet_concordance_index, new string[] { uniqueValues["PersonIdentifier"][_i].ToString(), uniqueValues["PersonIdentifier"][_i].ToString() });
                    }
                    workbook.Write(fs);
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
                        var r = new List<object> { new { OldPersonIdentifier = uniqueValues["PersonIdentifier"][_i].ToString(), NewPersonIdentifier = uniqueValues["PersonIdentifier"][_i].ToString() }, };
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
        public void CreateCodebook(string filename, string language)
        { 
            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            { 
                IWorkbook workbook = new XSSFWorkbook();

                #region MetaData
                ISheet sheet_metadata = workbook.CreateSheet("MetaData");

                int sheet_metadata_index = addRowValues(sheet_metadata, 0, new string[] { "Attribute", "AttributeValue" });
                sheet_metadata_index = addRowValues(sheet_metadata, sheet_metadata_index, CodebookDictionary.GetMetaDataLine("StudyName", "StudyName", language));
                sheet_metadata_index = addRowValues(sheet_metadata, sheet_metadata_index, CodebookDictionary.GetMetaDataLine("TestPlatform", "TestPlatform", language));

                #endregion
 
                #region Head
                ISheet sheet_head = workbook.CreateSheet("Head");
                  
                int sheet_head_index = addRowValues(sheet_head, 0, new string[] { "Column", "Description", "Identifier" });
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("ID", language) );
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("PersonIdentifier", language));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("Element", language));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("TimeStamp",  language));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("RelativeTime",  language));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("ParentEventID",  language));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("Path", language));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("ParentPath",  language));
                sheet_head_index = addRowValues(sheet_head, sheet_head_index, CodebookDictionary.GetHeadLine("EventName", language));

                #endregion

                #region Attributes
                ISheet sheet_data = workbook.CreateSheet("Attributes");
                int sheet_data_index = addRowValues(sheet_data, 0, new string[] { "Table", "Column", "Condition", "Description", "Anonymity", "Purification" });

                foreach (string _id in logDataTables.Keys)
                {
                    if (logDataTableColnames.ContainsKey(_id))
                    {
                        foreach (var _colname in logDataTableColnames[_id])
                        { 
                            List<string[]> _conditions = CodebookDictionary.GetConditions(_id, "a_" + _colname, language);
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

                int sheet_events_index = addRowValues(sheet_events, 0, new string[] { "EventName", "Table", "EventDescription" });

                foreach (string tab in logDataTables.Keys)
                {
                    var _res = CodebookDictionary.GetEvent(tab, language);
                    foreach (var _r in _res)
                        sheet_events_index = addRowValues(sheet_events, sheet_events_index, _r );
                }
              

             
                
                #endregion

                workbook.Write(fs);
            }
        }

        private static int addRowValues(ISheet sheet_head, int sheet_head_index, string[] row)
        {
            IRow sheet_head_row = sheet_head.CreateRow(sheet_head_index++);
            for (int i = 0; i < row.Length; i++)
                sheet_head_row.CreateCell(i).SetCellValue(row[i]);
            return sheet_head_index;
        }
    }

    public class logxDataRow
    {
        public long PersonIdentifier { get; set; }
        public int Element { get; set; }
        public DateTime TimeStamp { get; set; }
        public long RelativeTime { get; set; }
        public int ParentEventID { get; set; }
        public int EventID { get; set; }
        public int Path { get; set; }
        public int ParentPath { get; set; }
        public int EventName { get; set; }
        public List<Tuple<int, int>> AttributValues { get; set; }
    }

    public class logxGenericLogElement
    {
        public string PersonIdentifier { get; set; }
        public DateTime TimeStamp { get; set; }
        public long RelativeTime { get; set; }
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
        public logxCodebookDictionary()
        {
            MetaData = new Dictionary<string, logxCodebookMetaData>();
            Heads = new Dictionary<string, logxCodebookHead>();
            Events = new Dictionary<string, logxCodebookEvent>();
            Attributes = new Dictionary<string, logxCodebookAttribute>();
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
                                    VariableLableByLanguage = cn.GetValueByLanguage(row, "VariableLable")
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
                } 
            } 
            catch
            {
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

        public string[] GetMetaDataLine(string Attribute, string AttributeValue, string Language)
        {
            string[] _ret = new string[2] { Attribute, AttributeValue };

            if (MetaData.ContainsKey(Attribute))
            {
                if (MetaData[Attribute].ValuesByLanguage.ContainsKey(Language))
                {
                    _ret[1] = MetaData[Attribute].ValuesByLanguage[Language];
                }
            }
            return _ret;
        }
        
        public string GetColumnNameDescription(string Column, string Language)
        { 
            if (Heads.ContainsKey(Column))
            {
                if (Heads[Column].VariableLableByLanguage.ContainsKey(Language))
                    return Heads[Column].VariableLableByLanguage[Language];
            }
            else
            {
                switch (Column)
                {
                    case "ID": 
                        return "ID for this line (counter over all cases and events)";
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

        public string GetColumnNameIdentifies(string Column, string Language)
        {
            
            if (Heads.ContainsKey(Column))
            {
                return Heads[Column].Identifies;
            }
            else
            {
                switch (Column)
                {
                    case "ID": 
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
        public string[] GetHeadLine(string Column, string Language)
        {
            return new string[3] { Column, GetColumnNameDescription(Column, Language), GetColumnNameIdentifies(Column, Language) };
        }

        public string GetAttributeDescrition(string Table, string Column, string Language, int MaxLength)
        {
            string _ret = GetAttributeDescrition(Table, Column, Language);
            if (_ret.Length >= MaxLength)
                _ret = _ret.Substring(0, MaxLength) + " ...";

            return _ret;
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

        public List<string[]> GetConditions(string Table, string Column, string Language)
        {
            List<string[]> _ret = new List<string[]>();
            if (Attributes.ContainsKey(Table + Column))
            { 
                foreach (var l in Attributes[Table + Column].Conditions)
                {
                    string _description = "";
                    if (l.AttributeDescriptionByLanguage.ContainsKey(Language))
                    {
                        _description = l.AttributeDescriptionByLanguage[Language];
                    }
                    _ret.Add(new string[] { Table, Column, l.Condition, _description , l.Anonymity, l.Purification});
                }
            }
            if (_ret.Count == 0)
                _ret.Add(new string[] { Table, Column, "-", "", "-", "-"});

            return _ret;            
        }

        public List<string[]> GetEvent(string TableName, string Language)
        {
            List<string[]> _ret = new List<string[]>();
             
            if (Events.ContainsKey(TableName))
            { 
                if (Events[TableName].EventDescriptionByLanguage.ContainsKey(Language))
                {
                    _ret.Add(new string[] { Events[TableName].EventName, TableName, Events[TableName].EventDescriptionByLanguage[Language] });
                } 
                else
                {
                    _ret.Add(new string[] { "", TableName, "" });
                } 
            }
            return _ret;
        }

    }

    public class logxCodebookMetaData
    {
        public string Attribute { get; set; }
        public Dictionary<string,string> ValuesByLanguage { get; set; }
    }

    public class logxCodebookHead
    {
        public string Table{ get; set; }
        public string Column { get; set; }
        public string Identifies { get; set; }
        public Dictionary<string, string> VariableLableByLanguage { get; set; }
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
                if (_index != -1 && Row.Cells.Count >= _index)
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
                if (colNameSplit[0] == ColumnName && colNameSplit.Length > 0)
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
