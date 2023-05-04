namespace CsvHelperfs

(*
  FILE    : CsvWrite.fs
  AUTHOR  : callmekohei
  LICENSE :
  DESC    :
*)

module CsvWrite =

  open System.Collections.Generic
  open System.IO
  open System.Text
  open CsvHelper


  let private writeCsvByHand
    (cfg          : CsvWriteInfo)
    (csvWriter    : CsvWriter)
    (rcds         : seq<'T>) =

    async {

      let i = ref 0
      let total = rcds |> Seq.length
      let addLastNewLine = cfg.outputFileConfig.NewLineAppened

      let writeRecord (rcd:'T ) =
        async {

          i.Value <- i.Value + 1

          csvWriter.WriteRecord<'T>(rcd)

          if addLastNewLine
          then
            do! csvWriter.NextRecordAsync() |> Async.AwaitTask
          else
            if total <> i.Value
            then do! csvWriter.NextRecordAsync() |> Async.AwaitTask
        }

      return!
        rcds
        |> Seq.map writeRecord
        |> Async.Sequential
        |> Async.Ignore

    }


  let csvWriteFromExpandoIDictionaryArray
    (csvWriteInfo   : CsvWriteInfo)
    (outputFilepath : string)
    (rcds           : IDictionary<string,obj> array)
    =

    async {

      if ( rcds |> Array.isEmpty) || ( ( rcds.Length = 1) && ( rcds |> Array.exactlyOne |> isNull ))
      then
        let message = "NO DATA!"
        File.WriteAllText(outputFilepath,message)

      else

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
        use streamWriter = new StreamWriter( outputFilepath, csvWriteInfo.outputFileConfig.IsAppend , Encoding.GetEncoding( csvWriteInfo.outputFileConfig.CodePage  ) )
        use csvWriter    = new CsvWriter( streamWriter, CsvHelperConfig.csvWriterConfigImpl csvWriteInfo.csvHelperWriterConfig , csvWriteInfo.outputFileConfig.LeaveOpen)

        // header
        if  csvWriteInfo.csvHelperWriterConfig.HasHeaderRecord
        then
          rcds
          |> Array.item 0
          |> fun x -> x.Keys |> Seq.iter (fun headerName -> csvWriter.WriteField(headerName) )
          do! csvWriter.NextRecordAsync() |> Async.AwaitTask

        // body
        do! rcds |> writeCsvByHand csvWriteInfo csvWriter

        // text appended
        if csvWriteInfo.outputFileConfig.TextAppended |> System.String.IsNullOrEmpty |> not
        then csvWriter.WriteRecord({|x=csvWriteInfo.outputFileConfig.TextAppended|})

    }


  let csvWriteFromRecordArray<'T,'U when 'U :> Configuration.ClassMap>
    (csvWriteInfo   : CsvWriteInfo)
    (outputFilepath : string)
    (rcds           : array<'T>) =

    async {

      if (rcds |> Array.isEmpty)
      then
        let message = "NO DATA!"
        File.WriteAllText(outputFilepath,message)

      else

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
        use streamWriter = new StreamWriter( outputFilepath, csvWriteInfo.outputFileConfig.IsAppend , Encoding.GetEncoding( csvWriteInfo.outputFileConfig.CodePage  ) )
        use csvWriter    = new CsvWriter( streamWriter, CsvHelperConfig.csvWriterConfigImpl csvWriteInfo.csvHelperWriterConfig , csvWriteInfo.outputFileConfig.LeaveOpen)

        // set ClassMap if exists
        if isNull typeof<'U>.BaseType.BaseType |> not
        then csvWriter.Context.RegisterClassMap<'U>() |> ignore

        // header
        if  csvWriteInfo.csvHelperWriterConfig.HasHeaderRecord
        then
          csvWriter.WriteHeader<'T>()
          do! csvWriter.NextRecordAsync() |> Async.AwaitTask

        // body
        do! rcds |> writeCsvByHand csvWriteInfo csvWriter

        // text appended
        if csvWriteInfo.outputFileConfig.TextAppended |> System.String.IsNullOrEmpty |> not
        then csvWriter.WriteRecord({|x=csvWriteInfo.outputFileConfig.TextAppended|})

    }