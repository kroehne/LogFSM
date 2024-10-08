% Generated by roxygen2: do not edit by hand
% Please edit documentation in R/RunFSMSyntax.R
\name{RunFSMSyntax}
\alias{RunFSMSyntax}
\title{Anylze Log Data with LogFSM}
\usage{
RunFSMSyntax(
  fsmsyntax,
  datafilename,
  zipfilename,
  title = "title",
  workingdir,
  outfilename,
  fsmfilename,
  fsmfiletype = "custom01",
  datafiletype = "jsonlite",
  verbose = F,
  flags = "",
  maxnumberofcases = -1,
  datafilefilter = "",
  outputtimestampformatstring = "dd.MM.yyyy hh:mm:ss.fff tt",
  outputrelativetimeformatstring = "hh':'mm':'ss':'fff",
  ...
)
}
\arguments{
\item{fsmsyntax}{String variable containing the FSM syntax to be processed. The syntax string must include a valid LogFSM syntax segment in each line.}

\item{datafilename}{The filename of a single data file (input).}

\item{zipfilename}{The filename of a ZIP archive containing data files (input).}

\item{title}{Optional title of the analysis (will be used as filename).}

\item{workingdir}{The working directory (optional).}

\item{outfilename}{The filename of the (internal) JSON file in which the results are written before they are read in the R environment and returned as a list. This argument is optional. If no filename is specified, a file "{TITEL}.json" is created, where {TITEL} is the title of the analysis.}

\item{fsmfilename}{The filename for the temporary (internal) file in which the LogFSM syntax is written. This argument is optional.}

\item{fsmfiletype}{This parameter is intended for a possible extension by another syntax to define the finite-state machine.}

\item{datafiletype}{Type of the data format in which the log data are provided (default is ````jsonlite```).}

\item{verbose}{Request verbose output messages.}

\item{flags}{List of flags. Multiple flags can be combined in the string variable using the pipe (|).}

\item{maxnumberofcases}{Restrict the number of processed cases (use -1 to run the FSM for all cases that fit the \code{datafilefilter}  argument).}

\item{datafilefilter}{String mask, which is used to select files. If specified, only those files are processed whose file name matches the filter.}

\item{outputtimestampformatstring}{Format string for timestamps in the output (default: "dd.MM.yyyy hh:mm:ss.fff tt")}

\item{outputrelativetimeformatstring}{Format string for relative times in the output (default: "hh':'mm':'ss':'fff")}

\item{...}{(Further arguments will be passed on if necessary)}
}
\value{
The function returns a list with the following components (i as machine index):
\itemize{
\item \code{AugmentedLogDataTable}: The \code{AugmentedLogDataTable} created using the provided finite-state machine syntax.
\item \code{SequenceTable_i}: The \code{SequenceTable} created using the provided finite-state machine syntax (index i).
\item \code{TransitionFrequencyTable_i}: The \code{TransitionFrequencyTable} created using the provided finite-state machine syntax (index i).
\item \code{StateSummaryTable_i}: The \code{StateSummaryTable} created using the provided finite-state machine syntax (index i).
\item \code{NGramTable_i}: The \code{NGramTable} created using the provided finite-state machine syntax (index i).
\item \code{StateEventFrequencyTable_i}: The \code{StateEventFrequencyTable} created using the provided finite-state machine syntax (index i).
\item \code{VariableValueTable}: The \code{VariableValueTable} created using the provided finite-state machine syntax.
\item \code{TransitionFrequencyGraph} Tot graph syntax to plot the \code{TransitionFrequencyGraph}. (Use the function DiagrammeR::grViz() to plot this graph.)
\item \code{UmlDotGraph}: Dot graph syntax to plot the \code{UmlDotGraph}. (Use the function DiagrammeR::grViz() to plot this graph.)
\item \code{title}: The title of the analysis.
\item \code{lastfsmsyntax}: The last error message reported by the LogFSM-Console.
\item \code{cmd}: The internal command used to call the LogFSMConsole.
}
}
\description{
Main function of LogFSM for reconstructing the sequence of states using finite-state machines.
The finite-state machine syntax must be provided as string variable \code{fsmsyntax} with multiple
lines or as a vector of strings (line-by-line). The data that should be used
must be stored locally in the filesystem before calling \code{RunFSMSyntax}. The name of the data
should be provided as argument \code{datafilename} (for single files) or as argument
\code{zipfilename} (for ZIP archives with multiple data files).
}
\details{
For more details about the workflow, see the vignette \emph{LogFSM Workflows (Overview)}:
\code{vignette("Workflows", package = "LogFSM")}

For more details about the syntax, see the vignette \emph{LogFSM Syntax (Input)}:
\code{vignette("Syntax", package = "LogFSM")}

For more details about the output, see the vignette \emph{LogFSM Tables (Output)}:
\code{vignette("Tables", package = "LogFSM")}

The following data formats can be used directly (see parameter @datafiletype) either
as single files (\code{datafilename}) or as ZIP archives with multiple files (\code{zipfilename}):
\itemize{
\item \code{logfsmjson}: Format created by LogFSM when pre-processing using the function \code{PrepareLogData.}
\item \code{universal01} : Format created by LogFSM when calling the function \code{TransformToUniversalLogFormat.}
\item \code{eexml}: Format created by the CBA ItemBuilder using the excecution environment.
}

Data formats that require pre-processing using the function \code{PrepareLogData} to create
the \code{logfsmjson}-format are:
\itemize{
\item \code{dataflatv01a}: \emph{Flat and sparse log data table}
\item \code{nepszip01a}: \emph{Universal log format} (currently only stata files are supported; under development)
\item \code{piaacr1ldazip01a}: Date exported from the PIAAC LogData Analyzer  (r1 = round 1, 2012; lda = Log Data Analyzer)
}

Additional formats (under development) are:
\itemize{
\item \code{pisabqzip01a}, \code{pisacazip01a}
This function supports the following flags:
\item \code{RELATIVETIME}: The "RELATIVETIME" flag can be used to process log data that is provided with relative timestamps.
\item \code{DONT_ORDER_EVENTS}: The flag "DONT_ORDER_EVENTS" can be used to prevent the log events from being sorted by timestamp.
\item \code{ORDER_WITHIN_ELEMENTS}: The flag "ORDER_WITHIN_ELEMENTS" can be used to request sorting events by timestamp within elements.
}
}
