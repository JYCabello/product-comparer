module ProductComparer.Tests.ModelNormalizer

open ProductComparer.Models
open Xunit

[<Fact>]
let ``cleans stel product`` () =
  let stelProduct: StelProduct =
    { Barcode = " a-1A "
      PurchasePrice = 1M
      Name = "Product" }

  let cleaned = ModelNormalizer.Do stelProduct
  Assert.Equal("A-1A", cleaned.Barcode)

[<Fact>]
let ``cleans provider product`` () =
  let stelProduct: ProviderProduct =
    { Barcode = " a-1A "
      Price = 1M
      ProviderName = "banana" }

  let cleaned = ModelNormalizer.Do stelProduct
  Assert.Equal("A-1A", cleaned.Barcode)
