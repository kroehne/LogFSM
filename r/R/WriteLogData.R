#' Write Log Data
#'
#' Writes log data from data frame ```data``` to JSON format (```jsonlite```), that can be used to
#' run a finite-state machine with the function ```RunFSMSyntax.```
#'
#' The following columns names are taken as keywords:
#' * ```PersonIdentifier``` (required)
#' * ```TimeStamp``` (required)
#' * ```EventName``` (required)
#' * ```TimeDifferencePrevious``` (optional)
#'
#' @param file Filename
#' @param data Data frame with log data (*Flat and sparse log data table*)
#'
#' @export
#' @md
WriteLogData <- function(file,data){
  if (file.exists(file)){file.remove(file)}
  fileConn<-file(file)
  writeLines(jsonlite::toJSON(data, force=TRUE), fileConn)
  close(fileConn)
}
