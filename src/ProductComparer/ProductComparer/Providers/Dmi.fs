module ProductComparer.Providers_Dmi

open System
open System.Threading.Tasks
open System.Xml
open FsToolkit.ErrorHandling
open ProductComparer.Models
open ProductComparer.Providers
open ProductComparer.Singletons
open System.Linq

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

let allProductsTask: Task<ProviderProduct list> =
  match credentials with
  | None -> [] |> Task.FromResult
  | Some creds ->
    task {
      try
        let! response = ProductComparer.DMIPort.DMIPort.GetAsync(creds.Username, creds.Password)

        return
          response.ChildNodes.Cast<XmlNode>()
          |> Seq.map getProduct
          |> Seq.toList
          |> List.choose id
      with
      | _ -> return []
    }

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
