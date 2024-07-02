#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using LogFSM_LogX2019;
using LogFSMConsole;
using StataLib;
using Ionic.Zip;
#endregion

namespace LogDataTransformer_IBSD_V01
{
    public class LogDataTransformer_IBSD_Module_V01
    {
        public static void UpdateFiles(string web, string username, string password, string folder, string mask)
        {
            if (!Directory.Exists(folder))
            {
                Console.WriteLine(" Error - Specified directory '" + folder + "' not found");
            }
            else
            { 
                bool _error = false;
                string tmpfile = Path.GetTempFileName();
                try
                {
                    Console.Write(" - Download from '" + web + "' --> ");
                    WebClient _client = new WebClient();
                    if (username.Trim() != "" || password.Trim() != "")
                    {
                        string credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(username + ":" + password));
                        _client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                        Console.Write(" (with authentification: " + credentials + ")");
                    }

                    _client.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e)
                     {
                         Console.Write(".");
                     };
                     
                    _client.DownloadFile(web, tmpfile);
                    FileInfo _fi = new FileInfo(tmpfile);
                    Console.WriteLine(_fi.Length);

                }
                catch (System.Net.WebException _fnf)
                {
                    Console.WriteLine(" Error - " + _fnf.Status + " - " + _fnf.Message);
                    _error = true;

                }
                catch (Exception _ex)
                {
                    Console.WriteLine(" Unknown Error - " + _ex.ToString());
                    _error = true;
                }


                if (_error)
                {
                    Console.WriteLine(" Errors have occurred.");
                }
                else
                {
                     
                    // Unzip files 
                    try
                    {
                        using (ZipFile zip = ZipFile.Read(tmpfile))
                        {
                            foreach (var entry in zip)
                            {
                                if (CommandLineArguments.FitsMask(entry.FileName, mask))
                                    entry.Extract(folder, ExtractExistingFileAction.OverwriteSilently);
                            }
                        }
                    }
                    catch (Exception _ex)
                    {
                        Console.WriteLine(" Unknown Error - " + _ex.ToString());
                    }

                }

            }

        }
         
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

                bool _checkEventAttrbibutes = false;
                if (ParsedCommandLineArguments.Flags.Contains("CHECKEVENTATTRIBUTES"))
                    _checkEventAttrbibutes = true;


                // Create logXContainer 

                logXContainer _ret = new LogFSM_LogX2019.logXContainer()
               {
                    PersonIdentifierName = _personIdentifier,
                    PersonIdentifierIsNumber = _personIdentifierIsNumber
                };

                _ret.LoadCodebookDictionary(ParsedCommandLineArguments.Transform_Dictionary);

                // Update from Server if requested

                string _web = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_JOB_TRANSFORM_web))
                    _web = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_JOB_TRANSFORM_web];

                string _username = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_JOB_TRANSFORM_user))
                    _username = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_JOB_TRANSFORM_user];

                string _password = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_JOB_TRANSFORM_password))
                    _password = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_JOB_TRANSFORM_password];
                 
                string _mask = "";
                if (ParsedCommandLineArguments.ParameterDictionary.ContainsKey(CommandLineArguments._CMDA_mask))
                    _mask = ParsedCommandLineArguments.ParameterDictionary[CommandLineArguments._CMDA_mask];

                if (_web.Trim() != "")
                    UpdateFiles(_web, _username, _password, ParsedCommandLineArguments.Transform_InputFolders[0], _mask);

                if (ParsedCommandLineArguments.Transform_OutputStata.Trim() == "" && ParsedCommandLineArguments.Transform_OutputXLSX.Trim() == "" &&
                    ParsedCommandLineArguments.Transform_OutputZCSV.Trim() == "" && ParsedCommandLineArguments.Transform_OutputSPSS.Trim() == "")
                {
                    return;
                }
                 
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
                                                            LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.ParseLogElements(line, "IBSD_V01", _checkEventAttrbibutes, "");

                                    foreach (var _l in _log)
                                    {
                                        if (_l.Element.Trim() == "")
                                            _l.Element = "(Platform)";

                                        var g = new logxGenericLogElement()
                                        {
                                            Item = _l.Element,
                                            EventID = _l.EventID,
                                            EventName = _l.EventName,
                                            PersonIdentifier = _l.PersonIdentifier,
                                            TimeStamp = _l.TimeStamp
                                        };

                                        g.EventDataXML = LogDataTransformer_IB_REACT_8_12__8_13.JSON_IB_8_12__8_13_helper.XmlSerializeToString(_l);
                                        _ret.AddEvent(g, ParsedCommandLineArguments.Elements, ParsedCommandLineArguments.ExcludedElements);
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
                 
                logXContainer.ExportLogXContainerData(ParsedCommandLineArguments, _ret);
                 
            }
            catch (Exception _ex)
            {
                Console.WriteLine("Error transforming log data. Details: " + Environment.NewLine + _ex.Message.ToString());
            }
        }
    }
}
