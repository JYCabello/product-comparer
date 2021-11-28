module ProductComparer.Csv

open System
open System.Globalization
open System.IO
open System.Text
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
    cfg.Encoding <- Encoding.UTF8
    cfg

  match items with
  | [] -> asyncResult { return () }
  | _ ->
    asyncResult {
      try
        Directory.CreateDirectory folderName |> ignore

        use writer =
          new StreamWriter(Path.Combine(folderName, fileName), true, Encoding.UTF8)

        use csv = new CsvWriter(writer, config)

        return!
          csv.WriteRecordsAsync(items |> List.toSeq, Unchecked.defaultof<CancellationToken>)
          |> Async.AwaitTask
      with
      | ex ->
        return!
          Error
          <| Errors.CouldNotWriteCsv
               { FileName = fileName
                 Error = ex.Message }
    }
