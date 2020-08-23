namespace LogDataTransformer_PIAAC_R1_V01
{
    #region usings
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using LogFSM;
    using LogFSM_LogX2019;
    using LogFSMConsole;
    using System.Xml;
    using System.Xml.Linq;
    using StataLib;
    using Ionic.Zip;
    #endregion

    public class LogDataTransformer_PIAACR1_Module_V01
    {
        public static void ProcessLogFilesOnly(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {
            try
            {
                bool _personIdentifierIsNumber = false;
                if (ParsedCommandLineArguments.Flags.Contains("NUMERICPERSONIDENTIFIER"))
                    _personIdentifierIsNumber = true;

                string _personIdentifier = "PersonIdentifier";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("personidentifier"))
                    _personIdentifier = ParsedCommandLineArguments.ParameterDictionary["personidentifier"];

                string _language = "ENG";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey("language"))
                    _language = ParsedCommandLineArguments.ParameterDictionary["language"];

                if (!ParsedCommandLineArguments.RelativeTime)
                {
                    ParsedCommandLineArguments.RelativeTime = true;
                    Console.Write("Note: Changed to relative times. ");
                }

                EventDataListExtension.ESortType sort = EventDataListExtension.ESortType.ElementAndTime;
                if (ParsedCommandLineArguments.Flags.Contains("DONT_ORDER_EVENTS"))
                    sort = EventDataListExtension.ESortType.None;

                Dictionary<string, string> _lookup = LogDataTransformer_PIAACR1_Module_V01.GetPIAACR1LookupDictionary();

                #region Search Source Files

                List<string> _listOfTXTFiles = new List<string>();
                List<string> _listOfZIPArchivesWithTXTFiles = new List<string>();

                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    if (File.Exists(inFolder))
                    {
                        if (inFolder.ToLower().EndsWith(".zip"))
                        {
                            _listOfZIPArchivesWithTXTFiles.Add(inFolder);
                        }
                        else if (inFolder.ToLower().EndsWith(".txt"))
                        {
                            _listOfTXTFiles.Add(inFolder);
                        }
                    }
                    else
                    {
                        if (!Directory.Exists(inFolder))
                        {
                            if (ParsedCommandLineArguments.Verbose)
                                Console.WriteLine("Warning: Directory not exists: '" + inFolder + "'.");

                            continue;
                        }

                        var _tmpXMLFileList = Directory.GetFiles(inFolder, "*.txt", SearchOption.AllDirectories);

                        foreach (string s in _tmpXMLFileList)
                            _listOfTXTFiles.Add(s);

                        var _tmpZIPFileList = Directory.GetFiles(inFolder, "*.zip", SearchOption.AllDirectories);

                        foreach (string s in _tmpZIPFileList)
                            _listOfZIPArchivesWithTXTFiles.Add(s);
                    }

                }

                #endregion

                #region Process Source Files

                logXContainer _ret = new logXContainer() { PersonIdentifierIsNumber = _personIdentifierIsNumber, PersonIdentifierName = _personIdentifier };
                _ret.LoadCodebookDictionary(ParsedCommandLineArguments.Transform_Dictionary);
                 
                int _logcounter = 0;
                foreach (string zfilename in _listOfZIPArchivesWithTXTFiles)
                {
                    using (ZipFile zip = ZipFile.Read(zfilename))
                    {
                        foreach (var entry in zip)
                        {
                            if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                            {
                                if (ParsedCommandLineArguments.Verbose)
                                    Console.WriteLine("Info: Max number of cases reached.");
                                break;
                            }

                            // TODO: Check FitsMask
                            if (1 == 1 || CommandLineArguments.FitsMask(entry.FileName, ParsedCommandLineArguments.Mask))
                            {
                                if (ParsedCommandLineArguments.Verbose)
                                    Console.Write("Info: Read File  '" + entry.FileName + "' ");

                                using (MemoryStream zipStream = new MemoryStream())
                                {
                                    entry.ExtractWithPassword(zipStream, "");
                                    zipStream.Position = 0;
                                    try
                                    {
                                        StreamReader sr = new StreamReader(zipStream);
                                        string line = String.Empty;
                                        int _lineCounter = 0;
                                        while ((line = sr.ReadLine()) != null)
                                        {

                                            _logcounter = ReadLogDataPIAACFromLDAExportString(_logcounter, _lineCounter, line, _ret, _personIdentifier, ParsedCommandLineArguments, sort, _lookup, new string[] { });
                                            _lineCounter++;

                                            if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                                            {
                                                if (ParsedCommandLineArguments.Verbose)
                                                    Console.WriteLine("Info: Max number of cases reached.");
                                                break;
                                            }
                                        }
                                          
                                    }
                                    catch (Exception _ex)
                                    {
                                        Console.WriteLine("Error processing file '" + entry.FileName + "': " + _ex.Message);
                                        return;
                                    }
                                }

                                Console.WriteLine("ok.");
                            }

                        }

                    }

                    if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Info: Max number of cases reached.");
                        break;
                    }
                }
                 
                foreach (string txtFile in _listOfTXTFiles)
                {
                    if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Info: Max number of cases reached.");
                        break;
                    }

                    if (1 == 1 || CommandLineArguments.FitsMask(Path.GetFileName(txtFile), ParsedCommandLineArguments.Mask))
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Info: Read File  '" + Path.GetFileName(txtFile) + "' ");

                        try
                        {
                            StreamReader sr = new StreamReader(txtFile); 
                            string line = String.Empty;
                            int _lineCounter = 0;
                            while ((line = sr.ReadLine()) != null)
                            {
                                _logcounter = ReadLogDataPIAACFromLDAExportString(_logcounter, _lineCounter, line, _ret, _personIdentifier, ParsedCommandLineArguments, sort, _lookup, new string[] { });
                                _lineCounter++;

                                if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                                {
                                    if (ParsedCommandLineArguments.Verbose)
                                        Console.WriteLine("Info: Max number of cases reached.");
                                    break;
                                }
                            }
                        }
                        catch (Exception _ex)
                        {
                            Console.WriteLine("Error processing file '" + txtFile + "': " + _ex.Message);
                            return;
                        }
                        Console.WriteLine("ok.");
                    }
                }

                #endregion

                #region Export Universal Log Format

                // TODO: Check!
                //_ret.UpdateRelativeTimes();
                _ret.CreateLookup();

                if (ParsedCommandLineArguments.Transform_OutputStata.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create ZIP archive with Stata file(s).");

                    _ret.ExportStata(ParsedCommandLineArguments.Transform_OutputStata, _language);
                }

                if (ParsedCommandLineArguments.Transform_OutputXLSX.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create XLSX file.");

                    _ret.ExportXLSX(ParsedCommandLineArguments);

                }

                if (ParsedCommandLineArguments.Transform_OutputZCSV.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create ZIP archive with CSV file(s).");

                    _ret.ExportCSV(ParsedCommandLineArguments);
                }

                if (ParsedCommandLineArguments.Transform_Codebook.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create Codebook File.");

                    _ret.CreateCodebook(ParsedCommandLineArguments.Transform_Codebook, _language);
                }

                if (ParsedCommandLineArguments.Transform_ConcordanceTable.Trim() != "")
                {
                    if (!File.Exists(ParsedCommandLineArguments.Transform_ConcordanceTable))
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Create Concordance Table.");

                        _ret.CreateConcordanceTable(ParsedCommandLineArguments.Transform_ConcordanceTable);
                    }
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

                #endregion

            }
            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.Message.ToString());
            }
        }

        private static int ReadLogDataPIAACFromLDAExportString(int _logcounter, int _lineCounter, string line, logXContainer _ret, string _PersonIdentifier, CommandLineArguments ParsedCommandLineArguments, EventDataListExtension.ESortType sort, Dictionary<string, string> _lookup, string[] Element)
        {
            string[] _cols = line.Split('\t');
            if (_cols.Length > 1 && _lineCounter > 0)
            { 
                _PersonIdentifier = _cols[0] + "_" + _cols[1];

                string _rawBooklet = _cols[2];
                string _rawSequenceID = _cols[3];
                string _rawEventName = _cols[4];
                string _rawEventType = _cols[5];
                string _relativeTimeString = _cols[6];
                string _rawEventDescription = _cols[7];
                string _itemID = _rawBooklet + "_" + _rawSequenceID;
                long _relativeTime = long.Parse(_cols[6], CultureInfo.InvariantCulture);
                
                if (!ParsedCommandLineArguments.Flags.Contains("USE_BOOKLLET_SEQUENCE_ID"))
                {
                    if (_lookup.ContainsKey(_itemID))
                        _itemID = _lookup[_itemID];
                }

                bool _isFiltered = Element.Length == 0;
                if (Element.Contains(_itemID))
                    _isFiltered = true;

                if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                {
                    return _logcounter;
                }

                if (_isFiltered)
                {
                    if (_rawEventDescription.StartsWith("<cbaloggingmodel", StringComparison.InvariantCulture))
                    {
                        // IB xml events

                        _logcounter = piaac_R1_extract_IB_XML(_logcounter, _ret, _lookup, _rawEventDescription, _PersonIdentifier, _rawSequenceID, _rawBooklet, _rawEventType, _rawEventName, _relativeTime, ParsedCommandLineArguments);
                    }
                    else if (_rawEventDescription.StartsWith("<cbascoringresultmm", StringComparison.InvariantCulture))
                    {
                        // IB item score

                        if (_rawEventType == "ItemScore")
                        {
                            if (1==1|| ParsedCommandLineArguments.Flags.Contains("INCLUDE_ALL_ITEMSCORES") || !ParsedCommandLineArguments.Flags.Contains("INCLUDE_NO_ITEMSCORES"))
                            {
                                _logcounter = piaac_R1_extract_IB_XML(_logcounter, _ret, _lookup, _rawEventDescription, _PersonIdentifier, _rawSequenceID, _rawBooklet, _rawEventType, _rawEventName, _relativeTime, ParsedCommandLineArguments);
                            }
                        }
                        else if (_rawEventType == "itemScoreResult")
                        {
                            // by default ignore second item score 
                            if (1 == 1 || ParsedCommandLineArguments.Flags.Contains("INCLUDE_ALL_ITEMSCORES"))
                            { 
                                _logcounter = piaac_R1_extract_IB_XML(_logcounter, _ret, _lookup, _rawEventDescription, _PersonIdentifier, _rawSequenceID, _rawBooklet, _rawEventType, _rawEventName, _relativeTime, ParsedCommandLineArguments      );
                            }
                        }
                        else
                        {
                            throw new Exception("The value '" + _rawEventName + "' was not expected in line " + _lineCounter + ".");
                        }
                    }
                    else if (_rawEventDescription.StartsWith("http://localhost:8080", StringComparison.InvariantCulture))
                    {
                        // ignore snapshots
                    }
                    else
                    {
                        // Pre-process event values

                        logxGenericLogElement _parament = new logxGenericLogElement()
                        {
                            PersonIdentifier = _PersonIdentifier,
                            EventName = _rawEventType,
                            Item = _itemID,
                            RelativeTime = _relativeTime,
                            EventID = _logcounter
                        };
                        _logcounter++;

                        var doc = new XDocument(new XElement(_parament.EventName));
                        var root = doc.Root;

                        _rawEventDescription = _rawEventDescription.Replace("|$*", "|*$");
                        string[] _kvps = _rawEventDescription.Split("|*$");
                        foreach (string _kvp in _kvps)
                        {
                            string[] _kv = _kvp.Split('=');
                            if (_kv.Length == 2)
                            {
                                root.Add(new XAttribute(_kv[0], _kv[1]));
                            }
                            else if (_kv.Length == 1)
                            {
                                root.Add(new XAttribute("value", _kv[0]));
                            }
                            else
                            {
                                string _remainingvalue = _kv[1];
                                for (int i = 2; i < _kv.Length; i++)
                                {
                                    _remainingvalue = _remainingvalue + "=" + _kv[i];
                                }
                                root.Add(new XAttribute(_kv[0], _remainingvalue));
                            }
                        }

                        if (!ParsedCommandLineArguments.Flags.Contains("HIDE_BOOKLET_INFORMATION"))
                            root.Add(new XAttribute("PIAACBookelt", _rawBooklet));

                        if (!ParsedCommandLineArguments.Flags.Contains("HIDE_EVENT_INFORMATION"))
                        {
                            root.Add(new XAttribute("PIAACSequenceID", _rawSequenceID));
                            root.Add(new XAttribute("PIAACEventType", _rawEventType));
                            root.Add(new XAttribute("PIAACEventName", _rawEventName));
                        }

                        if (!ParsedCommandLineArguments.Flags.Contains("HIDE_TIME_INFORMATION"))
                            root.Add(new XAttribute("RelativeTimeString", _relativeTimeString));

                        _parament.EventDataXML = doc.ToString();
                        _ret.AddEvent(_parament);
                    }
                }
            }
            return _logcounter;

        }

        public static int piaac_R1_extract_IB_XML(int _logcounter,  logXContainer _ret, Dictionary<string, string> _lookup, string XML, string PersonIdentifier, string _rawSequenceID, string _rawBooklet, string _rawEventType, string _rawEventName, long _relativeTime, CommandLineArguments ParsedCommandLineArguments)
        {
           
            XmlDocument _xmlDocument = new XmlDocument();
            _xmlDocument.LoadXml(XML);

            return  piaac_R1_extract_IB_XML_process_child(_logcounter, _ret, _lookup, _xmlDocument,  "", 0, PersonIdentifier, _rawSequenceID, _rawBooklet, _rawEventType, _rawEventName, _relativeTime, ParsedCommandLineArguments);
             
        }

        private static int piaac_R1_extract_IB_XML_process_child(int _logcounter, logXContainer _ret, Dictionary<string, string> _lookup, XmlNode node, string Path, int Level, string PersonIdentifier, string _rawSequenceID, string _rawBooklet, string _rawEventType, string _rawEventName, long _relativeTime, CommandLineArguments ParsedCommandLineArguments)
        {
            string _name = "";
            foreach (XmlNode n in node.ChildNodes)
            {
                _name = n.LocalName;
                if (Path.Trim() != "")
                    _name = Path + "." + _name;

                string _itemID = _rawBooklet + "_" + _rawSequenceID;
                if (!ParsedCommandLineArguments.Flags.Contains("USE_BOOKLLET_SEQUENCE_ID"))
                {
                    if (_lookup.ContainsKey(_itemID))
                        _itemID = _lookup[_itemID];
                } 

                XmlAttributeCollection atributos = n.Attributes;
                logxGenericLogElement _parament = new logxGenericLogElement()
                {
                    PersonIdentifier = PersonIdentifier,
                    EventName = _name,
                    Item = _itemID, 
                    RelativeTime =  _relativeTime,
                    EventID = _logcounter
                };
                _logcounter++;

                var doc = new XDocument(new XElement(_parament.EventName));
                var root = doc.Root;

                foreach (XmlAttribute at in atributos)
                {
                    if (at.LocalName == "cbaloggingmodel" || at.LocalName == "cbascoringresultmm" || at.LocalName == "snapshot" || at.LocalName == "xmi" || at.LocalName == "xsi")
                    {
                        // ignore attributes by default

                        if (ParsedCommandLineArguments.Flags.Contains("INCLUDE_XML_ATTRIBUTES"))
                        { 
                            root.Add(new XAttribute(at.LocalName, at.Value));
                        }
                          
                    }
                    else
                    {
                        root.Add(new XAttribute(at.LocalName, at.Value)); 
                    }

                }

                if (!ParsedCommandLineArguments.Flags.Contains("HIDE_BOOKLET_INFORMATION"))
                    root.Add(new XAttribute("PIAACBookelt", _rawBooklet));

                if (!ParsedCommandLineArguments.Flags.Contains("HIDE_EVENT_INFORMATION"))
                {
                    root.Add(new XAttribute("PIAACEventType", _rawEventType));
                    root.Add(new XAttribute("PIAACEventName", _rawEventName));
                }

                if (!ParsedCommandLineArguments.Flags.Contains("HIDE_XML_NESTING_LEVEL"))
                    root.Add(new XAttribute("XMLNextingLevel", Level.ToString()));

                _parament.EventDataXML = doc.ToString();
                _ret.AddEvent(_parament);

            }

            foreach (XmlNode n in node.ChildNodes)
            {
                piaac_R1_extract_IB_XML_process_child(_logcounter, _ret, _lookup, n, _name, Level + 1, PersonIdentifier, _rawSequenceID, _rawBooklet, _rawEventType, _rawEventName, _relativeTime, ParsedCommandLineArguments);
            }

            return _logcounter;
        }

        public static Dictionary<string, string> GetPIAACR1LookupDictionary()
        {
            Dictionary<string, string> _lookup = new Dictionary<string, string>();

            _lookup.Add("L11_1", "C311B701");
            _lookup.Add("L11_2", "C321P001");
            _lookup.Add("L11_3", "C321P002");
            _lookup.Add("L11_4", "C308A117");
            _lookup.Add("L11_5", "C308A119");
            _lookup.Add("L11_6", "C308A120");
            _lookup.Add("L11_7", "C308A121");
            _lookup.Add("L11_8", "C305A215");
            _lookup.Add("L11_9", "C305A218");
            _lookup.Add("L12_1", "C315B512");
            _lookup.Add("L12_2", "C308A117");
            _lookup.Add("L12_3", "C308A118");
            _lookup.Add("L12_4", "C308A119");
            _lookup.Add("L12_5", "C308A121");
            _lookup.Add("L12_6", "C305A215");
            _lookup.Add("L12_7", "C305A218");
            _lookup.Add("L12_8", "C304B710");
            _lookup.Add("L12_9", "C304B711");
            _lookup.Add("L13_1", "C315B512");
            _lookup.Add("L13_2", "C304B710");
            _lookup.Add("L13_3", "C304B711");
            _lookup.Add("L13_4", "C308A116");
            _lookup.Add("L13_5", "C308A118");
            _lookup.Add("L13_6", "C327P001");
            _lookup.Add("L13_7", "C327P002");
            _lookup.Add("L13_8", "C327P003");
            _lookup.Add("L13_9", "C327P004");
            _lookup.Add("L21_1", "C307B401");
            _lookup.Add("L21_2", "C307B402");
            _lookup.Add("L21_3", "C309A319");
            _lookup.Add("L21_4", "C309A320");
            _lookup.Add("L21_5", "C309A321");
            _lookup.Add("L21_6", "C309A322");
            _lookup.Add("L21_7", "C322P001");
            _lookup.Add("L21_8", "C322P002");
            _lookup.Add("L21_9", "C322P005");
            _lookup.Add("L21_10", "C313A412");
            _lookup.Add("L21_11", "C313A414");
            _lookup.Add("L22_1", "C322P001");
            _lookup.Add("L22_2", "C322P002");
            _lookup.Add("L22_3", "C322P003");
            _lookup.Add("L22_4", "C322P005");
            _lookup.Add("L22_5", "C309A319");
            _lookup.Add("L22_6", "C309A322");
            _lookup.Add("L22_7", "C310A406");
            _lookup.Add("L22_8", "C310A407");
            _lookup.Add("L22_9", "C320P001");
            _lookup.Add("L22_10", "C320P003");
            _lookup.Add("L22_11", "C320P004");
            _lookup.Add("L23_1", "C310A406");
            _lookup.Add("L23_2", "C310A407");
            _lookup.Add("L23_3", "C322P003");
            _lookup.Add("L23_4", "C322P004");
            _lookup.Add("L23_5", "C306B110");
            _lookup.Add("L23_6", "C306B111");
            _lookup.Add("L23_7", "C313A410");
            _lookup.Add("L23_8", "C313A411");
            _lookup.Add("L23_9", "C313A413");
            _lookup.Add("L23_10", "C323P003");
            _lookup.Add("L23_11", "C323P004");
            _lookup.Add("L24_1", "C306P110");
            _lookup.Add("L24_2", "C306P111");
            _lookup.Add("L24_3", "C318P001");
            _lookup.Add("L24_4", "C318P003");
            _lookup.Add("L24_5", "C313A410");
            _lookup.Add("L24_6", "C313A411");
            _lookup.Add("L24_7", "C313A413");
            _lookup.Add("L24_8", "C329P002");
            _lookup.Add("L24_9", "C329P003");
            _lookup.Add("L24_10", "C323P002");
            _lookup.Add("L24_11", "C323P005");
            _lookup.Add("N11_1", "C615A602");
            _lookup.Add("N11_2", "C615A603");
            _lookup.Add("N11_3", "C624A619");
            _lookup.Add("N11_4", "C624A620");
            _lookup.Add("N11_5", "C604A505");
            _lookup.Add("N11_6", "C605A506");
            _lookup.Add("N11_7", "C605A507");
            _lookup.Add("N11_8", "C605A508");
            _lookup.Add("N11_9", "C650P001");
            _lookup.Add("N12_1", "C604A505");
            _lookup.Add("N12_2", "C605A506");
            _lookup.Add("N12_3", "C605A507");
            _lookup.Add("N12_4", "C605A508");
            _lookup.Add("N12_5", "C650P001");
            _lookup.Add("N12_6", "C623A616");
            _lookup.Add("N12_7", "C623A617");
            _lookup.Add("N12_8", "C657P001");
            _lookup.Add("N12_9", "C619A609");
            _lookup.Add("N13_1", "C623A616");
            _lookup.Add("N13_2", "C623A617");
            _lookup.Add("N13_3", "C657P001");
            _lookup.Add("N13_4", "C619A609");
            _lookup.Add("N13_5", "C632P001");
            _lookup.Add("N13_6", "C632P002");
            _lookup.Add("N13_7", "C646P002");
            _lookup.Add("N13_8", "C620A610");
            _lookup.Add("N13_9", "C620A612");
            _lookup.Add("N21_1", "C613A520");
            _lookup.Add("N21_2", "C614A601");
            _lookup.Add("N21_3", "C618A607");
            _lookup.Add("N21_4", "C618A608");
            _lookup.Add("N21_5", "C635P001");
            _lookup.Add("N21_6", "C607A510");
            _lookup.Add("N21_7", "C655P001");
            _lookup.Add("N21_8", "C602A501");
            _lookup.Add("N21_9", "C602A502");
            _lookup.Add("N21_10", "C602A503");
            _lookup.Add("N21_11", "C608A513");
            _lookup.Add("N22_1", "C655P001");
            _lookup.Add("N22_2", "C602A501");
            _lookup.Add("N22_3", "C602A502");
            _lookup.Add("N22_4", "C602A503");
            _lookup.Add("N22_5", "C608A513");
            _lookup.Add("N22_6", "C606A509");
            _lookup.Add("N22_7", "C611A516");
            _lookup.Add("N22_8", "C611A517");
            _lookup.Add("N22_9", "C622A615");
            _lookup.Add("N22_10", "C665P001");
            _lookup.Add("N22_11", "C665P002");
            _lookup.Add("N23_1", "C622A615");
            _lookup.Add("N23_2", "C665P001");
            _lookup.Add("N23_3", "C665P002");
            _lookup.Add("N23_4", "C636P001");
            _lookup.Add("N23_5", "C617A605");
            _lookup.Add("N23_6", "C617A606");
            _lookup.Add("N23_7", "C660P003");
            _lookup.Add("N23_8", "C660P004");
            _lookup.Add("N23_9", "C641P001");
            _lookup.Add("N23_10", "C661P001");
            _lookup.Add("N23_11", "C661P002");
            _lookup.Add("N24_1", "C660P003");
            _lookup.Add("N24_2", "C660P004");
            _lookup.Add("N24_3", "C641P001");
            _lookup.Add("N24_4", "C661P001");
            _lookup.Add("N24_5", "C661P002");
            _lookup.Add("N24_6", "C612A518");
            _lookup.Add("N24_7", "C651P002");
            _lookup.Add("N24_8", "C664P001");
            _lookup.Add("N24_9", "C634P001");
            _lookup.Add("N24_10", "C634P002");
            _lookup.Add("N24_11", "C644P002");
            _lookup.Add("PS1_1", "01a");
            _lookup.Add("PS1_2", "01b");
            _lookup.Add("PS1_3", "03a");
            _lookup.Add("PS1_4", "06a");
            _lookup.Add("PS1_5", "06b");
            _lookup.Add("PS1_6", "21");
            _lookup.Add("PS1_7", "04a");
            _lookup.Add("PS2_1", "19a");
            _lookup.Add("PS2_2", "19b");
            _lookup.Add("PS2_3", "07");
            _lookup.Add("PS2_4", "02");
            _lookup.Add("PS2_5", "16");
            _lookup.Add("PS2_6", "11b");
            _lookup.Add("PS2_7", "23");

            return _lookup;

        }
    }
}
