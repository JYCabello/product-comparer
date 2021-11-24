module ProductComparer.Models

open Newtonsoft.Json

[<CLIMutable>]
type StelProduct =
  { Barcode: string
    [<JsonProperty("purchase-price")>]
    PurchasePrice: decimal
    Name: string }

type ProviderProduct =
  { Barcode: string
    Price: decimal
    ProviderName: string }

[<CLIMutable>]
type InfortisaProduct = { EANUpc: string; Price: decimal }

type CleanBarcode =
  static member Do(sp: StelProduct) =
    { sp with
        Barcode = sp.Barcode.Trim().ToUpperInvariant() }

  static member Do(sp: ProviderProduct) =
    { sp with
        Barcode = sp.Barcode.Trim().ToUpperInvariant() }

  static member Do(sp: InfortisaProduct) =
    { sp with
        EANUpc = sp.EANUpc.Trim().ToUpperInvariant() }


type FailedFile = { FileName: string; Error: string }

type ProvidedProduct =
  { Name: string
    Provider: string
    OldPrice: decimal
    NewPrice: decimal
    Barcode: string }

type ProductsSummary =
  { Products: StelProduct list
    ProvidersNotUsed: string list
    ProductsFound: ProvidedProduct list }

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
