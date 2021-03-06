module ProductComparer.Tests.NotFound

open ProductComparer
open ProductComparer.Models
open Xunit

[<Fact>]
let ``returns not found products`` () =
  let providerProducts: ProviderProduct list =
    [ { Barcode = "A"
        Price = 10M
        ProviderName = "banana" } ]

  let stelProducts: OwnProduct list =
    [ { Barcode = "A"
        PurchasePrice = 9.99M
        Name = "Pen drive 32GB" }
      { Barcode = "B"
        PurchasePrice = 9.99M
        Name = "Pen drive 32GB" } ]

  let initialResult =
    { Products = stelProducts
      ProvidersNotUsed =
        [ { Name = "banana" }
          { Name = "pear" } ]
      ProductsFound = [] }

  let summary =
    (initialResult, stelProducts)
    ||> List.fold (Summary.getBuilder providerProducts)

  let notFound = summary |> Summary.notFound

  Assert.Contains(notFound, (fun p -> p.Barcode = "B"))
  Assert.DoesNotContain(notFound, (fun p -> p.Barcode = "A"))
