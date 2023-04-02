#region usings
using Ionic.Zip;
using LogFSM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#endregion


namespace LogFSMConsole
{
    public class CommandLineArguments
    {

        #region Command Line Arguments (Constants) 

        /// <summary>
        /// Request detailed output (verbose = true).
        /// </summary>
        public const string _CMDA_verbose = "verbose";
  
        /// <summary>
        /// Request experimental features
        /// </summary>
        public const string _CMDA_experimental = "experimental";

        /// <summary>
        /// File filter mask.
        /// </summary>
        public const string _CMDA_mask = "mask";

        /// <summary>
        /// Overwrite existing files.
        /// </summary>
        public const string _CMDA_overwrite = "overwrite";

        /// <summary>
        /// Excluded elements.
        /// </summary>
        public const string _CMDA_excludedelements = "excludedelements";

        /// <summary>
        /// Selected elements.
        /// </summary>
        public const string _CMDA_elements = "elements";

        /// <summary>
        /// Excluded events.
        /// </summary>
        public const string _CMDA_excludedevents = "excludedevents";

        /// <summary>
        /// Selected events.
        /// </summary>
        public const string _CMDA_events = "events";

        /// <summary>
        /// Flags
        /// </summary>
        public const string _CMDA_flags = "flags";


        /// <summary>
        /// Application can be used for different purposes, specified as jobs (either 'fsm', 'prepare' or 'transform')
        /// </summary>
        public const string _CMDA_JOB = "job";

        /// <summary>
        /// Run FSM.
        /// </summary>
        public const string _CMDA_JOB_fsm_default = "fsm";

        /// <summary>
        /// Prepare log data.
        /// </summary>
        public const string _CMDA_JOB_prepare = "prepare";

        /// <summary>
        /// Transform raw log data.
        /// </summary>
        public const string _CMDA_JOB_transform = "transform";

        /// <summary>
        /// Input folders for the job 'transform'. Raw log file data form all input folders are processed.
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_folders = "inputfolders";

        /// <summary>
        /// Transform to stata format.
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_output_stata = "stataoutput";

        /// <summary>
        /// Transform to SPSS format.
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_output_spss = "spssoutput";
         
        /// <summary>
        /// Transform to xlsx format.
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_output_xlsx = "xlsxoutput";


        /// <summary>
        /// Transform to xes format.
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_output_xes = "xesoutput";

        /// <summary>
        /// Transform to zcsv format.
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_output_zcsv = "zcsvoutput";

        /// <summary>
        /// Version of the log data.
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_logversion = "logversion";

        /// <summary>
        /// Dictionary used to generate codebook.
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_dictionary = "dictionary";

        /// <summary>
        /// Codebook
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_codebook = "codebook";

        /// <summary>
        /// Concordance
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_concordance_table = "table";
         
        /// <summary>
        /// Web
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_web = "web";

        /// <summary>
        /// User
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_user = "user";

        /// <summary>
        /// Password
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_password = "password";

        /// <summary>
        /// Key
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_key = "key";

        /// <summary>
        /// Input format constants
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_format = "inputformat";


        #endregion

        #region Input File Formats (Constants) 
         
        /// <summary>
        /// IBSD
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_ibsdraw01a = "ibsdraw01a";

        /// <summary>
        /// NEPS Testanwendung (prior to 2020)
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_nepsrawv01a = "nepsrawv01a";

        /// <summary>
        /// IRTlib (after 2020)
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_irtlib01a = "irtlibv01a";
         
        /// <summary>
        /// ItemBuilder Firebase (tryout in 2022)
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_ibfirebase01a = "ibfirebase01a";

        /// <summary>
        /// RAP EE
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_eeibraprawv01a = "eeibraprawv01a";

        /// <summary>
        /// PIAAC R1
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_piiacr1txtv01a = "piaacr1txt01a";

        /// <summary>
        /// TAO PCI 
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_taopci01a = "taopci01a";
         
        /// <summary>
        /// 
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_pisabqzip01a = "pisabqzip01a";

        /// <summary>
        /// 
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_pisabqzip01b = "pisabqzip01b";

        /// <summary>
        /// 
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_pisabqzip01c = "pisabqzip01c";

        /// <summary>
        /// PISA 2012 (OECD Download)
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_pisa2012zip01a = "pisa2012zip01a";
                 
        /// <summary>
        /// 
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_pisacazip01a = "pisacazip01a";


