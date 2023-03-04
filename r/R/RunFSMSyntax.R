#' Anylze Log Data with LogFSM
#'
#' Main function of LogFSM for reconstructing the sequence of states using finite-state machines.
#' The finite-state machine syntax must be provided as string variable ```fsmsyntax``` with multiple
#' lines or as a vector of strings (line-by-line). The data that should be used
#' must be stored locally in the filesystem before calling ```RunFSMSyntax```. The name of the data
#' should be provided as argument ```datafilename``` (for single files) or as argument
#' ```zipfilename``` (for ZIP archives with multiple data files).
#'
#' For more details about the workflow, see the vignette *LogFSM Workflows (Overview)*:
#' \code{vignette("Workflow", package = "LogFSM")}
#'
#' For more details about the syntax, see the vignette *LogFSM Syntax (Input)*:
#' \code{vignette("Syntax", package = "LogFSM")}
#'
#' For more details about the output, see the vignette *LogFSM Tables (Output)*:
#' \code{vignette("Tables", package = "LogFSM")}
#'
#' The following data formats can be used directly (see parameter @datafiletype) either
#' as single files (```datafilename```) or as ZIP archives with multiple files (```zipfilename```):
#'
#' * ```logfsmjson```: Format created by LogFSM when pre-processing using the function ```PrepareLogData.```
#' * ```universal01``` : Format created by LogFSM when calling the function ```TransformToUniversalLogFormat.```
#' * ```eexml```: Format created by the CBA ItemBuilder using the excecution environment.
#'
#' Data formats that require pre-processing using the function ```PrepareLogData``` to create
#' the ```logfsmjson```-format are:
#'
#' * ```dataflatv01a```: *Flat and sparse log data table*
#' * ```nepszip01a```: *Universal log format* (currently only stata files are supported; under development)
#' * ```piaacr1ldazip01a```: Date exported from the PIAAC LogData Analyzer  (r1 = round 1, 2012; lda = Log Data Analyzer)
#'
#' Additional formats (under development) are:
#' * ```pisabqzip01a```, ```pisacazip01a```
#
#' This function supports the following flags:
#' * ```RELATIVETIME```: The "RELATIVETIME" flag can be used to process log data that is provided with relative timestamps.
#' * ```DONT_ORDER_EVENTS```: The flag "DONT_ORDER_EVENTS" can be used to prevent the log events from being sorted by timestamp.
#' * ```ORDER_WITHIN_ELEMENTS```: The flag "ORDER_WITHIN_ELEMENTS" can be used to request sorting events by timestamp within elements.
#'
#' @param fsmsyntax String variable containing the FSM syntax to be processed. The syntax string must include a valid LogFSM syntax segment in each line.
#' @param datafilename The filename of a single data file (input).
#' @param zipfilename The filename of a ZIP archive containing data files (input).
#' @param workingdir The working directory (optional).
#' @param outfilename The filename of the (internal) JSON file in which the results are written before they are read in the R environment and returned as a list. This argument is optional. If no filename is specified, a file "{TITEL}.json" is created, where {TITEL} is the title of the analysis.
#' @param fsmfilename The filename for the temporary (internal) file in which the LogFSM syntax is written. This argument is optional.
#' @param fsmfiletype This parameter is intended for a possible extension by another syntax to define the finite-state machine.
#' @param datafiletype Type of the data format in which the log data are provided (default is ````jsonlite```).
#' @param title Optional title of the analysis (will be used as filename).
#' @param flags List of flags. Multiple flags can be combined in the string variable using the pipe (|).
#' @param verbose Request verbose output messages.
#' @param maxnumberofcases Restrict the number of processed cases (use -1 to run the FSM for all cases that fit the ```datafilefilter```  argument).
#' @param datafilefilter String mask, which is used to select files. If specified, only those files are processed whose file name matches the filter.
#' @param outputtimestampformatstring Format string for timestamps in the output (default: "dd.MM.yyyy hh:mm:ss.fff tt")
#' @param outputrelativetimeformatstring Format string for relative times in the output (default: "hh':'mm':'ss':'fff")
#'
#' @param ... (Further arguments will be passed on if necessary)
#'
#' @return The function returns a list with the following components (i as machine index):
#'   * \code{AugmentedLogDataTable}: The ```AugmentedLogDataTable``` created using the provided finite-state machine syntax.
#'   * \code{SequenceTable_i}: The ```SequenceTable``` created using the provided finite-state machine syntax (index i).
#'   * \code{TransitionFrequencyTable_i}: The ```TransitionFrequencyTable``` created using the provided finite-state machine syntax (index i).
#'   * \code{StateSummaryTable_i}: The ```StateSummaryTable``` created using the provided finite-state machine syntax (index i).
#'   * \code{NGramTable_i}: The ```NGramTable``` created using the provided finite-state machine syntax (index i).
#'   * \code{StateEventFrequencyTable_i}: The ```StateEventFrequencyTable``` created using the provided finite-state machine syntax (index i).
#'   * \code{VariableValueTable}: The ```VariableValueTable``` created using the provided finite-state machine syntax.
#'   * \code{TransitionFrequencyGraph} Tot graph syntax to plot the ```TransitionFrequencyGraph```. (Use the function DiagrammeR::grViz() to plot this graph.)
#'   * \code{UmlDotGraph}: Dot graph syntax to plot the ```UmlDotGraph```. (Use the function DiagrammeR::grViz() to plot this graph.)
#'   * \code{title}: The title of the analysis.
#'   * \code{lastfsmsyntax}: The last error message reported by the LogFSM-Console.
#'   * \code{cmd}: The internal command used to call the LogFSMConsole.
#'
#' @export
#' @md

