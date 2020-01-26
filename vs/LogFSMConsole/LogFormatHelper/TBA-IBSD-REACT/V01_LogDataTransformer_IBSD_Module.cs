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

                        string line;
                        var reader = new StreamReader(fileName);

                        int linecounter = 0;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (ParsedCommandLineArguments.Transform_LogVersion == "default")
                            {
                                LogDataTransformer_IBSD_Module_V0.JSON_IB_8_12_beta3_helper.AddProcessedLogDataToContainer(_ret, line);
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

                    _ret.ExportXLSX(ParsedCommandLineArguments.Transform_OutputXLSX);

                }

                if (ParsedCommandLineArguments.Transform_OutputZCSV.Trim() != "")
                {
                    if (ParsedCommandLineArguments.Verbose)
                        Console.WriteLine("Create ZIP archive with CSV file(s).");

                    _ret.ExportCSV(ParsedCommandLineArguments.Transform_OutputZCSV);
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
