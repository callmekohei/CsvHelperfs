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
    mutable OutputFolderName : string
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
    InputFolderName : string
    InputFileName   : string
    CodePage        : Nullable<int>
    LeaveOpen       : Nullable<bool>
    SkipRows        : Nullable<int>
  }


[<CLIMutable>]
type CsvWriteInfo =
  {
    OutputFileConfig      : OutputFileConfig
    CsvHelperWriterConfig : CsvHelperWriterConfig
  }


[<CLIMutable>]
type CsvReadInfo =
  {
    InputFileConfig       : InputFileConfig
    InputErrorFileConfig  : CsvWriteInfo
    CsvHelperReaderConfig : CsvHelperReaderConfig
  }