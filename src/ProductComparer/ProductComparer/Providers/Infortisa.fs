module ProductComparer.Providers_Infortisa

open System.Net.Http
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open Newtonsoft.Json
open ProductComparer.Providers
open ProductComparer.Singletons
open ProductComparer.Models


let searchUrl searchTerm pageNo =
  $"http://api.infortisa.com/api/Product/SearchProducts?searchString=%s{searchTerm}&pageNumber=%i{pageNo}"

// La API de infortisa muere a la que haces más de veinte búsquedas en un corto periodo de tiempo
// por suerte, no tienen demasiados productos (<8k) así que simplemente se puede pedir el paquete completo
// (correcto, no pagina) y dejar en memoria solamente los datos necesarios.
let allInfortisaProductsTask =
  match settings.InfortisaKeySafe with
  | None -> Task.FromResult<InfortisaProduct list>([])
  | Some key ->
    task {
      try
        use request =
          new HttpRequestMessage(HttpMethod.Get, "http://api.infortisa.com/api/Product/Get")

        request.Headers.Add("Authorization-Token", key)

        let! response = httpClient.SendAsync request
        let! body = response.Content.ReadAsStringAsync()

        return
          JsonConvert.DeserializeObject<InfortisaProduct list>(body)
          |> List.map ModelNormalizer.Do
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
            |> List.tryFind (fun p -> p.EANUpc = product.Barcode)

          return!
            Some
            <| { Barcode = prod.EANUpc
                 Price = prod.Price
                 ProviderName = "Infortisa" }
        with
        | _ -> return! None
      }

    member this.name = "Infortisa"
