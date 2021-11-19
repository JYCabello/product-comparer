﻿module ProductComparer.Providers_Infortisa

open System.IO
open System.Net.Http
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open Newtonsoft.Json
open ProductComparer.Providers
open ProductComparer.Singletons

let apiKey =
  try
    Some <| File.ReadAllLines("infortisa.key.txt").[0]
  with
  | _ -> None

let searchUrl searchTerm pageNo =
  $"http://api.infortisa.com/api/Product/SearchProducts?searchString=%s{searchTerm}&pageNumber=%i{pageNo}"

[<CLIMutable>]
type InfortisaProduct = { EANUpc: string; Price: decimal }

// La API de infortisa muere a la que haces más de veinte búsquedas en un corto periodo de tiempo
// por suerte, no tienen demasiados productos (<8k) así que simplemente se puede pedir el paquete completo
// (correcto, no pagina) y dejar en memoria solamente los datos necesarios.
let allInfortisaProductsTask =
  match apiKey with
  | None -> Task.FromResult<InfortisaProduct list>([])
  | Some key ->
    task {
      try
        use request =
          new HttpRequestMessage(HttpMethod.Get, "http://api.infortisa.com/api/Product/Get")

        request.Headers.Add("Authorization-Token", key)
        request.Headers.Add("Accept", "application/json")

        let! response = httpClient.SendAsync request
        let! body = response.Content.ReadAsStringAsync()
        return JsonConvert.DeserializeObject<InfortisaProduct list>(body)
      with
      | _ -> return []
    }

type Infortisa() =

  interface Provider with
    member this.get product =
      asyncOption {
        try
          let! prods = allInfortisaProductsTask

          let! prod =
            prods
            |> List.tryFind (fun p -> p.EANUpc.Trim() = product.Barcode.Trim())

          return!
            Some
            <| { Barcode = prod.EANUpc
                 Price = prod.Price
                 ProviderName = "Infortisa" }
        with
        | _ -> return! None
      }

    member this.name = "Infortisa"
