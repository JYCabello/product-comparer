module ProductComparer.StelOrders

open System
open System.IO
open System.Net.Http
open Newtonsoft.Json
open FsToolkit.ErrorHandling
open ProductComparer.Models
open ProductComparer.Singletons

let apiKey =
  File.ReadAllLines("stelorder.key.txt").[0]

let fetchUrl<'a> (url: string) =
  asyncResult {
    try
      use request =
        new HttpRequestMessage(HttpMethod.Get, url)

      request.Headers.Add("APIKEY", apiKey)

      let! response = httpClient.SendAsync request

      let! body =
        response.Content.ReadAsStringAsync()
        |> Async.AwaitTask

      return JsonConvert.DeserializeObject<'a>(body)
    with
    | ex -> return! Error <| Errors.StelOrderNotReachable ex.Message
  }

[<Literal>]
let pageSize = 500

let prodsUrl pageNo =
  $"https://app.stelorder.com/app/products?limit=500&start={pageNo * pageSize}"

let getAllProducts () =
  let batchSize = 10

  let fetchPage pageNo =
    fetchUrl<StelProduct list> <| prodsUrl pageNo

  let fetchPageBatch batchNo =
    asyncResult {
      let firstPage = batchNo * batchSize
      let lastPage = firstPage - 1 + batchSize

      let! fibers =
        [ firstPage .. lastPage ]
        |> List.map (fun p -> fetchPage p |> Async.StartChild)
        |> Async.Parallel

      let! resultArray = fibers |> Async.Parallel

      let! products =
        (Ok [], resultArray)
        ||> Array.fold
              (fun acc rslt ->
                result {
                  let! acc = acc
                  let! rslt = rslt
                  return acc @ rslt
                })

      return
        products
        |> List.filter (fun p -> not (String.IsNullOrWhiteSpace(p.Barcode)))
        |> List.map ModelNormalizer.Do
    }

  let rec getBatch (products: StelProduct list) batchNo =
    asyncResult {
      let! prods = fetchPageBatch batchNo

      return!
        match prods with
        | batch when batch.Length < (batchSize * pageSize) -> asyncResult { return (batch @ products) }
        | [] -> asyncResult { return products }
        | _ -> asyncResult { return! getBatch (prods @ products) (batchNo + 1) }
    }

  getBatch [] 0
