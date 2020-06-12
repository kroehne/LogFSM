## ----setup, include = FALSE---------------------------------------------------
knitr::opts_chunk$set(
  collapse = TRUE,
  comment = "#>"
)
 
library(dplyr)
library(DiagrammeR) 
library(kableExtra)
 

## ---- eval=F, echo=T----------------------------------------------------------
#  myFSM <- 'Start: {Start-State}
#            End: {End-State-1}; {End-State-2}'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: {State-From} -> {State-To} @ {Trigger 1}'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: Inst -> Text @ EventName=TextOpenEvent'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: Inst -> Text @ EventName=Click&id=Start'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: EndDialog -> Exit @ EventName=Click'
#  'Transition: StartPage -> Unit @ EventName=Click'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Ignore: {State-Name} @ {Trigger 1}'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Ignore: {State-Name} @ {Trigger 1};{Trigger 1}'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Ignore: {State-Name1};{State-Name2} @ {Trigger 1};{Trigger 1}'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: EndDialog -> Exit @ EventName=Click'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: EndDialog -> Exit @ Click'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: S1 -> S2 @ EventName=ButtonClick&id=MyButtonID'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: {From} -> {To} @ {Trigger 1}; {Trigger 2}'

## ---- eval=F, echo=T----------------------------------------------------------
#  ex01_demo_multiple_triggers_fsm <-"Start:From
#   End:To
#   Transitions:From->To@EventName=A;EventName=B;EventName=C"

## ---- eval=T, echo=F,  warning=F----------------------------------------------
ex01_test_multiple_triggers_fsm <-"Start: From
   End: To 
   Transitions: From -> To @EventName=A;EventName=B;EventName=C"

## ---- echo=FALSE--------------------------------------------------------------
grViz("digraph { compound=true; node [shape=oval fontsize=10] rankdir=\"LR\" From [label=\"From\"]; To [style='filled' fillcolor='gray' label=\"To\"];  From -> To [style=\"solid\", label=\"A\" fontsize=8]; From -> To [style=\"solid\", label=\"B\" fontsize=8]; From -> To [style=\"solid\", label=\"C\" fontsize=8];  init [label=\"\", shape=point];  init -> From[style = \"solid\"] }") 

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: {S1};{S2} -> {S3} @ {Trigger 1};{Trigger 2}'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: {S1} -> {S3} @ {Trigger1}'
#  'Transition: {S2} -> {S3} @ {Trigger1}'
#  'Transition: {S1} -> {S3} @ {Trigger2}'
#  'Transition: {S2} -> {S3} @ {Trigger2}'

## ---- eval=T, echo=T----------------------------------------------------------
ex01a_demo_multiple_states_fsm <-"Start: Start
   End: End 
   Transitions: Start -> S1 @ EventName=A;B
   Transitions: Start -> S1 @ EventName=C   
   Transitions: Start -> S2 @ D      
   Transitions: S1;S2 -> End @ EventName=E;EventName=F"

## -----------------------------------------------------------------------------
grViz("digraph { compound=true; node [shape=oval fontsize=10] rankdir=\"LR\" Start [label=\"Start\"]; S1 [label=\"S1\"]; S2 [label=\"S2\"]; End [style='filled' fillcolor='gray' label=\"End\"];  Start -> S1 [style=\"solid\", label=\"A\" fontsize=8]; Start -> S1 [style=\"solid\", label=\"B\" fontsize=8]; Start -> S1 [style=\"solid\", label=\"C\" fontsize=8]; Start -> S2 [style=\"solid\", label=\"D\" fontsize=8]; S1 -> End [style=\"solid\", label=\"E\" fontsize=8]; S1 -> End [style=\"solid\", label=\"F\" fontsize=8]; S2 -> End [style=\"solid\", label=\"E\" fontsize=8]; S2 -> End [style=\"solid\", label=\"F\" fontsize=8];  init [label=\"\", shape=point];  init -> Start[style = \"solid\"] }")

## ---- eval=T, echo=T----------------------------------------------------------
ex01a_demo_ignore_samestate_fsm <-"Start: Start
   End: End 
   Transitions: Start -> S1 @ EventName=A 
   Transitions: S1 -> S1 @ EventName=B     
   Transitions: S1 -> S2 @ EventName=C     
   Ignore: S2 @ EventName=D   
   Transitions: S2 -> End @ EventName=E"

