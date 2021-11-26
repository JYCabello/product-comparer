module ProductComparer.Models

open Newtonsoft.Json

[<CLIMutable>]
type SettingsDto =
  { StelOrderKey: string
    InfortisaKey: string
    DmiUsername: string
    DmiPassword: string }

type Settings =
  { StelOrderKey: string
    InfortisaKey: string option
    DmiUsername: string option
    DmiPassword: string option }
  static member from(dto: SettingsDto) : Settings =
    { StelOrderKey = dto.StelOrderKey
      InfortisaKey =
        match dto.InfortisaKey with
        | null -> None
        | key -> Some key
      DmiUsername =
        match dto.DmiUsername with
        | null -> None
        | username -> Some username
      DmiPassword =
        match dto.DmiPassword with
        | null -> None
        | password -> Some password }


[<CLIMutable>]
type OwnProduct =
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

type ModelNormalizer =
  static member Do(sp: OwnProduct) =
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

type ProviderInfo = { Name: string }

type ProductsSummary =
  { Products: OwnProduct list
    ProvidersNotUsed: ProviderInfo list
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
