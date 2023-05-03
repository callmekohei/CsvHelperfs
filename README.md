# CsvHelperfs

thin fsharp's CsvHelper's wrapper

## sample 

### csv read 

1. write [input.jsonc](https://github.com/callmekohei/CsvHelperfs/blob/main/Sample/input.jsonc)
1. write code like blow...

```F#
module Foo =

  open System.IO
  open System.Text
  open System.Text.Json
  open CsvHelperfs

  async {

    let fp_jsonc = "./../CsvHelperfs/Sample/input.jsonc"
    let fp_csv   = "./../CsvHelperfs/Sample/fruits.csv"
    let codepage = 65001 // utf8

    let csvReadInfo =
      use stream = new FileStream(fp_jsonc,FileMode.Open,FileAccess.Read)
      let opt = JsonSerializerOptions()
      opt.ReadCommentHandling <- JsonCommentHandling.Skip
      JsonSerializer.Deserialize<CsvHelperfs.CsvReadInfo>(stream, opt)

    let finfo = FileInfo(Path.Combine(Directory.GetCurrentDirectory(),fp_csv))
    let sr = new StreamReader(finfo.FullName,Encoding.GetEncoding(codepage))

    let! csvBad, csvGood = CsvRead0.readCsv csvReadInfo finfo sr

    printfn "%A" csvBad
    printfn "%A" csvGood

  }

  |> Async.RunSynchronously
```

output 
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


### csv read (header error)

output 
```F#
seq [{ FileName = "fruits.csv"
       LastWriteTime = 2023/05/03 18:40:41
       RowNumberPhysical = 1
       RowNumberLogical = 1
       Field = ""
       Message1 = "(Invalid Header) short: . invalid: a"
       Message2 = ""
       Record = "Id,Name,Price,Memo,a"
       Memo = "" }]
seq []
```