## ---- out.width='65%', fig.asp=.75, fig.align='center', echo=FALSE------------
grViz("digraph { compound=true; node [shape=oval fontsize=10] rankdir=\"LR\" Start [label=\"Start\"]; S1 [label=\"S1\"]; S2 [label=\"S2\"]; End [style='filled' fillcolor='gray' label=\"End\"];  Start -> S1 [style=\"solid\", label=\"A\" fontsize=8]; S1 -> S2 [style=\"solid\", label=\"C\" fontsize=8]; S1 -> S1 [style=\"solid\", label=\"B\" fontsize=8]; S2 -> End [style=\"solid\", label=\"E\" fontsize=8]; S2 -> S2 [style=\"solid\", label=\"D\" fontsize=8];  init [label=\"\", shape=point];  init -> Start[style = \"solid\"] }")

## -----------------------------------------------------------------------------
 p1 <-"digraph  {
    rankdir=LR; 
    Starting [peripheries = 2];
    Endstate [style='filled' fillcolor='gray']
    Starting -> State1 [label='EventA' fontsize=10 fontcolor='red']; 
    Starting -> State2 [label='EventA' fontsize=10 fontcolor='red']; 
    State1 -> Endstate [label='EventB' fontsize=10];
}"
  grViz(p1)

## ---- eval=F, echo=T,  warning=F----------------------------------------------
#  table(out$AugmentedLogDataTable$Result_1)

## ---- eval=T, echo=F,  warning=F----------------------------------------------
tab<-tab <- data.frame(EventNotDefinedAsTrigger =17)
tab %>% kable() %>%  kable_styling(font_size = 12)

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: {S1} -> {S2} @ {Trigger 1} ! {Operator 1}'
#  'Ignore: {S2} @ {Trigger 2} ! {Operator 2}'

## ---- eval=F, echo=T, tidy=F--------------------------------------------------
#  ex01a_counting_fsm <-"Start: Starting
#  End: Endstate
#  Transitions:Starting -> Working
#         @ Loading ! SetVariable(NavigationCounter,0)
#  Transitions:Working -> Endstate @ UnLoaded
#  Ignore: Working
#         @ NavigateTo ! IncreaseIntValue(NavigationCounter,1)"

## ----fsm_syntax_variables_example_02, eval=F, echo=T, tidy=F------------------
#  ex01a_copy_attribute_fsm <- "Start: Starting
#  End: Endstate
#  Transitions:Starting -> Working @ Loading
#      ! SetVariable(CurrentPage,text1)
#  Transitions:Working -> Endstate @ UnLoaded
#  Ignore: Working@NavigateTo
#      ! CopyAttributeToVariable(CurrentPage,Page)"

## ---- eval=F, echo=T----------------------------------------------------------
#  1 Start: Starting
#  1 End: Endstate
#  2 Start: Starting
#  2 End: Endstate
#  1 Transitions:Starting->Working @ Loading
#  1 ...
#  2 Transitions: Starting->Text1 @ EventName=Loading

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transitions : {List of States ;} -> {State}
#         @ {List of Triggers ;} ! {List of Operators ;} | {Guard}'
#  'Ignore : {List of States ;}
#         @ {List of Triggers ;} ! {List of Operators ;} | {Guard}'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transitions:Working-> AfterLastRadiobutton @
#      EventName=Radiobutton  | IsLastTrigger(EventName=Radiobutton)'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: {S1} -> {S2} @ {Trigger 1} |
#                 CompareAttribute(AttributeName,Value)'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: {S1} -> {S2} @ {Trigger 1} |
#                 CompareAttribute(AttributeName,Value,Comparer)'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: {S1} -> {S2} @ {Trigger 1} |
#                 CompareAttribute(AttributeName,Value,Comparer,Offset)'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: {S1} -> {S2} @ {Trigger 1} |
#                 CompareAttributes(AttributeName1,AttributeName2)'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: {S1} -> {S2} @ {Trigger 1} |
#                 CompareAttributes(AttributeName1,AttributeName2,Comparer)'

## ---- eval=F, echo=T----------------------------------------------------------
#  'Transition: {S1} -> {S2} @ {Trigger 1} |
#      CompareAttributes(AttributeName1,AttributeName2,Comparer,Offset)'

