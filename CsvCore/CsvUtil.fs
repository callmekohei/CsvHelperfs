namespace CsvHelperfs

(*
  FILE    : CsvUtil.fs
  AUTHOR  : callmekohei
  LICENSE :
  DESC    :
*)

module CsvUtil =

  open System
  open Microsoft.FSharp.Reflection
  open CsvHelper


  let csvHeaderValidate<'T,'U> (args: HeaderValidatedArgs) =
    let requiredHeaders =
      typeof<'T>
      |> FSharpType.GetRecordFields
      |> Array.filter(fun x -> Array.isEmpty (x.GetCustomAttributes(typeof<'U>,true)) )
      |> Array.map(fun x -> x.Name)
    let optionalHeaders =
      typeof<'T>
      |> FSharpType.GetRecordFields
      |> Array.filter(fun x -> Array.isEmpty (x.GetCustomAttributes(typeof<'U>,true)) |> not )
      |> Array.map(fun x -> x.Name)
    let headers = args.Context.Parser.Record
    let missingHeaders = requiredHeaders |> Array.except headers |> Array.except optionalHeaders
    let unexpectedHeaders = headers |> Array.except requiredHeaders |> Array.except optionalHeaders
    if (Seq.isEmpty missingHeaders && Seq.isEmpty unexpectedHeaders) |> not
    then failwith <| $"""(Invalid Header) short: {String.Join(" ",missingHeaders)}. invalid: {String.Join(" ",unexpectedHeaders)}"""


  let skipRows (csv:CsvReader) i =
    async {

      let skipRow (csv:CsvReader) =
        async {
          let! _ = csv.ReadAsync() |> Async.AwaitTask
          return ()
        }

      do!
        [1..i]
        |> Seq.map (fun _ -> skipRow csv )
        |> Async.Sequential
        |> Async.Ignore

    }