RunFSMSyntax <- function(fsmsyntax, datafilename, zipfilename, title="title", workingdir, outfilename,
                         fsmfilename, fsmfiletype = "custom01", datafiletype = "jsonlite", verbose = F, flags = "",
                         maxnumberofcases= -1, datafilefilter="",
                         outputtimestampformatstring = "dd.MM.yyyy hh:mm:ss.fff tt",
                         outputrelativetimeformatstring = "hh':'mm':'ss':'fff", ...){

  if(missing(workingdir)){
    workingdir <- path.expand(rappdirs::user_data_dir())
  }

  if(!missing(datafilename)){
    if(file.exists(file.path(workingdir, datafilename))){
      datafilename <- file.path(workingdir, datafilename)
    }
  }

  if(missing(fsmfilename)){
    fsmfilename <- file.path(workingdir, paste0(title,".fsm"))
  }

  if(missing(outfilename)){
    outfilename <- file.path(workingdir,paste0(title,".json"))
  }

  fileConn<-file(fsmfilename)
  writeLines(fsmsyntax, fileConn)
  close(fileConn)

  cmd <- internalRunFSMWithConsole(fsmfilename=fsmfilename,
                            outfilename=outfilename,
                            datafilename=datafilename,
                            zipfilename=zipfilename,
                            fsmfiletype = fsmfiletype,
                            datafiletype = datafiletype,
                            job="fsm",
                            verbose = verbose,
                            flags = flags,
                            maxnumberofcases = maxnumberofcases,
                            datafilefilter=datafilefilter,
                            ...)

  if (file.exists(outfilename)){
    res <- internalReadFSMResultsFromJSON(outfilename, workingdir)
  } else {
    res <- list()
  }
  res$title <- title
  res$lastfsmsyntax <- fsmsyntax

  res$cmd = cmd
  res

}


