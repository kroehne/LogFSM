## ----setup, include = FALSE---------------------------------------------------
knitr::opts_chunk$set(
  collapse = TRUE,
  comment = "#>"
)
  
 `%>%` <- magrittr::`%>%`

## ---- eval=F, echo=T, warning=F-----------------------------------------------
#  View(tab)

## ---- eval=T, echo=F, warning=F-----------------------------------------------
tab <- LogFSM::SyntheticLogData(example = "ex01a")
tab$Name[tab$EventName == "Radiobutton"] <- paste0("selected&value=",tab$Name[tab$EventName == "Radiobutton"])

tab$EventName <- paste0("Event:",tab$EventName )
missing_timestamp <- tab[2,2]
tab[2,2]<-NA
colnames(tab) <- c("ID","Time","Event","Page","Item","Name","Action")
tab %>% kableExtra::kable() %>%  kableExtra::kable_styling(font_size = 12)

## ---- eval=T, echo=T, warning=F-----------------------------------------------
names(tab)[names(tab)=="ID"] <- "PersonIdentifier"
names(tab)[names(tab)=="Time"] <- "TimeStamp"
names(tab)[names(tab)=="Event"] <- "EventName"

names(tab)

## ---- eval=T, echo=T, warning=F-----------------------------------------------
# Event names contain ':'
unique(tab$EventName) 

# Remove 'Event:' with gsub()
tab$EventName <- gsub("Event:","",tab$EventName)

# Event names ready for LogFSM
unique(tab$EventName) 

## ---- eval=T, echo=T, warning=F-----------------------------------------------
# Attribute 'Name' contains '&'
unique(tab$Name) 

# Remove 'selected&value=' with gsub()
tab$Name <- gsub("selected&value=","",tab$Name)

# Event-specific values of attribute 'Name' ready for LogFSM
unique(tab$Name) 

## ---- eval=T, echo=T, warning=F-----------------------------------------------
tab[2,2] <- missing_timestamp

## ---- eval=T, echo=T, warning=F-----------------------------------------------
tab$Element <- "Item01"

## ---- eval=F, echo=T, warning=F-----------------------------------------------
#  View(tab)

## ---- eval=T, echo=F, warning=F-----------------------------------------------
tab %>% kableExtra::kable() %>%  kableExtra::kable_styling(font_size = 12)

## ---- eval=T, echo=T, warning=F-----------------------------------------------
tab <- tab[tab$Element == "Item01",]

## ---- eval=T, echo=T, warning=F-----------------------------------------------
tmpfile <- tempfile(fileext = ".json")
LogFSM::WriteLogData(file=tmpfile, data = tab)

## ---- eval=T, echo=T, warning=F-----------------------------------------------
tmpfile <- tempfile(fileext = "")
LogFSM::WriteLogData(file=tmpfile, data = tab)

