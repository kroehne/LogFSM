## ----setup, include = FALSE---------------------------------------------------
knitr::opts_chunk$set(
  collapse = TRUE,
  comment = "#>"
)

## ---- eval = FALSE------------------------------------------------------------
#  myinfolder <- ""    # Absolute file path to the directory containing ZIP archives
#                      # or XML files exported from the CBA ItemBuilder Execution Environment
#  mystatazip <- ""    # Absolute file path for a ZIP archive that will be created containing the
#                      # log data in the universal log format
#  mycodebook <- ""    # Absolute file path for the XLSX document that will be created containg the
#                      # codebook
#  mydictionary <- ""  # Absolute file path for a XLSX document with event-related platform-specific
#                      # documentation that will be used as dictionary while creating the codebook
#  
#  TransformToUniversalLogFormat(input = myinfolder
#                                stata = mystatazip,
#                                inputformat = "eeibraprawv01a",
#                                dictionary = mydictionary,
#                                codebook = mycodebook)

## ---- eval = FALSE------------------------------------------------------------
#  # create data for sheet 'metadata'
#  
#  m <- rbind(c("StudyName", "Name of the study" ,"Studienname"),
#             c("TestPlatform", "Software version","Version der Software"))
#  
#  colnames(m) <- c("Attribute","AttributeValue_ENG","AttributeValue_DE")

## ---- eval = FALSE------------------------------------------------------------
#  # create data for sheet 'head'
#  
#  h <- rbind(
#    c("ID", "ID for this line (counter over all cases and events)",
#            "ID für diese Zeile (Zähler über alle Fälle und Ereignisse)",
#      "line"),
#    c("PersonIdentifier", "ID of the person which triggered the event data in this row",
#                          "ID der Person, die das Log-Ereignis in dieser Zeile ausgelöst hat",
#      "target-person"),
#    c("Element", "Item or page name (source of the event data in this row)",
#                 "Item oder Seitennahme (Quelle der Eventdaten in dieser Zeile)",
#      "instrument-part"),
#    c("TimeStamp", "Time stamp for the event data in this line",
#                   "Zeitstempel für die Ereignisdaten in dieser Zeile",
#      "assessment-time"),
#    c("RelativeTime", "Relative time for the log event (milliseconds relative to the start)",
#                      "Relativzeit für das Log-Ereignis (Millisekunden relativ zum Start)",
#      "-"),
#    c("EventID", "ID for the current event (starts with 0 within the current 'Path')",
#                 "ID für das aktuelle Ereignis (beginnt mit 0 innerhalb des aktuellen 'Path')",
#      "event"),
#    c("ParentEventID", "ID of the parent event (used for the nested data structures of an event)",
#                       "ID des übergeordneten Ereignisses (wird für die geschachtelten Datenstrukturen eines Ereignisses verwendet)",
#      "event"),
#    c("Path", "Hierarchy of the nested data structure",
#              "Hierarchie der geschachtelten Datenstruktur",
#      "table"),
#    c("ParentPath", "Hierarchy of the parent element (empty for the elements at the root level)",
#                    "Hierarchie des Elternelements (leer für die Elemente auf der Wurzelebene)",
#      "table"),
#    c("EventName", "Name of the log event / data in this line",
#                   "Name des Logevents in dieser Zeile",
#      "-"))
#  
#  colnames(h) <- c("Column","VariableLable_ENG","VariableLable_DE","Identifies")

## ---- eval = FALSE------------------------------------------------------------
#  # create data for sheet 'events'
#  
#  e <- rbind(
#    c("ButtonLogEntry",
#      "Log.ButtonLogEntry",
#      "Jeder Interaktion eines Testteilnehmers mit einer Schaltfläche führt zum Speichern eines Log-Ereignisses vom Typ ButtonLogEntry. Neben dem Zeitstempel wird im Attribut a_id gespeichert, welche Schaltfläche gedrückt wurde. Die Namen der Schaltflächen sind als so-genannte user-defined ID innerhalb des Items festgelegt. ",
#      "Each interaction of a test participant with a button results in the saving of a log event of the type ButtonLogEntry. In addition to the timestamp, the a_id attribute stores which button was pressed. The names of the buttons are defined as a so-called user-defined ID within the item. ") # ...
#  )
#  
#  colnames(e) <- c("EventName","Table","EventDescription_DE","EventDescription_ENG")

## ---- eval = FALSE------------------------------------------------------------
#  # create data for sheet 'attributes'
#  
#  a <- rbind(
#    c("Log.CBAItemLogEntry",
#      "a_webClientUserAgent","-",
#      "User Agent String des Browsers.",
#      "User Agent String of the browser.",
#      "paradata ","-"),
#    c("Log.ButtonLogEntry",
#      "a_id","-",
#      "User-Defined ID der Schaltfläche, welche gedrückt wurde. Die Namen der Schaltflächen sind als so-genannte user-defined ID innerhalb des Items festgelegt. ",
#      "User-Defined ID of the button that was pressed. The names of the buttons are defined as a so-called user-defined ID within the item. ",
#      "-","-")# ...
#  )
#  
#  colnames(a)<- c("Table","Column","Condition","Description_DE","Description_ENG", "Anonymity", "Purification")
#  

## ---- eval = FALSE------------------------------------------------------------
#  
#  # combine datasets and write as xlsx file
#  
#  b <- createWorkbook(f)
#  
#  addWorksheet(b, "MetaData")
#  writeData(b, sheet = 1, m)
#  
#  addWorksheet(b, "Head")
#  writeData(b, sheet = 2, h)
#  
#  addWorksheet(b, "Events")
#  writeData(b, sheet = 3, e)
#  
#  addWorksheet(b, "Attributes")
#  writeData(b, sheet = 4, a)
#  
#  saveWorkbook(b, file = "dictionary.xlsx", overwrite = TRUE)