        /// <summary>
        /// 
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_pisacazip01b = "pisacazip01b";

        /// <summary>
        /// 
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_pisacazip01c = "pisacazip01c";
        /// <summary>
        /// 
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_piaacldazip01a = "piaacldazip01a";
         
        /// <summary>
        /// 
        /// </summary>
        public const string _CMDA_JOB_TRANSFORM_input_timsst19zip01a = "timsst19zip01a";
        

        #endregion

        #region ParsedConfiguration

        public Dictionary<string, string> ParameterDictionary = new Dictionary<string, string>();

        #endregion
         
        /// <summary>
        /// Name of the called application.
        /// </summary>
        public string Application = "";

        /// <summary>
        /// Version of the called application.
        /// </summary>
        public string Version = "";

        /// <summary>
        /// System provided temp path.
        /// </summary>
        public string TempPath = Path.GetTempPath();
        
        /// <summary>
        /// Current Job.
        /// </summary>
        public string Job = "";

        /// <summary>
        /// Verbose style of messages.
        /// </summary>
        public bool Verbose = false;

        /// <summary>
        /// Activate experimental features.
        /// </summary>
        public bool Experimental = false;

        /// <summary>
        /// Mask for file filters.
        /// </summary>
        public string Mask = "";

        #region Requested Configuration -- Job = 'transform'

        /// <summary>
        /// Input folders.
        /// </summary>
        public string[] Transform_InputFolders = { };

        /// <summary>
        /// Input format. 
        /// </summary>
        public string Transform_InputFormat = "";

        /// <summary>
        /// Create universal log format, format zipped stata files
        /// </summary>
        public string Transform_OutputStata = "";

        /// <summary>
        /// Create universal log format, format zipped SPSS files
        /// </summary>
        public string Transform_OutputSPSS = "";
          
        /// <summary>
        /// Create universal log format, format xlsx
        /// </summary>
        public string Transform_OutputXLSX = "";

        /// <summary>
        /// Create standardized XES format
        /// </summary>
        public string Transform_OutputXES = "";

        /// <summary>
        /// Create universal log format, format zipped csv files
        /// </summary>
        public string Transform_OutputZCSV = "";

        /// <summary>
        /// Log version expected for transformation
        /// </summary>
        public string Transform_LogVersion = "";

        /// <summary>
        /// Dictionary file used to create codebook
        /// </summary>
        public string Transform_Dictionary = "";

        /// <summary>
        /// Codebook file name
        /// </summary>
        public string Transform_Codebook = "";

        /// <summary>
        /// File name for a concordance table
        /// </summary>
        public string Transform_ConcordanceTable = "";


        #endregion

        public string OutputPath = "";
        public string DataFileType = "";
        public string DataFileName = "";
        public string DataFileFilter = "";
        public string ZIPFileName = "";
        public string FSMFileType = "";
        public string FSMFileName = "";
        public string OutFileName = "";
        public string DataFolder = "";
        public string PersonIdentifier = "";
        public string[] Elements = { };
        public string[] ExcludedElements = { };
        public string[] Events = { };
        public string[] ExcludedEvents= { };
        public string ZIPPassword = "";
        public string DataFileTypeDetails = "";
        public string ZIPFileFilterName = "";
        public bool AddEventInfo = false;
        public List<string> Flags = new List<string>();
        public string UniversalFileFormat = "Stata";

        public bool RelativeTime = false;

        // limit the number of processed persons (-1 = unlimited)
        public int MaxNumberOfCases = -1;

        // path to the dlls  
        public string RuntimePath = System.AppContext.BaseDirectory;
        public bool IsDebug = false;

        #region Application Startup

        public void PrintWelcomeMessage()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            string _app = Application + " (Version: " + typeof(Program).Assembly.GetName().Version.ToString() + ")";
            Console.WriteLine(_app);
            Console.WriteLine(new string('=', (_app).Length));
            Console.ResetColor();
             
            if (Verbose)
            {  
                Console.WriteLine("Arguments: ");
                foreach (var key in ParameterDictionary.Keys)
                {
                    if (ParameterDictionary[key].Trim() != "")
                        Console.WriteLine(" - " + key + ": " + ParameterDictionary[key]);
                }
                    
                Console.WriteLine("System:");
                Console.WriteLine(" - Temp Path: " + TempPath);
                Console.WriteLine(" - Current Path: " + RuntimePath);
                Console.WriteLine(" - .NET Environment Version: " + Environment.Version.ToString()); 
                Console.WriteLine(" - OS: " + System.Runtime.InteropServices.RuntimeInformation.OSDescription);

                Console.WriteLine("General:");
                Console.WriteLine(" - Relative Time: " + RelativeTime);
                Console.WriteLine();

            }
        }
 
