namespace ProductComparer

open FsToolkit.ErrorHandling

type ResultCollector =
  static member collect(list: list<Result<_ list, _>>) : Result<_ list, _> =
    (Ok [], list)
    ||> Seq.fold
          (fun acc rslt ->
            result {
              let! acc = acc
              let! rslt = rslt
              return acc @ rslt
            })

  static member collect(list: seq<Result<_ list, _>>) : Result<_ list, _> =
    list |> Seq.toList |> ResultCollector.collect
