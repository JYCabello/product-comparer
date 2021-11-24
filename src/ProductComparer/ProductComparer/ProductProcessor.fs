module ProductComparer.ProductProcessor

open ProductComparer.Models

let getFolder (provProds: ProviderProduct list) =
  let folder (acc: ProductComparisonSummary) (p: StelProduct) =
    let provided =
      provProds
      |> List.filter (fun pp -> pp.Barcode = p.Barcode)
      |> List.sortBy (fun pp -> pp.Price)
      |> List.tryHead

    match provided with
    | None -> acc
    | Some pp ->
      let cp =
        { Barcode = pp.Barcode
          OldPrice = p.PurchasePrice
          NewPrice = pp.Price
          Provider = pp.ProviderName
          Name = p.Name }

      { acc with
          ProductsFound = cp :: acc.ProductsFound
          ProvidersNotUsed =
            acc.ProvidersNotUsed
            |> List.filter (fun prv -> not (prv = pp.ProviderName)) }
  folder
