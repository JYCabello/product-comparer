module ProductComparer.Providers_Dmi

open System
open System.IO
open System.Text
open System.Threading
open System.Threading.Tasks
open System.Xml
open FsToolkit.ErrorHandling
open Newtonsoft.Json
open ProductComparer.Models
open ProductComparer.Providers
open ProductComparer.Singletons
open System.Linq
open ProductComprarer.DMIPort.Productos

type DmiCredentials = { Username: string; Password: string }


let credentials =
  option {
    let! userName = settings.DmiUsername
    let! password = settings.DmiPassword

    return
      { Username = userName
        Password = password }
  }

let getProduct (node: XmlNode) : ProviderProduct option =
  option {
    let! barcode =
      node.ChildNodes.Cast<XmlNode>()
      |> Seq.filter (fun n -> n.Name.ToUpperInvariant() = "EAN")
      |> Seq.filter (fun n -> n.InnerText |> String.IsNullOrWhiteSpace |> not)
      |> Seq.map (fun n -> n.InnerText)
      |> Seq.tryHead

    let! price =
      node.ChildNodes.Cast<XmlNode>()
      |> Seq.filter (fun n -> n.Name.ToUpperInvariant() = "PRECIO")
      |> Seq.map
           (fun n ->
             match n.InnerText |> Decimal.TryParse with
             | true, v -> Some v
             | false, _ -> None)
      |> Seq.choose id
      |> Seq.tryHead

    return
      { Barcode = barcode.ToUpperInvariant()
        Price = price
        ProviderName = "DMI" }
  }

[<Literal>]
let cachedDmiFolder = "./cache/dmi"

[<Literal>]
let timestampFormat = "yyyyMMdd-HHmm"

let getCacheFileName () =
  DateTime.Now.ToString timestampFormat
  |> sprintf "%s.json"

let saveCache (result: CatalogoResponseCatalogoResult) =
  try
    let isError =
      result.Any1.ChildNodes.Cast<XmlNode>()
      |> Seq.head
      |> fun n -> n.InnerText.StartsWith("Lo sentimos")

    match isError with
    | true -> Async.singleton ()
    | false ->
      File.WriteAllTextAsync(
        Path.Combine(cachedDmiFolder, getCacheFileName ()),
        JsonConvert.SerializeObject result,
        Encoding.UTF8,
        Unchecked.defaultof<CancellationToken>
      )
      |> Async.AwaitTask
  with
  | _ -> Async.singleton ()

let private map (result: CatalogoResponseCatalogoResult) =
  try
    result.Any1.ChildNodes.Cast<XmlNode>()
    |> Seq.head
    |> fun n -> n.ChildNodes.Cast<XmlNode>()
    |> Seq.map getProduct
    |> Seq.toList
    |> List.choose id
  with
  | _ -> []

let private tryParse timestamp =
  try
    Some
    <| DateTime.ParseExact(timestamp, timestampFormat, null :> IFormatProvider)
  with
  | _ -> None

let private getFromCache shouldGetOld =
  asyncOption {
    try
      Directory.CreateDirectory cachedDmiFolder
      |> ignore

      let! file =
        Directory.GetFiles(cachedDmiFolder)
        |> Seq.sortByDescending id
        |> Seq.tryHead

      let! dateTime = Path.GetFileNameWithoutExtension file |> tryParse

      let! validFile =
        match (DateTime.Now - dateTime) with
        | diff when diff.Hours < 3 || shouldGetOld -> Some file
        | _ -> None

      let! json = File.ReadAllTextAsync(validFile, Encoding.UTF8, Unchecked.defaultof<CancellationToken>)

      return
        JsonConvert.DeserializeObject<CatalogoResponseCatalogoResult> json
        |> map
    with
    | _ -> return! None
  }

let private getFromService creds =
  async {
    try
      let! response =
        ProductComparer.DMIPort.DMIPort.GetAsync(creds.Username, creds.Password)
        |> Async.AwaitTask

      do! saveCache response

      return response |> map
    with
    | _ -> return []
  }

let private allProductsTask: Task<ProviderProduct list> =
  match credentials with
  | None -> [] |> Task.FromResult
  | Some creds ->
    async {
      let! fromCache = getFromCache false

      return!
        match fromCache with
        | Some cache -> Async.singleton cache
        | None -> getFromService creds
    }
    |> Async.StartAsTask


type Dmi() =
  interface Provider with
    member this.get product =
      asyncOption {
        let! products = allProductsTask

        let! product =
          products
          |> List.tryFind (fun p -> p.Barcode = product.Barcode)

        return product
      }

    member this.name = "DMI"
