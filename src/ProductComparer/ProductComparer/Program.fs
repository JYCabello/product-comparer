open ProductComparer
open FsToolkit.ErrorHandling
open ProductComparer.Models

type ProvidedProduct =
  { Name: string
    Provider: string
    OldPrice: decimal
    NewPrice: decimal
    Barcode: string }

type Results =
  { Products: StelProduct list
    ProvidersNotUsed: string list
    ProductsFound: ProvidedProduct list }


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
      |> List.map
           (fun p ->
             { p with
                 Barcode = p.Barcode.Trim().ToUpperInvariant() })
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
      ||> List.fold
            (fun acc p ->
              let provided =
                providedProducts
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
                      |> List.filter (fun prv -> not (prv = pp.ProviderName)) })

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
