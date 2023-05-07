namespace CsvHelperfs

(*
  FILE    : CsvHelperConfig.fs
  AUTHOR  : callmekohei
  LICENSE :
  DESC    :
*)

open System

[<CLIMutable>]
type CsvHelperWriterConfig =
  {
    AllowComments                       : Nullable<bool>
    BufferSize                          : Nullable<int>
    Comment                             : Nullable<char>
    Delimiter                           : string
    Escape                              : Nullable<char>
    ExceptionMessagesContainRawData     : Nullable<bool>
    HasHeaderRecord                     : bool
    IgnoreReferences                    : Nullable<bool>
    IncludePrivateMembers               : Nullable<bool>
    InjectionCharacters                 : char array
    InjectionEscapeCharacter            : Nullable<char>
    InjectionOptions                    : Nullable<int>
    IsNewLineSet                        : Nullable<bool>
    MemberTypes                         : Nullable<int>
    Mode                                : Nullable<int>
    NewLine                             : string
    Quote                               : Nullable<char>
    ShouldQuote                         : Nullable<bool>
    TrimOptions                         : Nullable<int>
    UseNewObjectForNullReferenceMembers : Nullable<bool>
    Validate                            : Nullable<int>
  }


[<CLIMutable>]
type CsvHelperReaderConfig =
  {
    AllowComments                   : Nullable<bool>
    BufferSize                      : Nullable<int>
    CacheFields                     : Nullable<bool>
    Comment                         : Nullable<char>
    CountBytes                      : Nullable<bool>
    Delimiter                       : string
    DetectColumnCountChanges        : Nullable<bool>
    DetectDelimiter                 : Nullable<bool>
    DetectDelimiterValues           : string array
    Escape                          : Nullable<char>
    ExceptionMessagesContainRawData : Nullable<bool>
    HasHeaderRecord                 : bool
    IgnoreBlankLines                : Nullable<bool>
    IgnoreReferences                : Nullable<bool>
    IncludePrivateMembers           : Nullable<bool>
    LineBreakInQuotedFieldIsBadData : Nullable<bool>
    MaxFieldSize                    : Nullable<float>
    MemberTypes                     : Nullable<int>
    Mode                            : Nullable<int>
    NewLine                         : string
    ProcessFieldBufferSize          : Nullable<int>
    Quote                           : Nullable<char>
    TrimOptions                     : Nullable<int>
    Validate                        : Nullable<int>
    WhiteSpaceChars                 : char array
  }


