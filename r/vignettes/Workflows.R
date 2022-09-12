## ----setup, include = FALSE---------------------------------------------------
knitr::opts_chunk$set(
  collapse = TRUE,
  comment = "#>"
)
 

## ---- echo=F, out.width="100%"------------------------------------------------
 
p1 <-" digraph G {
           
          graph [ranksep='0.2'];            
          node [shape=record, fontname = Arial];
          PreProcess  [shape=ellipse, label = <Prepared data table <br/> (any data that fit into  a flat format,<br/> use <i>write.csv(df, file=gzfile(file)</i><br/>to write to disk)'>];
          
          Columns  [shape=ellipse, label = <Name / provide required columns <br/>(PersonIdentifier, EventName, <br/> Element, and either Timestamp <br/> or RelativeTime) >];
          Separate  [shape=ellipse, label = <Separate and trim event<br/> specific data in distinct <br/>columns, remove special <br/>characters (if necessary) >];
          Simplify  [shape=ellipse, label = <Simplify JSON or XML <br/> structure into simple <br/>key-value pairs>];
          Ohter [shape=ellipse, label = <Other file formats...>];
          Filter [shape=ellipse label = 'Filter (optional)']
          
          PrepareLogData  [shape=ellipse, label = <Function <br/>PrepareLogData(...)>];
          InputFile [label = <File name of the input file>]
          OutputFile [label = <File name of the output file>]
          ElemeentFilter  [label = <Element name or names<br/>(separatedb by ;)<br/>to be included>]
          
          Separate -> PreProcess
          Columns -> PreProcess
          Simplify -> PreProcess
          PreProcess -> InputFile [label = <datafiletype = \"dataflatv01a\">] 
          Ohter -> InputFile [label = <e.g., datafiletype = <br/> \"piaacldazip01a\">] 
          
          Flags [label = <Additional flags, e.g., <br/>RELATIVETIME to use <br/><i>relative times</i> instead <br/>of <i>timestamps</i> >]
            
          InputFile-> PrepareLogData [label = <zipfilename = >]
          OutputFile-> PrepareLogData [label = <outfilename = >]
           Flags-> PrepareLogData [label = <flags = >]
           Filter->PrepareLogData
           ElemeentFilter -> Filter [label = <elements = >]
      }" 
 
DiagrammeR::grViz(p1)
 

## ---- echo=F, out.width="100%"------------------------------------------------
 
p1 <-" digraph G {
          
          graph [ranksep='0.2'];            
          node [shape=record, fontname = Arial];
          RunFSMSyntax [shape=ellipse, label = <Function <br/>RunFSMSyntax(...)>];
          FSMSyntax [label =<Prepare a string variable in R that <br/> contains the FSM syntax (one statement <br/> in  each line).  Before running <br/>RunFSMSyntax(), the string variable<br/> must be updated, if it was changed <br/> in the syntax editor.> ];
          PreparedData [shape=ellipse label = 'Data']
          DataFilter [shape=ellipse label = 'Filter (optional)']
          FileNameFilter [label = <Filename filter (e.g. 'DEU*' ) <br/> to use only files with a name  <br/> starting with the text 'DEU'>]
          MaxCaseFilter [label = <Maximal number of cases <br/>or -1 (default), if all <br/> cases should be processed> ]
          ResultList [label = <R-list object <br/> containing the output <br/>of the LogFSM run>]
          Flags [label = <Additional flags, e.g., <br/>RELATIVETIME to use <br/><i>relative times</i> instead <br/>of <i>timestamps</i> >]
          ZIP [label = <Prepared ZIP file with <br/> log data in a generic<br/>  representation, chunked by<br/>  persons and elements> ]
          
          FSMSyntax -> RunFSMSyntax [label = 'fsmsyntax='] 
          PreparedData -> RunFSMSyntax 
          ZIP -> PreparedData [label = 'zipfilename='] 
          DataFilter -> RunFSMSyntax 
          FileNameFilter -> DataFilter [label = 'datafilefilter='] 
          MaxCaseFilter -> DataFilter [label = 'maxnumberofcases='] 
          RunFSMSyntax -> ResultList  [label = '<-'] 
          Flags -> RunFSMSyntax  [label = 'flags='] 
           
      }" 
 
DiagrammeR::grViz(p1)
 

