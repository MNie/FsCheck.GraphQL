module ShrinkerTests

open Models
open Xunit
open Swensen.Unquote

[<Fact>]
let ``shrink should generate all possible shrinked values``() =
    let input = { name = "name"; argument = None; fields = [ "field1"; "field2"; "field3"; "field4" ] }
    let expectedOutput = [
        { name = "name"; argument = None; fields = ["field2"; "field3"; "field4"] }
        { name = "name"; argument = None; fields = ["field1"; "field3"; "field4"] }
        { name = "name"; argument = None; fields = ["field1"; "field2"; "field4"] }
        { name = "name"; argument = None; fields = ["field1"; "field2"; "field3"] }
    ]

    let result = (Shrinker.shrink input) |> Seq.toList

    result =! expectedOutput

[<Fact>]
let ``shrink should generate none options when fields contains only single element``() =
    let input = { name = "name"; argument = None; fields = [ "field1" ] }

    let result = (Shrinker.shrink input) |> Seq.toList

    result =! []
