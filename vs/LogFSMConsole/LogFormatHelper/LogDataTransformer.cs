#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
#endregion


namespace LogFSMConsole
{
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
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'IBSD 01A' (Raw files provided by the assessment platform ItemBuilder Static Delivery, IBSD).");
                Console.ResetColor();

                LogDataTransformer_IBSD_V01.LogDataTransformer_IBSD_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_irtlib01a)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'IRTlib 01A' (Raw files provided by the assessment platform IRTlib).");
                Console.ResetColor();

                LogDataTransformer_IRTlibPlayer_V01.LogDataTransformer_IRTLIB_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_alea01a)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'Alea 01A' (Raw files provided by the alea.schule assessment platform).");
                Console.ResetColor(); 
                LogDataTransformer_IRTlibPlayer_V01.LogDataTransformer_ALEA_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_ibfirebase01a)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'IBFirebase' (Experimental platform using firebase for CBA ItemBuilder hosting).");
                Console.ResetColor();

                LogDataTransformer_Firebase_V01.LogDataTransformer_Firebase_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_nepsrawv01a)
            {  
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'NEPS-RAP' (Raw files provided by the assessment platform used in NEPS with RAP ItemBuilder).");
                Console.ResetColor();

                LogDataTransformer_NEPS_V01.LogDataTransformer_NEPS_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_eeibraprawv01a)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'CBA ItemBuilder RAP Execution Environment' (data provided by CBA ItemBuilder Execution Environment for RAP ItemBuilder).");
                Console.ResetColor();

                LogDataTransformer_EE_RAP_V01.LogDataTransformer_EE_RAP_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_piiacr1txtv01a)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'PIAAC R1' (TXT files with preprocessed raw log data from https://piaac-logdata.tba-hosting.de/).");
                Console.ResetColor();

                LogDataTransformer_PIAAC_R1_V01.LogDataTransformer_PIAACR1_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_taopci01a)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'TAOPCI 01A' (CSV files exported from TAO using https://github.com/DIPFtba/fastib2pci).");
                Console.ResetColor();

                LogDataTransformer_TAOPCI_V01.LogDataTransformer_TAOPCI_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_taopci02a)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'TAOPCI 02A' (CSV files exported from TAO using https://github.com/DIPFtba/fastib2pci and external data storage).");
                Console.ResetColor();

                LogDataTransformer_TAOPCI_V02.LogDataTransformer_TAOPCI_Module_V02.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01a ||
                ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01b)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'PISA Background Questionnaire' (2015 / 2018, data form session2-zip files).");
                Console.ResetColor();

                LogDataTransformer_PISA15to18BQ_Module_V01.LogDataTransformer_PISA15to18BQ_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01c)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'PISA Background Questionnaire' (2022, data form session2-zip files).");
                Console.ResetColor();

                LogDataTransformer_PISA22BQ_Module_V01.LogDataTransformer_PISA22BQ_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisacazip01a ||
                ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisacazip01b)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'PISA Cognitive Assessment' (2015 / 2018, data form session1-zip files).");
                Console.ResetColor();

                LogDataTransformer_PISA15to18CA_Module_V01.LogDataTransformer_PISA15to18CA_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisacazip01c)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'PISA Cognitive Assessment' (2022, data form session1-zip files).");
                Console.ResetColor();

                LogDataTransformer_PISA22CA_Module_V01.LogDataTransformer_PISA22CA_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_timsst19zip01a)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'TIMSS T19'.");
                Console.ResetColor();

                LogDataTransformer_TIMSSeT19_V01.LogDataTransformer_TIMSSeT19_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }
            else if (ParsedCommandLineArguments.Transform_InputFormat == CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisa2012zip01a)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Module: Transform 'PISA Log Data 2012' (zip archives with SPSS files downloaded from OECD).");
                Console.ResetColor();
        
                LogDataTransformer_PISA12CA_Module_V01.LogDataTransformer_PISA12CA_Module_V01.ProcessLogFilesOnly(Watch, ParsedCommandLineArguments);
            }


            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No input format specified. Specify either '" + 
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_ibsdraw01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_nepsrawv01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_irtlib01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_alea01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_ibfirebase01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_taopci01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_taopci02a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_timsst19zip01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisa2012zip01a + "', '" + 
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01a + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01b + "', '" +
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_pisabqzip01c + "' or '" + 
                    CommandLineArguments._CMDA_JOB_TRANSFORM_input_eeibraprawv01a + "'.");
                Console.ResetColor();
                return;
            }
        }
    }
}
