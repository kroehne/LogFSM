.onLoad <- function(libname, pkgname){
 setPathVariable(pkgname)
}

.onAttach <- function(libname, pkgname){
  packageStartupMessage(paste0("- LogFSM  -- Version: ", utils::packageVersion("LogFSM")," -- See www.logfsm.com for updates. \n  For more information type ??LogFSM or help(package = \"LogFSM\")."))
}

setPathVariable <-  function(pkgname){

  osname <- Sys.info()["sysname"]
  ostype <- .Platform$OS.type
  is64 <- Sys.info()[["machine"]] == "x86_64" || Sys.info()[["machine"]] == "x86-64"

  logFSMWorkerFile <<- "LogFSMConsole.exe"
  logFSMworkerPath <<- file.path(system.file(package=pkgname), 'extdata/win-x64')

  if (osname == "Darwin"){  # MacOS (64 bit)
    logFSMWorkerFile <<- "LogFSMConsole"
    logFSMworkerPath <<- file.path(system.file(package=pkgname), 'extdata/osx-x64')
  }
  else if (ostype == "unix"){ # Linux (64 bit)
    logFSMWorkerFile <<- "LogFSMConsole"
    logFSMworkerPath <<- file.path(system.file(package=pkgname), 'extdata/linux-x64')
  }

  logFSMworkerPathAndExecutable <<- file.path(logFSMworkerPath, logFSMWorkerFile)

#  if(!file.exists(logFSMworkerPathAndExecutable)) {
#    stop(paste0("The binary worker '",logFSMworkerPathAndExecutable,"' was not found at '",logFSMworkerPath,"'. \nPackage will not work."))
#  }
#
#  logFSMWorkerTest <- system(paste0("\"",logFSMworkerPathAndExecutable,"\""," \"?job=info\""),intern=TRUE)
#  if (substr(logFSMWorkerTest,1,2) != "OK"){
#    stop(paste0("Package loading failed. ", logFSMWorkerTest))
#  }

}
