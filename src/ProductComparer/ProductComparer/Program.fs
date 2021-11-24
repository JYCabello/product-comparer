open ProductComparer
open FsToolkit.ErrorHandling
open ProductComparer.Models


let provProd (product: StelProduct) =
  async {
    let! prods =
      ProviderFactory.providers
      |> List.map (fun prov -> prov.get product)
      |> Async.Parallel

    return
      prods
      |> Array.choose id
      |> Array.toList
      |> List.map CleanBarcode.Do
  }

let handleProducts (products: StelProduct list) =
  asyncResult {
    let! providedProducts = products |> List.map provProd |> Async.Parallel

    let providedProducts =
      providedProducts
      |> Array.toList
      |> List.collect id

    let initialResults =
      { Products = products
        ProvidersNotUsed =
          ProviderFactory.providers
          |> List.map (fun p -> p.name)
        ProductsFound = [] }

    let results =
      (initialResults, products)
      ||> List.fold (ProductProcessor.getFolder providedProducts)

    do! Csv.write products "productos.csv"

    let increased =
      results.ProductsFound
      |> List.filter (fun p -> p.NewPrice > p.OldPrice)

    do! Csv.write increased "precios-incrementados.csv"

    let reduced =
      results.ProductsFound
      |> List.filter (fun p -> p.NewPrice < p.OldPrice)

    do! Csv.write reduced "precios-reducidos.csv"

    let notFound =
      products
      |> List.filter
           (fun p ->
             results.ProductsFound
             |> List.exists (fun pf -> pf.Barcode = p.Barcode)
             |> not)

    do! Csv.write notFound "productos-no-encontrados.csv"

    do!
      match results.ProvidersNotUsed with
      | notUsed when notUsed.Length > 0 -> Csv.write notUsed "proveedores-no-usados.csv"
      | _ -> async { return Ok() }

    return ()
  }

asyncResult {
  let! prods = StelOrders.getAllProducts ()
  printfn $"Procesando %i{prods.Length} productos en los siquientes proveedores:"

  for prov in ProviderFactory.providers do
    printfn $"%s{prov.name}"

  do! handleProducts prods
  return prods
}
|> Async.RunSynchronously
|> function
  | Ok _ -> ()
  | Error err -> handler err
