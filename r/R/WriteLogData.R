#' Write Log Data
#'
#' Writes log data from data fram ```data``` to json format, that can be used to run FSM.
#'
#' The following columns names are taken as keywords:
#' * ```PersonIdentifier``` (required)
#' * ```TimeStamp``` (required)
#' * ```EventName``` (required)
#' * ```TimeDifferencePrevious``` (optional)
#'
#' @param file Filename
#' @param data Data frame with log data
#'
#' @export
#' @md
WriteLogData <- function(file,data){
  if (file.exists(file)){file.remove(file)}
  fileConn<-file(file)
  writeLines(jsonlite::toJSON(data, force=TRUE), fileConn)
  close(fileConn)
}
