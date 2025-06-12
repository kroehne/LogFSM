rmarkdown::render_site(encoding = 'UTF-8', input="www")

current_version <- '0.4.5.35'

install_script <- c(paste0("cat('LogFSM - Install Version ",current_version, "\\n\\n')"),
                    paste0("current_version <- '",current_version,"'"),
                    "installed_version <- ''",
                    "if('LogFSM' %in% rownames(installed.packages())) {",
                    "  installed_version <- gsub('‘','',packageVersion('LogFSM'))",
                    "}",
                    "",
                    "if (current_version != installed_version){",
                    "list.of.packages <- c('remotes')",
                    "new.packages <- list.of.packages[!(list.of.packages %in% installed.packages()[,'Package'])]",
                    "if(length(new.packages)) {",
                    "  cat(paste('Install package(s):',new.packages))",
                    "  install.packages(new.packages)",
                    "}", 
                    "is64 <- Sys.info()[['machine']] == 'x86_64' || Sys.info()[['machine']] == 'x86-64'  || Sys.info()[['machine']] == 'arm64'",
                    "if (!is64){ ",
                    "  cat(paste('LogFSM is not available for:',Sys.info()[['machine']],'\n'))",
                    "  cat('Installation failed.') ",
                    "} else {",
                    paste0("  remotes::install_url('https://github.com/kroehne/LogFSM/releases/download/",current_version,"/LogFSM_",current_version,".tar.gz', build_vignettes=T)"),
                    "  cat('Done.\\nType library(LogFSM) to start. For more information type ??LogFSM or help(package = \"LogFSM\").')",
                    "}",
                    "} else {",
                    paste0("  cat('The current version ", current_version, " is installed.')"),
                    "}")

write(install_script, file = "www/_site/latest")
