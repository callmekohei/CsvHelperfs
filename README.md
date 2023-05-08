# CsvHelperfs

thin fsharp's CsvHelper's wrapper


## Usage

### A.Read csv 

1. `open CsvHelperfs` at main code
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

    // read Config from Json
    let csvReadInfo =
      use stream = new FileStream(fp_jsonc,FileMode.Open,FileAccess.Read)
      let opt = JsonSerializerOptions()
      opt.ReadCommentHandling <- JsonCommentHandling.Skip
      JsonSerializer.Deserialize<CsvHelperfs.CsvReadInfo>(stream, opt)

    // read csv
    let folderPath       = csvReadInfo.InputFileConfig.InputFolderName
    let filePath         = csvReadInfo.InputFileConfig.InputFileName
    let finfo            = FileInfo(Path.Combine(Directory.GetCurrentDirectory(), folderPath, filePath))
    let codepage         = csvReadInfo.InputFileConfig.CodePage.Value
    use sr               = new StreamReader(finfo.FullName,Encoding.GetEncoding(codepage))
    let! csvBad, csvGood = CsvRead0.readCsv csvReadInfo finfo sr

    printfn "%A" csvBad
    printfn "%A" csvGood

    // write csv of error log
    let filePath' = csvReadInfo.InputErrorFileConfig.OutputFileConfig.OutputFileName
    let fp_errCsv = Path.Combine(Directory.GetCurrentDirectory(),filePath')
    do! csvBad |> Seq.toArray |> CsvWrite.csvWriteFromRecordArray csvReadInfo.InputErrorFileConfig fp_errCsv

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

1. `open CsvHelperfs` at main code
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

    // date is DapperRow:IDictionary or ExpandoObject.IDictionary
    let idic =
      seq {
        Map([("name",box "cat")   ; ("size",box "small") ]) |> Map.toSeq |> dict
        Map([("name",box "horse") ; ("size",box "big") ])   |> Map.toSeq |> dict
      }
      |> Seq.map CsvUtil.asExpandoIDictionary
      |> Seq.toArray

    let fp_jsonc   = "./Sample/output.jsonc"

    // read Config from Json
    let csvWriteInfo =
      use stream = new FileStream(fp_jsonc,FileMode.Open,FileAccess.Read)
      let opt = JsonSerializerOptions()
      opt.ReadCommentHandling <- JsonCommentHandling.Skip
      JsonSerializer.Deserialize<CsvHelperfs.CsvWriteInfo>(stream, opt)

    // write csv
    let filePath = csvWriteInfo.OutputFileConfig.OutputFileName
    let finfo    = FileInfo(Path.Combine(Directory.GetCurrentDirectory(),filePath))
    do! CsvWrite.csvWriteFromExpandoIDictionaryArray csvWriteInfo finfo.FullName idic

  }
  |> Async.RunSynchronously
```

output(animal.csv)
```csv
"name","size"
"cat","small"
"horse","big"
```


### C. Extra Config

```fsharp
module Foo =

  open System.IO
  open System.Text.Json

  // wrap dummy record if read records directly from json
  type ExtraInputJson = {ExtraInput:ExtraInput}
  and ExtraInput =
    {
      ExtraInputFileConfig      : ExtraInputFileConfig
      ExtraInputErrorFileConfig : ExtraInputErrorFileConfig
    }
  and ExtraInputFileConfig =
    {
      CheckCsvFiles        : bool
      LimitRecords         : Nullable<int>
      IgnoreInvalidRecords : bool
    }
  and ExtraInputErrorFileConfig =
    {
      OutputFileNameDateFormat : string
      IsPrefixDateFormat       : bool
    }

  async{

    let fp_jsonc   = "./Sample/input.jsonc"

    // read Config from Json
    let extraInput =
      use stream = new FileStream(fp_jsonc,FileMode.Open,FileAccess.Read)
      let opt = JsonSerializerOptions()
      opt.ReadCommentHandling <- JsonCommentHandling.Skip
      JsonSerializer.Deserialize<ExtraInputJson>(stream, opt) |> fun a -> a.ExtraInput

    extraInput.ExtraInputFileConfig.LimitRecords.Value |> printfn "%d"

  }
  |> Async.RunSynchronously
```

output(console)
```
9999
```