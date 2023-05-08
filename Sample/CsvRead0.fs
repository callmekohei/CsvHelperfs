namespace CsvHelperfs

(*
  FILE    : CsvRead.fs
  AUTHOR  : callmekohei
  License :
  DESC    :
*)

module CsvRead0 =

  open System
  open System.IO
  open CsvHelper
  open CsvHelper.Configuration
  open CsvUtil

  // for header validate (required if do header validate)
  // for csv index (optional)
  type OptionalColumn () = inherit Attribute()
  [<RequireQualifiedAccess>]
  type MyRealCsv =
    | Id    = 0
    | Name  = 1
    | Price = 2
    | [<OptionalColumn>] Memo = 3


  [<CLIMutable>]
  type MyCsv = {
    Id    : int
    Name  : string
    Price : int
    Memo  : string
  }


  type MyCsvReaderMap () as this =
    inherit ClassMap<MyCsv>()

    let sb = Text.StringBuilder()

    do

      // validate any fields first mapping
      this.Map(fun (x:MyCsv) -> x.Id ).Convert(this.ValidateAnyFields)  |> ignore

      this.Map(fun (x:MyCsv) -> x.Name ).Name("Name")                   |> ignore
      this.Map(fun (x:MyCsv) -> x.Price ).Index(int MyRealCsv.Price)    |> ignore
      this.Map(fun (x:MyCsv) -> x.Memo ).Optional().Name("Memo")        |> ignore

    member this.ValidateAnyFields:ConvertFromStringArgs -> int =
      fun rowObj ->
        sb.Clear() |> ignore

        // validate any fields
        let x = rowObj.Row.GetField( int MyRealCsv.Id ) |> int
        if x > 2
        then sb.Append($" Id is invalid(over 3) ") |> ignore

        if sb.ToString() <> ""
        then raise <| MyCsvException (sb.ToString())
        x

  // set reader config in code
  let private csvReaderConfig (cfg:CsvHelperReaderConfig) =

    CsvHelperConfig.csvReaderConfigImpl cfg
    :?> CsvConfiguration
    |> fun x ->

      // IReaderConfiguration.cs
      // https://github.com/JoshClose/CsvHelper/blob/master/src/CsvHelper/Configuration/IReaderConfiguration.cs

      // x.GetConstructor                  <- (fun x -> Reflection.ConstructorInfo )
      // x.GetDynamicPropertyName          <- (fun x -> "")

      (* validate header *)
      x.HeaderValidated <- (fun (args: HeaderValidatedArgs) ->
        csvHeaderValidate<MyRealCsv,OptionalColumn> args
      )

      // x.MissingFieldFound               <- (fun x -> printfn "hello world")
      // x.PrepareHeaderForMatch           <- (fun x -> "foo")
      // x.ReadingExceptionOccurred        <- (fun x -> true)
      // x.ReferenceHeaderPrefix           <- (fun x -> "foo")
      // x.ShouldSkipRecord                <- (fun x -> x.Row.GetField(0) = "EOF")
      // x.ShouldUseConstructorParameters  <- (fun _ -> true)

      // IParserConfiguration.cs
      // https://github.com/JoshClose/CsvHelper/blob/master/src/CsvHelper/Configuration/IParserConfiguration.cs

      // x.BadDataFound                    <- (fun x -> printfn "hello world")
      // x.CultureInfo                     <- ""
      // x.Encoding                        <- Encoding.UTF8 // Gets the encoding used when counting bytes.
      // x.GetDelimiter                    <- (fun x -> ",")
      // x.IsNewLineSet                    <- false  // Private members

      x :> IReaderConfiguration


  let private readCsvByHand
    (csvReader   : CsvReader)
    (finfo       : FileInfo)
    (arrGood     : ResizeArray<MyCsv>)
    (arrBad      : ResizeArray<CsvError>) =

    async {

      let mutable flg = true
      while flg do

        try

          let! tmp = csvReader.ReadAsync() |> Async.AwaitTask

          flg <- tmp
          if flg
          then

            csvReader.GetRecord<MyCsv>() |> arrGood.Add

        with

          | :? CsvHelperException as e ->

            match e with

            | :? TypeConversion.TypeConverterException as te ->

                {
                  FileName          = finfo.Name
                  LastWriteTime     = finfo.LastWriteTime
                  RowNumberPhysical = te.Context.Parser.RawRow
                  RowNumberLogical  = te.Context.Parser.Row
                  Field             = te.Text
                  Message1          = te.Message
                  Message2          = ""
                  Record            = te.Context.Parser.RawRecord.TrimEnd()
                  Memo              = ""
                }
                |> arrBad.Add

            | _ ->

              match e.InnerException with

              | :? MyCsvException as err ->

                {
                  FileName          = finfo.Name
                  LastWriteTime     = finfo.LastWriteTime
                  RowNumberPhysical = e.Context.Parser.RawRow
                  RowNumberLogical  = e.Context.Parser.Row
                  Field             = ""
                  Message1          = err.Data0 // my own error message
                  Message2          = ""
                  Record            = e.Context.Parser.RawRecord.TrimEnd()
                  Memo              = ""
                }
                |> arrBad.Add

              | _ ->

                {
                  FileName          = finfo.Name
                  LastWriteTime     = finfo.LastWriteTime
                  RowNumberPhysical = e.Context.Parser.RawRow
                  RowNumberLogical  = e.Context.Parser.Row
                  Field             = ""
                  Message1          = e.Message
                  Message2          = ""
                  Record            = e.Context.Parser.RawRecord.TrimEnd()
                  Memo              = ""
                }
                |> arrBad.Add


          | e ->

            {
              FileName          = finfo.Name
              LastWriteTime     = finfo.LastWriteTime
              RowNumberPhysical = 0
              RowNumberLogical  = 0
              Field             = ""
              Message1          = e.Message
              Message2          = ""
              Record            = ""
              Memo              = ""
            }
            |> arrBad.Add

    }


  let readCsv
    (cfg          : CsvReadInfo)
    (finfo        : FileInfo)
    (streamReader : StreamReader) =

    async {

      let arrBad  = new ResizeArray<CsvError>()
      let arrGood = new ResizeArray<MyCsv>()

      let csv =
        if cfg.inputFileConfig.LeaveOpen.HasValue
        then new CsvReader( streamReader , csvReaderConfig(cfg.csvHelperReaderConfig),cfg.inputFileConfig.LeaveOpen.Value)
        else new CsvReader( streamReader , csvReaderConfig(cfg.csvHelperReaderConfig))

      // add any ClassMap if Multiple Record type e.g. <classmap1> , <classmap2> ...
      let _ = csv.Context.RegisterClassMap<MyCsvReaderMap>()

      match cfg.csvHelperReaderConfig.HasHeaderRecord with
      // headerless
      | false ->

        // skip rows
        if cfg.inputFileConfig.SkipRows.HasValue
        then do! CsvUtil.skipRows csv cfg.inputFileConfig.SkipRows.Value

        do! readCsvByHand csv finfo arrGood arrBad

      // exsits header
      | _ ->

        try

          (* Read Csv Header *)
          let! _ = csv.ReadAsync() |> Async.AwaitTask
          csv.ReadHeader() |> ignore
          csv.ValidateHeader()

          (* Read Csv body *)
          do! readCsvByHand csv finfo arrGood arrBad

        // header error
        with e ->
          {
            FileName          = finfo.Name
            LastWriteTime     = finfo.LastWriteTime
            RowNumberPhysical = csv.Context.Parser.RawRow
            RowNumberLogical  = csv.Context.Parser.Row
            Field             = ""
            Message1          = e.Message
            Message2          = ""
            Record            = csv.Context.Parser.RawRecord.Trim()
            Memo              = ""
          }
          |> arrBad.Add

      return
        (
            arrBad
          , arrGood
        )

    }