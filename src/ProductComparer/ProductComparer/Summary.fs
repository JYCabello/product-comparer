module ProductComparer.Summary

open ProductComparer.Models

let getBuilder (provProds: ProviderProduct list) =
  let folder (acc: ProductsSummary) (p: StelProduct) =
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
            |> List.filter (fun prv -> not (prv.Name = pp.ProviderName)) }

  folder

let increased summary =
  summary.ProductsFound
  |> List.filter (fun p -> p.NewPrice > p.OldPrice)

let decreased summary =
  summary.ProductsFound
  |> List.filter (fun p -> p.NewPrice < p.OldPrice)

let notFound summary =
  summary.Products
  |> List.filter
       (fun p ->
         summary.ProductsFound
         |> List.exists (fun pf -> pf.Barcode = p.Barcode)
         |> not)
