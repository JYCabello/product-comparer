module ProductComparer.Models

open Newtonsoft.Json

[<CLIMutable>]
type StelProduct =
  { Barcode: string
    [<JsonProperty("purchase-price")>]
    PurchasePrice: decimal
    Name: string }


type FailedFile = { FileName: string; Error: string }

type Errors =
  | StelOrderNotReachable of string
  | CouldNotWriteCsv of FailedFile

let handler =
  function
  | StelOrderNotReachable sonr ->
    printfn
      $"No se pudo acceder a stelorder\n%s{sonr
                                           |> function
                                             | s -> s}"
  | CouldNotWriteCsv fn -> printfn $"No se pudo escribir el fichero %s{fn.FileName}\n%s{fn.Error}"
