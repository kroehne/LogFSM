#' Prepare Log Data from Different Formats for Use with LogFSM
#'
#' Log data originating from different assessment platforms or data sources can be used with the LogFSM package with the help of the function ```PrepareLogData```.
#' ```PrepareLogData``` will convert the data from a supported data format into the internal LogFSM representation (```logfsmjson```), which can be used for decomposing test-taking processes with the function ```RunFSMSyntax```.
#'
#' Data formats that require pre-processing using the function ```PrepareLogData```
#' to create the ```logfsmjson```(i.e. data that can be used with ```RunFSMSyntax```) are:
#' * ```dataflatv01a```: Flat table with log data (i.e., flat and sparse log data table)
#' * ```piaacldazip01a```: Text file with log data from the 'Programme for the International Assessment of Adult Competencies (PIAAC)', Round 1, exported from PIAAC LogDataAnalyzer (LDA)
#' * ```nepszip01a```: ZIP archive with data in the universal log data format used in the National Educational Panel Study (NEPS, TBT-studies 2009-2020)
#'
#' If data are provided as  ```dataflatv01a```, the following columns are expected:
#' - ```PersonIdentifier```: Identifier for the test-taker / person / student
#' - ```EventName```: Name of the log event
#' - ```Element```: Item, unit, task (part of the assessment) to which the event belongs
#' - ```TimeStamp``` or ```RelativeTime```: Timestamp (or relative time, if the flag ```RELATIVETIME``` is used)
#'
#' If data are provided as  ```nepszip01a```, the following columns are expected:
#' - ```PersonIdentifier```:  Identifier for the test-taker / person / student. This default can be overwritten by providing an additional argument 'ColumnNamePersonIdentifier,' which specifies the name of the column that contains the person identifier.
#' - ```EventName```: Name of the log event. This default can be overwritten by providing an additional argument 'ColumnNameEventName,' which specifies the name of the column that contains the event name (resp. event type).
#' - ```Element```: Item, unit, task (part of the assessemnt) to which the event belongs. This default can be overwritten by providing an additional argument 'ColumnNameElement,' which specifies the name of the column that contains the element (item name, unit name, etc.).
#' - ```TimeStamp``` or ```RelativeTime```: Timestamp (Absolut time stamps are expected. For relative times, use the flag ```RELATIVETIME```. This default can be overwritten by providing an additional argument 'ColumnNameTimeStamp,' which specifies the name of the column that contains the timestamp.
#'
#' For more details about the workflow see the vignette *LogFSM Workflows (Overview)*:
#' \code{vignette("Workflow", package = "LogFSM")}
#'
#' Additional formats (under development)
#' *  ```pisabqzip01a``` / ```pisabqzip01b``` / ```pisabqzip01c``` / ```pisacazip01a```
#'
#' @param zipfilename Name of the file containing the raw log data that should be used with LogFSM. 'PrepareLogData' will read from this file.
#' @param outfilename Name of the file in which 'PrepareLogData' will write the prepared log data.
#' @param workingdir Working directory (optional).
#' @param verbose Requesting more detailed output from LogFSM.
#' @param elements A string variable referring to the element names (i.e., items, units, or tasks) that should be extracted. Multiple elements can be combinded using ';'.
#' @param datafiletype Type of the data provided in the file ```zipfilename``` (either ```dataflatv01a``` or ```piaaczip01a```)
#' @param flags Optional flags as documented for the specific data formats. The flag "RELATIVETIME" can be used to prepare log data provided with relative timestamps. Multiple flags can be combined in the string variable using the pipe (|).
#' @param outputtimestampformatstring Format string for timestamps in the output (default: "dd.MM.yyyy hh:mm:ss.fff tt")
#' @param outputrelativetimeformatstring Format string for relative times in the output (default: "hh':'mm':'ss':'fff")
#' @param ... (Further arguments will be passed on if necessary)
#'
#' @return The function returns TRUE if a file was created.
#'
#' @export
#' @md

PrepareLogData <- function(zipfilename, outfilename, workingdir, verbose=F, elements = "", datafiletype = "dataflatv01a", flags="",
                           outputtimestampformatstring = "dd.MM.yyyy hh:mm:ss.fff tt",
                           outputrelativetimeformatstring = "hh':'mm':'ss':'fff", ...){

  if(missing(workingdir)){
    workingdir <- rappdirs::user_data_dir()
  }

  if(file.exists(file.path(workingdir,zipfilename))){
    zipfilename <- file.path(workingdir, zipfilename)
  }

  cmd <- internalRunFSMWithConsole(outfilename=outfilename,
                                   zipfilename=zipfilename,
                                   elements = elements,
                                   datafiletype = datafiletype,
                                   verbose = verbose,
                                   flags = flags,
                                   job="prepare", ...)

  return(file.exists(outfilename))

}
