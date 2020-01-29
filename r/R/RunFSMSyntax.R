#' Anylze Log Data with LogFSM
#'
#' Function for reconstructing the list of states using LogFSM. The syntax to be used is passed via the variable ```fsmsyntax```, the data to be used must be stored in the file
#' system and must be readable via the file name ```datafilename`` (single file) or ```zipfilename``` (ZIP archive).
#'
#' The following data formats that can be used directly (see parameter @datafiletype) either as single case or as zip archive with multiple files for multiple cases:
#'
#' * ```logfsmjson``` (Format created by LogFSM when pre-processing using the function ```PrepareLogData``` )
#' * ```eexml``` (Format created by the CBA ItemBuilder using the excecution environment.
#'
#' Data formats that require pre-processing using the function ```PrepareLogData``` to create
#' the ```logfsmjson```-format:
#'
#' * ```dataflatv01a```: Flat table with log data
#' * ```piaacr1ldazip01a```  (r1 = round 1, 2012; lda = Log Data Analyzer)
#'
#' Additional formats (under development)
#' * ```pisabqzip01a```, ```pisacazip01a```,  ```nepszip01a```
#
#' This function supports the following flags:
#' * ```RELATIVETIME``` The "RELATIVETIME" flag can be used to process log data that is provided with relative time stamps.
#' * ```DONT_ORDER_EVENTS``` The flag "DONT_ORDER_EVENTS" can be used to prevent the log events from being sorted by timestamp.
#' * ```ORDER_WITHIN_ELEMENTS```
#'
#' @param fsmsyntax String variable containing the FSM syntax to be processed. The syntax string must contain a valid LogFSM syntax in each line.
#' @param datafilename File name of a single data file (input).
#' @param zipfilename File name of a ZIP archive containing data files (input).
#' @param workingdir Working directory (optional).
#' @param outfilename File name of the (internal) file in which the results are cached before they are returned as a list (optional). If no file name is specified, a file "{TITEL}.json" is created, where {TITEL} is the title of the analysis.
#' @param fsmfilename File name for the temporary file in which the LogFSM syntax is stored (optional).
#' @param fsmfiletype Typ der Syntax (default '''custom01'''). Intended for a possible extension by another syntax to define the finite state machine.
#' @param datafiletype Type of data format (default ````jsonlite```) in which the log data prepared with the function ```PrepareLogData()``` are stored.
#' @param title Optional title of the analysis (will be used as file name)
#' @param flags List of flags. Multiple flages can be combined in the string variable using the pipe (|).
#' @param verbose Request verbose output messages.
#' @param maxnumberofcases Restrict number of processed cases (use -1 for all cases)
#' @param datafilefilter String mask, which is used to select files. If specified, only those files are processed whose file name matches the filter.
#' @param ... (Further arguments will be passed on if necessary)
#'
#' @return The function returns a list with the following components:
#'   * \code{AugmentedLogDataTable}
#'   * For each state machine $i$ the following tables are provided in the list
#'   ** \code{SequenceTable_i}
#'   ** \code{SequenceTable_i}
#'   ** ...
#'
#' @export
#' @md

RunFSMSyntax <- function(fsmsyntax, datafilename, zipfilename, title="title", workingdir, outfilename,
                         fsmfilename, fsmfiletype = "custom01", datafiletype = "jsonlite", verbose = F, flags = "",  maxnumberofcases= -1, datafilefilter="", ...){

  if(missing(workingdir)){
    workingdir <- rappdirs::user_data_dir()
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




