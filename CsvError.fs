namespace CsvHelperfs

(*
  FILE    : CsvError.fs
  AUTHOR  : callmekohei
  LICENSE :
  DESC    :
*)

open System
open System.Globalization
open CsvHelper.Configuration

exception MyCsvException of string

type CsvError = {
  FileName          : string
  LastWriteTime     : DateTime
  RowNumberPhysical : int
  RowNumberLogical  : int
  Field             : string
  Message1          : string
  Message2          : string
  Record            : string
  Memo              : string
}

type CsvErrorMap () as this =
  inherit ClassMap<CsvError>()
  do
    this.AutoMap(CultureInfo.CurrentCulture)