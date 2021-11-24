﻿module ProductComparer.Summary

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
            |> List.filter (fun prv -> not (prv = pp.ProviderName)) }

  folder

let increased summary =
  summary.ProductsFound
  |> List.filter (fun p -> p.NewPrice > p.OldPrice)

let decreased summary =
  summary.ProductsFound
  |> List.filter (fun p -> p.NewPrice < p.OldPrice)

let notFound (products: StelProduct list) summary =
  products
  |> List.filter
       (fun p ->
         summary.ProductsFound
         |> List.exists (fun pf -> pf.Barcode = p.Barcode)
         |> not)