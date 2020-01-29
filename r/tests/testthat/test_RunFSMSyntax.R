library(testthat)

test_that("tests with simple synthetical data", {

  # Create temp file for data

  tmpdatafilename <- tempfile(pattern = "file", tmpdir = tempdir(), fileext = "")
  tmpdata <- SyntheticLogData("ex01a")
  WriteLogData(file = tmpdatafilename, data = tmpdata)

  # TEST: File should exist

  expect_equal(file.exists(tmpdatafilename), TRUE)

  ex01a_total_time_fsm <-"Start: Starting
   End: Endstate
   Transitions: Starting  -> WorkingInUnit @ EventName=Loaded
   Transitions: WorkingInUnit -> Closing @ EventName=UnLoading
   Transitions: Closing -> Endstate @ EventName=UnLoaded"

  ex01a_total_time_out <- RunFSMSyntax(
    datafilename = tmpdatafilename,
    fsmsyntax = ex01a_total_time_fsm, verbose = F)

  # TEST: Same number of rows for input data and augmented log data table

  expect_equal(dim(ex01a_total_time_out$AugmentedLogDataTable)[1], dim(tmpdata)[1])

  # TEST: Four different states should be reported

  expect_equal(length(unique(ex01a_total_time_out$SequenceTable_1$State)),4)

})


test_that("tests with simple synthetical data", {

  expect_error( RunFSMSyntax())

})
