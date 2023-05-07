# CsvHelperfs

thin fsharp's CsvHelper's wrapper


## Usage

### A.Read csv 

1. open `CsvHelper.fs` at main code
1. customize `CsvRead0.csv` and `input.jsonc` (see Sample folder)
1. write code at main code - like blow...

```F#
module Foo =

  open System.IO
  open System.Text
  open System.Text.Json
  open CsvHelperfs

  async {

    let fp_jsonc   = "./Sample/input.jsonc"
    let fp_csv     = "./Sample/fruits.csv"
    let codepage = 65001 // utf8

    let csvReadInfo =
      use stream = new FileStream(fp_jsonc,FileMode.Open,FileAccess.Read)
      let opt = JsonSerializerOptions()
      opt.ReadCommentHandling <- JsonCommentHandling.Skip
      JsonSerializer.Deserialize<CsvHelperfs.CsvReadInfo>(stream, opt)

    let finfo = FileInfo(Path.Combine(Directory.GetCurrentDirectory(),fp_csv))
    let sr = new StreamReader(finfo.FullName,Encoding.GetEncoding(codepage))

    // read csv
    let! csvBad, csvGood = CsvRead0.readCsv csvReadInfo finfo sr

    printfn "%A" csvBad
    printfn "%A" csvGood

    // write csv
    let fp_errCsv = Path.Combine(Directory.GetCurrentDirectory(),csvReadInfo.inputErrorFileConfig.outputFileConfig.OutputFileName)
    do! csvBad |> Seq.toArray |> CsvWrite.csvWriteFromRecordArray csvReadInfo.inputErrorFileConfig fp_errCsv

  }

  |> Async.RunSynchronously
```

output(console) 
```F#

seq [{ FileName = "fruits.csv"
       LastWriteTime = 2023/05/03 18:51:48 
       RowNumberPhysical = 4
       RowNumberLogical = 4
       Field = ""
       Message1 = " Id is invalid(over 3) "
       Message2 = ""
       Record = "3,cherry,500,black"       
       Memo = "" }]

seq [{ Id = 1
       Name = "apple"
       Price = 300
       Memo = "red" }; { Id = 2
                         Name = "banana"   
                         Price = 250       
                         Memo = "yellow" }]
```

output(CsvError.csv)
```csv
"FileName","LastWriteTime","RowNumberPhysical","RowNumberLogical","Field","Message1","Message2","Record","Memo"
"fruits.csv","2023/05/04 17:17:25","4","4",""," Id is invalid(over 3) ","","3,cherry,500,black",""
```

### B.Write csv 

1. open `CsvHelper.fs` at main code
1. customize `output.jsonc` (see Sample folder)
1. write code at main code - like blow...

```F#
module Foo =

  open System.Collections.Generic
  open System.IO
  open System.Dynamic
  open System.Text.Json
  open CsvHelperfs

  async {

    let fp_jsonc   = "./Sample/output.jsonc"

    // idic is ExpandoObject.IDictionary or DapperRow
    let idic =
      seq {
        Map([("name",box "cat")   ; ("size",box "small") ]) |> Map.toSeq |> dict
        Map([("name",box "horse") ; ("size",box "big") ])   |> Map.toSeq |> dict
      }
      |> Seq.map CsvUtil.asExpandoIDictionary
      |> Seq.toArray

    let csvWriteInfo =
      use stream = new FileStream(fp_jsonc,FileMode.Open,FileAccess.Read)
      let opt = JsonSerializerOptions()
      opt.ReadCommentHandling <- JsonCommentHandling.Skip
      JsonSerializer.Deserialize<CsvHelperfs.CsvWriteInfo>(stream, opt)

    let finfo = FileInfo(Path.Combine(Directory.GetCurrentDirectory(),csvWriteInfo.outputFileConfig.OutputFileName))
    do! idic |> CsvWrite.csvWriteFromExpandoIDictionaryArray csvWriteInfo finfo.FullName

  }

  |> Async.RunSynchronously
```

output(animal.csv)
```csv
"name","size"
"cat","small"
"horse","big"
```


### C.Other(Read Json) 

the way of read direct record

```fsharp
// record (wrap outer record)
type ExtraInputFileConfigJson = { ExtraInputFileConfig : ExtraInputFileConfig }
and ExtraInputFileConfig =
  {
    InputFolderName      : string
    CheckCsvFiles        : bool
    LimitRecords         : Nullable<int>
    IgnoreInvalidRecords : bool
  }

// extract
open System.Text.Json
let jsonText = ""
let opt = JsonSerializerOptions()
opt.ReadCommentHandling <- JsonCommentHandling.Skip
let x = JsonSerializer.Deserialize<ExtraInputFileConfigJson>(jsonText, opt) |> fun a -> a.ExtraInputFileConfig

x.InputFolderName |> printfn "%s"
```