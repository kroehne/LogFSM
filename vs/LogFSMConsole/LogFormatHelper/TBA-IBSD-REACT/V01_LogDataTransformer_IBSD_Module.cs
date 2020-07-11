namespace LogDataTransformer_IBSD_V01
{
    #region usings
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using LogFSM_LogX2019;
    using LogFSMConsole;
    using StataLib;
    #endregion

    public class LogDataTransformer_IBSD_Module_V01
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

                // Create logXContainer 

                logXContainer _ret = new LogFSM_LogX2019.logXContainer()
               {
                    PersonIdentifierName = _personIdentifier,
                    PersonIdentifierIsNumber = _personIdentifierIsNumber
                };

                _ret.LoadCodebookDictionary(ParsedCommandLineArguments.Transform_Dictionary);

                // Iterate over all input filters

                foreach (string inFolder in ParsedCommandLineArguments.Transform_InputFolders)
                {
                    if (!Directory.Exists(inFolder))
                    {
                        if (ParsedCommandLineArguments.Verbose)
                            Console.WriteLine("Warning: Directory not exists: '" + inFolder + "'.");

                        continue;
                    }

                    string[] listOfFiles = Directory.GetFiles(inFolder, ParsedCommandLineArguments.Mask, SearchOption.AllDirectories);

                    foreach (string fileName in listOfFiles)
                    { 
                        if (ParsedCommandLineArguments.MaxNumberOfCases > 0 && _ret.GetNumberOfPersons >= ParsedCommandLineArguments.MaxNumberOfCases)
                        {
                            if (ParsedCommandLineArguments.Verbose)
                                Console.WriteLine("Info: Max number of cases reached.");
                            break;
                        }

                        if (ParsedCommandLineArguments.Verbose)
                            Console.Write("Info: Read File  '" + fileName + "' ");

                        try
                        {
                            string line;
                            var reader = new StreamReader(fileName);

                            int linecounter = 0;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (ParsedCommandLineArguments.Transform_LogVersion == "default")
                                {

                                    List<LogDataTransformer_IB_REACT_8_12__8_13.Log_IB_8_12__8_13> _log =
                                                            LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseLogElements(line, "IBSD_V01");

                                    foreach (var _l in _log)
                                    {
                                        var g = new logxGenericLogElement()
                                        {
                                            Item = _l.Element,
                                            EventID = _l.EventID,
                                            EventName = _l.EventName,
                                            PersonIdentifier = _l.PersonIdentifier,
                                            TimeStamp = _l.TimeStamp
                                        };

                                        g.EventDataXML = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.XmlSerializeToString(_l);
                                        _ret.AddEvent(g);
                                    }

                                }
                                else
                                {
                                    if (ParsedCommandLineArguments.Verbose)
                                        Console.WriteLine("failed.");

                                    Console.WriteLine("Version '" + ParsedCommandLineArguments.Transform_LogVersion + "' not supported.");
                                    return;
                                }
                                linecounter++;
                            }
                            if (ParsedCommandLineArguments.Verbose)
                                Console.WriteLine(" ok ('" + linecounter + " lines).");

                            reader.Close();
                        } 
                        catch (Exception exfile)
                        {
                            Console.WriteLine("Error processing file '" + fileName +  "'. Details: " + Environment.NewLine + exfile.Message.ToString());
                        }
                       
                    }

                }

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


                if (ParsedCommandLineArguments.Transform_OutputSPSS.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create ZIP archive with SPSS / PSPP file(s).");

                    _ret.ExportCSV(ParsedCommandLineArguments);
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
            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.Message.ToString());
            }
        }
    }
}
