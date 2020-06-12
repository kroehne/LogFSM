#' Transform Log Data with LogFSM (Under Development)
#'
#' Function for transforming raw log data files (from different supported sources) to the universal log format.
#'
#' The transformation of log data from different platforms using the function 'TransformToUniversalLogFormat' is available for the
#' following source formats:
#'
#' - ```nepsrawv01a```: Folder(s) with extracted output files from NEPS TBT modules (created by DIPF/TBA).
#' - ```ibsdraw01a```: Folder(s) with log data generated with CBA 'ItemBuilder static delivery' (IBSD, provided by DIPF/TBA, CBA ItemBuilder >= 8.12, REACT).
#' - ```eeibraprawv01a```: Folder(s) with ZIP archives or XML files created with the CBA ItemBuilder Execution Environment (EE, provided by DIPF/TBA, CBA ItemBuilder <= 8.12, RAP).
#'
#'
#' The following flags are supported:
#'
#' -  NUMERICPERSONIDENTIFIER: The person identifier is created as numeric variable (instead of the default, a string variable).
#'
#' The following optional parameters can be provided in LogFSM:
#'
#' - ```PersonIdentifier```: Specify the name of the column that contains the person identifiers (default is 'PersonIdentifier').
#'
#' @param verbose Request verbose output messages.
#' @param inputfolders Input folder to be processed.
#' @param stataoutput Output file name for the generated universal log format, type Stata (i.e., absolute path to the zip file containing log data as Stata files, one file for each event type).
#' @param zcsvoutput Output file name for the generated universal log format, type zip-compressed CSV (i.e., absolute path to the zip file containing log data as CSV files, one file for each event type).
#' @param xlsxoutput Output file name for the generated universal log format, type XLSX (i.e., absolute path to XLSX file containing log data, one sheet for each event type).
#' @param inputformat File format of the raw log data to be processed (see above for valid options).
#' @param mask File filter mask. Only files that match the specified mask will be used (e.g., *.jsonl).
#' @param excludedelements A string variable refering to the element names (i.e., items, units or tasks), that should be ignored.
#' @param flags Optional flags as documented for the specific data formats (see below).
#' @param logversion Version information about the raw data (see below).
#' @param dictionary Dictionary file for the creation of an integrated codebook.
#' @param codebook Codebook file name (XLSX file). An XLSX file is created, which documents the generated log data as a codebook in universal log format.
#' @param table Concordance table file name (Stata, XLSX or CSV file). If the file exists and has two columns (PersonIdentifierOld and PersonIdentifierNew) it is used as a constants table. If a file name is specified that does not exist, a template for a concordance table is created.
#' @param ... (Further arguments will be passed on if necessary)
#'
#' @return The function returns TRUE, if a file was created.
#'
#' @export
#' @md
TransformToUniversalLogFormat <- function(inputfolders, stataoutput="", zcsvoutput="", xlsxoutput="",
                                          inputformat = "", mask="*.*",verbose=F, excludedelements = "",
                                          table="",dictionary="",codebook="",logversion="default",
                                          flags="", ...){

  cmd <- internalRunFSMWithConsole(job="transform",
                                   inputfolders=inputfolders,
                                   stataoutput=stataoutput,
                                   zcsvoutput=zcsvoutput,
                                   xlsxoutput=xlsxoutput,
                                   inputformat=inputformat,
                                   codebook=codebook,
                                   dictionary=dictionary,
                                   mask=mask,
                                   table=table,
                                   verbose=verbose,
                                   logversion=logversion,
                                   excludedelements=excludedelements,
                                   flags=flags,
                                   ...)

  return(file.exists(stataoutput) | file.exists(zcsvoutput) | file.exists(xlsxoutput))

}
