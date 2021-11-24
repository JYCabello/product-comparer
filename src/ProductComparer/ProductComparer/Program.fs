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

    let summary =
      (initialResults, products)
      ||> List.fold (Summary.getBuilder providedProducts)

    do! Csv.write products "productos.csv"

    do! Csv.write (Summary.increased summary) "precios-incrementados.csv"

    do! Csv.write (Summary.decreased summary) "precios-reducidos.csv"

    let notFound =
      products
      |> List.filter
           (fun p ->
             summary.ProductsFound
             |> List.exists (fun pf -> pf.Barcode = p.Barcode)
             |> not)

    do! Csv.write notFound "productos-no-encontrados.csv"

    do!
      match summary.ProvidersNotUsed with
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