module CsvHelperConfig =

  open System.Globalization
  open CsvHelper
  open CsvHelper.Configuration

  let csvWriterConfigImpl (cfg:CsvHelperWriterConfig) =
    CsvConfiguration(CultureInfo.CurrentCulture)
    |> fun x ->

      // IWriterConfiguration.cs
      // https://github.com/JoshClose/CsvHelper/blob/master/src/CsvHelper/Configuration/IWriterConfiguration.cs

      if cfg.AllowComments.HasValue                        then x.AllowComments                       <- cfg.AllowComments.Value                   // default true
      if cfg.BufferSize.HasValue                           then x.BufferSize                          <- cfg.BufferSize.Value                      // default 0x1000
      if cfg.Comment.HasValue                              then x.Comment                             <- cfg.Comment.Value                         // default #
      if cfg.Delimiter |> isNull |> not                    then x.Delimiter                           <- cfg.Delimiter                             // default ,
      if cfg.Escape.HasValue                               then x.Escape                              <- cfg.Escape.Value                          // default "
      if cfg.ExceptionMessagesContainRawData.HasValue      then x.ExceptionMessagesContainRawData     <- cfg.ExceptionMessagesContainRawData.Value // default true
      x.HasHeaderRecord <- cfg.HasHeaderRecord
      if cfg.IgnoreReferences.HasValue                     then x.IgnoreReferences                    <- cfg.IgnoreReferences.Value                // default false
      if cfg.IncludePrivateMembers.HasValue                then x.IncludePrivateMembers               <- cfg.IncludePrivateMembers.Value           // default false
      if cfg.InjectionCharacters |> Array.isEmpty |> not   then x.InjectionCharacters                 <- cfg.InjectionCharacters
      if cfg.InjectionEscapeCharacter.HasValue             then x.InjectionEscapeCharacter            <- cfg.InjectionEscapeCharacter.Value
      if cfg.InjectionOptions.HasValue                     then x.InjectionOptions                    <- match cfg.InjectionOptions.Value with
                                                                                                          | 0 -> InjectionOptions.None
                                                                                                          | 1 -> InjectionOptions.Escape
                                                                                                          | 2 -> InjectionOptions.Strip
                                                                                                          | _ -> InjectionOptions.Exception
      if cfg.MemberTypes.HasValue                          then x.MemberTypes                         <- match cfg.MemberTypes.Value with         // default Properties
                                                                                                          | 0 -> MemberTypes.None
                                                                                                          | 1 -> MemberTypes.Properties
                                                                                                          | _ -> MemberTypes.Fields
      if cfg.Mode.HasValue                                 then x.Mode                                <- match cfg.Mode.Value |> int with
                                                                                                          | 0 -> CsvMode.RFC4180
                                                                                                          | 1 -> CsvMode.Escape
                                                                                                          | _ -> CsvMode.NoEscape
      if cfg.NewLine |> isNull |> not                      then x.NewLine                             <- cfg.NewLine                              // Default \r\n (CRLF)
      if cfg.Quote.HasValue                                then x.Quote                               <- cfg.Quote.Value                          // default "
      if cfg.ShouldQuote.HasValue                          then x.ShouldQuote                         <- fun x -> cfg.ShouldQuote.Value
      if cfg.TrimOptions.HasValue                          then x.TrimOptions                         <- match cfg.TrimOptions.Value with
                                                                                                          | 0 -> TrimOptions.None
                                                                                                          | 1 -> TrimOptions.Trim
                                                                                                          | 2 -> TrimOptions.InsideQuotes
                                                                                                          | _ -> TrimOptions.Trim ||| TrimOptions.InsideQuotes
      if cfg.UseNewObjectForNullReferenceMembers.HasValue  then x.UseNewObjectForNullReferenceMembers <- cfg.UseNewObjectForNullReferenceMembers.Value
      if cfg.Validate.HasValue                             then x.Validate()

      x :> IWriterConfiguration


  let csvReaderConfigImpl (cfg:CsvHelperReaderConfig) =
    CsvConfiguration(CultureInfo.CurrentCulture)
    |> fun x ->

      // IReaderConfiguration.cs
      // https://github.com/JoshClose/CsvHelper/blob/master/src/CsvHelper/Configuration/IReaderConfiguration.cs

      if cfg.DetectColumnCountChanges.HasValue then x.DetectColumnCountChanges  <- cfg.DetectColumnCountChanges.Value  // default false
      x.HasHeaderRecord <- cfg.HasHeaderRecord
      if cfg.IgnoreReferences.HasValue         then x.IgnoreReferences          <- cfg.IgnoreReferences.Value          // default false
      if cfg.IncludePrivateMembers.HasValue    then x.IncludePrivateMembers     <- cfg.IncludePrivateMembers.Value     // default false
      if cfg.MemberTypes.HasValue              then x.MemberTypes               <- match cfg.MemberTypes.Value with    // default MemberTypes.Properties
                                                                                    | 0 -> MemberTypes.None
                                                                                    | 1 -> MemberTypes.Properties
                                                                                    | _ -> MemberTypes.Fields

      // IParserConfiguration.cs
      // https://github.com/JoshClose/CsvHelper/blob/master/src/CsvHelper/Configuration/IParserConfiguration.cs

      if cfg.AllowComments.HasValue                        then x.AllowComments                   <- cfg.AllowComments.Value                   // default true
      if cfg.BufferSize.HasValue                           then x.BufferSize                      <- cfg.BufferSize.Value                      // default 0x1000
      if cfg.CacheFields.HasValue                          then x.CacheFields                     <- cfg.CacheFields.Value                     // default false
      if cfg.Comment.HasValue                              then x.Comment                         <- cfg.Comment.Value                         // default #
      if cfg.CountBytes.HasValue                           then x.CountBytes                      <- cfg.CountBytes.Value                      // default false
      if cfg.Delimiter |> isNull |> not                    then x.Delimiter                       <- cfg.Delimiter                             // default ,
      if cfg.DetectDelimiter.HasValue                      then x.DetectDelimiter                 <- cfg.DetectDelimiter.Value                 // default false
      if cfg.DetectDelimiterValues |> Array.isEmpty |> not then x.DetectDelimiterValues           <- cfg.DetectDelimiterValues                 // default [|","; ";"; "|";"\t"|]
      if cfg.Escape.HasValue                               then x.Escape                          <- cfg.Escape.Value                          // default "
      if cfg.ExceptionMessagesContainRawData.HasValue      then x.ExceptionMessagesContainRawData <- cfg.ExceptionMessagesContainRawData.Value // default true
      if cfg.IgnoreBlankLines.HasValue                     then x.IgnoreBlankLines                <- cfg.IgnoreBlankLines.Value                // default true
      if cfg.LineBreakInQuotedFieldIsBadData.HasValue      then x.LineBreakInQuotedFieldIsBadData <- cfg.LineBreakInQuotedFieldIsBadData.Value // default false
      if cfg.MaxFieldSize.HasValue                         then x.MaxFieldSize                    <- cfg.MaxFieldSize.Value                    // default 0
      if cfg.Mode.HasValue                                 then x.Mode                            <- match cfg.Mode.Value |> int with  // default 0
                                                                                                      | 0 -> CsvMode.RFC4180
                                                                                                      | 1 -> CsvMode.Escape
                                                                                                      | _ -> CsvMode.NoEscape
      if cfg.NewLine |> isNull |> not                      then x.NewLine                         <- cfg.NewLine                               // If not set, the parser uses one of \r\n, \r, or \n.
      if cfg.ProcessFieldBufferSize.HasValue               then x.ProcessFieldBufferSize          <- cfg.ProcessFieldBufferSize.Value          // default 1024
      if cfg.Quote.HasValue                                then x.Quote                           <- cfg.Quote.Value                           // default "
      if cfg.TrimOptions.HasValue                          then x.TrimOptions                     <- match cfg.TrimOptions.Value with
                                                                                                      | 0 -> TrimOptions.None
                                                                                                      | 1 -> TrimOptions.Trim
                                                                                                      | 2 -> TrimOptions.InsideQuotes
                                                                                                      | _ -> TrimOptions.Trim ||| TrimOptions.InsideQuotes
      if cfg.Validate.HasValue                             then x.Validate()
      if cfg.WhiteSpaceChars |>Array.isEmpty |> not        then x.WhiteSpaceChars                 <- cfg.WhiteSpaceChars                       // default [|' '|]

      x :> IReaderConfiguration