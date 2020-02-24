# LogFSM
 
![LogFSM Package Build Script](https://github.com/kroehne/LogFSM/workflows/LogFSM%20Package%20Build%20Script/badge.svg?branch=master)


## R Package 'LogFSM'

In this repository the software developed to implement the method described in the following paper is provided:

Kroehne, U., & Goldhammer, F. (2018). How to conceptualize, represent, and analyze log data from technology-based assessments? A generic framework and an application to questionnaire items. Behaviormetrika, 45 (2), 527–563.
https://doi.org/10.1007/s41237-018-0063-y

To install the package, type `source("http://logfsm.com/latest")` in R.

## Details about the Build

The source of the R package LogFSM can be found in the 'r/' folder of this repository. The LogFSM packge requires a dotnetcore worker application. The source of this application can be found in the 'vs/' folder of the repository. The compiled worker application is embedded in the R package in the in the folder 'r/inst/extdata/{platform}' for different platforms (linux-x64, win-x64 and osx-x64). The changelog and the reference for this package is published on http://www.logfsm.com, using the code in 'www/' folder.





