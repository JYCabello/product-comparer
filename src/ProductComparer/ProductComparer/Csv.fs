module ProductComparer.Csv

open System
open System.Collections.Generic
open System.Globalization
open System.IO
open System.Threading
open CsvHelper
open CsvHelper.Configuration
open FsToolkit.ErrorHandling
open ProductComparer.Models

let folderName =
  let timestamp = DateTime.Now.ToString "yyyyMMdd-HHmm"
  $"informes-%s{timestamp}"

let write items (fileName: string) =
  let config =
    let cfg =
      CsvConfiguration(CultureInfo.InvariantCulture)

    cfg.HasHeaderRecord <- true
    cfg

  match items with
  | its when (its |> Seq.length) < 1 -> asyncResult { return () }
  | _ ->
    asyncResult {
      try
        Directory.CreateDirectory folderName |> ignore

        use writer =
          new StreamWriter(Path.Combine(folderName, fileName))

        use csv = new CsvWriter(writer, config)

        return!
          csv.WriteRecordsAsync((items: IEnumerable<'a>), Unchecked.defaultof<CancellationToken>)
          |> Async.AwaitTask
      with
      | ex ->
        return!
          Error
          <| Errors.CouldNotWriteCsv
               { FileName = fileName
                 Error = ex.Message }
    }
