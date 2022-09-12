## ----setup, include = FALSE---------------------------------------------------
knitr::opts_chunk$set(
  collapse = TRUE,
  comment = "#>"
)
   
`%>%` <- magrittr::`%>%`

## ---- echo=F, out.width="100%"------------------------------------------------
 
p1 <-" digraph G {
          rankdir=LR; 
          node [shape=record, fontname = Arial];
          a [label = '<f0> Data | <f1> Syntax | (Parameters and Flags)', width = 3];
          b [label = '<f0> Augmented Data | <f1> Graph', width = 2.5];
          a:f0 -> b:f0 
          a:f1 -> b:f1 
           
          tab2 [label = '(aggregated)' shape=ellipse, color=gray]; 
          tab1 [label = 'for each machine i'  shape=ellipse, color=gray]; 
        
          b:f0 -> tab1:f0   
          b:f0 -> tab2:f0   
          c [label = '<f0> AugmentedLogDataTable', width = 3];
         
          d [label = '<f0> SequenceTable_i', width = 3];
          e [label = '<f0> TransitionFrequencyTable_i', width = 3];
          f [label = '<f0> NGramTable', width = 3];
          g [label = '<f0> StateEventFrequencyTable_i', width = 3];
          h [label = '<f0> TransitionFrequencyTotalsTable', width = 3];
          i [label = '<f0> VariableValueTable', width = 3];
          j [label = '<f0> UmlDotGraph [[i]]', width = 3];
          l [label = '<f0> StateFrequencyGraph [[i]]', width = 3];
          k [label = '<f0> StateSummaryTable_i', width = 3];
          
          subgraph cluster_0 {
              label='Input';
             a
          }
          subgraph cluster_1 {
              label='Output';
              c; b; d; e; f; g; h; i; j; l; tab1; tab2; k
          }
    
          tab1:f0 -> d:f0 
          tab1:f0 -> e:f0 
          tab1:f0 -> f:f0 
          tab1:f0 -> g:f0 
          tab1:f0 -> k:f0 
          
          tab2:f0 -> h:f0 
          tab2:f0 -> i:f0 
          b:f0 -> c:f0 
          b:f1 -> j:f0 
          b:f1 -> l:f0
          
      }" 
 
 DiagrammeR::grViz(p1)
 

## ---- eval=F, echo=T----------------------------------------------------------
#  View(out$AugmentedLogDataTable)
#  View(out$VariableValueTable)
#  View(out$TransitionFrequencyTotalsTable)

## ---- eval=F, echo=T----------------------------------------------------------
#  View(out$StateEventFrequencyTable_1)
#  View(out$SequenceTable_1)
#  View(out$StateSummaryTable_1)
#  View(out$TransitionFrequencyTable_1)
#  View(out$NGramTable_1)

## ---- eval=F, echo=T----------------------------------------------------------
#  DiagrammeR::grViz(out$UmlDotGraph[[1]])                 # input
#  DiagrammeR::grViz(out$TransitionFrequencyGraph[[1]])    # output

## ---- out.width='100%', echo=F------------------------------------------------
 
p1 <-" digraph G {
          graph [ranksep='0.175', overlap = true, fontsize = 12];  
          node [shape=record, fontname = Arial];
          step1 [label = 'Start with Minimal FSM Syntax', width = 4];
          step2 [label = 'Run FSM With Selected Case', width = 4];
          step3 [label = 'Inspect Output Tables and Graphs', width = 4];
          check [label = 'Are the unexpeced \\nrejected events or \\nmissing state changes?', shape='ellipse'];
          step4 [label = 'Extend / Modify FSM Syntax', width = 4];
          step5 [label = 'Run FSM With All Case', width = 4];
          step6 [label = 'Extrat Indicator using R Syntax...', width = 4];
          step1 -> step2 -> step3  
          step4 -> step2  [label='Gradually increase  \\nthe number of cases \\n in order to \\n progressively \\n develop the FSM...', 
                           fontsize = 10, lp=30]; 
          check -> step4 [label='Yes']; 
          step3 -> check
          check -> step5  [label='No']; 
          step5 -> step6
          step6 ->step2  [label='If unexpected or \\nimplausible results \\noccur, consider these \\ncases separately...' , fontsize = 10]; 
      }" 
DiagrammeR::grViz(p1)

## ---- eval=F, echo=T, tidy=F--------------------------------------------------
#  View(out$AugmentedLogDataTable)

## ---- eval=T, echo=F, warning=F-----------------------------------------------

df1 <- data.frame(V1=c("ID01","ID01", "...","ID02", "..."),
                 V2=rep("...",5),
                 V3=rep("...",5),
                 V4=rep("...",5),
                 V5=rep("...",5),
                 V6=rep("...",5))

colnames(df1) <- c("PersonIdentifier","{Time}","Element","EventName",
                   "Event-Specific Data e(x)", "...")

df1 %>% kableExtra::kable() %>%  kableExtra::kable_styling(font_size = 12)

## ---- eval=T, echo=F, warning=F-----------------------------------------------

