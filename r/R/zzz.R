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
  isarm <- Sys.info()[["machine"]] == "arm64" || Sys.info()[["machine"]] == "arm"

  logFSMWorkerFile <<- "LogFSMConsole.exe"
  logFSMworkerPath <<- file.path(system.file(package=pkgname), 'extdata/win-x64')

  if (osname == "Darwin"){  # MacOS (64 bit)
    if (isarm){
      logFSMWorkerFile <<- "LogFSMConsole"
      logFSMworkerPath <<- file.path(system.file(package=pkgname), 'extdata/osx-arm64')
    } else {
      logFSMWorkerFile <<- "LogFSMConsole"
      logFSMworkerPath <<- file.path(system.file(package=pkgname), 'extdata/osx-x64')
    }

  } else if (ostype == "unix"){ # Linux (64 bit)
    logFSMWorkerFile <<- "LogFSMConsole"
    logFSMworkerPath <<- file.path(system.file(package=pkgname), 'extdata/linux-x64')
  }

  logFSMworkerPathAndExecutable <<- file.path(logFSMworkerPath, logFSMWorkerFile)

}