        public static bool FitsMask(string sFileName, string sFileMask)
        {
            Regex mask = new Regex(sFileMask.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));
            return mask.IsMatch(sFileName);
        }

        public bool CheckCommandLineArguments()
        {
            bool _return = true;

            if (Job == _CMDA_JOB_fsm_default)
            {
                #region job: fsm - Type file
                try
                {
                    if (FSMFileTypeIsCSharp || FSMFileTypeIsCustom01)
                    {
                        if (File.Exists(FSMFileName))
                        { 
                            Console.WriteLine(" - Process FSM file '" + FSMFileName.Replace("\\", "/") + "' (syntax type: '" + FSMFileType  + "').");
                        }
                        else
                        {
                            Console.WriteLine(" - FSM file '" + FSMFileName + "' not found.");
                            _return = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine(" - FSM FileType '" + FSMFileType + "' not supported.");
                        _return = false;
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("  - Error checking FSM file: " + _ex.Message);
                    _return = false;
                }
                #endregion

                #region job: fsm -- Data files

                try
                {
                    if (DataFileTypeIsEE || DataFileTypeIsLogFSMJson || DataFileTypeIsJsonLite)
                    {
                        if (!File.Exists(DataFileName))
                        {
                            Console.WriteLine("Error (DF105 / Data file not found): Data file '" + DataFileName + "' not found.");
                            _return = false;
                        }
                    }
                    else if (DataFileTypeIsEEZip || DataFileTypeIsLogFSMJsonZip || DataFileTypeIsJsonLiteZip)
                    {
                        if (!File.Exists(ZIPFileName))
                        {
                            Console.WriteLine(" - Error (DF104 / Archive not found): Archive file '" + ZIPFileName + "' not found. Check the value provided as 'zipfilename'.");
                            _return = false;
                        }

                        int _matchedFilesInZIP = 0;
                        using (ZipFile zip = ZipFile.Read(ZIPFileName))
                        {
                            foreach (var entry in zip)
                            {
                                if ((DataFileFilter != "" && FitsMask(entry.FileName, DataFileFilter)) || (entry.FileName == DataFileName) || DataFileFilter == "")
                                    _matchedFilesInZIP += 1;
                            }
                        }
                        if (_matchedFilesInZIP == 0)
                        {
                            if (DataFileFilter != "")
                                Console.WriteLine("Error (DF103 / No file matches pattern): No file(s) found in archive '" + ZIPFileName + "' that match filter: '" + DataFileName + "'. Check the value provdied as 'datafilename'.");
                            else
                                Console.WriteLine("Error (DF102 / Specified file not found): File '" + DataFileName + "' not found in archive '" + ZIPFileName + "'. Check the value provdied as 'datafilename'.");
                            _return = false;
                        }
                    }
                    else if (DataFileTypeIsUniversalLogFormat)
                    {
                        if (!File.Exists(ZIPFileName))
                        {
                            Console.WriteLine(" - Error (DF106 / Archive not found): Archive file '" + ZIPFileName + "' not found. Check the value provided as 'zipfilename'.");
                            _return = false;

                            int _NumberOfStataFiles = 0;
                            int _NumberOfSPSSFiles = 0;
                            int _NumberOfSCSVSFiles = 0; 
                            using (ZipFile zip = ZipFile.Read(ZIPFileName))
                            {
                                foreach (var entry in zip)
                                {
                                    if (entry.FileName.ToLower().EndsWith(".dta"))
                                        _NumberOfStataFiles += 1;
                                    else if (entry.FileName.ToLower().EndsWith(".sav"))
                                        _NumberOfSPSSFiles += 1;
                                    else if (entry.FileName.ToLower().EndsWith(".csv"))
                                        _NumberOfSCSVSFiles += 1;
                                }
                            }

                            if (_NumberOfSCSVSFiles != 0 && _NumberOfSPSSFiles==0 && _NumberOfStataFiles == 0)
                                UniversalFileFormat = "CSV";
                            else if (_NumberOfSCSVSFiles == 0 && _NumberOfSPSSFiles != 0 && _NumberOfStataFiles == 0)
                                UniversalFileFormat = "SPSS";
                            else if (_NumberOfSCSVSFiles == 0 && _NumberOfSPSSFiles == 0 && _NumberOfStataFiles != 0)
                                UniversalFileFormat = "STATA";
                            else
                            {
                                 Console.WriteLine(" - Error (DF107 / Universal Log Format not Detected): Data format of not detected (SPSS: '" + _NumberOfSPSSFiles + "', Stata: '" + _NumberOfStataFiles + "', CSV: '" + _NumberOfStataFiles +"')");
                                _return = false;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(" - Error (DF101 / Format Unknown): Data of file type '" + DataFileType + "' not supported. Check the value provided as 'datafiletype'. ");
                        _return = false;
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine(" - Error (DF100 / Unknown error): Unknown error checking the data file: " + _ex.Message);
                    _return = false;
                }

                #endregion

            }
            else if (Job == _CMDA_JOB_prepare)
            {
                #region job: prepare

                if (DataFileTypeIsPISABQZipVersion01A ||
                    DataFileTypeIsPISABQZipVersion01B ||
                    DataFileTypeIsPISABQZipVersion01C ||
                    DataFileTypeIsPISACAZipVersion01A ||
                    DataFileTypeIsPISACAZipVersion01B ||
                    DataFileTypeIsPISACAZipVersion01C ||
                    DataFileTypeIsNDataFlatV01A ||
                    DataFileTypeIsUniversalLogFormat ||
                    DataFileTypeIsPIAACLdaRawZipVersion01A)
                {
                    if (!File.Exists(ZIPFileName))
                    {
                        Console.WriteLine("Data file '" + ZIPFileName + "' not found.");
                        _return = false;
                    }
                }
                else
                {
                    Console.WriteLine("Data file type unknown.");
                    _return = false;
                }

                if (OutFileName.Trim() == "")
                {
                    Console.WriteLine("Out file name not specified.");
                    _return = false;
                }
               
                #endregion
            }
            else if (Job == _CMDA_JOB_transform)
            {
                #region job: transform
             
                if (this.Transform_InputFolders.Count() == 0)
                {
                    // Error
                    Console.WriteLine("No input folder specified.");
                    _return = false;
                }
                 
                if (this.Transform_OutputStata.Trim() == ""  && this.Transform_OutputXLSX.Trim() == "" && this.Transform_OutputZCSV.Trim() ==  "" && this.Transform_OutputSPSS.Trim() == "")
                {
                    // Warning
                    Console.WriteLine("No output format specified. "); 
                }
                #endregion
            }
            else
            {
                _return = false;
            }

            return _return;
        }

        public bool PrepareDefaulfFiles()
        {
            bool _return = true;
            OutputPath = Path.GetDirectoryName(OutFileName);
            if (Directory.Exists(OutputPath))
            {
                try
                {
                    if (File.Exists(Path.Combine(OutputPath, "logfsmlasterror.txt")))
                        File.Delete(Path.Combine(OutputPath, "logfsmlasterror.txt"));

                    if (File.Exists(Path.Combine(OutputPath, "logfsmlastfsmjson.txt")))
                        File.Delete(Path.Combine(OutputPath, "logfsmlastfsmjson.txt"));

                    if (File.Exists(Path.Combine(OutputPath, "logfsmlastsource.cs")))
                        File.Delete(Path.Combine(OutputPath, "logfsmlastsource.txt"));

                    // TODO: Check 
                    var _oldfiles = Directory.GetFiles(OutputPath, "logfsmmachine*.txt");
                    foreach (var f in _oldfiles)
                        File.Delete(Path.Combine(f));

                }
                catch  
                {
                    return false;
                }
            }
            return _return;
        }

        public bool CheckFSMCustom01(IFSMSyntaxReader _syntax)
        {
            // TODO: Add feedback from worskhop for additional syntax checks

            bool _return = true;
            string _results = "";
            
            if (_syntax.Errors.Count > 0)
            {
                _return = false;
                foreach (var _e in _syntax.Errors)
                {
                    _results += " - Error: " + _e + Environment.NewLine;
                }
            }


            string _startStates = "";
            foreach (var _k in _syntax.Start.Keys)
                _startStates += _syntax.Start[_k] + "; ";
            if (_startStates.EndsWith("; "))
                _startStates = _startStates.Substring(0, _startStates.Length - 2);
             
            if (_syntax.Start.Count == 0)
            {
                _return = false;
                _results += " - Error: No start state defined. " + Environment.NewLine;
            }
            else
            {
                _results += " - Info: Start state is " + _startStates + "." + Environment.NewLine;
            }
             
            string _endStates = "";
            foreach (var _k in _syntax.End.Keys)
                _endStates += _syntax.End[_k].Count() + "; ";
            if (_endStates.EndsWith("; "))
                _endStates = _endStates.Substring(0, _endStates.Length - 2);

            _results += " - Info: Number of end states is " + _syntax.End.Count() + "." + Environment.NewLine;
            if (_syntax.End.Count == 0)
            {
                _results += " - Warning: No end state defined. " + Environment.NewLine;
            }
            else
            {
                _results += " - Info: End state is / end states are '" + _endStates + "'." + Environment.NewLine;
            }

            _results += " - Info: Number of triggers is " + _syntax.Triggers.Length + "." + Environment.NewLine;

            for (int i = 0; i < _syntax.NumberOfMachines; i++)
                _results += " - Info: Total Number of states is " + _syntax.States[i].Count + "." + Environment.NewLine;
             
            Dictionary<int, int> _numberOfTransitionsPerMachine = new Dictionary<int, int>();
            foreach (var t in _syntax.Transitions)
            {
                if (!_numberOfTransitionsPerMachine.ContainsKey(t.MachineIndex))
                    _numberOfTransitionsPerMachine.Add(t.MachineIndex, 0);

                _numberOfTransitionsPerMachine[t.MachineIndex] += 1;
            }
            
            /*
            int _numberOfDefinedMachines = 0;
            if (_syntax.End.Keys.Count > _numberOfDefinedMachines)
                _numberOfDefinedMachines = _syntax.End.Keys.Count;
            if (_syntax.Start.Keys.Count > _numberOfDefinedMachines)
                _numberOfDefinedMachines = _syntax.Start.Keys.Count;
            if (_numberOfTransitionsPerMachine.Keys.Count > _numberOfDefinedMachines)
                _numberOfDefinedMachines = _numberOfTransitionsPerMachine.Keys.Count;
            */
            _results += " - Info: Number of machines is " + _syntax.NumberOfMachines + "." +  Environment.NewLine;

            

            string _numberOfTransitions = "";
            foreach (var _k in _numberOfTransitionsPerMachine.Keys)
                _numberOfTransitions += _numberOfTransitionsPerMachine[_k] + "; ";
            if (_numberOfTransitions.EndsWith("; "))
                _numberOfTransitions = _numberOfTransitions.Substring(0, _numberOfTransitions.Length - 2);

            _results += " - Info: Number of transitions is " + _numberOfTransitions  + "." + Environment.NewLine;
    
            if (!_return)
                File.AppendAllText(Path.Combine(OutputPath, "logfsmlasterror.txt"), _results);

            if (Verbose || IsDebug)
                Console.WriteLine(_results);

            return _return;
        }


        public bool FSMFileTypeIsCSharp
        {
            get
            {
                return (FSMFileType.Trim().ToLower() == "c#" || FSMFileType.Trim().ToLower() == "cs" || FSMFileType.Trim().ToLower() == "csharp" || FSMFileType.Trim().ToLower() == "csharpsyntax");
            }
        }

        public bool FSMFileTypeIsCustom01
        {
            get
            {
                return (FSMFileType.Trim().ToLower() == "custom01" || FSMFileType.Trim().ToLower() == "01");
            }
        }

        public bool DataFileTypeIsUniversalLogFormat
        {
            get
            {
                return (DataFileType.Trim().ToLower() == "universal01");
            }
        }

        public bool DataFileTypeIsEE
        {
            get
            {
                return (DataFileType.Trim().ToLower() == "eexml" && ZIPFileName.Trim() == "");
            }
        }

        public bool DataFileTypeIsEEZip
        {
            get
            {
                return (DataFileType.Trim().ToLower() == "eexml" && ZIPFileName.Trim() != "");
            }
        }

        public bool DataFileTypeIsLogFSMJson
        {
            get
            {
                return (DataFileType.Trim().ToLower() == "logfsmjson" && ZIPFileName.Trim() == "");
            }
        }

        public bool DataFileTypeIsLogFSMJsonZip
        {
            get
            {
                return (DataFileType.Trim().ToLower() == "logfsmjson" && ZIPFileName.Trim() != "");
            }
        }

        public bool DataFileTypeIsJsonLite
        {
            get
            {
                return (DataFileType.Trim().ToLower() == "jsonlite" && ZIPFileName.Trim() == "");
            }
        }

        public bool DataFileTypeIsJsonLiteZip
        {
            get
            {
                return (DataFileType.Trim().ToLower() == "jsonlite" && ZIPFileName.Trim() != "");
            }
        }

        
        public bool DataFileTypeIsNDataFlatV01A
        {
            get
            {
                return (DataFileType.Trim().ToLower() == "dataflatv01a" && ZIPFileName.Trim() != "");
            }
        }
         

        public bool DataFileTypeIsPISABQZipVersion01A
        {
            get
            {
                return (DataFileType.Trim().ToLower() == _CMDA_JOB_TRANSFORM_input_pisabqzip01a && ZIPFileName.Trim() != "");
            }
        }

        public bool DataFileTypeIsPISABQZipVersion01B
        {
            get
            {
                return (DataFileType.Trim().ToLower() == _CMDA_JOB_TRANSFORM_input_pisabqzip01b && ZIPFileName.Trim() != "");
            }
        }

        public bool DataFileTypeIsPISABQZipVersion01C
        {
            get
            {
                return (DataFileType.Trim().ToLower() == _CMDA_JOB_TRANSFORM_input_pisabqzip01c && ZIPFileName.Trim() != "");
            }
        }

        public bool DataFileTypeIsPISACAZipVersion01A
        {
            get
            {
                return (DataFileType.Trim().ToLower() == _CMDA_JOB_TRANSFORM_input_pisacazip01a && ZIPFileName.Trim() != "");
            }
        }
        public bool DataFileTypeIsPISACAZipVersion01B
        {
            get
            {
                return (DataFileType.Trim().ToLower() == _CMDA_JOB_TRANSFORM_input_pisacazip01b && ZIPFileName.Trim() != "");
            }
        }
        public bool DataFileTypeIsPISACAZipVersion01C
        {
            get
            {
                return (DataFileType.Trim().ToLower() == _CMDA_JOB_TRANSFORM_input_pisacazip01c && ZIPFileName.Trim() != "");
            }
        }

        public bool DataFileTypeIsPIAACLdaRawZipVersion01A
        {
            get
            {
                return (DataFileType.Trim().ToLower() == _CMDA_JOB_TRANSFORM_input_piaacldazip01a && ZIPFileName.Trim() != "");
            }
        }

        public bool DataFileTypeIsTIMSST19ZipVersion01A
        {
            get
            {
                return (DataFileType.Trim().ToLower() == _CMDA_JOB_TRANSFORM_input_timsst19zip01a && ZIPFileName.Trim() != "");
            }
        }

        public bool DataFileTypeIsPISA12ZipVersion01A
        {
            get
            {
                return (DataFileType.Trim().ToLower() == _CMDA_JOB_TRANSFORM_input_pisa2012zip01a && ZIPFileName.Trim() != "");
            }
        }
        

        public CommandLineArguments(string[] args, string Application, string Version)
        {
            PrepareComandLineArguments(args);
         
            this.Application = Application;
            this.Version = Version;
            this.Job = GetParameterOrDefault(_CMDA_JOB, _CMDA_JOB_fsm_default);

            #region General Settings (all Jobs)

            this.Experimental = GetParameterOrDefault(_CMDA_experimental, "").ToLower().StartsWith("t", StringComparison.InvariantCulture);
            this.Verbose = GetParameterOrDefault(_CMDA_verbose, "").Trim().ToLower().StartsWith("t", StringComparison.InvariantCulture);
            this.Mask = GetParameterOrDefault(_CMDA_mask, "");
             
            string _flags = GetParameterOrDefault(_CMDA_flags, "").Replace("|","-");
            this.Flags = _flags.Split('-').ToList();
            for (int i = 0; i < Flags.Count; i += 1)
            {
                this.Flags[i] = Flags[i].Trim().ToUpper();
            }

            if (Flags.Contains("RELATIVETIME"))
                RelativeTime = true;
             
            string _excludedElementsString = GetParameterOrDefault(_CMDA_excludedelements, "");
            this.ExcludedElements = _excludedElementsString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            string _elementsString = GetParameterOrDefault(_CMDA_elements, "");
            this.Elements = _elementsString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            string _excludedEventsString = GetParameterOrDefault(_CMDA_excludedevents, "");
            this.ExcludedEvents = _excludedEventsString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            string _eventsString = GetParameterOrDefault(_CMDA_events, "");
            this.Events = _eventsString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            #endregion

            DataFileType = GetParameterOrDefault("datafiletype", "");
            DataFileName = GetParameterOrDefault("datafilename", "");
            FSMFileType = GetParameterOrDefault("fsmfiletype", "");
            FSMFileName = GetParameterOrDefault("fsmfilename", "");
            OutFileName = GetParameterOrDefault("outfilename", "");
            DataFolder = GetParameterOrDefault("datafolder", "");
            PersonIdentifier = GetParameterOrDefault("personidentifier", "");
            ZIPFileName = GetParameterOrDefault("zipfilename", "");
            DataFileFilter = GetParameterOrDefault("datafilefilter", "");
            ZIPPassword = GetParameterOrDefault("zippassword", "");
            MaxNumberOfCases = GetParameterOrDefault("maxnumberofcases", -1);
            DataFileTypeDetails = GetParameterOrDefault("datafiletypedetails", "");
            ZIPFileFilterName = GetParameterOrDefault("zipfilefiltername", "");
            AddEventInfo = GetParameterOrDefault("addeventinfo", true);

            #region Specific settings: JOB transform

            string _inputFoldersString = GetParameterOrDefault(_CMDA_JOB_TRANSFORM_input_folders, "");
            this.Transform_InputFolders = _inputFoldersString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            this.Transform_OutputStata = GetParameterOrDefault(_CMDA_JOB_TRANSFORM_output_stata, "");
            this.Transform_OutputXLSX = GetParameterOrDefault(_CMDA_JOB_TRANSFORM_output_xlsx, "");
            this.Transform_OutputXES = GetParameterOrDefault(_CMDA_JOB_TRANSFORM_output_xes, "");
            
            this.Transform_OutputZCSV = GetParameterOrDefault(_CMDA_JOB_TRANSFORM_output_zcsv, "");
            this.Transform_OutputSPSS = GetParameterOrDefault(_CMDA_JOB_TRANSFORM_output_spss, "");

            this.Transform_LogVersion = GetParameterOrDefault(_CMDA_JOB_TRANSFORM_logversion, "");
            this.Transform_Dictionary = GetParameterOrDefault(_CMDA_JOB_TRANSFORM_dictionary, "");
            this.Transform_Codebook = GetParameterOrDefault(_CMDA_JOB_TRANSFORM_codebook, "");

            this.Transform_ConcordanceTable = GetParameterOrDefault(_CMDA_JOB_TRANSFORM_concordance_table, "");

            this.Transform_InputFormat = GetParameterOrDefault(_CMDA_JOB_TRANSFORM_input_format, "");

        #endregion
             
    }

        private void PrepareComandLineArguments(string[] args)
        {
            string _foo = "?";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Trim() != "")
                {
                    if (_foo.Trim() != "?")
                    {
                        _foo += "&";
                    }
                    _foo += args[i].Trim();
                }
            }
            _foo = _foo.Replace("&=&", "=");

            if (_foo.EndsWith("\""))
                _foo = _foo.Substring(0, _foo.Length - 1);

            string[] querySegments = _foo.Split('&');
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
        }

        private bool GetParameterOrDefault(string ParameterName, bool Default)
        {
            if (ParameterDictionary.ContainsKey(ParameterName))
            {
                bool _ret = false;
                if (ParameterDictionary[ParameterName].Trim().ToLower() == "true" || ParameterDictionary[ParameterName].Trim().ToLower() == "t")
                    _ret = true;

                return _ret;
            }
            else
                return Default;
        }

        private string GetParameterOrDefault(string ParameterName, string Default)
        {
            if (ParameterDictionary.ContainsKey(ParameterName))
                return ParameterDictionary[ParameterName];
            else
                return Default;
        }

        private int GetParameterOrDefault(string ParameterName, int Default)
        {
            if (ParameterDictionary.ContainsKey(ParameterName))
            {
                int ret = Default;
                if (int.TryParse(ParameterDictionary[ParameterName], out ret))
                {
                    return ret;
                }
            } 
            return Default;
        }

        #endregion

        #region Running Job

        public int currentNumberOfPersons = -1;

        #endregion
    }

}
