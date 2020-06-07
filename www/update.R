rmarkdown::render_site(encoding = 'UTF-8', input="www")

# TODO: Add check for currently installed version of LogFSM!

install_script <- c("cat('LogFSM - Install\\n\\n')",
                    "list.of.packages <- c('remotes')",
                    "new.packages <- list.of.packages[!(list.of.packages %in% installed.packages()[,'Package'])]",
                    "if(length(new.packages)) {",
                    "  cat(paste('Install package(s):',new.packages))",
                    "  install.packages(new.packages)",
                    "}",
                    "osname <- Sys.info()['sysname']",
                    "ostype <- .Platform$OS.type",
                    "is64 <- Sys.info()[['machine']] == 'x86_64' || Sys.info()[['machine']] == 'x86-64'",
                    "if (!is64){ ",
                    "  cat(paste('LogFSM is not available for:',Sys.info()[['machine']],'\n'))",
                    "  cat('Installation failed.') ",
                    "} else {",
                    "  remotes::install_url('https://github.com/kroehne/LogFSM/releases/download/0.4.5.2/LogFSM_0.4.5.2.tar.gz')",
                    "  cat('Done.\\nType library(LogFSM) to start. For more information type ??LogFSM or ?vignette(package=\"LogFSM\").')",
                    "}")

write(install_script, file = "www/_site/latest")
