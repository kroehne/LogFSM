#' Prepare Log Data form Different Fromats for Use with LogFSM
#'
#' Differtent data sources can be used with the LogFSM package. In order to harmonize the processing, data
#' must be converted in the internal LogFSM format (```logfsmjson```) using this function. The function must be
#' called with the filename (```zipfilename```) in which the data are stored and a filename (```outfilename```)
#' in which the prepared data are written.
#'
#' Data formats that require pre-processing using the function ```PrepareLogData```
#' to create the ```logfsmjson```(i.e. data that can be used with ```RunFSMSyntax```)
#' * ```dataflatv01a```: Flat table with log data
#' * ```piaaczip01a```: Text file with log data from the Programme for the International Assessment of Adult Competencies (PIAAC), exported from PIAAC LogDataAnalyzer (R1)
#' * ```nepszip01a```: ZIP archive with data in generic log data format used in the National Educational Panel Study (NEPS, TBT-studies 2009-2020)
#'
#' If data are provided as  ```dataflatv01a```, the following columns are expected:
#' - ```PersonIdentifier```: Identifier for the test-taker / person / student
#' - ```EventName```: Name of the log event
#' - ```Element```: Item, unit, task (part of the assessemnt) to which the event belongs
#' - ```TimeStamp``` or ```RelativeTime```: Timestamp (or relative time, if the flag ```RELATIVETIME``` is used)
#'
#' If data are provided as  ```nepszip01a```, the following columns are expected:
#' - ```PersonIdentifier```:  Identifier for the test-taker / person / student (Provide a value as argument 'ColumnNamePersonIdentifier' to override this default.)
#' - ```EventName```: Name of the log event (Provide a value as argument 'ColumnNameEventName' to override this default.)
#' - ```Element```: Item, unit, task (part of the assessemnt) to which the event belongs (Provide a value as argument 'ColumnNameElement' to override this default.)
#' - ```TimeStamp``` or ```RelativeTime```: Timestamp (Absolut time stamps are expected. For relative times use the flag ```RELATIVETIME```. Provide a value as argument 'ColumnNameTimeStamp' to override the default column name.)
#'
#'
#' Additional formats (under development)
#' *  ```pisabqzip01a``` / ```pisabqzip01b``` / ```pisabqzip01c``` / ```pisacazip01a```
#'
#'
#' @param zipfilename Name of the file containing the raw log data that should be used with LogFSM. 'PrepareLogData' will read from this file.
#' @param outfilename Name of the file in which 'PrepareLogData' will write the prepared log data.
#' @param workingdir Working directory (optional).
#' @param verbose Requesting more detailed output from LogFSM.
#' @param elements A string variable refering to the element names (i.e., items, units or tasks), that should be extracted. Multiple elemtns can be combinded using ';'.
#' @param datafiletype Type of the data provided in the file ```zipfilename``` (either ```dataflatv01a``` or ```piaaczip01a```)
#' @param flags Optional flags as documented for the specific data formats. The flag "RELATIVETIME" can be used to prepare log data provided with relative timestamps. Multiple flages can be combined in the string variable using the pipe (|).
#' @param ... (Further arguments will be passed on if necessary)
#'
#' @return The function returns TRUE, if a file was created.
#'
#' @export
#' @md

PrepareLogData <- function(zipfilename, outfilename, workingdir, verbose=F, elements = "", datafiletype = "dataflatv01a", flags="", ...){

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
