module ProductComparer.Tests.ResultCollector

open ProductComparer
open Xunit

[<Fact>]
let ``collects all values`` () =
  let collection = [| Ok [ 1; 2 ]; Ok [ 3 ] |]
  let expected = [ 1; 2; 3 ]

  match ResultCollector.collect collection with
  | Ok result -> Assert.True((expected = result))
  | Error _ -> Assert.True(false, "Should be Ok")


[<Fact>]
let ``collects first error found`` () =
  let collection =
    [| Ok [ 1; 2 ]
       Error "banana"
       Ok [ 3 ]
       Error "pear" |]

  match ResultCollector.collect collection with
  | Ok _ -> Assert.True(false, "Should be error")
  | Error e -> Assert.Equal("banana", e)
