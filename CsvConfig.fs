namespace CsvHelperfs

(*
  FILE    : CsvConfig.fs
  AUTHOR  : callmekohei
  LICENSE :
  DESC    :
*)

open System

[<CLIMutable>]
type OutputFileConfig =
  {
    mutable OutputFileName   : string
    CodePage                 : int
    NewLineAppened           : bool
    TextAppended             : string
    IsAppend                 : bool
    LeaveOpen                : bool
  }

[<CLIMutable>]
type InputFileConfig =
  {
    InputFileNameRegexPattern : string
    CodePage                  : Nullable<int>
    LeaveOpen                 : Nullable<bool>
    SkipRows                  : Nullable<int>
  }


[<CLIMutable>]
type CsvWriteInfo =
  {
    outputFileConfig      : OutputFileConfig
    csvHelperWriterConfig : CsvHelperWriterConfig
  }


[<CLIMutable>]
type CsvReadInfo =
  {
    inputFileConfig       : InputFileConfig
    inputErrorFileConfig  : CsvWriteInfo
    csvHelperReaderConfig : CsvHelperReaderConfig
  }