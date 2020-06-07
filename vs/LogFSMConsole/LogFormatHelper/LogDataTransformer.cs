namespace LogFSMConsole
{
    #region usings
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    #endregion

    public static class LogDataTransformer
    {
        public enum ERelativeTimeType
        {
            None,
            PerPerson,
            PerElementPerPerson
        }

        public static void RunJobTransform(Stopwatch Watch, CommandLineArguments ParsedCommandLineArguments)
        {

            if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_ibsdraw01a)
            {
                Console.WriteLine("Module: Transform 'IBSD' log data to universal log format V01.");

                LogDataTransformer_IBSD_V01.LogDataTransformer_IBSD_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_irtlib01a)
            {
                Console.WriteLine("Module: Transform 'IRTlib' log data to universal log format V01.");

                LogDataTransformer_IRTlibPlayer_V01.LogDataTransformer_IRTLIB_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_nepsrawv01a)
            { 
                Console.WriteLine("Module: Transform 'NEPS' log data to universal log format V01.");

                LogDataTransformer_NEPS_V01.LogDataTransformer_NEPS_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_eeibraprawv01a)
            {
                Console.WriteLine("Module: Transform 'EE (ItemBuilder RAP)' log data to universal log format V01.");

                LogDataTransformer_EE_RAP_V01.LogDataTransformer_EE_RAP_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_piiacr1txtv01a)
            {
                Console.WriteLine("Module: Transform 'PIAAC R1 (Preprocessed TXT files from PIAAC LDA)' log data to universal log format V01.");

                LogDataTransformer_PIAAC_R1_V01.LogDataTransformer_PIAACR1_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01a ||
                ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01b ||
                ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01c)
            {
                Console.WriteLine("Module: Transform 'PISA BQ (2015 / 2018)' session2-zip files to log data to universal log format V01.");
                LogDataTransformer_PISA_BQ_V01.LogDataTransformer_PISABQ_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }

            else
            {
                Console.WriteLine("No input format specified. Specify either '" + 
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_ibsdraw01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_nepsrawv01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_irtlib01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01b + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01c + "' or '" + 
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_eeibraprawv01a + ".");
                return;
            }
        }
    }
}
