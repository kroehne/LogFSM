﻿using CommandLine;
using LogFSM_LogX2019;
using LogFSMConsole;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TransformToUniversalLogFormat
{
    class Program
    {
        public class Options
        {        

            [Option('i', "input", Required = false, HelpText = "Input folder to be processed.")]
            public IEnumerable<string> InputFolders { get; set; }

            [Option('r', "rawformat", Required = false, HelpText = "Raw format (either 'ibsdraw01a', 'nepsrawv01a', 'irtlibv01a', 'ibfirebase01a', 'taopci01a', 'pisabqzip01a', 'pisabqzip01b', 'pisabqzip01c' or 'eeibraprawv01a').")]
            public string RawFormat { get; set; } = "";

            [Option('z', "zcsv", Required = false, HelpText = "Output file name for the generated universal log format, type zip-compressed CSV (i.e., absolute path to the zip file containing log data as CSV files, one file for each event type).")]
            public string ZCSV { get; set; } = "";

            [Option('x', "xes", Required = false, HelpText = "Output file name for the generated log file, type XES (i.e., absolute path to the XES file containing log data as .gzip file, if flag SERIALIZERAWXML is not provided, or as .xml file otherwise).")]
            public string XES { get; set; } = "";

            [Option('s', "stata", Required = false, HelpText = "Output file name for the generated universal log format, type Stata (i.e., absolute path to the zip file containing log data as Stata files, one file for each event type).")]
            public string Stata { get; set; } = "";

            [Option('q', "spss", Required = false, HelpText = "Output file name for the generated universal log format, type SPSS / PSPP (i.e., absolute path to the zip file containing log data as SPSS / PSPP files, one file for each event type).")]
            public string SPSS { get; set; } = "";

            [Option('o', "xlsx", Required = false, HelpText = "Output file name for the generated universal log format, type  open office XLSX (i.e., absolute path to XLSX file containing log data, one sheet for each event type).")]
            public string XLSX { get; set; } = "";

            [Option('m', "mask", Required = false, HelpText = "File filter mask. Only files that match the specified mask will be used (e.g., *.jsonl).")]
            public string Mask { get; set; } = "*.*";

            [Option('v', "verbose", Required = false, HelpText = "Request verbose output messages.")]
            public bool Verbose { get; set; }

            [Option('e', "excludedelements", Required = false, HelpText = "Element names (i.e., items, units or tasks), that should be ignored.")]
            public IEnumerable<string> ExcludedElements { get; set; }

            [Option('f', "flags", Required = false, HelpText = "Optional flags to be used for the specific transformation (see https://github.com/kroehne/LogFSM/wiki/Command-line-tool-TransformToUniversalLogFormat/).")]
            public IEnumerable<string> Flags { get; set; }

            [Option('d', "dictionary", Required = false, HelpText = "Dictionary file for the creation of an integrated codebook.")]
            public string Dictionary { get; set; } = "";

            [Option('c', "codebook", Required = false, HelpText = "File name for the XLSX file created as codebook. ")]
            public string Codebook { get; set; } = "";
              
            [Option('t', "table", Required = false, HelpText = "Concordance table file name.")]
            public string Table { get; set; } = "";

            [Option('u', "user", Required = false, HelpText = "User name for web access.")]
            public string User { get; set; } = "";

            [Option('w', "web", Required = false, HelpText = "Web address.")]
            public string Web { get; set; } = "";
             
            [Option('p', "password", Required = false, HelpText = "Password for web access.")]
            public string Password { get; set; } = "";

            [Option('k', "key", Required = false, HelpText = "API-Key for web access.")]
            public string Key { get; set; } = "";

            [Option('a', "arguments", Required = false, HelpText = "Additional arguments (URL style, i.e., ?name1=value&name2=value).")]
            public string Arguments { get; set; } = "";

            [Option('l', "logversion", Required = false, HelpText = "Version information about the raw data.")]
            public string Logversion { get; set; } = "default"; 
        }

        static void Main(string[] args)
        { 
            // Register Code Page for Stata / SPSS  
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

#if DEBUG

#endif

            try
            {
                Parser.Default.ParseArguments<Options>(args)
                  .WithNotParsed<Options>((errs) => HandleParseError(errs))
                  .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts));

            }
            catch (Exception _ex)
            {
                Console.WriteLine("Error parsing: " + string.Join<string>(" ", args));
                Console.WriteLine("Details: " + _ex.ToString());
                Console.WriteLine();
                Console.WriteLine("Call with '--help' for more information or visit https://github.com/kroehne/LogFSM/wiki/Command-line-tool-TransformToUniversalLogFormat.");
            }
        }

        static int RunOptionsAndReturnExitCode(Options options)
        {
            try
            {
                Stopwatch _watch = new Stopwatch();
                _watch.Start();

                // Map arguments to LogFSM console style

                List<string> argsForLogFSM = new List<string>();
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB + "=" + CommandLineArguments._CMDA_JOB_transform);
                argsForLogFSM.Add(CommandLineArguments._CMDA_verbose + "=" + options.Verbose.ToString());
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_input_folders + "=" + string.Join<string>(";", options.InputFolders));
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_output_stata + "=" + options.Stata);
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_output_xlsx + "=" + options.XLSX);
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_output_zcsv + "=" + options.ZCSV);
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_output_spss + "=" + options.SPSS);
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_output_xes + "=" + options.XES);
                argsForLogFSM.Add(CommandLineArguments._CMDA_mask + "=" + options.Mask);
                argsForLogFSM.Add(CommandLineArguments._CMDA_excludedelements + "=" + string.Join<string>(";", options.ExcludedElements));
                argsForLogFSM.Add(CommandLineArguments._CMDA_flags + "=" + string.Join<string>("|", options.Flags));
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_logversion + "=" + options.Logversion);
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_dictionary + "=" + options.Dictionary);
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_codebook + "=" + options.Codebook);
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_input_format + "=" + options.RawFormat);
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_concordance_table + "=" + options.Table);
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_web + "=" + options.Web);
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_user + "=" + options.User);
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_password + "=" + options.Password); 
                argsForLogFSM.Add(CommandLineArguments._CMDA_JOB_TRANSFORM_key + "=" + options.Key);

                #region Extract Additional Arguments

                Dictionary<string, string> ParameterDictionary = new Dictionary<string, string>();

                string _argumentString = System.Web.HttpUtility.UrlDecode(options.Arguments);
                if (_argumentString.EndsWith("\""))
                    _argumentString = _argumentString.Substring(0, _argumentString.Length - 1);

                string[] querySegments = _argumentString.Split('&');
                foreach (string segment in querySegments)
                {
                    string[] parts = segment.Split('=');
                    if (parts.Length > 0)
                    {
                        string key = parts[0].Trim(new char[] { '?', ' ' }).ToLower();
                        if (key.Trim() != "")
                        {
                            if (parts.Length > 1)
                            {
                                string val = System.Web.HttpUtility.UrlDecode(parts[1].Trim());
                                if (!ParameterDictionary.ContainsKey(key))
                                    ParameterDictionary.Add(key, val);
                                else
                                    ParameterDictionary[key] = val;
                            }
                            else
                            {
                                if (!ParameterDictionary.ContainsKey(key))
                                    ParameterDictionary.Add(key, "true");
                                else
                                    ParameterDictionary[key] = "true";
                            }
                        }
                    }

                }

                foreach (string _key in ParameterDictionary.Keys)
                    argsForLogFSM.Add(_key + "=" + ParameterDictionary[_key]);

                #endregion

                string[] _assemblyFullName = Assembly.GetExecutingAssembly().FullName.Split(',');
                CommandLineArguments _parsedCommandLineArguments = new CommandLineArguments(argsForLogFSM.ToArray(), _assemblyFullName[0], _assemblyFullName[1].Split('=')[1]);

                _parsedCommandLineArguments.PrintWelcomeMessage();

                #region Check Arguments 

                if (!_parsedCommandLineArguments.CheckCommandLineArguments())
                {
                    _watch.Stop();
                    Console.WriteLine("Application stopped. Time elapsed: {0}", _watch.Elapsed);
                    Console.WriteLine();
                    Console.WriteLine("Call with '--help' for more information or visit https://github.com/kroehne/LogFSM/wiki/Command-line-tool-TransformToUniversalLogFormat.");
                    return 0;
                }

                #endregion
                
                #region Run Transformation

                LogDataTransformer.RunJobTransform(_watch, _parsedCommandLineArguments);

                #endregion
            }
            catch (Exception _ex)
            {
                Console.WriteLine("Error processing data: " + _ex.ToString());
            }
            return 1;
        }
         
        static void HandleParseError(IEnumerable<Error> errs)
        { 
        } 
    }
}
