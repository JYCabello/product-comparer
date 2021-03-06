open ProductComparer
open FsToolkit.ErrorHandling
open ProductComparer.Models


let provProd product =
  async {
    let! prods =
      ProviderFactory.providers
      |> List.map (fun prov -> prov.get product)
      |> Async.Parallel

    return
      prods
      |> Array.choose id
      |> Array.toList
      |> List.map ModelNormalizer.Do
  }

let handleProducts products =
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
          |> List.map (fun p -> { Name = p.name })
        ProductsFound = [] }

    let summary =
      (initialResults, products)
      ||> List.fold (Summary.getBuilder providedProducts)

    do! Csv.write products "productos.csv"
    do! Csv.write (summary |> Summary.increased) "precios-incrementados.csv"
    do! Csv.write (summary |> Summary.decreased) "precios-reducidos.csv"
    do! Csv.write (summary |> Summary.notFound) "productos-no-encontrados.csv"
    do! Csv.write summary.ProvidersNotUsed "proveedores-no-usados.csv"

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
  | Ok _ -> printfn "Finalizado"
  | Error err -> handler err
