module ProductComparer.Tests.GetChanged

open ProductComparer
open ProductComparer.Models
open Xunit

[<Fact>]
let ``finds lowest of increased prices`` () =
  let stelProducts: StelProduct list =
    [ { Barcode = "A"
        PurchasePrice = 9.99M
        Name = "Pen drive 32GB" } ]

  let providerProducts: ProviderProduct list =
    [ { Barcode = "A"
        Price = 10M
        ProviderName = "banana" }
      { Barcode = "A"
        Price = 11M
        ProviderName = "pear" } ]

  let initialResult =
    { Products = stelProducts
      ProvidersNotUsed =
        [ { Name = "banana" }
          { Name = "banana" } ]
      ProductsFound = [] }

  let summary =
    (initialResult, stelProducts)
    ||> List.fold (Summary.getBuilder providerProducts)

  let increased = Summary.increased summary
  Assert.Contains(increased, (fun p -> p.Barcode = "A"))
  Assert.Contains(increased, (fun p -> p.OldPrice = 9.99M))
  Assert.Contains(increased, (fun p -> p.NewPrice = 10M))
  Assert.Contains(increased, (fun p -> p.Provider = "banana"))

[<Fact>]
let ``finds decreased prices`` () =
  let stelProducts: StelProduct list =
    [ { Barcode = "A"
        PurchasePrice = 10M
        Name = "Pen drive 32GB" } ]

  let providerProducts: ProviderProduct list =
    [ { Barcode = "A"
        Price = 8M
        ProviderName = "banana" }
      { Barcode = "A"
        Price = 9M
        ProviderName = "pear" } ]

  let initialResult =
    { Products = stelProducts
      ProvidersNotUsed =
        [ { Name = "banana" }
          { Name = "pear" } ]
      ProductsFound = [] }

  let summary =
    (initialResult, stelProducts)
    ||> List.fold (Summary.getBuilder providerProducts)

  let increased = Summary.decreased summary
  Assert.Contains(increased, (fun p -> p.Barcode = "A"))
  Assert.Contains(increased, (fun p -> p.OldPrice = 10M))
  Assert.Contains(increased, (fun p -> p.NewPrice = 8M))
  Assert.Contains(increased, (fun p -> p.Provider = "banana"))
