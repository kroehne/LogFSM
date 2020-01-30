#' Synthetic Data for the Ddocumentation of the LogFSM Syntax
#'
#' This function provides synthetic example data for to illustrate the useage of the LogFSM syntax.
#'
#' Example ```ex01a``` and ```ex01b```
#' * Simple item with 3 pages (text1, text2, question)
#'
#' Example ```ex02a```
#' * Simple log data to illustrate guards.
#'
#' @param example Name of the example (e.g. ```ex01a````)
#'
#' @importFrom dplyr %>%
#'
#' @export
#' @md
SyntheticLogData <- function(example="ex01a") {

  if (!example %in% c("ex01a","ex01b","ex02a")){
    stop(paste0("Example '", example,"' not found."))

  }

  set.seed(555)

  startLog <- function(){
    l <- data.frame()
    return (l)
  }

  addLoadingAndLoaded <- function(l){
    l <- dplyr::bind_rows(l,c(EventName = "Loading", TimeDiffPrevious = sample(10:20,1)))
    l <- dplyr::bind_rows(l,c(EventName = "Loaded", TimeDiffPrevious = sample(1:5,1)))
    return (l)
  }

  page <- function(l,p){
    l <- dplyr::bind_rows(l,c(EventName = "NavigateTo", TimeDiffPrevious = sample(10:1000,1), Page=p))
    return (l)
  }

  mcselect <- function(l,i,n){
    l <- dplyr::bind_rows(l,c(EventName = "Radiobutton", TimeDiffPrevious = sample(10:1000,1),Item=i, Name=n))
    return (l)
  }

  dialog <- function(l,n,a){
    l <- dplyr::bind_rows(l,c(EventName = "Dialog", TimeDiffPrevious = sample(10:1000,1), Name=n, Action=a))
    return (l)
  }

  helpbutton <- function(l){
    l <- dplyr::bind_rows(l,c(EventName = "HelpButton", TimeDiffPrevious = sample(10:1000,1)))
    return (l)
  }

  addUnLoadingAndUnLoaded <- function(l){
    l <- dplyr::bind_rows(l,c(EventName = "UnLoading", TimeDiffPrevious = sample(10:20,1)))
    l <- dplyr::bind_rows(l,c(EventName = "UnLoaded", TimeDiffPrevious = sample(5:10,1)))
    return (l)
  }


  button <- function(l,n){
    l <- dplyr::bind_rows(l,c(EventName = "Button", TimeDiffPrevious = sample(10:1000,1), Name=n))
    return (l)
  }

  anyattribute <- function(l,v,a){
    l <- dplyr::bind_rows(l,c(EventName = a, TimeDiffPrevious = sample(10:1000,1), Value=v))
    return (l)
  }

  t_start <- Sys.time() + sort(sample(1:100000, 1))

  l <- startLog()

  if (example == "ex01a"){

    l <- addLoadingAndLoaded(l)
    l <- page(l,"text2")
    l <- page(l,"text1")
    l <- page(l,"text2")
    l <- page(l,"question1")
    l <- mcselect(l,"item1","a")
    l <- mcselect(l,"item1","b")
    l <- page(l,"text2")
    l <- page(l,"question1")
    l <- mcselect(l,"item2","c")
    l <- dialog(l,"exit","show")
    l <- button(l,"yes")
    l <- dialog(l,"exit","close")
    l <- addUnLoadingAndUnLoaded(l)

  }

  if (example == "ex01b"){

    l <- addLoadingAndLoaded(l)
    l <- page(l,"text2")
    l <- page(l,"text1")
    l <- page(l,"text2")
    l <- page(l,"question1")
    l <- mcselect(l,"item1","a")
    l <- mcselect(l,"item1","b")
    l <- page(l,"text2")
    l <- mcselect(l,"item1","a")
    l <- page(l,"question1")
    l <- mcselect(l,"item2","c")
    l <- dialog(l,"exit","show")
    l <- button(l,"yes")
    l <- dialog(l,"exit","close")
    l <- addUnLoadingAndUnLoaded(l)

  }

  if (example == "ex02a"){
    l <- startLog()
    l <- addLoadingAndLoaded(l)

    l <- anyattribute(l,a="MyEvent",v="B")
    l <- anyattribute(l,a="MyEvent",v="C")
    l <- anyattribute(l,a="MyEvent",v="B")
    l <- anyattribute(l,a="MyEvent",v="A")
    l <- anyattribute(l,a="MyEvent",v="C")
    l <- anyattribute(l,a="MyEvent",v="B")
    l <- anyattribute(l,a="MyEvent",v="A")
    l <- anyattribute(l,a="MyEvent",v="B")
    l <- anyattribute(l,a="MyEvent",v="C")

    l <- addUnLoadingAndUnLoaded(l)
    l

  }

  l$PersonIdentifier <- "0001"
  l$TimeStamp <- format(t_start + as.numeric( (l %>% dplyr::mutate(RelativeTime = cumsum(l$TimeDiffPrevious)))$RelativeTime),
                        format = '%H:%M:%OS')

  if (example %in% c("ex01a","ex01b")){
    l <- l[,c("PersonIdentifier","TimeStamp","EventName","Page","Item","Name","Action")]
  } else  if (example %in% c("ex02a")){
    l <- l[,c("PersonIdentifier","TimeStamp","EventName","Value")]
  }

  return (l)
}

