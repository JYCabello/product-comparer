module Tests

open ProductComparer
open Xunit
open ProductComparer.Models

[<Fact>]
let ``finds the product`` () =
  let providerProducts: ProviderProduct list =
    [ { Barcode = "A"
        Price = 10M
        ProviderName = "banana" } ]

  let stelProducts: StelProduct list =
    [ { Barcode = "A"
        PurchasePrice = 9.99M
        Name = "Pen drive 32GB" }
      { Barcode = "B"
        PurchasePrice = 9.99M
        Name = "Pen drive 32GB" } ]

  let initialResult =
    { Products = stelProducts
      ProvidersNotUsed = [ "banana" ]
      ProductsFound = [] }

  let summary =
    (initialResult, stelProducts)
    ||> List.fold (Summary.getBuilder providerProducts)

  Assert.Contains(summary.Products, (fun p -> p.Barcode = "A"))
  Assert.Contains(summary.Products, (fun p -> p.Barcode = "B"))

  Assert.Contains(summary.ProductsFound, (fun p -> p.Barcode = "A"))
  Assert.DoesNotContain(summary.ProductsFound, (fun p -> p.Barcode = "B"))

[<Fact>]
let ``contains unused provider`` () =
  let providerProducts: ProviderProduct list =
    [ { Barcode = "A"
        Price = 10M
        ProviderName = "banana" } ]

  let stelProducts: StelProduct list =
    [ { Barcode = "A"
        PurchasePrice = 9.99M
        Name = "Pen drive 32GB" }
      { Barcode = "B"
        PurchasePrice = 9.99M
        Name = "Pen drive 32GB" } ]

  let initialResult =
    { Products = stelProducts
      ProvidersNotUsed = [ "banana"; "pear" ]
      ProductsFound = [] }

  let summary =
    (initialResult, stelProducts)
    ||> List.fold (Summary.getBuilder providerProducts)

  Assert.Contains(summary.ProvidersNotUsed, (fun p -> p = "pear"))
  Assert.DoesNotContain(summary.ProvidersNotUsed, (fun p -> p = "banana"))