internalRunFSMWithConsole <- function(fsmfilename,
                                      outfilename,
                                      datafilename,
                                      zipfilename,
                                      elements,
                                      datafiletype,
                                      fsmfiletype,
                                      job,
                                      verbose,
                                      flags,
                                      maxnumberofcases,
                                      datafilefilter,
                                      ...){

  if(!exists("logFSMworkerPathAndExecutable") ) {
    setPathVariable("LogFSM")
  }

  o <- getwd()

  parameters <- ""

  if(!missing(job)){
    parameters <- paste0(parameters,"job=",utils::URLencode(job),"&")
  }

  if(!missing(datafilename)){
    parameters <- paste0(parameters,"datafilename=",utils::URLencode(datafilename),"&")
  }

  if(!missing(fsmfilename)){
    parameters <- paste0(parameters,"fsmfilename=",utils::URLencode(fsmfilename),"&")
  }

  if(!missing(outfilename)){
    parameters <- paste0(parameters,"outfilename=",utils::URLencode(outfilename),"&")
  }

  if(!missing(zipfilename)){
    parameters <- paste0(parameters,"zipfilename=",utils::URLencode(zipfilename),"&")
  }

  if(!missing(elements)){
    parameters <- paste0(parameters,"elements=",utils::URLencode(elements),"&")
  }

  if(!missing(datafiletype)){
    parameters <- paste0(parameters,"datafiletype=",utils::URLencode(datafiletype),"&")
  }

  if(!missing(fsmfiletype)){
    parameters <- paste0(parameters,"fsmfiletype=",utils::URLencode(fsmfiletype),"&")
  }

  if(!missing(datafilefilter)){
    parameters <- paste0(parameters,"datafilefilter=",utils::URLencode(datafilefilter),"&")
  }

  if(!missing(flags)){
    parameters <- paste0(parameters,"flags=",utils::URLencode(flags),"&")
  }

  if(!missing(maxnumberofcases)){
    parameters <- paste0(parameters,"maxnumberofcases=",maxnumberofcases,"&")
  }

  if(!missing(verbose)){
    parameters <- paste0(parameters,"verbose=",verbose,"&")
  }

  x <- list(...)
  if (length(x)>0){
    for (i in 1:length(x)){
      parameters <- paste0(parameters, names(x)[i],"=",utils::URLencode(paste0(x[[i]])),"&")
    }
  }

  if (!exists("logFSMworkerPathAndExecutable")){
    setPathVariable("LogFSM")
  }

  if (verbose)
    print(parameters)

  cmd <- paste0("\"",logFSMworkerPathAndExecutable,"\""," \"?",parameters,"\"")
  setwd(logFSMworkerPath)
  system(cmd)
  setwd(o)

  cmd
}

internalReadFSMResultsFromJSON <- function(outfile, workingdir){
  res <- jsonlite::fromJSON(outfile)$Tables
  res$AugmentedLogDataTable[res$AugmentedLogDataTable=="##NA##"]<-NA

  if(file.exists(file.path(workingdir, "logfsmlasterror.txt"))){
    res$logfsmlasterror <- readLines(file.path(workingdir, "logfsmlasterror.txt"), warn = F)
  } else {
    res$logfsmlasterror <- ""
  }

  # read json representation of parsed fsm syntax (if file exists)

  if(file.exists(file.path(workingdir, "logfsmlastfsmjson.txt"))){

    logfsmlastfsmjson <- jsonlite::fromJSON(file.path(workingdir, "logfsmlastfsmjson.txt"))
    res$logfsmlastfsmjson <- readLines(file.path(workingdir, "logfsmlastfsmjson.txt"), warn = F)

    # read UML DOT graph for each machine (if file exists)

    if (logfsmlastfsmjson$NumberOfMachines >= 1){
      res$TransitionFrequencyGraph<- list()
      res$UmlDotGraph <- list()
      for (i in 1:logfsmlastfsmjson$NumberOfMachines){
        if(file.exists(file.path(workingdir, paste0("logfsmmachine_",(i-1),".txt")))){
          res$UmlDotGraph[[i]] <- readLines(file.path(workingdir, paste0("logfsmmachine_",(i-1),".txt")), warn = F)
        } else {
          res$UmlDotGraph[[i]] <- ""
        }

        if(file.exists(file.path(workingdir, paste0("logfsmtransitionfrequencygraph_",(i-1),".txt")))){
          res$TransitionFrequencyGraph[[i]] <- readLines(file.path(workingdir, paste0("logfsmtransitionfrequencygraph_",(i-1),".txt")), warn = F)
        } else {
          res$TransitionFrequencyGraph[[i]] <- ""
        }
      }
    }

  } else {
    res$logfsmlastfsmjson <- ""
  }

  res
}