df2 <- data.frame(V1=rep("...",3),
                 V2=rep("...",3),
                 V3=rep("...",3),
                 V4=rep("...",3),
                 V5=rep("...",3),
                 V6=rep("...",3),
                 V7=rep("...",3))

colnames(df2) <- c("...","TimeDifference", "StateBefore_1", 
                   "StateAfter_1", "Result_1","{Variables}", "...")

df2 %>% kableExtra::kable() %>%  kableExtra::kable_styling(font_size = 12)

## ---- eval=F, echo=T, tidy=F--------------------------------------------------
#  View(out$TransitionFrequencyTotalsTable)

## ---- eval=T, echo=F, warning=F-----------------------------------------------

df3 <- data.frame(V1=c("1","1", "...","1", "..."),
                 V2=c("Startstate","State1","...","State3","..."),
                 V3=c("State1","State2","...","Endstate","..."),
                 V4=c("100","...","...","...","...") )

colnames(df3) <- c("Machine","From","To","Frequency")

df3 %>% kableExtra::kable() %>%  kableExtra::kable_styling(font_size = 12)
 

## ---- eval=F, echo=T, tidy=F--------------------------------------------------
#  View(out$TransitionFrequencyTable_1)

## ---- eval=T, echo=F, warning=F-----------------------------------------------

df1 <- data.frame(V1=c("ID01","ID01", "..."),
                 V2=rep("...",3),
                 V3=rep("...",3),
                 V4=rep("...",3))

colnames(df1) <- c("PersonIdentifier","From","To", "Frequency")

df1 %>% kableExtra::kable() %>%  kableExtra::kable_styling(font_size = 12)

## ----logfsm-output-transitionfrequencytable-02, eval=F, echo=T, tidy=F--------
#  out$TransitionFrequencyTotalsTable %>%
#    filter(From == "Page7" & To == "Page8") %>%
#    select(Frequency)

## ---- eval=F, echo=T, warning=F-----------------------------------------------
#  out <- RunFSMSyntax(...)
#  library(DiagrammeR)
#  grViz(out$UmlDotGraph[[1]])

## ---- eval=F, echo=T, warning=F-----------------------------------------------
#  out <- RunFSMSyntax(...)
#  library(DiagrammeR)
#  grViz(out$TransitionFrequencyGraph[[1]])

## ---- eval=F, echo=T, tidy=F--------------------------------------------------
#  View(out$StateEventFrequencyTable_1)

## ---- eval=T, echo=F, warning=F-----------------------------------------------

df1 <- data.frame(V1=c("ID01","..."),
                 V2=rep("...",2),
                 V3=rep("...",2),
                 V4=rep("...",2),
                 V5=rep("...",2))

colnames(df1) <- c("PersonIdentifier","State","Name", "Result", "Frequency")

df1 %>% kableExtra::kable() %>%  kableExtra::kable_styling(font_size = 12)

## ---- eval=F, echo=T, tidy=F--------------------------------------------------
#  View(out$SequenceTable_1)

## ---- eval=T, echo=F, warning=F-----------------------------------------------

df1 <- data.frame(V1=c("ID01","ID01","..."),
                 V2=c("State1","State2","..."),
                 V3=rep("...",3))

colnames(df1) <- c("PersonIdentifier","State","Duration")

df1 %>% kableExtra::kable() %>%  kableExtra::kable_styling(font_size = 12)

## ---- eval=F, echo=T, tidy=F--------------------------------------------------
#  View(out$NGramTable_1)

## ---- eval=T, echo=F, warning=F-----------------------------------------------

df1 <- data.frame(V1=c("ID01","ID01", "..."),
                 V2=rep("...",3),
                 V3=rep("...",3),
                 V4=rep("...",3),
                 V4=rep("...",3))

colnames(df1) <- c("PersonIdentifier","Size","Sequence", "Frequency", "TotalTime")

df1 %>% kableExtra::kable() %>%  kableExtra::kable_styling(font_size = 12)

## ---- eval=F, echo=T, tidy=F--------------------------------------------------
#  View(out$StateSummaryTable_1)

## ---- eval=T, echo=F, warning=F-----------------------------------------------

df1 <- data.frame(V1=c("ID01","ID01", "..."),
                 V2=rep("...",3),
                 V3=rep("...",3),
                 V4=rep("...",3),
                 V5=rep("...",3),
                 V6=rep("...",3),
                 V7=rep("...",3))

colnames(df1) <- c("PersonIdentifier","State","TotalTime", "Frequency", "ShortesttVisit", "LongestVisit", "AverageVisit")

df1 %>% kableExtra::kable() %>%  kableExtra::kable_styling(font_size = 12)

## ---- eval=F, echo=T, tidy=F--------------------------------------------------
#  View(out$VariableValueTable)

## ---- eval=T, echo=F, warning=F-----------------------------------------------

df1 <- data.frame(V1=c("ID01","..."),
                 V2=rep("...",2),
                 V3=rep("...",2),
                 V4=rep("...",2))

colnames(df1) <- c("PersonIdentifier","Variable","Value", "NumberOfChanges")

df1 %>% kableExtra::kable() %>%  kableExtra::kable_styling(font_size = 12)

